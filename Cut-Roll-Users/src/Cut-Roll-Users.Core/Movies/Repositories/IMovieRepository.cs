namespace Cut_Roll_Users.Core.Movies.Repositories;

using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Common.Repositories.Base;
using Cut_Roll_Users.Core.Movies.Dtos;
using Cut_Roll_Users.Core.Movies.Models;


public interface IMovieRepository : ISearchAsync<MovieSearchRequest, PagedResult<MovieSimplifiedDto>>, IGetByIdAsync<Guid, Movie?>,
IUpdateAsync<MovieUpdateDto, Guid?>, IDeleteByIdAsync<Guid, Guid?>, ICreateAsync<MovieCreateDto, Guid?>, ICountAsync
{
    // Movie activity methods
    Task<int> GetMovieReviewCountAsync(Guid movieId);
    Task<int> GetMovieWatchedCountAsync(Guid movieId);
    Task<int> GetMovieLikeCountAsync(Guid movieId);
    Task<int> GetMovieWantToWatchCountAsync(Guid movieId);
    Task<double> GetMovieAverageRatingAsync(Guid movieId);
    Task<bool> IsMovieWatchedByUserAsync(Guid movieId, string userId);
    Task<bool> IsMovieLikedByUserAsync(Guid movieId, string userId);
    Task<bool> IsMovieInUserWantToWatchAsync(Guid movieId, string userId);
}