using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Cut_Roll_Users.Core.MovieEmbeddings.Services;
using Cut_Roll_Users.Core.MovieEmbeddings.Dtos;
using Cut_Roll_Users.Core.Common.VectorDatabases.Options;
using System.Text.Json;
using System.Net;

namespace Cut_Roll_Users.Infrastructure.Common.VectorDatabases.Services;

/// <summary>
/// VectorMovieDatabaseService implementation using Pinecone HTTP API for Serverless indexes
/// </summary>
public class VectorMovieDatabaseService : IVectorMovieDatabaseService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly PineconeOptions _options;
    private readonly ILogger<VectorMovieDatabaseService> _logger;
    private bool _disposed = false;
    private readonly string _baseUrl;

    public VectorMovieDatabaseService(
        IOptions<PineconeOptions> options,
        ILogger<VectorMovieDatabaseService> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // For Serverless indexes, we need to use the HTTP API directly
        // The base URL format for Serverless is: https://{index-name}-{environment}.svc.{region}.pinecone.io
        // Based on your actual index, the format is: https://movie-imbeddings-svsa9sf.svc.aped-4627-b74a.pinecone.io
        _baseUrl = "https://movie-imbeddings-svsa9sf.svc.aped-4627-b74a.pinecone.io";
        
        // Initialize HTTP client with optional proxy support for Traefik
        if (!string.IsNullOrEmpty(_options.ProxyHost) && _options.ProxyPort > 0)
        {
            // Use proxy if configured
            var handler = new HttpClientHandler
            {
                Proxy = new WebProxy($"{_options.ProxyHost}:{_options.ProxyPort}")
            };
            _httpClient = new HttpClient(handler);
            _logger.LogInformation("Using proxy {ProxyHost}:{ProxyPort} for Pinecone connection", 
                _options.ProxyHost, _options.ProxyPort);
        }
        else
        {
            // Use direct connection
            _httpClient = new HttpClient();
            _logger.LogInformation("Using direct connection to Pinecone (no proxy)");
        }
        
        // Set up authentication header
        if (string.IsNullOrEmpty(_options.ApiKey))
        {
            _logger.LogError("PINECONE_API_KEY is null or empty! Check your environment variables.");
            throw new InvalidOperationException("PINECONE_API_KEY is not configured");
        }
        
        _httpClient.DefaultRequestHeaders.Add("Api-Key", _options.ApiKey);
        
        _logger.LogInformation("VectorMovieDatabaseService initialized with Pinecone Serverless index: {IndexName} at {BaseUrl}", 
            _options.IndexName, _baseUrl);
        _logger.LogWarning("API Key (first 10 chars): {ApiKeyPrefix}", 
            _options.ApiKey?.Length > 10 ? _options.ApiKey.Substring(0, 10) + "..." : _options.ApiKey);
    }

    public async Task<bool> UpsertMovieEmbeddingAsync(MovieEmbeddingDto embedding)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(VectorMovieDatabaseService));

        try
        {
            var upsertRequest = new
            {
                vectors = new[]
                {
                    new
                    {
                        id = embedding.MovieId.ToString(),
                        values = embedding.Embedding.ToArray(),
                        metadata = new Dictionary<string, object>
                        {
                            ["movieId"] = embedding.MovieId.ToString(),
                            ["title"] = embedding.Title,
                            ["posterPath"] = embedding.PosterPath ?? ""
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(upsertRequest);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            _logger.LogWarning("Attempting to upsert embedding for movie {MovieId} to URL: {Url}", embedding.MovieId, $"{_baseUrl}/vectors/upsert");
            _logger.LogWarning("Request headers: {Headers}", string.Join(", ", _httpClient.DefaultRequestHeaders.Select(h => $"{h.Key}={h.Value.FirstOrDefault()}")));
            _logger.LogWarning("Request body: {RequestBody}", json);
            
            // Create a fresh request with explicit headers
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/vectors/upsert");
            request.Headers.Add("Api-Key", _options.ApiKey);
            request.Content = content;
            
            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Successfully upserted embedding for movie {MovieId}", embedding.MovieId);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to upsert embedding for movie {MovieId}. Status: {StatusCode}, Error: {Error}, URL: {Url}", 
                    embedding.MovieId, response.StatusCode, errorContent, $"{_baseUrl}/vectors/upsert");
                return false;
            }
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
            var queryRequest = new
            {
                vector = queryVector.ToArray(),
                topK = limit,
                includeValues = false,
                includeMetadata = true
            };

            var json = JsonSerializer.Serialize(queryRequest);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/query", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var queryResponse = JsonSerializer.Deserialize<QueryResponse>(responseContent);
                
                var recommendations = new List<MovieRecommendationDto>();
                
                if (queryResponse?.Matches != null)
                {
                    foreach (var match in queryResponse.Matches)
                    {
                        if (match.Metadata != null && Guid.TryParse(match.Metadata.MovieId, out var movieId))
                        {
                            // Skip excluded movies
                            if (excludeMovieIds != null && excludeMovieIds.Contains(movieId))
                                continue;
                                
                            recommendations.Add(new MovieRecommendationDto
                            {
                                MovieId = movieId,
                                Title = match.Metadata.Title ?? "Unknown",
                                PosterPath = match.Metadata.PosterPath,
                                SimilarityScore = match.Score ?? 0.0
                            });
                        }
                    }
                }
                
                return recommendations;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to query similar movies. Status: {StatusCode}, Error: {Error}", 
                    response.StatusCode, errorContent);
                return new List<MovieRecommendationDto>();
            }
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
            var deleteRequest = new
            {
                ids = new[] { movieId.ToString() }
            };

            var json = JsonSerializer.Serialize(deleteRequest);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/vectors/delete", content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Successfully deleted embedding for movie {MovieId}", movieId);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to delete embedding for movie {MovieId}. Status: {StatusCode}, Error: {Error}", 
                    movieId, response.StatusCode, errorContent);
                return false;
            }
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
            // For Serverless indexes, we can't use DescribeIndexStatsAsync
            // Instead, we'll try a simple query to verify the index is accessible
            try
            {
                // Try to query with a dummy vector to test connectivity
                var dummyVector = new float[_options.VectorDimension];
                var queryRequest = new
                {
                    vector = dummyVector,
                    topK = 1,
                    includeValues = false,
                    includeMetadata = false
                };
                
                var json = JsonSerializer.Serialize(queryRequest);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_baseUrl}/query", content);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Index {IndexName} is ready and accessible", _options.IndexName);
                    return true;
                }
                else
                {
                    _logger.LogError("Index {IndexName} is not accessible. Status: {StatusCode}", 
                        _options.IndexName, response.StatusCode);
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Index {IndexName} not found or not accessible. Please check your Pinecone configuration.", _options.IndexName);
                return false;
            }
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
            // For Serverless indexes, we can't get exact count via HTTP API
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
            return await InitializeIndexAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check vector database health");
            return false;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient?.Dispose();
            _disposed = true;
        }
    }
}

// Helper classes for JSON deserialization
public class QueryResponse
{
    public List<VectorMatch>? Matches { get; set; }
}

public class VectorMatch
{
    public float? Score { get; set; }
    public VectorMetadata? Metadata { get; set; }
}

public class VectorMetadata
{
    public string? MovieId { get; set; }
    public string? Title { get; set; }
    public string? PosterPath { get; set; }
}