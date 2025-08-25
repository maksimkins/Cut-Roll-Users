namespace Cut_Roll_Users.Core.Movies.Service;

using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Movies.Dtos;
using Cut_Roll_Users.Core.Movies.Models;

public interface IMovieService
{
    Task<PagedResult<MovieSimplifiedDto>> SearchMovieAsync(MovieSearchRequest? movieSearchRequest);
    Task<Movie?> GetMovieByIdAsync(Guid? id);
    Task<Guid> UpdateMovieAsync(MovieUpdateDto? dto);
    Task<Guid> DeleteMovieByIdAsync(Guid? id);
    Task<Guid> CreateMovieAsync(MovieCreateDto? dto);
    Task<int> CountMoviesAsync();
    
    // Movie activity methods
    Task<int> GetMovieReviewCountAsync(Guid? movieId);
    Task<int> GetMovieWatchedCountAsync(Guid? movieId);
    Task<int> GetMovieLikeCountAsync(Guid? movieId);
    Task<double> GetMovieAverageRatingAsync(Guid? movieId);
    Task<bool> IsMovieWatchedByUserAsync(Guid? movieId, string? userId);
    Task<bool> IsMovieLikedByUserAsync(Guid? movieId, string? userId);
}

