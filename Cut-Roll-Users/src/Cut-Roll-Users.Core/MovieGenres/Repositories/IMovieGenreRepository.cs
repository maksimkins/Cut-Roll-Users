namespace Cut_Roll_Users.Core.MovieGenres.Repositories;

using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Common.Repositories.Base;
using Cut_Roll_Users.Core.Genres.Models;
using Cut_Roll_Users.Core.MovieGenres.Dtos;
using Cut_Roll_Users.Core.Movies.Dtos;

public interface IMovieGenreRepository : ICreateAsync<MovieGenreDto, Guid?>, IDeleteAsync<MovieGenreDto, Guid?>, IDeleteRangeById<Guid, bool>,
    IBulkCreateAsync<MovieGenreDto, bool>, IBulkDeleteAsync<MovieGenreDto, bool>
{
    Task<IEnumerable<Genre>> GetGenresByMovieIdAsync(Guid movieId);
    Task<PagedResult<MovieSimplifiedDto>> GetMoviesByGenreIdAsync(MovieSearchByGenreDto searchDto);
    Task<bool> ExistsAsync(MovieGenreDto dto);
}
