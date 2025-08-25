namespace Cut_Roll_Users.Core.MovieOriginCountries.Repository;

using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Common.Repositories.Base;
using Cut_Roll_Users.Core.Countries.Models;
using Cut_Roll_Users.Core.MovieOriginCountries.Dtos;
using Cut_Roll_Users.Core.Movies.Dtos;

public interface IMovieOriginCountryRepository : ICreateAsync<MovieOriginCountryDto, Guid?>, IDeleteAsync<MovieOriginCountryDto, Guid?>,
    IDeleteRangeById<Guid, bool>, IBulkCreateAsync<MovieOriginCountryDto, bool>, IBulkDeleteAsync<MovieOriginCountryDto, bool>
{
    Task<IEnumerable<Country>> GetCountriesByMovieIdAsync(Guid movieId);
    Task<PagedResult<MovieSimplifiedDto>> GetMoviesByOriginCountryIdAsync(MovieSearchByCountryDto movieSearchByCountryDto);
    Task<bool> ExistsAsync(MovieOriginCountryDto dto);
}
