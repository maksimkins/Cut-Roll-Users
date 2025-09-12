using Microsoft.Extensions.Logging;
using Cut_Roll_Users.Core.MovieImages.Dtos;
using Cut_Roll_Users.Core.MovieImages.Models;
using Cut_Roll_Users.Core.MovieImages.Repositories;
using Cut_Roll_Users.Core.MovieImages.Service;

namespace Cut_Roll_Users.Infrastructure.MovieImages.Services;

public class MovieImageService : IMovieImageService
{
    private readonly IMovieImageRepository _movieImageRepository;
    private readonly ILogger<MovieImageService> _logger;

    public MovieImageService(
        IMovieImageRepository movieImageRepository,
        ILogger<MovieImageService> logger)
    {
        _movieImageRepository = movieImageRepository ?? throw new ArgumentNullException(nameof(movieImageRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> BulkMovieImageCreateAsync(IEnumerable<MovieImageCreateDto?>? toCreate)
    {
        try
        {
            if (toCreate == null || !toCreate.Any())
            {
                _logger.LogWarning("No movie images to create");
                return false;
            }

            var validImages = toCreate.Where(dto => dto != null).Cast<MovieImageCreateDto>().ToList();
            if (!validImages.Any())
            {
                _logger.LogWarning("No valid movie images to create");
                return false;
            }

            var result = await _movieImageRepository.BulkCreateAsync(validImages);

            _logger.LogInformation("Bulk created movie images: {Success}", result);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk movie image creation");
            return false;
        }
    }

    public async Task<bool> BulkMovieImageDeleteAsync(IEnumerable<MovieImageDeleteDto?>? toDelete)
    {
        try
        {
            if (toDelete == null || !toDelete.Any())
            {
                _logger.LogWarning("No movie images to delete");
                return false;
            }

            var validDeletes = toDelete.Where(dto => dto != null).Cast<MovieImageDeleteDto>().ToList();
            if (!validDeletes.Any())
            {
                _logger.LogWarning("No valid movie images to delete");
                return false;
            }

            var result = await _movieImageRepository.BulkDeleteAsync(validDeletes);

            _logger.LogInformation("Bulk deleted movie images: {Success}", result);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk movie image deletion");
            return false;
        }
    }

    public async Task<Guid> CreateMovieGenreAsync(MovieImageCreateDto? dto)
    {
        try
        {
            if (dto == null)
            {
                _logger.LogWarning("Movie image create DTO is null");
                return Guid.Empty;
            }

            var result = await _movieImageRepository.CreateAsync(dto);
            if (result.HasValue)
            {
                _logger.LogInformation("Created movie image with ID {ImageId}", result.Value);
            }
            else
            {
                _logger.LogWarning("Failed to create movie image");
            }

            return result ?? Guid.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating movie image");
            return Guid.Empty;
        }
    }

    public async Task<Guid> DeleteMovieImageByIdAsync(Guid? id)
    {
        try
        {
            if (id == null || id == Guid.Empty)
            {
                _logger.LogWarning("Invalid movie image ID for deletion");
                return Guid.Empty;
            }

            var result = await _movieImageRepository.DeleteByIdAsync(id.Value);
            if (result.HasValue)
            {
                _logger.LogInformation("Deleted movie image with ID {ImageId}", result.Value);
            }
            else
            {
                _logger.LogWarning("Movie image with ID {ImageId} not found for deletion", id);
            }

            return result ?? Guid.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting movie image with ID {ImageId}", id);
            return Guid.Empty;
        }
    }

    public async Task<bool> DeleteMovieImageRangeByMovieId(Guid? movieId)
    {
        try
        {
            if (movieId == null || movieId == Guid.Empty)
            {
                _logger.LogWarning("Invalid movie ID for image range deletion");
                return false;
            }

            var result = await _movieImageRepository.DeleteRangeById(movieId.Value);
            _logger.LogInformation("Deleted movie images for movie {MovieId}: {Success}", movieId, result);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting movie images for movie {MovieId}", movieId);
            return false;
        }
    }

    public async Task<IEnumerable<MovieImage>> GetMovieImagesByMovieIdAsync(Guid? movieId)
    {
        try
        {
            if (movieId == null || movieId == Guid.Empty)
            {
                _logger.LogWarning("Invalid movie ID for image retrieval");
                return Enumerable.Empty<MovieImage>();
            }

            _logger.LogDebug("Retrieving movie images for movie {MovieId}", movieId);
            return await _movieImageRepository.GetImagesByMovieIdAsync(movieId.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving movie images for movie {MovieId}", movieId);
            return Enumerable.Empty<MovieImage>();
        }
    }

    public async Task<IEnumerable<MovieImage>> GetMovieImagesByTypeAsync(Guid? movieId, string? type)
    {
        try
        {
            if (movieId == null || movieId == Guid.Empty)
            {
                _logger.LogWarning("Invalid movie ID for image retrieval by type");
                return Enumerable.Empty<MovieImage>();
            }

            if (string.IsNullOrEmpty(type))
            {
                _logger.LogWarning("Invalid image type for retrieval");
                return Enumerable.Empty<MovieImage>();
            }

            var searchDto = new MovieImageSearchDto
            {
                MovieId = movieId,
                Type = type
            };

            _logger.LogDebug("Retrieving movie images of type {Type} for movie {MovieId}", type, movieId);
            return await _movieImageRepository.GetImagesByTypeAsync(searchDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving movie images of type {Type} for movie {MovieId}", type, movieId);
            return Enumerable.Empty<MovieImage>();
        }
    }
}
