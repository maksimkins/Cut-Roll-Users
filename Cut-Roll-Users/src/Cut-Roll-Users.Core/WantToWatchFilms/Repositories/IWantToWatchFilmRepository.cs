using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Common.Repositories.Base;
using Cut_Roll_Users.Core.Movies.Dtos;
using Cut_Roll_Users.Core.WantToWatchFilms.Dtos;

namespace Cut_Roll_Users.Core.WantToWatchFilms.Repositories;

public interface IWantToWatchFilmRepository :
    ICreateAsync<WantToWatchFilmDto, Guid?>,
    IDeleteAsync<WantToWatchFilmDto, Guid?>
{
    Task<bool> IsInWantToWatchAsync(WantToWatchFilmDto dto);
    Task<int> GetWantToWatchCountByUserIdAsync(string userId);
    Task<PagedResult<MovieSimplifiedDto>> GetWantToWatchMoviesAsync(WantToWatchFilmPaginationUserDto dto);
}
