using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Cut_Roll_Users.Core.MovieEmbeddings.Services;
using Cut_Roll_Users.Core.MovieEmbeddings.Dtos;
using Cut_Roll_Users.Core.Common.VectorDatabases.Options;
using Pinecone;

namespace Cut_Roll_Users.Infrastructure.Common.VectorDatabases.Services;

/// <summary>
/// VectorMovieDatabaseService implementation using official Pinecone C# SDK
/// </summary>
public class VectorMovieDatabaseService : IVectorMovieDatabaseService, IDisposable
{
    private readonly PineconeClient _pineconeClient;
    private readonly IndexClient _index;
    private readonly PineconeOptions _options;
    private readonly ILogger<VectorMovieDatabaseService> _logger;
    private bool _disposed = false;

    public VectorMovieDatabaseService(
        IOptions<PineconeOptions> options,
        ILogger<VectorMovieDatabaseService> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Log configuration values
        _logger.LogInformation("Pinecone configuration loaded:");
        _logger.LogInformation("  - API Key: {ApiKey}...", _options.ApiKey?.Substring(0, Math.Min(10, _options.ApiKey?.Length ?? 0)));
        _logger.LogInformation("  - Environment: {Environment}", _options.Environment);
        _logger.LogInformation("  - Index Name: {IndexName}", _options.IndexName);
        _logger.LogInformation("  - Vector Dimensions: {VectorDimensions}", _options.VectorDimensions);




        // Initialize Pinecone client with official SDK
        _pineconeClient = new PineconeClient(_options.ApiKey);
        _index = _pineconeClient.Index(_options.IndexName);
        
        _logger.LogInformation("VectorMovieDatabaseService initialized with official Pinecone SDK");
    }

    public async Task<bool> UpsertMovieEmbeddingAsync(MovieEmbeddingDto embedding, bool hasEmbedding = false)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(VectorMovieDatabaseService));

        try
        {
            _logger.LogInformation("Attempting to upsert embedding for movie {MovieId}", embedding.MovieId);

            if (hasEmbedding)
            {
                _logger.LogInformation("Embedding already exists for movie {MovieId}, updating...", embedding.MovieId);
            }
            else
            {
                _logger.LogInformation("Creating new embedding for movie {MovieId}", embedding.MovieId);
            }

            var upsertRequest = new UpsertRequest
            {
                Vectors = new List<Vector>
                {
                    new Vector
                    {
                        Id = embedding.MovieId.ToString(),
                        Values = embedding.Embedding.ToArray(),
                        Metadata = new Metadata
                        {
                            ["movieId"] = embedding.MovieId.ToString(),
                            ["title"] = embedding.Title,
                            ["posterPath"] = embedding.PosterPath ?? string.Empty
                        }
                    }
                },
                Namespace = _options.Namespace ?? "__default__"
            };

            var upsertResponse = await _index.UpsertAsync(upsertRequest);

            _logger.LogInformation("Successfully upserted embedding for movie {MovieId}. Upserted count: {UpsertedCount}", 
                embedding.MovieId, upsertResponse.UpsertedCount);
            
            // Log the response details
            _logger.LogInformation("Upsert response - Upserted: {Upserted}", 
                upsertResponse.UpsertedCount);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upsert embedding for movie {MovieId}", embedding.MovieId);
            return false;
        }
    }

    public async Task<List<MovieRecommendationDto>> FindSimilarMoviesAsync(List<float> queryVector, int limit = 10, List<Guid>? excludeMovieIds = null)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(VectorMovieDatabaseService));

        try
        {
            _logger.LogInformation("Finding similar movies with limit {Limit}, excluding {ExcludeCount} movies", 
                limit, excludeMovieIds?.Count ?? 0);
            
            _logger.LogInformation("Query vector dimensions: {Dimensions}, First 5 values: [{Values}]", 
                queryVector.Count, string.Join(", ", queryVector.Take(5)));

            // Check index stats first
            var totalVectors = await GetEmbeddedMoviesCountAsync();
            _logger.LogInformation("Pinecone index contains {TotalVectors} vectors", totalVectors);

            if (totalVectors == 0)
            {
                _logger.LogWarning("Pinecone index is empty! No movies have been embedded yet.");
                return new List<MovieRecommendationDto>();
            }

            var queryResponse = await _index.QueryAsync(new QueryRequest
            {
                Vector = queryVector.ToArray(),
                Namespace = _options.Namespace ?? "__default__",
                TopK = (uint)(excludeMovieIds != null ? limit + excludeMovieIds.Count : limit),
                IncludeMetadata = true,
                // Add similarity threshold to filter out very dissimilar results
                Filter = null, // Could add metadata filters here if needed
                // Note: Pinecone similarity metric is configured at index creation time
                // Common metrics: cosine (default), dotproduct, euclidean
            });

            var recommendations = new List<MovieRecommendationDto>();

            // Debug logging to see what we're getting
            _logger.LogInformation("Query response received. Matches count: {MatchesCount}", 
                queryResponse.Matches?.Count() ?? 0);

            // Parse the response structure: matches[]
            if (queryResponse.Matches != null)
            {
                foreach (var match in queryResponse.Matches)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(match.Id) || !Guid.TryParse(match.Id, out var movieId))
                            continue;

                        // Skip if movie ID is in exclusion list
                        if (excludeMovieIds != null && excludeMovieIds.Contains(movieId))
                            continue;

                        string title = "Unknown Title";
                        string? posterPath = null;

                        if (match.Metadata != null)
                        {
                            if (match.Metadata.TryGetValue("title", out var titleValue))
                                title = titleValue?.ToString() ?? "Unknown Title";
                            if (match.Metadata.TryGetValue("posterPath", out var posterValue))
                                posterPath = posterValue?.ToString();
                        }

                        var similarityScore = (double)(match.Score ?? 0f);
                        
                        // Filter out recommendations with very low similarity scores
                        // Adjust this threshold based on your similarity metric:
                        // - For cosine similarity: 0.0 to 1.0 (higher is better)
                        // - For dot product: can be negative (higher is better)
                        // - For euclidean: 0.0 to infinity (lower is better)
                        var minSimilarityThreshold = 0.3; // Adjust this value based on your needs
                        
                        if (similarityScore >= minSimilarityThreshold)
                        {
                            var recommendation = new MovieRecommendationDto
                            {
                                MovieId = movieId,
                                Title = title,
                                SimilarityScore = similarityScore,
                                PosterPath = posterPath
                            };

                            recommendations.Add(recommendation);
                            _logger.LogDebug("Added recommendation: {Title} with similarity score {Score}", title, similarityScore);
                        }
                        else
                        {
                            _logger.LogDebug("Filtered out recommendation: {Title} with low similarity score {Score}", title, similarityScore);
                        }

                        // Stop if we have enough recommendations
                        if (recommendations.Count >= limit)
                            break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error processing match {MatchId}", match.Id);
                    }
                }
            }

            _logger.LogInformation("Found {Count} similar movies", recommendations.Count);
            return recommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to find similar movies");
            return new List<MovieRecommendationDto>();
        }
    }

    public async Task<bool> UpdateMovieEmbeddingAsync(MovieEmbeddingDto embedding)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(VectorMovieDatabaseService));

        try
        {
            _logger.LogInformation("Updating embedding for movie {MovieId}", embedding.MovieId);

            var updateRequest = new UpdateRequest
            {
                Id = embedding.MovieId.ToString(),
                Values = embedding.Embedding.ToArray(),
                SetMetadata = new Metadata
                {
                    ["movieId"] = embedding.MovieId.ToString(),
                    ["title"] = embedding.Title,
                    ["posterPath"] = embedding.PosterPath ?? string.Empty
                },
                Namespace = _options.Namespace ?? "__default__"
            };

            await _index.UpdateAsync(updateRequest);

            _logger.LogInformation("Successfully updated embedding for movie {MovieId}", embedding.MovieId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update embedding for movie {MovieId}", embedding.MovieId);
            return false;
        }
    }

    public async Task<bool> DeleteMovieEmbeddingAsync(Guid movieId, bool hasEmbedding = true)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(VectorMovieDatabaseService));

        try
        {
            _logger.LogInformation("Deleting embedding for movie {MovieId}", movieId);

            // Check if embedding exists before attempting to delete
            if (!hasEmbedding)
            {
                _logger.LogWarning("Embedding does not exist for movie {MovieId}, nothing to delete", movieId);
                return true; // Return true since the desired state (no embedding) is already achieved
            }

            await _index.DeleteAsync(new DeleteRequest
            {
                Ids = new[] { movieId.ToString() },
                Namespace = _options.Namespace ?? "__default__"
            });

            _logger.LogInformation("Successfully deleted embedding for movie {MovieId}", movieId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete embedding for movie {MovieId}", movieId);
            return false;
        }
    }

    public async Task<bool> InitializeIndexAsync()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(VectorMovieDatabaseService));

        try
        {
            _logger.LogInformation("Initializing vector database index");

            // Test with a simple query to check if the index is accessible
            var testVector = new float[_options.VectorDimensions]; // Dummy vector
            var response = await _index.QueryAsync(new QueryRequest
            {
                Vector = testVector,
                Namespace = _options.Namespace ?? "__default__",
                TopK = 1,
                IncludeMetadata = false
            });
            
            _logger.LogInformation("Vector database initialized and accessible");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize vector database index");
            return false;
        }
    }

    public async Task<int> GetEmbeddedMoviesCountAsync()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(VectorMovieDatabaseService));

        try
        {
            var stats = await _index.DescribeIndexStatsAsync(new DescribeIndexStatsRequest());
            var count = (int)(stats.TotalVectorCount ?? 0);
            
            _logger.LogInformation("Retrieved embedded movies count from Pinecone: {Count}", count);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get embedded movies count");
            return 0;
        }
    }

    public async Task<bool> IsVectorDbEmptyAsync()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(VectorMovieDatabaseService));

        try
        {
            var count = await GetEmbeddedMoviesCountAsync();
            return count == 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if vector database is empty");
            return false; // Return false since we know Pinecone has embeddings
        }
    }

    public async Task<bool> CheckVectorDbHealthAsync()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(VectorMovieDatabaseService));

        try
        {
            // Test with a simple query to verify Pinecone connectivity
            var testVector = new float[_options.VectorDimensions]; // Create a test vector with correct dimensions
            for (int i = 0; i < _options.VectorDimensions; i++)
            {
                testVector[i] = _options.TestVectorValue; // Configurable test values
            }

            var response = await _index.QueryAsync(new QueryRequest
            {
                Vector = testVector,
                Namespace = _options.Namespace ?? "__default__",
                TopK = 1,
                IncludeMetadata = false
            });
            
            _logger.LogInformation("Vector database health check passed");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Vector database health check failed");
            return false;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            // PineconeClient doesn't implement IDisposable in this version
            _disposed = true;
        }
    }
}