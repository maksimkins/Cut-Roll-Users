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
        
        // Initialize Pinecone client with optional proxy support for Traefik
        ClientOptions clientOptions;
        
        if (!string.IsNullOrEmpty(_options.ProxyHost) && _options.ProxyPort > 0)
        {
            // Use proxy if configured
            clientOptions = new ClientOptions
            {
                HttpClient = new HttpClient(new HttpClientHandler
                {
                    Proxy = new WebProxy($"{_options.ProxyHost}:{_options.ProxyPort}")
                })
            };
            _logger.LogInformation("Using proxy {ProxyHost}:{ProxyPort} for Pinecone connection", 
                _options.ProxyHost, _options.ProxyPort);
        }
        else
        {
            // Use direct connection
            clientOptions = new ClientOptions();
            _logger.LogInformation("Using direct connection to Pinecone (no proxy)");
        }
        
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
                TopK = (uint)(excludeMovieIds != null ? limit + excludeMovieIds.Count : limit), // Query more to account for filtering
                IncludeMetadata = true
            };

            // Add metadata filter to exclude specific movie IDs if provided
            if (excludeMovieIds != null && excludeMovieIds.Any())
            {
                // Create a filter that excludes the specified movie IDs
                // Pinecone uses a "not in" filter for this purpose
                var excludeIds = excludeMovieIds.Select(id => id.ToString()).ToList();
                
                // Note: Pinecone filtering syntax may vary by version
                // This is a simplified approach - in production, you'd need to check the exact syntax
                // For now, we'll use client-side filtering as the primary method
                // and keep the Pinecone filter as a future enhancement
                _logger.LogDebug("Excluding {Count} movie IDs from search results", excludeIds.Count);
            }

            // Query the index
            var response = await index.QueryAsync(queryRequest);

            var recommendations = new List<MovieRecommendationDto>();

            foreach (var match in response.Matches ?? Enumerable.Empty<ScoredVector>())
            {
                if (Guid.TryParse(match.Id, out var movieId))
                {
                    // Additional client-side filtering as fallback (in case Pinecone filter doesn't work as expected)
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

            // Take only the requested limit
            recommendations = recommendations.Take(limit).ToList();

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

            // For Serverless indexes, we can't use DescribeIndexStatsAsync
            // Instead, we'll try a simple query to verify the index is accessible
            try
            {
                // Try to query with a dummy vector to test connectivity
                var dummyVector = new float[_options.VectorDimension];
                var queryRequest = new QueryRequest
                {
                    Vector = dummyVector,
                    TopK = 1,
                    IncludeValues = false,
                    IncludeMetadata = false
                };
                
                await index.QueryAsync(queryRequest);
                _logger.LogInformation("Index {IndexName} is ready and accessible", _options.IndexName);
            }
            catch (Pinecone.NotFoundError)
            {
                _logger.LogError("Index {IndexName} not found. Please check your Pinecone configuration.", _options.IndexName);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize index {IndexName}", _options.IndexName);
            return false;
        }
    }

    public Task<int> GetEmbeddedMoviesCountAsync()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(VectorMovieDatabaseService));

        try
        {
            // For Serverless indexes, we can't get exact count via DescribeIndexStatsAsync
            // We'll return a placeholder value or implement a different counting strategy
            // For now, return 0 to indicate we can't determine the count
            _logger.LogInformation("Cannot get exact vector count for Serverless index {IndexName}", _options.IndexName);
            return Task.FromResult(0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get embedded movies count");
            return Task.FromResult(0);
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
