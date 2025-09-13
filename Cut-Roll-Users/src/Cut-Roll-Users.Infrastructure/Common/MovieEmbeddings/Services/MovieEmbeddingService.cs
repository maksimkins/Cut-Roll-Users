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

    // Processing state tracking
    private volatile bool _isProcessing = false;
    private DateTime? _lastProcessedAt = null;

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


    public async Task ProcessAllMoviesAsync(int? batchSize = null)
    {
        if (_isProcessing)
        {
            _logger.LogWarning("Movie processing is already in progress");
            return;
        }

        try
        {
            _isProcessing = true;
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

            _lastProcessedAt = DateTime.UtcNow;
            _logger.LogInformation("Completed processing all movies. Total processed: {Processed}, Failed: {Failed}", 
                processedCount, failedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing all movies");
            throw;
        }
        finally
        {
            _isProcessing = false;
        }
    }

    public async Task<(int successCount, int failedCount)> ProcessMoviesBatchAsync(int offset, int limit)
    {
        try
        {
            _logger.LogInformation("Processing movies batch: offset {Offset}, limit {Limit}", offset, limit);

            var movies = await _sqlDataReaderService.ExtractMovieDataBatchAsync(offset, limit);
            _logger.LogInformation("Retrieved {MovieCount} movies for batch processing", movies.Count);
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

            _logger.LogInformation("Completed batch processing. Success: {Success}, Failed: {Failed}", successCount, failedCount);
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
                IsProcessing = _isProcessing,
                LastProcessedAt = _lastProcessedAt,
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
