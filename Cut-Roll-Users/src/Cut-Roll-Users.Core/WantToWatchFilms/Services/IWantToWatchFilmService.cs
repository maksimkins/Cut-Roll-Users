using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Movies.Dtos;
using Cut_Roll_Users.Core.WantToWatchFilms.Dtos;

namespace Cut_Roll_Users.Core.WantToWatchFilms.Services;

public interface IWantToWatchFilmService
{
    Task<bool> AddToWantToWatchAsync(WantToWatchFilmDto? wantToWatchDto);
    Task<bool> RemoveFromWantToWatchAsync(WantToWatchFilmDto? wantToWatchDto);
    Task<PagedResult<MovieSimplifiedDto>> GetWantToWatchMoviesAsync(WantToWatchFilmPaginationUserDto? paginationDto);
    Task<bool> IsMovieInWantToWatchAsync(string? userId, Guid? movieId);
    Task<int> GetWantToWatchCountByUserIdAsync(string? userId);
}
