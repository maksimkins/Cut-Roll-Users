using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.WatchedMovies.Dtos;
using Cut_Roll_Users.Core.WatchedMovies.Repositories;
using Cut_Roll_Users.Core.WatchedMovies.Services;

namespace Cut_Roll_Users.Infrastructure.WatchedMovies.Services;

public class WatchedMovieService : IWatchedMovieService
{
    private readonly IWatchedMovieRepository _watchedMovieRepository;

    public WatchedMovieService(IWatchedMovieRepository watchedMovieRepository)
    {
        _watchedMovieRepository = watchedMovieRepository;
    }

    public async Task<Guid> MarkMovieAsWatchedAsync(WatchedMovieDto? watchedMovieDto)
    {
        if (watchedMovieDto == null)
            throw new ArgumentNullException(nameof(watchedMovieDto), "Watched movie data cannot be null.");
        
        if (string.IsNullOrWhiteSpace(watchedMovieDto.UserId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(watchedMovieDto.UserId));
        
        if (watchedMovieDto.MovieId == Guid.Empty)
            throw new ArgumentException("Movie ID cannot be empty.", nameof(watchedMovieDto.MovieId));

        
        var isAlreadyWatched = await _watchedMovieRepository.IsMovieWatchedByUserAsync(watchedMovieDto.UserId, watchedMovieDto.MovieId);
        if (isAlreadyWatched)
            return watchedMovieDto.MovieId; 

        var result = await _watchedMovieRepository.CreateAsync(watchedMovieDto);
        return result ?? throw new InvalidOperationException("Failed to mark movie as watched.");
    }

    public async Task<Guid> UnmarkMovieAsWatchedAsync(WatchedMovieDto? watchedMovieDto)
    {
        if (watchedMovieDto == null)
            throw new ArgumentNullException(nameof(watchedMovieDto), "Remove watched movie data cannot be null.");
        
        if (string.IsNullOrWhiteSpace(watchedMovieDto.UserId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(watchedMovieDto.UserId));
        
        if (watchedMovieDto.MovieId == Guid.Empty)
            throw new ArgumentException("Movie ID cannot be empty.", nameof(watchedMovieDto.MovieId));

        
        var isWatched = await _watchedMovieRepository.IsMovieWatchedByUserAsync(watchedMovieDto.UserId, watchedMovieDto.MovieId);
        if (!isWatched)
            return watchedMovieDto.MovieId; 

        var result = await _watchedMovieRepository.DeleteAsync(watchedMovieDto);
        if (result == null)
            throw new InvalidOperationException($"WatchedMovie not found for UserId: {watchedMovieDto.UserId} and MovieId: {watchedMovieDto.MovieId}");
        
        return result.Value;
    }

    public async Task<PagedResult<WatchedMovieResponseDto>> SearchWatchedMoviesAsync(WatchedMovieSearchDto? searchDto)
    {
        if (searchDto == null)
            throw new ArgumentNullException(nameof(searchDto), "Search data cannot be null.");

        return await _watchedMovieRepository.SearchAsync(searchDto);
    }

    public async Task<WatchedMovieResponseDto?> GetWatchedMovieAsync(string? userId, Guid? movieId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
        
        if (!movieId.HasValue || movieId.Value == Guid.Empty)
            throw new ArgumentException("Movie ID cannot be null or empty.", nameof(movieId));

        return await _watchedMovieRepository.GetByUserAndMovieAsync(userId, movieId.Value);
    }

    public async Task<PagedResult<WatchedMovieResponseDto>> GetWatchedMoviesByUserIdAsync(WatchedMoviePaginationUserDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "Pagination data cannot be null.");
        
        if (dto.UserId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(dto.UserId));

        return await _watchedMovieRepository.GetByUserIdAsync(dto);
    }

    public async Task<int> GetWatchedCountByUserIdAsync(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

        return await _watchedMovieRepository.GetWatchedCountByUserIdAsync(userId);
    }

    public async Task<bool> IsMovieWatchedByUserAsync(string? userId, Guid? movieId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
        
        if (!movieId.HasValue || movieId.Value == Guid.Empty)
            throw new ArgumentException("Movie ID cannot be null or empty.", nameof(movieId));

        return await _watchedMovieRepository.IsMovieWatchedByUserAsync(userId, movieId.Value);
    }
}
