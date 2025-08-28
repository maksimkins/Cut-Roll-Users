using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Common.Repositories.Base;
using Cut_Roll_Users.Core.MovieLikes.Dtos;
using Cut_Roll_Users.Core.Movies.Dtos;

namespace Cut_Roll_Users.Core.MovieLikes.Repositories;

public interface IMovieLikeRepository :
    ICreateAsync<MovieLikeDto, Guid?>,
    IDeleteAsync<MovieLikeDto, Guid?>
{
    Task<bool> IsLikedByUserAsync(string userId, Guid movieId);
    Task<int> GetLikeCountByMovieIdAsync(Guid movieId);
    Task<PagedResult<MovieSimplifiedDto>> GetByUserIdAsync(MovieLikePaginationUserDto dto);
}
