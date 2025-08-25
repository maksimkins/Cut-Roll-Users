namespace Cut_Roll_Users.Core.MovieProductionCountries.Service;

using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Countries.Models;
using Cut_Roll_Users.Core.MovieProductionCountries.Dtos;
using Cut_Roll_Users.Core.Movies.Dtos;

public interface IMovieProductionCountryService
{
    Task<Guid> CreateMovieProductionCountryAsync(MovieProductionCountryDto? dto);
    Task<Guid> DeleteMovieProductionCountryAsyncMovieProductionCountryAsync(MovieProductionCountryDto? dto);
    Task<bool> DeleteMovieProductionCountryRangeByMovieId(Guid? movieId);
    Task<PagedResult<MovieSimplifiedDto>> GetMoviesByCountryIdAsync(MovieSearchByCountryDto? movieSearchByCountryDto);
    Task<IEnumerable<Country>> GetCountriesByMovieIdAsync(Guid? movieId);
    Task<bool> BulkCreateMovieProductionCountryAsync(IEnumerable<MovieProductionCountryDto>? toCreate);
    Task<bool> BulkDeleteMovieProductionCountryAsync(IEnumerable<MovieProductionCountryDto>? toDelete);
}


