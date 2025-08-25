namespace Cut_Roll_Users.Core.MovieProductionCountries.Repositories;

using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Common.Repositories.Base;
using Cut_Roll_Users.Core.Countries.Models;
using Cut_Roll_Users.Core.MovieProductionCountries.Dtos;
using Cut_Roll_Users.Core.Movies.Dtos;
using Cut_Roll_Users.Core.Movies.Models;


public interface IMovieProductionCountryRepository : ICreateAsync<MovieProductionCountryDto, Guid?>, IDeleteAsync<MovieProductionCountryDto, Guid?>,
IDeleteRangeById<Guid, bool>, IBulkCreateAsync<MovieProductionCountryDto, bool>, IBulkDeleteAsync<MovieProductionCountryDto, bool>
{
    Task<bool> ExistsAsync(MovieProductionCountryDto dto);
    Task<PagedResult<MovieSimplifiedDto>> GetMoviesByCountryIdAsync(MovieSearchByCountryDto movieSearchByCountryDto);
    Task<IEnumerable<Country>> GetCountriesByMovieIdAsync(Guid movieId);
}
