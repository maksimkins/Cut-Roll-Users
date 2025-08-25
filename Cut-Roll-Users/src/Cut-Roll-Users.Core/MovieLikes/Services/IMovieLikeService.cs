using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.MovieLikes.Dtos;
using Cut_Roll_Users.Core.Movies.Dtos;

namespace Cut_Roll_Users.Core.MovieLikes.Services;

public interface IMovieLikeService
{
    Task<bool> LikeMovieAsync(MovieLikeCreateDto? likeDto);
    Task<bool> UnlikeMovieAsync(MovieLikeCreateDto? likeDto);
    Task<PagedResult<MovieSimplifiedDto>> GetLikedMovies(MovieLikePaginationUserDto dto);
    Task<bool> IsMovieLikedByUserAsync(string? userId, Guid? movieId);
    Task<int> GetMovieLikeCountAsync(Guid? movieId);
}
