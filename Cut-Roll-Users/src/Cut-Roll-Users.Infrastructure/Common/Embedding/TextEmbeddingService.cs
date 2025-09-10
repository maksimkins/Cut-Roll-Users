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
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models", "sentence-transformers.onnx");
                
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
            if (!string.IsNullOrEmpty(_localEmbeddingOptions.TokenizerPath) && File.Exists(_localEmbeddingOptions.TokenizerPath))
            {
                var jsonContent = File.ReadAllText(_localEmbeddingOptions.TokenizerPath);
                _tokenizerVocabulary = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(jsonContent);
                if (_tokenizerVocabulary != null)
                {
                    _logger.LogInformation("Tokenizer vocabulary loaded from {TokenizerPath} with {Count} tokens", 
                        _localEmbeddingOptions.TokenizerPath, _tokenizerVocabulary.Count);
                }
                else
                {
                    _logger.LogWarning("Failed to deserialize tokenizer vocabulary from {TokenizerPath}", _localEmbeddingOptions.TokenizerPath);
                }
            }
            else
            {
                _logger.LogWarning("Tokenizer path not provided or file not found: {TokenizerPath}", _localEmbeddingOptions.TokenizerPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load tokenizer vocabulary from {TokenizerPath}", _localEmbeddingOptions.TokenizerPath);
        }
    }

    // ILocalEmbeddingService properties
    public bool IsModelLoaded => _inferenceSession != null;

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
                embedding = GenerateEmbeddingWithOnnxAsync(text);
            }
            else
            {
                embedding = GenerateFallbackEmbedding(text);
            }

            // Cache the result
            _cache.Set(cacheKey, embedding, TimeSpan.FromHours(24));
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

            // Cache the result
            _cache.Set(cacheKey, embedding, TimeSpan.FromHours(24));
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

        // Basic movie information
        textParts.Add($"Title: {movieData.Title}");
        
        if (!string.IsNullOrEmpty(movieData.OriginalTitle) && movieData.OriginalTitle != movieData.Title)
        {
            textParts.Add($"Original Title: {movieData.OriginalTitle}");
        }

        if (!string.IsNullOrEmpty(movieData.Overview))
        {
            textParts.Add($"Overview: {movieData.Overview}");
        }

        if (!string.IsNullOrEmpty(movieData.Tagline))
        {
            textParts.Add($"Tagline: {movieData.Tagline}");
        }

        // Genres
        if (movieData.Genres.Any())
        {
            textParts.Add($"Genres: {string.Join(", ", movieData.Genres)}");
        }

        // Keywords
        if (movieData.Keywords.Any())
        {
            textParts.Add($"Keywords: {string.Join(", ", movieData.Keywords)}");
        }

        // Cast (top 10)
        if (movieData.Cast.Any())
        {
            var topCast = movieData.Cast.Take(10);
            textParts.Add($"Cast: {string.Join(", ", topCast)}");
        }

        // Crew (top 10)
        if (movieData.Crew.Any())
        {
            var topCrew = movieData.Crew.Take(10);
            textParts.Add($"Crew: {string.Join(", ", topCrew)}");
        }

        // Production companies
        if (movieData.ProductionCompanies.Any())
        {
            textParts.Add($"Production Companies: {string.Join(", ", movieData.ProductionCompanies)}");
        }

        // Production countries
        if (movieData.ProductionCountries.Any())
        {
            textParts.Add($"Production Countries: {string.Join(", ", movieData.ProductionCountries)}");
        }

        // Spoken languages
        if (movieData.SpokenLanguages.Any())
        {
            textParts.Add($"Languages: {string.Join(", ", movieData.SpokenLanguages)}");
        }

        // Additional metadata
        if (movieData.ReleaseDate.HasValue)
        {
            textParts.Add($"Release Year: {movieData.ReleaseDate.Value.Year}");
        }

        if (movieData.Runtime.HasValue)
        {
            textParts.Add($"Runtime: {movieData.Runtime.Value} minutes");
        }

        if (movieData.Budget.HasValue && movieData.Budget.Value > 0)
        {
            textParts.Add($"Budget: ${movieData.Budget.Value:N0}");
        }

        return string.Join(" | ", textParts);
    }

    /// <summary>
    /// Prepares movie data into a text string suitable for embedding
    /// </summary>
    private string PrepareTextForEmbedding(SqlMovieData movieData)
    {
        var textParts = new List<string>();

        // Basic movie information
        textParts.Add($"Title: {movieData.Title}");
        
        if (!string.IsNullOrEmpty(movieData.OriginalTitle) && movieData.OriginalTitle != movieData.Title)
        {
            textParts.Add($"Original Title: {movieData.OriginalTitle}");
        }

        if (!string.IsNullOrEmpty(movieData.Overview))
        {
            textParts.Add($"Overview: {movieData.Overview}");
        }

        if (!string.IsNullOrEmpty(movieData.Tagline))
        {
            textParts.Add($"Tagline: {movieData.Tagline}");
        }

        // Genres
        if (movieData.Genres.Any())
        {
            textParts.Add($"Genres: {string.Join(", ", movieData.Genres)}");
        }

        // Keywords
        if (movieData.Keywords.Any())
        {
            textParts.Add($"Keywords: {string.Join(", ", movieData.Keywords)}");
        }

        // Cast (top 10)
        if (movieData.Cast.Any())
        {
            var topCast = movieData.Cast.Take(10);
            textParts.Add($"Cast: {string.Join(", ", topCast)}");
        }

        // Crew (top 10)
        if (movieData.Crew.Any())
        {
            var topCrew = movieData.Crew.Take(10);
            textParts.Add($"Crew: {string.Join(", ", topCrew)}");
        }

        // Production companies
        if (movieData.ProductionCompanies.Any())
        {
            textParts.Add($"Production Companies: {string.Join(", ", movieData.ProductionCompanies)}");
        }

        // Production countries
        if (movieData.ProductionCountries.Any())
        {
            textParts.Add($"Production Countries: {string.Join(", ", movieData.ProductionCountries)}");
        }

        // Spoken languages
        if (movieData.SpokenLanguages.Any())
        {
            textParts.Add($"Languages: {string.Join(", ", movieData.SpokenLanguages)}");
        }

        // Additional metadata
        if (movieData.ReleaseDate.HasValue)
        {
            textParts.Add($"Release Year: {movieData.ReleaseDate.Value.Year}");
        }

        if (movieData.Runtime.HasValue)
        {
            textParts.Add($"Runtime: {movieData.Runtime.Value} minutes");
        }

        if (movieData.Budget.HasValue && movieData.Budget.Value > 0)
        {
            textParts.Add($"Budget: ${movieData.Budget.Value:N0}");
        }

        return string.Join(" | ", textParts);
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

        // Tokenize text using proper tokenizer or fallback
        var inputIds = TokenizeText(text);

        // Create input tensor
        var inputTensor = new DenseTensor<long>(inputIds, new[] { 1, inputIds.Length });
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_ids", inputTensor)
        };

        // Run inference
        using var results = _inferenceSession.Run(inputs);
        var output = results.First().AsEnumerable<float>().ToArray();

        // Normalize the embedding
        var norm = Math.Sqrt(output.Sum(x => x * x));
        var normalizedOutput = output.Select(x => (float)(x / norm)).ToArray();
        
        return normalizedOutput;
    }

    /// <summary>
    /// Fallback embedding generation using simple text features
    /// </summary>
    private float[] GenerateFallbackEmbedding(string text)
    {
        var words = text.ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 2)
            .ToArray();

        var embeddingDimension = _pineconeOptions.VectorDimension;
        var embedding = new float[embeddingDimension];
        var wordCount = words.Length;

        if (wordCount == 0)
            return embedding;

        // Simple bag-of-words approach with hashing
        foreach (var word in words)
        {
            var hash = Math.Abs(word.GetHashCode());
            var index = hash % embeddingDimension;
            embedding[index] += 1.0f / wordCount;
        }

        // Normalize
        var norm = Math.Sqrt(embedding.Sum(x => x * x));
        if (norm > 0)
        {
            for (int i = 0; i < embedding.Length; i++)
            {
                embedding[i] = (float)(embedding[i] / norm);
            }
        }

        return embedding;
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

