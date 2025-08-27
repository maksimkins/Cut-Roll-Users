using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Movies.Dtos;
using Cut_Roll_Users.Core.WantToWatchFilms.Dtos;
using Cut_Roll_Users.Core.WantToWatchFilms.Repositories;
using Cut_Roll_Users.Core.WantToWatchFilms.Services;

namespace Cut_Roll_Users.Infrastructure.WantToWatchFilms.Services;

public class WantToWatchFilmService : IWantToWatchFilmService
{
    private readonly IWantToWatchFilmRepository _wantToWatchFilmRepository;

    public WantToWatchFilmService(IWantToWatchFilmRepository wantToWatchFilmRepository)
    {
        _wantToWatchFilmRepository = wantToWatchFilmRepository;
    }

    public async Task<Guid> AddToWantToWatchAsync(WantToWatchFilmDto? wantToWatchDto)
    {
        if (wantToWatchDto == null)
            throw new ArgumentNullException(nameof(wantToWatchDto), "Want to watch data cannot be null.");
        
        if (string.IsNullOrWhiteSpace(wantToWatchDto.UserId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(wantToWatchDto.UserId));
        
        if (wantToWatchDto.MovieId == Guid.Empty)
            throw new ArgumentException("Movie ID cannot be empty.", nameof(wantToWatchDto.MovieId));

        // Check if already in want to watch
        var isAlreadyInWantToWatch = await _wantToWatchFilmRepository.IsInWantToWatchAsync(wantToWatchDto);
        if (isAlreadyInWantToWatch)
            return wantToWatchDto.MovieId; // Already in want to watch, return success

        var result = await _wantToWatchFilmRepository.CreateAsync(wantToWatchDto);
        return result ?? throw new InvalidOperationException("Failed to add movie to want to watch.");
    }

    public async Task<Guid> RemoveFromWantToWatchAsync(WantToWatchFilmDto? wantToWatchDto)
    {
        if (wantToWatchDto == null)
            throw new ArgumentNullException(nameof(wantToWatchDto), "Remove from want to watch data cannot be null.");
        
        if (string.IsNullOrWhiteSpace(wantToWatchDto.UserId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(wantToWatchDto.UserId));
        
        if (wantToWatchDto.MovieId == Guid.Empty)
            throw new ArgumentException("Movie ID cannot be empty.", nameof(wantToWatchDto.MovieId));

        // Check if not in want to watch
        var isInWantToWatch = await _wantToWatchFilmRepository.IsInWantToWatchAsync(wantToWatchDto);
        if (!isInWantToWatch)
            return wantToWatchDto.MovieId; // Not in want to watch, return success

        var result = await _wantToWatchFilmRepository.DeleteAsync(wantToWatchDto);
        if (result == null)
            throw new InvalidOperationException($"WantToWatchFilm not found for UserId: {wantToWatchDto.UserId} and MovieId: {wantToWatchDto.MovieId}");
        
        return result.Value;
    }

    public async Task<PagedResult<MovieSimplifiedDto>> GetWantToWatchMoviesAsync(WantToWatchFilmPaginationUserDto? paginationDto)
    {
        if (paginationDto == null)
            throw new ArgumentNullException(nameof(paginationDto), "Pagination data cannot be null.");
        
        if (string.IsNullOrWhiteSpace(paginationDto.UserId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(paginationDto.UserId));

        return await _wantToWatchFilmRepository.GetWantToWatchMoviesAsync(paginationDto);
    }

    public async Task<bool> IsMovieInWantToWatchAsync(string? userId, Guid? movieId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
        
        if (!movieId.HasValue || movieId.Value == Guid.Empty)
            throw new ArgumentException("Movie ID cannot be null or empty.", nameof(movieId));

        var dto = new WantToWatchFilmDto
        {
            UserId = userId,
            MovieId = movieId.Value
        };

        return await _wantToWatchFilmRepository.IsInWantToWatchAsync(dto);
    }

    public async Task<int> GetWantToWatchCountByUserIdAsync(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

        return await _wantToWatchFilmRepository.GetWantToWatchCountByUserIdAsync(userId);
    }
}
