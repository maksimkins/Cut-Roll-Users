namespace Cut_Roll_Users.Core.Movies.Repositories;

using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Common.Repositories.Base;
using Cut_Roll_Users.Core.Movies.Dtos;
using Cut_Roll_Users.Core.Movies.Models;

public interface IMovieRepository : ISearchAsync<MovieSearchRequest, PagedResult<MovieSimplifiedDto>>, IGetByIdAsync<Guid, Movie?>,
IUpdateAsync<MovieUpdateDto, Guid?>, IDeleteByIdAsync<Guid, Guid?>, ICreateAsync<MovieCreateDto, Guid?>, ICountAsync
{
    Task<int> GetMovieReviewCountAsync(Guid movieId);
    Task<int> GetMovieWatchedCountAsync(Guid movieId);
    Task<int> GetMovieLikeCountAsync(Guid movieId);
    Task<int> GetMovieWantToWatchCountAsync(Guid movieId);
    Task<double> GetMovieAverageRatingAsync(Guid movieId);
    Task<bool> IsMovieWatchedByUserAsync(Guid movieId, string userId);
    Task<bool> IsMovieLikedByUserAsync(Guid movieId, string userId);
    Task<bool> IsMovieInUserWantToWatchAsync(Guid movieId, string userId);
    Task<List<Movie>> GetMoviesWithPaginationAsync(int offset, int limit);
    Task<List<Movie>> GetLikedMoviesByUserIdAsync(string userId);
    Task<List<Movie>> GetWatchedMoviesByUserIdAsync(string userId);
    Task<List<Movie>> GetLikedMoviesByUserIdAsync(string userId, int offset, int limit);
    Task<List<Movie>> GetWatchedMoviesByUserIdAsync(string userId, int offset, int limit);
    Task<int> GetLikedMoviesCountByUserIdAsync(string userId);
    Task<int> GetWatchedMoviesCountByUserIdAsync(string userId);
    Task<List<Movie>> GetMoviesWithoutEmbeddingsAsync(int offset, int limit);
    Task<int> GetMoviesWithoutEmbeddingsCountAsync();
    Task<bool> MarkMovieAsEmbeddedAsync(Guid movieId);
    Task<bool> MarkMovieAsNotEmbeddedAsync(Guid movieId);
    Task<string?> GetMoviePosterPathAsync(Guid movieId);

}