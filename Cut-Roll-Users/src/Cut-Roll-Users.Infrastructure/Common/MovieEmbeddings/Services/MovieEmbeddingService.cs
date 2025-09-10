using Microsoft.Extensions.Logging;
using Cut_Roll_Users.Core.MovieEmbeddings.Services;
using Cut_Roll_Users.Core.MovieEmbeddings.Dtos;
using Cut_Roll_Users.Core.Common.DataProcessing;
using Cut_Roll_Users.Core.Common.DataProcessing.Models;
using Cut_Roll_Users.Core.Movies.Service;

namespace Cut_Roll_Users.Infrastructure.Common.MovieEmbeddings.Services;

/// <summary>
/// Service for managing movie embeddings and recommendations
/// </summary>
public class MovieEmbeddingService : IMovieEmbeddingService
{
    private readonly ITextEmbeddingService _textEmbeddingService;
    private readonly IVectorMovieDatabaseService _vectorDatabaseService;
    private readonly ISqlDataReaderService _sqlDataReaderService;
    private readonly IMovieService _movieService;
    private readonly ILogger<MovieEmbeddingService> _logger;

    public MovieEmbeddingService(
        ITextEmbeddingService textEmbeddingService,
        IVectorMovieDatabaseService vectorDatabaseService,
        ISqlDataReaderService sqlDataReaderService,
        IMovieService movieService,
        ILogger<MovieEmbeddingService> logger)
    {
        _textEmbeddingService = textEmbeddingService ?? throw new ArgumentNullException(nameof(textEmbeddingService));
        _vectorDatabaseService = vectorDatabaseService ?? throw new ArgumentNullException(nameof(vectorDatabaseService));
        _sqlDataReaderService = sqlDataReaderService ?? throw new ArgumentNullException(nameof(sqlDataReaderService));
        _movieService = movieService ?? throw new ArgumentNullException(nameof(movieService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> GenerateAndStoreMovieEmbeddingAsync(Guid movieId)
    {
        try
        {
            _logger.LogInformation("Starting embedding generation for movie {MovieId}", movieId);

            // Check if movie exists and get its data
            var movieData = await _sqlDataReaderService.ExtractMovieDataByIdAsync(movieId);
            if (movieData == null)
            {
                _logger.LogWarning("Movie with ID {MovieId} not found", movieId);
                return false;
            }

            // Convert SqlMovieData to MovieDataForEmbeddingDto
            var movieDataForEmbedding = ConvertToMovieDataForEmbedding(movieData);

            // Generate embedding
            var embedding = await _textEmbeddingService.GenerateMovieEmbeddingAsync(movieDataForEmbedding);
            if (embedding == null || !embedding.Any())
            {
                _logger.LogError("Failed to generate embedding for movie {MovieId}", movieId);
                return false;
            }

            // Create embedding DTO
            var movieEmbedding = new MovieEmbeddingDto
            {
                MovieId = movieId,
                Title = movieData.Title,
                PosterPath = movieData.PosterPath,
                Embedding = embedding,
                Metadata = new Dictionary<string, object>
                {
                    { "dimension", embedding.Count },
                    { "created_at", DateTime.UtcNow },
                    { "model_version", "1.0" }
                }
            };

            // Store in vector database
            var success = await _vectorDatabaseService.UpsertMovieEmbeddingAsync(movieEmbedding);
            if (!success)
            {
                _logger.LogError("Failed to store embedding in vector database for movie {MovieId}", movieId);
                return false;
            }

            // Mark movie as having embedding
            var marked = await _movieService.MarkMovieAsEmbeddedAsync(movieId);
            if (!marked)
            {
                _logger.LogWarning("Failed to mark movie {MovieId} as embedded", movieId);
            }

            _logger.LogInformation("Successfully generated and stored embedding for movie {MovieId}", movieId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating and storing embedding for movie {MovieId}", movieId);
            return false;
        }
    }

    public async Task<bool> UpdateMovieEmbeddingAsync(Guid movieId)
    {
        try
        {
            _logger.LogInformation("Updating embedding for movie {MovieId}", movieId);

            // First, delete the existing embedding
            await _vectorDatabaseService.DeleteMovieEmbeddingAsync(movieId);

            // Generate and store new embedding
            var success = await GenerateAndStoreMovieEmbeddingAsync(movieId);
            
            if (success)
            {
                _logger.LogInformation("Successfully updated embedding for movie {MovieId}", movieId);
            }
            else
            {
                _logger.LogError("Failed to update embedding for movie {MovieId}", movieId);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating embedding for movie {MovieId}", movieId);
            return false;
        }
    }

    public async Task<List<MovieRecommendationDto>> GetSimilarMoviesAsync(Guid movieId, int limit = 10)
    {
        try
        {
            _logger.LogDebug("Getting similar movies for movie {MovieId} with limit {Limit}", movieId, limit);

            // Get movie data to generate query embedding
            var movieData = await _sqlDataReaderService.ExtractMovieDataByIdAsync(movieId);
            if (movieData == null)
            {
                _logger.LogWarning("Movie with ID {MovieId} not found", movieId);
                return new List<MovieRecommendationDto>();
            }

            // Convert SqlMovieData to MovieDataForEmbeddingDto
            var movieDataForEmbedding = ConvertToMovieDataForEmbedding(movieData);

            // Generate embedding for the query movie
            var queryEmbedding = await _textEmbeddingService.GenerateMovieEmbeddingAsync(movieDataForEmbedding);
            if (queryEmbedding == null || !queryEmbedding.Any())
            {
                _logger.LogError("Failed to generate query embedding for movie {MovieId}", movieId);
                return new List<MovieRecommendationDto>();
            }

            // Find similar movies (exclude the query movie itself)
            var excludeMovieIds = new List<Guid> { movieId };
            var recommendations = await _vectorDatabaseService.FindSimilarMoviesAsync(queryEmbedding, limit, excludeMovieIds);

            _logger.LogDebug("Found {Count} similar movies for movie {MovieId}", recommendations.Count, movieId);
            return recommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting similar movies for movie {MovieId}", movieId);
            return new List<MovieRecommendationDto>();
        }
    }

    public Task<List<MovieRecommendationDto>> GetContentBasedRecommendationsAsync(string userId, int limit = 10)
    {
        try
        {
            _logger.LogDebug("Getting content-based recommendations for user {UserId} with limit {Limit}", userId, limit);

            // TODO: Implement user preference-based recommendations
            // For now, return empty list as this requires user preference data
            // This could be enhanced to:
            // 1. Get user's watched movies
            // 2. Get user's liked movies
            // 3. Generate average embedding from user preferences
            // 4. Find similar movies based on user's taste

            _logger.LogWarning("Content-based recommendations not yet implemented for user {UserId}", userId);
            return Task.FromResult(new List<MovieRecommendationDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting content-based recommendations for user {UserId}", userId);
            return Task.FromResult(new List<MovieRecommendationDto>());
        }
    }

    public async Task ProcessAllMoviesAsync(int? batchSize = null)
    {
        try
        {
            var batch = batchSize ?? 32;
            _logger.LogInformation("Starting to process all movies with batch size {BatchSize}", batch);

            var totalMovies = await GetTotalMovieCountAsync();
            var processedCount = 0;
            var failedCount = 0;

            for (int offset = 0; offset < totalMovies; offset += batch)
            {
                var (successCount, failed) = await ProcessMoviesBatchAsync(offset, batch);
                processedCount += successCount;
                failedCount += failed;

                _logger.LogInformation("Processed batch {Offset}-{End} of {TotalMovies}. Success: {Success}, Failed: {Failed}", 
                    offset, Math.Min(offset + batch, totalMovies), totalMovies, successCount, failed);

                // Small delay between batches to avoid overwhelming the system
                await Task.Delay(100);
            }

            _logger.LogInformation("Completed processing all movies. Total processed: {Processed}, Failed: {Failed}", 
                processedCount, failedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing all movies");
            throw;
        }
    }

    public async Task<(int successCount, int failedCount)> ProcessMoviesBatchAsync(int offset, int limit)
    {
        try
        {
            _logger.LogDebug("Processing movies batch: offset {Offset}, limit {Limit}", offset, limit);

            var movies = await _sqlDataReaderService.ExtractMovieDataBatchAsync(offset, limit);
            var successCount = 0;
            var failedCount = 0;

            foreach (var movie in movies)
            {
                try
                {
                    var success = await GenerateAndStoreMovieEmbeddingAsync(movie.Id);
                    if (success)
                    {
                        successCount++;
                    }
                    else
                    {
                        failedCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process movie {MovieId}: {Title}", movie.Id, movie.Title);
                    failedCount++;
                }
            }

            _logger.LogDebug("Completed batch processing. Success: {Success}, Failed: {Failed}", successCount, failedCount);
            return (successCount, failedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing movies batch");
            throw;
        }
    }

    public async Task<EmbeddingStatusDto> GetEmbeddingStatusAsync()
    {
        try
        {
            var totalMovies = await GetTotalMovieCountAsync();
            var totalEmbeddings = await GetProcessedMovieCountAsync();
            var isVectorDbEmpty = await _vectorDatabaseService.IsVectorDbEmptyAsync();
            var isHealthy = await _vectorDatabaseService.CheckVectorDbHealthAsync();

            var status = new EmbeddingStatusDto
            {
                IsVectorDbEmpty = isVectorDbEmpty,
                TotalMoviesInDatabase = totalMovies,
                TotalEmbeddingsInVectorDb = totalEmbeddings,
                IsProcessing = false, // TODO: Track processing state
                LastProcessedAt = null, // TODO: Track last processing time
                Status = isHealthy ? "Healthy" : "Unhealthy"
            };

            _logger.LogDebug("Embedding status: {Status}", status.Status);
            return status;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting embedding status");
            return new EmbeddingStatusDto
            {
                IsVectorDbEmpty = true,
                TotalMoviesInDatabase = 0,
                TotalEmbeddingsInVectorDb = 0,
                IsProcessing = false,
                LastProcessedAt = null,
                Status = "Error"
            };
        }
    }

    public async Task<int> GetTotalMovieCountAsync()
    {
        try
        {
            return await _sqlDataReaderService.GetTotalMovieCountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total movie count");
            return 0;
        }
    }

    public async Task<int> GetProcessedMovieCountAsync()
    {
        try
        {
            return await _vectorDatabaseService.GetEmbeddedMoviesCountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting processed movie count");
            return 0;
        }
    }

    /// <summary>
    /// Converts SqlMovieData to MovieDataForEmbeddingDto
    /// </summary>
    private static MovieDataForEmbeddingDto ConvertToMovieDataForEmbedding(SqlMovieData sqlData)
    {
        return new MovieDataForEmbeddingDto
        {
            Id = sqlData.Id,
            Title = sqlData.Title,
            Overview = sqlData.Overview,
            Tagline = sqlData.Tagline,
            OriginalTitle = sqlData.OriginalTitle,
            ReleaseDate = sqlData.ReleaseDate,
            PosterPath = sqlData.PosterPath,
            Budget = sqlData.Budget,
            Revenue = sqlData.Revenue,
            Runtime = sqlData.Runtime,
            Status = sqlData.Status,
            OriginalLanguage = sqlData.OriginalLanguage,
            Genres = sqlData.Genres,
            Keywords = sqlData.Keywords,
            Cast = sqlData.Cast,
            Crew = sqlData.Crew,
            ProductionCompanies = sqlData.ProductionCompanies,
            ProductionCountries = sqlData.ProductionCountries,
            SpokenLanguages = sqlData.SpokenLanguages
        };
    }
}
