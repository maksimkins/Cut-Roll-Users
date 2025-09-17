
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Cut_Roll_Users.Core.Common.DataProcessing;
using Cut_Roll_Users.Core.Common.DataProcessing.Models;
using Cut_Roll_Users.Core.Movies.Service;
using Cut_Roll_Users.Core.Common.Options;
using Cut_Roll_Users.Core.MovieEmbeddings.Dtos;
using Cut_Roll_Users.Core.MovieEmbeddings.Services;
using Cut_Roll_Users.Core.Common.VectorDatabases.Options;

namespace Cut_Roll_Users.Infrastructure.Common.Embedding;

public class TextEmbeddingService : ITextEmbeddingService, ILocalEmbeddingService, IDisposable
{
    private readonly ISqlDataReaderService _sqlDataReaderService;
    private readonly IMovieService _movieService;
    private readonly IVectorMovieDatabaseService _vectorDatabaseService;
    private readonly ILogger<TextEmbeddingService> _logger;
    private readonly IMemoryCache _cache;
    private readonly MLContext _mlContext;
    private readonly InferenceSession? _inferenceSession;
    private readonly SemaphoreSlim _semaphore;
    private readonly LocalEmbeddingOptions _localEmbeddingOptions;
    private readonly PineconeOptions _pineconeOptions;
    private Dictionary<string, int>? _tokenizerVocabulary;
    private bool _disposed = false;

    // Cache statistics tracking
    private long _cacheHits = 0;
    private long _cacheMisses = 0;
    private long _cacheSize = 0;
    private int _cacheCount = 0;

    // Configuration constants
    private const int MAX_RETRIES = 3;
    private const string CACHE_KEY_PREFIX = "embedding_";

    // ILocalEmbeddingService property
    public bool IsModelLoaded => _inferenceSession != null;

    public TextEmbeddingService(
        ISqlDataReaderService sqlDataReaderService,
        IMovieService movieService,
        IVectorMovieDatabaseService vectorDatabaseService,
        ILogger<TextEmbeddingService> logger,
        IMemoryCache cache,
        IOptions<LocalEmbeddingOptions> localEmbeddingOptions,
        IOptions<PineconeOptions> pineconeOptions)
    {
        _sqlDataReaderService = sqlDataReaderService ?? throw new ArgumentNullException(nameof(sqlDataReaderService));
        _movieService = movieService ?? throw new ArgumentNullException(nameof(movieService));
        _vectorDatabaseService = vectorDatabaseService ?? throw new ArgumentNullException(nameof(vectorDatabaseService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _localEmbeddingOptions = localEmbeddingOptions?.Value ?? throw new ArgumentNullException(nameof(localEmbeddingOptions));
        _pineconeOptions = pineconeOptions?.Value ?? throw new ArgumentNullException(nameof(pineconeOptions));
        _mlContext = new MLContext();
        _semaphore = new SemaphoreSlim(1, 1); // Single-threaded processing for now

        // Initialize ONNX Runtime session (if model file exists)
        try
        {
            var modelPath = !string.IsNullOrEmpty(_localEmbeddingOptions.ModelPath) 
                ? _localEmbeddingOptions.ModelPath 
                : Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "Data", "Models", "model.onnx");
                
            if (File.Exists(modelPath))
            {
                _inferenceSession = new InferenceSession(modelPath);
                _logger.LogInformation("ONNX model loaded successfully from {ModelPath}", modelPath);
            }
            else
            {
                _logger.LogWarning("ONNX model not found at {ModelPath}. Using fallback embedding generation.", modelPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize ONNX Runtime session");
        }

        // Initialize tokenizer vocabulary if path is provided
        InitializeTokenizer();
    }

    /// <summary>
    /// Initializes the tokenizer vocabulary from JSON file
    /// </summary>
    private void InitializeTokenizer()
    {
        try
        {
            var tokenizerPath = !string.IsNullOrEmpty(_localEmbeddingOptions.TokenizerPath) 
                ? _localEmbeddingOptions.TokenizerPath 
                : Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "Data", "Models", "tokenizer.json");
                
            if (File.Exists(tokenizerPath))
            {
                var jsonContent = File.ReadAllText(tokenizerPath);
                _tokenizerVocabulary = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(jsonContent);
                if (_tokenizerVocabulary != null)
                {
                    _logger.LogInformation("Tokenizer vocabulary loaded from {TokenizerPath} with {Count} tokens", 
                        tokenizerPath, _tokenizerVocabulary.Count);
                }
                else
                {
                    _logger.LogWarning("Failed to deserialize tokenizer vocabulary from {TokenizerPath}", tokenizerPath);
                }
            }
            else
            {
                _logger.LogWarning("Tokenizer path not provided or file not found: {TokenizerPath}", tokenizerPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load tokenizer vocabulary from {TokenizerPath}", _localEmbeddingOptions.TokenizerPath);
        }
    }

    // ILocalEmbeddingService properties
    // ITextEmbeddingService methods
    public async Task<List<float>> GenerateEmbeddingAsync(string text)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(TextEmbeddingService));
        
        var embedding = await GenerateEmbeddingInternalAsync(text);
        return embedding?.ToList() ?? new List<float>();
    }

    public async Task<List<float>> GenerateMovieEmbeddingAsync(MovieDataForEmbeddingDto movieData)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(TextEmbeddingService));
        
        var text = PrepareTextForEmbedding(movieData);
        var embedding = await GenerateEmbeddingInternalAsync(text);
        return embedding?.ToList() ?? new List<float>();
    }

    public async Task<List<List<float>>> GenerateMovieEmbeddingsBatchAsync(List<MovieDataForEmbeddingDto> moviesData)
    {
        var embeddings = new List<List<float>>();
        foreach (var movieData in moviesData)
        {
            var embedding = await GenerateMovieEmbeddingAsync(movieData);
            embeddings.Add(embedding);
        }
        return embeddings;
    }

    // ILocalEmbeddingService methods
    public async Task<List<List<float>>> GenerateEmbeddingsBatchAsync(List<string> texts)
    {
        var embeddings = new List<List<float>>();
        foreach (var text in texts)
        {
            var embedding = await GenerateEmbeddingInternalAsync(text);
            embeddings.Add(embedding?.ToList() ?? new List<float>());
        }
        return embeddings;
    }

    public async Task<bool> InitializeModelAsync()
    {
        try
        {
            // Test the model with a simple text
            var testEmbedding = await GenerateEmbeddingInternalAsync("test");
            return testEmbedding != null && testEmbedding.Any();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize model");
            return false;
        }
    }

    /// <summary>
    /// Generates embeddings for movies without embeddings in batches
    /// </summary>
    public async Task<int> GenerateEmbeddingsForMoviesAsync(int batchSize = 32)
    {
        await _semaphore.WaitAsync();
        try
        {
            _logger.LogInformation("Starting embedding generation for movies without embeddings");

            var totalProcessed = 0;
            var offset = 0;
            var totalMovies = await _sqlDataReaderService.GetTotalMovieCountAsync();
            var effectiveBatchSize = batchSize > 0 ? batchSize : _localEmbeddingOptions.BatchSize;

            _logger.LogInformation("Total movies to process: {TotalMovies}", totalMovies);

            while (offset < totalMovies)
            {
                var movies = await _sqlDataReaderService.ExtractMovieDataBatchAsync(offset, effectiveBatchSize);
                if (!movies.Any())
                    break;

                _logger.LogInformation("Processing batch {Offset}-{End} of {TotalMovies}", 
                    offset, offset + movies.Count - 1, totalMovies);

                var processedInBatch = await ProcessMovieBatchAsync(movies);
                totalProcessed += processedInBatch;

                offset += effectiveBatchSize;

                // Small delay to prevent overwhelming the system
                await Task.Delay(100);
            }

            _logger.LogInformation("Completed embedding generation. Total processed: {TotalProcessed}", totalProcessed);
            return totalProcessed;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Generates embedding for a specific movie by ID
    /// </summary>
    public async Task<bool> GenerateEmbeddingForMovieAsync(Guid movieId)
    {
        try
        {
            var movieData = await _sqlDataReaderService.ExtractMovieDataByIdAsync(movieId);
            if (movieData == null)
            {
                _logger.LogWarning("Movie with ID {MovieId} not found", movieId);
                return false;
            }

            var embedding = await GenerateEmbeddingForMovieDataAsync(movieData);
            if (embedding == null)
            {
                _logger.LogError("Failed to generate embedding for movie {MovieId}", movieId);
                return false;
            }

            // Store embedding (this would typically be stored in a vector database like Pinecone)
            await StoreEmbeddingAsync(movieData, embedding);

            // Mark movie as having embedding
            var success = await _movieService.MarkMovieAsEmbeddedAsync(movieId);
            if (success)
            {
                _logger.LogInformation("Successfully generated and stored embedding for movie {MovieId}", movieId);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embedding for movie {MovieId}", movieId);
            return false;
        }
    }

    /// <summary>
    /// Processes a batch of movies and generates embeddings
    /// </summary>
    private async Task<int> ProcessMovieBatchAsync(List<SqlMovieData> movies)
    {
        var processedCount = 0;

        foreach (var movie in movies)
        {
            try
            {
                var embedding = await GenerateEmbeddingForMovieDataAsync(movie);
                if (embedding != null)
                {
                    await StoreEmbeddingAsync(movie, embedding);
                    
                    var success = await _movieService.MarkMovieAsEmbeddedAsync(movie.Id);
                    if (success)
                    {
                        processedCount++;
                        _logger.LogDebug("Generated embedding for movie {MovieId}: {Title}", movie.Id, movie.Title);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process movie {MovieId}: {Title}", movie.Id, movie.Title);
            }
        }

        return processedCount;
    }

    /// <summary>
    /// Generates embedding vector for movie data
    /// </summary>
    private Task<float[]?> GenerateEmbeddingInternalAsync(string text)
    {
        try
        {
            // Check cache first
            var cacheKey = $"{CACHE_KEY_PREFIX}{text.GetHashCode()}";
            if (_cache.TryGetValue(cacheKey, out float[]? cachedEmbedding) && cachedEmbedding != null)
            {
                Interlocked.Increment(ref _cacheHits);
                return Task.FromResult<float[]?>(cachedEmbedding);
            }
            
            Interlocked.Increment(ref _cacheMisses);

            // Generate embedding using ONNX model or fallback method
            float[] embedding;
            if (_inferenceSession != null)
            {
                _logger.LogDebug("Using ONNX model for embedding generation");
                embedding = GenerateEmbeddingWithOnnxAsync(text);
            }
            else
            {
                _logger.LogDebug("Using fallback method for embedding generation");
                embedding = GenerateFallbackEmbedding(text);
            }

            // Cache the result with size-based eviction
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24),
                Size = CalculateEmbeddingSize(embedding),
                Priority = CacheItemPriority.Normal
            };
            _cache.Set(cacheKey, embedding, cacheOptions);
            UpdateCacheStats(embedding);

            return Task.FromResult<float[]?>(embedding);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embedding for text");
            return Task.FromResult<float[]?>(null);
        }
    }

    /// <summary>
    /// Generates embedding vector for movie data
    /// </summary>
    private Task<float[]?> GenerateEmbeddingForMovieDataAsync(SqlMovieData movieData)
    {
        try
        {
            // Check cache first
            var cacheKey = $"{CACHE_KEY_PREFIX}{movieData.Id}";
            if (_cache.TryGetValue(cacheKey, out float[]? cachedEmbedding) && cachedEmbedding != null)
            {
                Interlocked.Increment(ref _cacheHits);
                return Task.FromResult<float[]?>(cachedEmbedding);
            }
            
            Interlocked.Increment(ref _cacheMisses);

            // Prepare text for embedding
            var textToEmbed = PrepareTextForEmbedding(movieData);
            
            // Generate embedding using ONNX model or fallback method
            float[] embedding;
            if (_inferenceSession != null)
            {
                embedding = GenerateEmbeddingWithOnnxAsync(textToEmbed);
            }
            else
            {
                embedding = GenerateFallbackEmbedding(textToEmbed);
            }

            // Cache the result with size-based eviction
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24),
                Size = CalculateEmbeddingSize(embedding),
                Priority = CacheItemPriority.Normal
            };
            _cache.Set(cacheKey, embedding, cacheOptions);
            UpdateCacheStats(embedding);

            return Task.FromResult<float[]?>(embedding);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embedding for movie {MovieId}", movieData.Id);
            return Task.FromResult<float[]?>(null);
        }
    }

    /// <summary>
    /// Prepares movie data into a text string suitable for embedding
    /// </summary>
    private string PrepareTextForEmbedding(MovieDataForEmbeddingDto movieData)
    {
        var textParts = new List<string>();

        // Basic movie information with unique identifiers
        textParts.Add($"MOVIE_TITLE: {movieData.Title}");
        
        if (!string.IsNullOrEmpty(movieData.OriginalTitle) && movieData.OriginalTitle != movieData.Title)
        {
            textParts.Add($"ORIGINAL_TITLE: {movieData.OriginalTitle}");
        }

        // Add unique movie ID for better distinction
        textParts.Add($"MOVIE_ID: {movieData.Id}");

        if (!string.IsNullOrEmpty(movieData.Overview))
        {
            textParts.Add($"PLOT: {movieData.Overview}");
        }

        if (!string.IsNullOrEmpty(movieData.Tagline))
        {
            textParts.Add($"TAGLINE: {movieData.Tagline}");
        }

        // Genres with weighted importance
        if (movieData.Genres.Any())
        {
            var genreText = string.Join(" ", movieData.Genres.Select(g => $"GENRE_{g}"));
            textParts.Add(genreText);
        }

        // Keywords with weighted importance
        if (movieData.Keywords.Any())
        {
            var keywordText = string.Join(" ", movieData.Keywords.Select(k => $"KEYWORD_{k}"));
            textParts.Add(keywordText);
        }

        // Cast with character names and actor names
        if (movieData.Cast.Any())
        {
            var topCast = movieData.Cast.Take(15); // Increased from 10
            var castText = string.Join(" ", topCast.Select(c => $"ACTOR_{c}"));
            textParts.Add(castText);
        }

        // Crew with specific roles
        if (movieData.Crew.Any())
        {
            var topCrew = movieData.Crew.Take(15); // Increased from 10
            var crewText = string.Join(" ", topCrew.Select(c => $"CREW_{c}"));
            textParts.Add(crewText);
        }

        // Production companies with weighted importance
        if (movieData.ProductionCompanies.Any())
        {
            var companyText = string.Join(" ", movieData.ProductionCompanies.Select(pc => $"STUDIO_{pc}"));
            textParts.Add(companyText);
        }

        // Production countries
        if (movieData.ProductionCountries.Any())
        {
            var countryText = string.Join(" ", movieData.ProductionCountries.Select(pc => $"COUNTRY_{pc}"));
            textParts.Add(countryText);
        }

        // Spoken languages
        if (movieData.SpokenLanguages.Any())
        {
            var languageText = string.Join(" ", movieData.SpokenLanguages.Select(sl => $"LANGUAGE_{sl}"));
            textParts.Add(languageText);
        }

        // Additional metadata with specific formatting
        if (movieData.ReleaseDate.HasValue)
        {
            textParts.Add($"YEAR_{movieData.ReleaseDate.Value.Year}");
            textParts.Add($"DECADE_{movieData.ReleaseDate.Value.Year / 10 * 10}s");
        }

        if (movieData.Runtime.HasValue)
        {
            var runtimeCategory = movieData.Runtime.Value switch
            {
                < 90 => "SHORT_FILM",
                < 120 => "STANDARD_LENGTH",
                < 180 => "LONG_FILM",
                _ => "EPIC_LENGTH"
            };
            textParts.Add($"RUNTIME_{runtimeCategory}_{movieData.Runtime.Value}min");
        }

        if (movieData.Budget.HasValue && movieData.Budget.Value > 0)
        {
            var budgetCategory = movieData.Budget.Value switch
            {
                < 1000000 => "LOW_BUDGET",
                < 10000000 => "MEDIUM_BUDGET",
                < 100000000 => "HIGH_BUDGET",
                _ => "BLOCKBUSTER_BUDGET"
            };
            textParts.Add($"BUDGET_{budgetCategory}");
        }

        // Add movie uniqueness markers
        textParts.Add($"UNIQUE_MOVIE_{movieData.Id.ToString().Substring(0, 8)}");

        return string.Join(" ", textParts);
    }

    /// <summary>
    /// Prepares movie data into a text string suitable for embedding
    /// </summary>
    private string PrepareTextForEmbedding(SqlMovieData movieData)
    {
        var textParts = new List<string>();

        // Basic movie information with unique identifiers
        textParts.Add($"MOVIE_TITLE: {movieData.Title}");
        
        if (!string.IsNullOrEmpty(movieData.OriginalTitle) && movieData.OriginalTitle != movieData.Title)
        {
            textParts.Add($"ORIGINAL_TITLE: {movieData.OriginalTitle}");
        }

        // Add unique movie ID for better distinction
        textParts.Add($"MOVIE_ID: {movieData.Id}");

        if (!string.IsNullOrEmpty(movieData.Overview))
        {
            textParts.Add($"PLOT: {movieData.Overview}");
        }

        if (!string.IsNullOrEmpty(movieData.Tagline))
        {
            textParts.Add($"TAGLINE: {movieData.Tagline}");
        }

        // Genres with weighted importance
        if (movieData.Genres.Any())
        {
            var genreText = string.Join(" ", movieData.Genres.Select(g => $"GENRE_{g}"));
            textParts.Add(genreText);
        }

        // Keywords with weighted importance
        if (movieData.Keywords.Any())
        {
            var keywordText = string.Join(" ", movieData.Keywords.Select(k => $"KEYWORD_{k}"));
            textParts.Add(keywordText);
        }

        // Cast with character names and actor names
        if (movieData.Cast.Any())
        {
            var topCast = movieData.Cast.Take(15); // Increased from 10
            var castText = string.Join(" ", topCast.Select(c => $"ACTOR_{c}"));
            textParts.Add(castText);
        }

        // Crew with specific roles
        if (movieData.Crew.Any())
        {
            var topCrew = movieData.Crew.Take(15); // Increased from 10
            var crewText = string.Join(" ", topCrew.Select(c => $"CREW_{c}"));
            textParts.Add(crewText);
        }

        // Production companies with weighted importance
        if (movieData.ProductionCompanies.Any())
        {
            var companyText = string.Join(" ", movieData.ProductionCompanies.Select(pc => $"STUDIO_{pc}"));
            textParts.Add(companyText);
        }

        // Production countries
        if (movieData.ProductionCountries.Any())
        {
            var countryText = string.Join(" ", movieData.ProductionCountries.Select(pc => $"COUNTRY_{pc}"));
            textParts.Add(countryText);
        }

        // Spoken languages
        if (movieData.SpokenLanguages.Any())
        {
            var languageText = string.Join(" ", movieData.SpokenLanguages.Select(sl => $"LANGUAGE_{sl}"));
            textParts.Add(languageText);
        }

        // Additional metadata with specific formatting
        if (movieData.ReleaseDate.HasValue)
        {
            textParts.Add($"YEAR_{movieData.ReleaseDate.Value.Year}");
            textParts.Add($"DECADE_{movieData.ReleaseDate.Value.Year / 10 * 10}s");
        }

        if (movieData.Runtime.HasValue)
        {
            var runtimeCategory = movieData.Runtime.Value switch
            {
                < 90 => "SHORT_FILM",
                < 120 => "STANDARD_LENGTH",
                < 180 => "LONG_FILM",
                _ => "EPIC_LENGTH"
            };
            textParts.Add($"RUNTIME_{runtimeCategory}_{movieData.Runtime.Value}min");
        }

        if (movieData.Budget.HasValue && movieData.Budget.Value > 0)
        {
            var budgetCategory = movieData.Budget.Value switch
            {
                < 1000000 => "LOW_BUDGET",
                < 10000000 => "MEDIUM_BUDGET",
                < 100000000 => "HIGH_BUDGET",
                _ => "BLOCKBUSTER_BUDGET"
            };
            textParts.Add($"BUDGET_{budgetCategory}");
        }

        // Add movie uniqueness markers
        textParts.Add($"UNIQUE_MOVIE_{movieData.Id.ToString().Substring(0, 8)}");

        return string.Join(" ", textParts);
    }

    /// <summary>
    /// Tokenizes text using vocabulary or fallback method
    /// </summary>
    private long[] TokenizeText(string text)
    {
        const int maxTokens = 512; // Standard BERT limit
        const int vocabSize = 30000; // Common vocabulary size
        
        if (_tokenizerVocabulary != null)
        {
            // Use proper tokenizer vocabulary
            var tokens = text.ToLowerInvariant()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Take(maxTokens)
                .Select(token => _tokenizerVocabulary.TryGetValue(token, out var id) ? id : 0) // 0 for unknown tokens
                .ToArray();
            
            // Pad or truncate to maxTokens
            var inputIds = new long[maxTokens];
            Array.Copy(tokens, inputIds, Math.Min(tokens.Length, maxTokens));
            return inputIds;
        }
        else
        {
            // Fallback to simple hash-based tokenization
            var tokens = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var inputIds = new long[Math.Min(tokens.Length, maxTokens)];
            for (int i = 0; i < inputIds.Length; i++)
            {
                inputIds[i] = Math.Abs(tokens[i].GetHashCode()) % vocabSize;
            }
            return inputIds;
        }
    }

    /// <summary>
    /// Generates embedding using ONNX model
    /// </summary>
    private float[] GenerateEmbeddingWithOnnxAsync(string text)
    {
        if (_inferenceSession == null)
            throw new InvalidOperationException("ONNX session not initialized");

        _logger.LogInformation("Generating ONNX embedding for text: {Text}", text.Substring(0, Math.Min(100, text.Length)));

        // Tokenize text using proper tokenizer or fallback
        var inputIds = TokenizeText(text);
        _logger.LogDebug("Tokenized text into {TokenCount} tokens", inputIds.Length);

        // Create input tensors
        var inputTensor = new DenseTensor<long>(inputIds, new[] { 1, inputIds.Length });
        
        // Create attention mask (1 for real tokens, 0 for padding)
        var attentionMask = new long[inputIds.Length];
        Array.Fill(attentionMask, 1L); // All tokens are real (no padding in our case)
        var attentionMaskTensor = new DenseTensor<long>(attentionMask, new[] { 1, attentionMask.Length });
        
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_ids", inputTensor),
            NamedOnnxValue.CreateFromTensor("attention_mask", attentionMaskTensor)
        };

        // Run inference
        using var results = _inferenceSession.Run(inputs);
        
        // Debug: Log available outputs
        _logger.LogInformation("ONNX model outputs: {Count}", results.Count());
        foreach (var result in results)
        {
            var dimensions = result.AsTensor<float>().Dimensions.ToArray();
            _logger.LogInformation("Output name: {Name}, Shape: {Shape}", result.Name, string.Join("x", dimensions));
        }
        
        // Prioritize last_hidden_state for rich token-level embeddings
        var embeddingOutput = results.FirstOrDefault(r => r.Name.Contains("last_hidden_state")) ??
            results.FirstOrDefault(r => r.Name.Contains("pooler_output")) ??
            results.FirstOrDefault(r => r.Name.Contains("sentence_embedding")) ??
            results.FirstOrDefault(r => r.Name.Contains("embeddings")) ??
            results.FirstOrDefault(r => r.Name.Contains("output"));
            
        // If we still don't have a good output, log all available outputs and use the first one
        if (embeddingOutput == null)
        {
            _logger.LogWarning("No suitable output found. Available outputs:");
            foreach (var result in results)
            {
                var dimensions = result.AsTensor<float>().Dimensions.ToArray();
                _logger.LogWarning("  - {Name}: {Shape}", result.Name, string.Join("x", dimensions));
            }
            embeddingOutput = results.First();
        }
        
        // Log which output was selected
        var selectedDimensions = embeddingOutput.AsTensor<float>().Dimensions.ToArray();
        _logger.LogInformation("Selected output: {Name}, Shape: {Shape}", embeddingOutput.Name, string.Join("x", selectedDimensions));
            
        var output = embeddingOutput.AsEnumerable<float>().ToArray();
        
        // Handle different output shapes for last_hidden_state
        if (embeddingOutput.Name.Contains("last_hidden_state"))
        {
            _logger.LogInformation("Processing last_hidden_state output with shape {Shape}", string.Join("x", selectedDimensions));
            
            // last_hidden_state is typically [batch_size, sequence_length, hidden_size]
            // We need to convert it to a single vector
            if (selectedDimensions.Length == 3 && selectedDimensions[0] == 1)
            {
                var sequenceLength = selectedDimensions[1];
                var hiddenSize = selectedDimensions[2];
                var expectedTotalSize = sequenceLength * hiddenSize;
                
                _logger.LogInformation("last_hidden_state dimensions: batch={Batch}, seq_len={SeqLen}, hidden_size={HiddenSize}, total={Total}", 
                    selectedDimensions[0], sequenceLength, hiddenSize, expectedTotalSize);
                
                if (output.Length == expectedTotalSize)
                {
                    // Use the full last_hidden_state as a flattened vector (4,608 dimensions)
                    // This preserves all token-level information for maximum semantic richness
                    _logger.LogInformation("Using full last_hidden_state: {SeqLen} tokens x {HiddenSize} dimensions = {Total} dimensions", 
                        sequenceLength, hiddenSize, output.Length);
                    // output is already the correct size (4,608), no pooling needed
                }
                else
                {
                    _logger.LogError("last_hidden_state size mismatch: expected {Expected}, got {Actual}", expectedTotalSize, output.Length);
                    return GenerateFallbackEmbedding(text);
                }
            }
            else
            {
                _logger.LogWarning("Unexpected last_hidden_state shape: {Shape}", string.Join("x", selectedDimensions));
            }
        }
        
        // Check if the output dimension matches expected Pinecone dimension
        var expectedDimension = _pineconeOptions.VectorDimensions;
        _logger.LogInformation("Final embedding dimensions: {Actual}, Expected: {Expected}", output.Length, expectedDimension);
        
        if (output.Length != expectedDimension)
        {
            _logger.LogError("Embedding dimension mismatch! Actual: {Actual}, Expected: {Expected}. Using fallback.", 
                output.Length, expectedDimension);
            return GenerateFallbackEmbedding(text);
        }

        // Normalize the embedding
        var norm = Math.Sqrt(output.Sum(x => x * x));
        if (norm > 0)
        {
            var normalizedOutput = output.Select(x => (float)(x / norm)).ToArray();
            return normalizedOutput;
        }
        
        return output;
    }

    /// <summary>
    /// Enhanced fallback embedding generation using improved text features and semantic understanding
    /// </summary>
    private float[] GenerateFallbackEmbedding(string text)
    {
        _logger.LogInformation("Using enhanced fallback embedding generation for text: {Text}", text.Substring(0, Math.Min(100, text.Length)));
        
        var words = text.ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 2)
            .ToArray();

        var embeddingDimension = _pineconeOptions.VectorDimensions;
        var embedding = new float[embeddingDimension];
        var wordCount = words.Length;
        
        _logger.LogInformation("Enhanced fallback embedding: {WordCount} words, dimension: {Dimension}", wordCount, embeddingDimension);

        if (wordCount == 0)
            return embedding;

        // Enhanced approach: Use semantic word categories and better weighting
        var uniqueWords = words.Distinct().ToArray();
        var wordFrequencies = words.GroupBy(w => w).ToDictionary(g => g.Key, g => g.Count());
        
        // Generate n-grams (1-gram, 2-gram, 3-gram, 4-gram) for better context
        var nGrams = GenerateNGrams(words, 4);
        
        // Enhanced hash functions with better distribution
        var hashFunctions = new Func<string, int>[] 
        {
            s => Math.Abs(s.GetHashCode()),
            s => Math.Abs(s.GetHashCode() * 31 + 17),
            s => Math.Abs(s.GetHashCode() * 37 + 23),
            s => Math.Abs(s.GetHashCode() * 41 + 29),
            s => Math.Abs(s.GetHashCode() * 43 + 31), // Additional hash function
            s => Math.Abs(s.GetHashCode() * 47 + 37)  // Additional hash function
        };

        // Categorize words by importance and type
        var importantWords = CategorizeWordsByImportance(uniqueWords);
        
        // Process words with category-based weighting
        foreach (var word in uniqueWords)
        {
            var frequency = wordFrequencies[word];
            var baseWeight = (float)frequency / wordCount;
            
            // Apply category-based weight multipliers
            var categoryWeight = importantWords.TryGetValue(word, out var category) 
                ? GetCategoryWeightMultiplier(category) 
                : 1.0f;
            
            var finalWeight = baseWeight * categoryWeight;
            
            foreach (var hashFunc in hashFunctions)
            {
                var hash = hashFunc(word);
                var index = hash % embeddingDimension;
                embedding[index] += finalWeight * (1.0f / hashFunctions.Length);
            }
        }

        // Process n-grams with enhanced weighting
        foreach (var nGram in nGrams)
        {
            var nGramText = string.Join(" ", nGram);
            var baseWeight = 1.0f / (nGram.Length * wordCount);
            
            // Higher weight for longer n-grams (more context)
            var nGramWeight = baseWeight * (1.0f + (nGram.Length - 1) * 0.2f);
            
            foreach (var hashFunc in hashFunctions)
            {
                var hash = hashFunc(nGramText);
                var index = hash % embeddingDimension;
                embedding[index] += nGramWeight * 0.15f; // Increased weight for n-grams
            }
        }

        // Add semantic diversity through controlled randomness
        var random = new Random(text.GetHashCode());
        for (int i = 0; i < embedding.Length; i++)
        {
            // Use different noise patterns for different vector regions
            var noisePattern = (i % 4) switch
            {
                0 => (float)(random.NextDouble() - 0.5) * 0.02f, // Slightly higher noise
                1 => (float)(random.NextDouble() - 0.5) * 0.015f,
                2 => (float)(random.NextDouble() - 0.5) * 0.01f,
                _ => (float)(random.NextDouble() - 0.5) * 0.005f
            };
            embedding[i] += noisePattern;
        }

        // Enhanced normalization with L2 regularization
        var norm = Math.Sqrt(embedding.Sum(x => x * x));
        if (norm > 0)
        {
            // Apply L2 regularization to prevent overfitting
            var regularizationFactor = 0.01f;
            var regularizedNorm = Math.Sqrt(norm * norm + regularizationFactor);
            
            for (int i = 0; i < embedding.Length; i++)
            {
                embedding[i] = (float)(embedding[i] / regularizedNorm);
            }
        }

        return embedding;
    }

    /// <summary>
    /// Categorizes words by their semantic importance for movie recommendations
    /// </summary>
    private Dictionary<string, WordCategory> CategorizeWordsByImportance(string[] words)
    {
        var categories = new Dictionary<string, WordCategory>();
        
        // Define important word patterns
        var genreKeywords = new[] { "action", "comedy", "drama", "horror", "thriller", "romance", "sci-fi", "fantasy", "documentary", "animation" };
        var qualityKeywords = new[] { "award", "oscar", "nominated", "critically", "acclaimed", "masterpiece", "classic", "cult" };
        var technicalKeywords = new[] { "director", "producer", "cinematography", "score", "soundtrack", "visual", "effects" };
        var thematicKeywords = new[] { "love", "war", "family", "friendship", "adventure", "mystery", "crime", "justice", "freedom" };
        
        foreach (var word in words)
        {
            if (genreKeywords.Any(k => word.Contains(k)))
                categories[word] = WordCategory.Genre;
            else if (qualityKeywords.Any(k => word.Contains(k)))
                categories[word] = WordCategory.Quality;
            else if (technicalKeywords.Any(k => word.Contains(k)))
                categories[word] = WordCategory.Technical;
            else if (thematicKeywords.Any(k => word.Contains(k)))
                categories[word] = WordCategory.Thematic;
            else if (word.StartsWith("genre_") || word.StartsWith("keyword_") || word.StartsWith("actor_") || word.StartsWith("crew_"))
                categories[word] = WordCategory.Metadata;
            else
                categories[word] = WordCategory.General;
        }
        
        return categories;
    }

    /// <summary>
    /// Gets weight multiplier based on word category
    /// </summary>
    private float GetCategoryWeightMultiplier(WordCategory category)
    {
        return category switch
        {
            WordCategory.Genre => 2.0f,      // Genres are very important
            WordCategory.Quality => 1.8f,    // Quality indicators are important
            WordCategory.Metadata => 1.5f,   // Structured metadata is important
            WordCategory.Technical => 1.3f,  // Technical aspects matter
            WordCategory.Thematic => 1.2f,   // Themes are somewhat important
            WordCategory.General => 1.0f,    // General words get base weight
            _ => 1.0f
        };
    }

    /// <summary>
    /// Word categories for semantic understanding
    /// </summary>
    private enum WordCategory
    {
        Genre,
        Quality,
        Metadata,
        Technical,
        Thematic,
        General
    }

    /// <summary>
    /// Generates n-grams from a word array
    /// </summary>
    private List<string[]> GenerateNGrams(string[] words, int maxN)
    {
        var nGrams = new List<string[]>();
        
        for (int n = 1; n <= maxN; n++)
        {
            for (int i = 0; i <= words.Length - n; i++)
            {
                var nGram = new string[n];
                Array.Copy(words, i, nGram, 0, n);
                nGrams.Add(nGram);
            }
        }
        
        return nGrams;
    }

    /// <summary>
    /// Stores embedding in vector database
    /// </summary>
    private async Task StoreEmbeddingAsync(SqlMovieData movieData, float[] embedding)
    {
        try
        {
            var movieEmbedding = new MovieEmbeddingDto
            {
                MovieId = movieData.Id,
                Title = movieData.Title,
                PosterPath = movieData.PosterPath,
                Embedding = embedding.ToList(),
                Metadata = new Dictionary<string, object>
                {
                    { "dimension", embedding.Length },
                    { "created_at", DateTime.UtcNow },
                    { "model_version", "1.0" }
                }
            };

            var success = await _vectorDatabaseService.UpsertMovieEmbeddingAsync(movieEmbedding);
            if (success)
            {
                _logger.LogDebug("Successfully stored embedding for movie {MovieId} with dimension {Dimension}", 
                    movieData.Id, embedding.Length);
            }
            else
            {
                _logger.LogWarning("Failed to store embedding for movie {MovieId}", movieData.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing embedding for movie {MovieId}", movieData.Id);
        }
    }

    /// <summary>
    /// Clears embedding cache
    /// </summary>
    public void ClearCache()
    {
        _cache.Remove(CACHE_KEY_PREFIX);
        
        // Reset statistics
        Interlocked.Exchange(ref _cacheCount, 0);
        Interlocked.Exchange(ref _cacheSize, 0);
        Interlocked.Exchange(ref _cacheHits, 0);
        Interlocked.Exchange(ref _cacheMisses, 0);
        
        _logger.LogInformation("Embedding cache cleared and statistics reset");
    }

    /// <summary>
    /// Updates cache statistics when adding new items
    /// </summary>
    private void UpdateCacheStats(float[] embedding)
    {
        Interlocked.Increment(ref _cacheCount);
        Interlocked.Add(ref _cacheSize, CalculateEmbeddingSize(embedding));
    }

    /// <summary>
    /// Calculates the approximate size of an embedding array in bytes
    /// </summary>
    private static long CalculateEmbeddingSize(float[] embedding)
    {
        // Size = array length * sizeof(float) + object overhead
        return embedding.Length * sizeof(float) + 24; // 24 bytes for object overhead
    }

    /// <summary>
    /// Gets cache statistics
    /// </summary>
    public (int Count, long Size) GetCacheStats()
    {
        return (_cacheCount, _cacheSize);
    }

    /// <summary>
    /// Gets detailed cache statistics including hit/miss ratios
    /// </summary>
    public (int Count, long Size, long Hits, long Misses, double HitRatio) GetDetailedCacheStats()
    {
        var totalRequests = _cacheHits + _cacheMisses;
        var hitRatio = totalRequests > 0 ? (double)_cacheHits / totalRequests : 0.0;
        
        return (_cacheCount, _cacheSize, _cacheHits, _cacheMisses, hitRatio);
    }

    /// <summary>
    /// Gets cache performance metrics for monitoring
    /// </summary>
    public string GetCachePerformanceReport()
    {
        var stats = GetDetailedCacheStats();
        var avgSizePerItem = stats.Count > 0 ? stats.Size / stats.Count : 0;
        
        return $"Cache Performance Report:\n" +
               $"- Items: {stats.Count}\n" +
               $"- Total Size: {FormatBytes(stats.Size)}\n" +
               $"- Average Size per Item: {FormatBytes(avgSizePerItem)}\n" +
               $"- Cache Hits: {stats.Hits}\n" +
               $"- Cache Misses: {stats.Misses}\n" +
               $"- Hit Ratio: {stats.HitRatio:P2}";
    }

    /// <summary>
    /// Formats bytes into human-readable format
    /// </summary>
    private static string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int counter = 0;
        decimal number = bytes;
        while (Math.Round(number / 1024) >= 1)
        {
            number = number / 1024;
            counter++;
        }
        return $"{number:n1} {suffixes[counter]}";
    }

    /// <summary>
    /// Reduces embedding dimensions using mean pooling and PCA-like reduction
    /// </summary>
    private float[] ReduceEmbeddingDimensions(float[] embedding, int targetDimensions)
    {
        try
        {
            _logger.LogDebug("Reducing embedding dimensions from {Source} to {Target}", embedding.Length, targetDimensions);
            
            // For large embeddings, try to identify if it's a sequence of embeddings
            // Common sequence lengths: 512, 256, 128, 64, 32, 16, 8
            var possibleSequenceLengths = new[] { 512, 256, 128, 64, 32, 16, 8 };
            
            foreach (var seqLen in possibleSequenceLengths)
            {
                if (embedding.Length % seqLen == 0)
                {
                    var hiddenSize = embedding.Length / seqLen;
                    _logger.LogDebug("Detected potential sequence structure: seq_len={SeqLen}, hidden_size={HiddenSize}", seqLen, hiddenSize);
                    
                    // Apply mean pooling over the sequence dimension
                    var pooledEmbedding = new float[hiddenSize];
                    for (int i = 0; i < hiddenSize; i++)
                    {
                        float sum = 0;
                        for (int j = 0; j < seqLen; j++)
                        {
                            sum += embedding[j * hiddenSize + i];
                        }
                        pooledEmbedding[i] = sum / seqLen;
                    }
                    
                    // If the pooled embedding is already the target size, return it
                    if (hiddenSize == targetDimensions)
                    {
                        _logger.LogDebug("Mean pooling resulted in target dimension {Target}", targetDimensions);
                        return pooledEmbedding;
                    }
                    
                    // If pooled embedding is still too large, apply further reduction
                    if (hiddenSize > targetDimensions)
                    {
                        _logger.LogDebug("Applying further reduction from {HiddenSize} to {Target}", hiddenSize, targetDimensions);
                        return ApplyDimensionReduction(pooledEmbedding, targetDimensions);
                    }
                    
                    // If pooled embedding is smaller than target, pad it
                    if (hiddenSize < targetDimensions)
                    {
                        _logger.LogDebug("Padding embedding from {HiddenSize} to {Target}", hiddenSize, targetDimensions);
                        return PadEmbedding(pooledEmbedding, targetDimensions);
                    }
                }
            }
            
            // If no sequence structure detected, apply direct dimension reduction
            _logger.LogDebug("No sequence structure detected, applying direct dimension reduction");
            return ApplyDimensionReduction(embedding, targetDimensions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during dimension reduction, using fallback");
            return GenerateFallbackEmbedding("dimension_reduction_fallback");
        }
    }

    /// <summary>
    /// Applies improved dimension reduction using weighted random projection
    /// </summary>
    private float[] ApplyDimensionReduction(float[] embedding, int targetDimensions)
    {
        if (embedding.Length <= targetDimensions)
        {
            return embedding;
        }
        
        // Improved random projection with better weight distribution
        var random = new Random(42); // Fixed seed for consistency
        var reduced = new float[targetDimensions];
        
        // Calculate the reduction ratio
        var reductionRatio = (float)targetDimensions / embedding.Length;
        
        for (int i = 0; i < targetDimensions; i++)
        {
            float sum = 0;
            var weightSum = 0f;
            
            for (int j = 0; j < embedding.Length; j++)
            {
                // Use different weight strategies for different dimensions
                float weight;
                if (i < targetDimensions / 2)
                {
                    // First half: use Gaussian-like weights
                    weight = (float)(NextGaussian(random) * 0.5 + 0.5);
                }
                else
                {
                    // Second half: use uniform weights with some bias
                    weight = (float)(random.NextDouble() * 2 - 1);
                }
                
                // Apply weight based on original position importance
                var positionWeight = 1.0f - (float)j / embedding.Length;
                weight *= positionWeight;
                
                sum += embedding[j] * weight;
                weightSum += Math.Abs(weight);
            }
            
            // Normalize by weight sum to maintain scale
            reduced[i] = weightSum > 0 ? sum / weightSum : 0;
        }
        
        _logger.LogDebug("Successfully reduced dimensions from {Source} to {Target} using improved projection", embedding.Length, targetDimensions);
        return reduced;
    }

    /// <summary>
    /// Pads embedding to target dimensions by repeating values
    /// </summary>
    private float[] PadEmbedding(float[] embedding, int targetDimensions)
    {
        if (embedding.Length >= targetDimensions)
        {
            return embedding;
        }
        
        var padded = new float[targetDimensions];
        var repeatCount = targetDimensions / embedding.Length;
        var remainder = targetDimensions % embedding.Length;
        
        for (int i = 0; i < targetDimensions; i++)
        {
            var sourceIndex = i % embedding.Length;
            padded[i] = embedding[sourceIndex];
        }
        
        _logger.LogDebug("Padded embedding from {Source} to {Target} dimensions", embedding.Length, targetDimensions);
        return padded;
    }

    /// <summary>
    /// Generates a Gaussian random number using Box-Muller transform
    /// </summary>
    private static double NextGaussian(Random random, double mean = 0, double stdDev = 1)
    {
        // Box-Muller transform
        double u1 = 1.0 - random.NextDouble(); // uniform(0,1] random doubles
        double u2 = 1.0 - random.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); // random normal(0,1)
        return mean + stdDev * randStdNormal; // random normal(mean,stdDev^2)
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _inferenceSession?.Dispose();
            _semaphore?.Dispose();
            // MLContext doesn't need explicit disposal
            _disposed = true;
        }
    }
}

