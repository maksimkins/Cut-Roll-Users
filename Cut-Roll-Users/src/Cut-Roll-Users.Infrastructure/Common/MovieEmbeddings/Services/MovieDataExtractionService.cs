using Microsoft.Extensions.Logging;
using Cut_Roll_Users.Core.MovieEmbeddings.Services;
using Cut_Roll_Users.Core.MovieEmbeddings.Dtos;
using Cut_Roll_Users.Core.Common.DataProcessing;
using Cut_Roll_Users.Core.Common.DataProcessing.Models;

namespace Cut_Roll_Users.Infrastructure.Common.MovieEmbeddings.Services;

/// <summary>
/// Service for extracting movie data for embedding generation
/// </summary>
public class MovieDataExtractionService : IMovieDataExtractionService
{
    private readonly ISqlDataReaderService _sqlDataReaderService;
    private readonly ILogger<MovieDataExtractionService> _logger;

    public MovieDataExtractionService(
        ISqlDataReaderService sqlDataReaderService,
        ILogger<MovieDataExtractionService> logger)
    {
        _sqlDataReaderService = sqlDataReaderService ?? throw new ArgumentNullException(nameof(sqlDataReaderService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<MovieDataForEmbeddingDto?> ExtractCompleteMovieDataAsync(Guid movieId)
    {
        try
        {
            _logger.LogDebug("Extracting complete movie data for movie {MovieId}", movieId);

            var sqlMovieData = await _sqlDataReaderService.ExtractMovieDataByIdAsync(movieId);
            if (sqlMovieData == null)
            {
                _logger.LogWarning("No data found for movie {MovieId}", movieId);
                return null;
            }

            var movieDataForEmbedding = ConvertSqlDataToEmbeddingData(sqlMovieData);
            
            _logger.LogDebug("Successfully extracted movie data for movie {MovieId}", movieId);
            return movieDataForEmbedding;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting complete movie data for movie {MovieId}", movieId);
            return null;
        }
    }

    public async Task<List<MovieDataForEmbeddingDto>> ExtractMoviesDataBatchAsync(int offset, int limit)
    {
        try
        {
            _logger.LogDebug("Extracting movie data batch {Offset}-{End}", offset, offset + limit - 1);

            var sqlMoviesData = await _sqlDataReaderService.ExtractMovieDataBatchAsync(offset, limit);
            if (!sqlMoviesData.Any())
            {
                _logger.LogDebug("No movies found in batch {Offset}-{End}", offset, offset + limit - 1);
                return new List<MovieDataForEmbeddingDto>();
            }

            var moviesDataForEmbedding = sqlMoviesData
                .Select(ConvertSqlDataToEmbeddingData)
                .Where(data => data != null)
                .Cast<MovieDataForEmbeddingDto>()
                .ToList();

            _logger.LogDebug("Successfully extracted {Count} movies in batch {Offset}-{End}", 
                moviesDataForEmbedding.Count, offset, offset + limit - 1);

            return moviesDataForEmbedding;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting movie data batch {Offset}-{End}", offset, offset + limit - 1);
            return new List<MovieDataForEmbeddingDto>();
        }
    }

    public async Task<int> GetTotalMovieCountAsync()
    {
        try
        {
            _logger.LogDebug("Getting total movie count");

            var count = await _sqlDataReaderService.GetTotalMovieCountAsync();
            
            _logger.LogDebug("Total movie count: {Count}", count);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total movie count");
            return 0;
        }
    }

    private MovieDataForEmbeddingDto? ConvertSqlDataToEmbeddingData(SqlMovieData sqlData)
    {
        try
        {
            return new MovieDataForEmbeddingDto
            {
                Id = sqlData.Id,
                Title = sqlData.Title,
                Overview = sqlData.Overview ?? string.Empty,
                Tagline = sqlData.Tagline,
                OriginalTitle = sqlData.OriginalTitle,
                ReleaseDate = sqlData.ReleaseDate,
                PosterPath = sqlData.PosterPath,
                Budget = sqlData.Budget,
                Revenue = sqlData.Revenue,
                Runtime = sqlData.Runtime,
                Status = sqlData.Status,
                OriginalLanguage = sqlData.OriginalLanguage,
                Genres = sqlData.Genres ?? new List<string>(),
                Keywords = sqlData.Keywords ?? new List<string>(),
                Cast = sqlData.Cast ?? new List<string>(),
                Crew = sqlData.Crew ?? new List<string>(),
                ProductionCompanies = sqlData.ProductionCompanies ?? new List<string>(),
                ProductionCountries = sqlData.ProductionCountries ?? new List<string>(),
                SpokenLanguages = sqlData.SpokenLanguages ?? new List<string>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting SQL data to embedding data for movie {MovieId}", sqlData.Id);
            return null;
        }
    }
}
