using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Movies.Dtos;
using Cut_Roll_Users.Core.WantToWatchFilms.Dtos;

namespace Cut_Roll_Users.Core.WantToWatchFilms.Services;

public interface IWantToWatchFilmService
{
    Task<Guid> AddMovieToWantToWatchAsync(WantToWatchFilmDto? wantToWatchFilmDto);
    Task<Guid> RemoveMovieFromWantToWatchAsync(WantToWatchFilmDto? wantToWatchFilmDto);
    Task<PagedResult<MovieSimplifiedDto>> GetWantToWatchFilmsByUserIdAsync(WantToWatchFilmPaginationUserDto? dto);
    Task<int> GetWantToWatchCountByUserIdAsync(string? userId);
    Task<bool> IsMovieInWantToWatchByUserAsync(string? userId, Guid? movieId);
}

