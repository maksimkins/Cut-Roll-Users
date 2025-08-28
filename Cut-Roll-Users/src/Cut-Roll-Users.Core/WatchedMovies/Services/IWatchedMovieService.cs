using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.WatchedMovies.Dtos;

namespace Cut_Roll_Users.Core.WatchedMovies.Services;

public interface IWatchedMovieService
{
    Task<Guid> MarkMovieAsWatchedAsync(WatchedMovieDto? watchedMovieDto);
    Task<Guid> UnmarkMovieAsWatchedAsync(WatchedMovieDto? watchedMovieDto);
    Task<WatchedMovieResponseDto?> GetWatchedMovieAsync(string? userId, Guid? movieId);
    Task<PagedResult<WatchedMovieResponseDto>> GetWatchedMoviesByUserIdAsync(WatchedMoviePaginationUserDto dto);
    Task<int> GetWatchedCountByUserIdAsync(string? userId);
    Task<bool> IsMovieWatchedByUserAsync(string? userId, Guid? movieId);
}
