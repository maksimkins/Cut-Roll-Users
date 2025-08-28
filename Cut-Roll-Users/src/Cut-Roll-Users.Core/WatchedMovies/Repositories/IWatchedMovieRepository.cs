using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Common.Repositories.Base;
using Cut_Roll_Users.Core.WatchedMovies.Dtos;

namespace Cut_Roll_Users.Core.WatchedMovies.Repositories;

public interface IWatchedMovieRepository :
    ICreateAsync<WatchedMovieDto, Guid?>,
    IDeleteAsync<WatchedMovieDto, Guid?>,
    ISearchAsync<WatchedMovieSearchDto, PagedResult<WatchedMovieResponseDto>>
{
    Task<WatchedMovieResponseDto?> GetByUserAndMovieAsync(string userId, Guid movieId);
    Task<PagedResult<WatchedMovieResponseDto>> GetByUserIdAsync(WatchedMoviePaginationUserDto dto);
    Task<int> GetWatchedCountByUserIdAsync(string userId);
    Task<bool> IsMovieWatchedByUserAsync(string userId, Guid movieId);
}
