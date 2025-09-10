using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Cut_Roll_Users.Core.MovieEmbeddings.Services;
using Cut_Roll_Users.Core.MovieEmbeddings.Dtos;
using Cut_Roll_Users.Core.Common.VectorDatabases.Options;
using Pinecone;
using System.Net;

namespace Cut_Roll_Users.Infrastructure.Common.VectorDatabases.Services;

/// <summary>
/// VectorMovieDatabaseService implementation using Pinecone.NET with Traefik proxy support
/// </summary>
public class VectorMovieDatabaseService : IVectorMovieDatabaseService, IDisposable
{
    private readonly PineconeOptions _options;
    private readonly PineconeClient _client;
    private readonly ILogger<VectorMovieDatabaseService> _logger;
    private bool _disposed = false;

    public VectorMovieDatabaseService(
        IOptions<PineconeOptions> options,
        ILogger<VectorMovieDatabaseService> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Initialize Pinecone client with proxy support for Traefik
        var clientOptions = new ClientOptions
        {
            HttpClient = new HttpClient(new HttpClientHandler
            {
                Proxy = new WebProxy($"{_options.ProxyHost}:{_options.ProxyPort}")
            })
        };
        
        _client = new PineconeClient(_options.ApiKey, clientOptions);
        
        _logger.LogInformation("VectorMovieDatabaseService initialized with Pinecone index: {IndexName}", _options.IndexName);
    }

    public async Task<bool> UpsertMovieEmbeddingAsync(MovieEmbeddingDto embedding)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(VectorMovieDatabaseService));
        if (embedding == null) throw new ArgumentNullException(nameof(embedding));

        try
        {
            // Get index reference
            var index = _client.Index(_options.IndexName);

            // Create vector for Pinecone
            var vector = new Vector
            {
                Id = embedding.MovieId.ToString(),
                Values = embedding.Embedding.ToArray(),
                Metadata = new Metadata
                {
                    ["title"] = new MetadataValue(embedding.Title),
                    ["movie_id"] = new MetadataValue(embedding.MovieId.ToString()),
                    ["poster_path"] = new MetadataValue(embedding.PosterPath ?? string.Empty),
                    ["dimension"] = new MetadataValue(embedding.Embedding.Count),
                    ["created_at"] = new MetadataValue(DateTime.UtcNow.ToString("O"))
                }
            };

            // Add any additional metadata
            foreach (var kvp in embedding.Metadata)
            {
                vector.Metadata[kvp.Key] = kvp.Value switch
                {
                    string s => new MetadataValue(s),
                    int i => new MetadataValue(i),
                    double d => new MetadataValue(d),
                    bool b => new MetadataValue(b),
                    _ => new MetadataValue(kvp.Value.ToString() ?? string.Empty)
                };
            }

            // Upsert the vector
            var upsertRequest = new UpsertRequest
            {
                Vectors = new[] { vector }
            };
            await index.UpsertAsync(upsertRequest);

            _logger.LogDebug("Successfully upserted embedding for movie {MovieId} with dimension {Dimension}", 
                embedding.MovieId, embedding.Embedding.Count);

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
        if (queryVector == null) throw new ArgumentNullException(nameof(queryVector));

        try
        {
            // Get index reference
            var index = _client.Index(_options.IndexName);

            // Create query request
            var queryRequest = new QueryRequest
            {
                Vector = queryVector.ToArray(),
                TopK = (uint)limit,
                IncludeMetadata = true
            };

            // Note: Filtering by excluded movie IDs is complex with Pinecone API
            // For now, we'll filter results after querying
            // TODO: Implement proper Pinecone filtering when needed

            // Query the index
            var response = await index.QueryAsync(queryRequest);

            var recommendations = new List<MovieRecommendationDto>();

            foreach (var match in response.Matches ?? Enumerable.Empty<ScoredVector>())
            {
                if (Guid.TryParse(match.Id, out var movieId))
                {
                    // Filter out excluded movie IDs
                    if (excludeMovieIds != null && excludeMovieIds.Contains(movieId))
                        continue;

                    recommendations.Add(new MovieRecommendationDto
                    {
                        MovieId = movieId,
                        Title = match.Metadata?.GetValueOrDefault("title")?.ToString() ?? "Unknown",
                        PosterPath = match.Metadata?.GetValueOrDefault("poster_path")?.ToString(),
                        SimilarityScore = (float)(match.Score ?? 0.0)
                    });
                }
            }

            _logger.LogDebug("Found {Count} similar movies for query vector", recommendations.Count);
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
            // Get index reference
            var index = _client.Index(_options.IndexName);

            // Create delete request
            var deleteRequest = new DeleteRequest
            {
                Ids = new List<string> { movieId.ToString() }
            };

            // Delete the vector
            await index.DeleteAsync(deleteRequest);

            _logger.LogDebug("Successfully deleted embedding for movie {MovieId}", movieId);
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
            // Get index reference
            var index = _client.Index(_options.IndexName);

            // Check if index exists and get stats
            var stats = await index.DescribeIndexStatsAsync(new DescribeIndexStatsRequest());

            _logger.LogInformation("Index {IndexName} is ready with {TotalVectorCount} vectors", 
                _options.IndexName, stats.TotalVectorCount);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize index {IndexName}", _options.IndexName);
            return false;
        }
    }

    public async Task<int> GetEmbeddedMoviesCountAsync()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(VectorMovieDatabaseService));

        try
        {
            // Get index reference
            var index = _client.Index(_options.IndexName);
            var stats = await index.DescribeIndexStatsAsync(new DescribeIndexStatsRequest());
            return (int)(stats.TotalVectorCount ?? 0);
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
            return true; // Assume empty if we can't check
        }
    }

    public async Task<bool> CheckVectorDbHealthAsync()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(VectorMovieDatabaseService));

        try
        {
            // Get index reference
            var index = _client.Index(_options.IndexName);
            var stats = await index.DescribeIndexStatsAsync(new DescribeIndexStatsRequest());

            _logger.LogDebug("Vector database health check passed. Total vectors: {Count}", 
                stats.TotalVectorCount);

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
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            // PineconeClient doesn't implement IDisposable
            _disposed = true;
        }
    }
}
