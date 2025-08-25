namespace Cut_Roll_Users.Core.Countries.Services;

using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Countries.Dtos;
using Cut_Roll_Users.Core.Countries.Models;

public interface ICountryService
{
    Task<PagedResult<Country>> GetAllCountriesAsync(ContryPaginationDto? dto);
    Task<PagedResult<Country>> SearchCountryByNameAsync(ContrySearchByNameDto? dto);
    Task<Country?> GetCountryByIsoCodeAsync(string? isoCode);
}
