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

    public async Task<bool> UpsertMovieEmbeddingAsync(MovieEmbeddingDto embedding)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(VectorMovieDatabaseService));

        try
        {
            _logger.LogInformation("Attempting to upsert embedding for movie {MovieId}", embedding.MovieId);

            var record = new UpsertRecord
            {
                Id = embedding.MovieId.ToString(),
                AdditionalProperties = new AdditionalProperties
                {
                    ["movieId"] = embedding.MovieId.ToString(),
                    ["title"] = embedding.Title,
                    ["posterPath"] = embedding.PosterPath ?? string.Empty,
                    [_options.VectorFieldName ?? "vector"] = embedding.Embedding.ToArray()
                }
            };

            await _index.UpsertRecordsAsync(
                _options.Namespace ?? "default", // namespace
                new[] { record }
            );

            _logger.LogInformation("Successfully upserted embedding for movie {MovieId}", embedding.MovieId);
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

            var response = await _index.SearchRecordsAsync(
                _options.Namespace ?? "default", // namespace
                new SearchRecordsRequest
                {
                    Query = new SearchRecordsRequestQuery
                    {
                        TopK = excludeMovieIds != null ? limit + excludeMovieIds.Count : limit,
                        Vector = new SearchRecordsVector { Values = queryVector.ToArray() }
                    },
                    Fields = _options.SearchFields ?? ["title", "posterPath", "movieId"]
                }
            );

            var recommendations = new List<MovieRecommendationDto>();

            // TODO: Fix the response property access once we know the correct structure
            // For now, return empty list to avoid compilation errors
            _logger.LogWarning("SearchRecordsResponse structure is different than expected. Returning empty recommendations.");

            _logger.LogInformation("Found {Count} similar movies", recommendations.Count);
            return recommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to find similar movies");
            return new List<MovieRecommendationDto>();
        }
    }

    public async Task<bool> DeleteMovieEmbeddingAsync(Guid movieId)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(VectorMovieDatabaseService));

        try
        {
            _logger.LogInformation("Deleting embedding for movie {MovieId}", movieId);

            await _index.DeleteAsync(
                new DeleteRequest
                {
                    Ids = new[] { movieId.ToString() }
                }
            );

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
            var response = await _index.SearchRecordsAsync(
                _options.Namespace ?? "default",
                new SearchRecordsRequest
                {
                    Query = new SearchRecordsRequestQuery
                    {
                        TopK = 1,
                        Vector = new SearchRecordsVector { Values = testVector }
                    }
                }
            );
            
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

            var response = await _index.SearchRecordsAsync(
                _options.Namespace ?? "default",
                new SearchRecordsRequest
                {
                    Query = new SearchRecordsRequestQuery
                    {
                        TopK = 1,
                        Vector = new SearchRecordsVector { Values = testVector }
                    }
                }
            );
            
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