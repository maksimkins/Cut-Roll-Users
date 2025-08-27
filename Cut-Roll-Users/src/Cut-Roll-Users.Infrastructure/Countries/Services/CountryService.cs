namespace Cut_Roll_Users.Infrastructure.Countries.Services;

using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Countries.Dtos;
using Cut_Roll_Users.Core.Countries.Models;
using Cut_Roll_Users.Core.Countries.Repositories;
using Cut_Roll_Users.Core.Countries.Services;

public class CountryService : ICountryService
{
    private readonly ICountryRepository _countryRepository;
    public CountryService(ICountryRepository countryRepository)
    {
        _countryRepository = countryRepository ?? throw new Exception($"{nameof(_countryRepository)}");
    }
    public async Task<PagedResult<Country>> GetAllCountriesAsync(ContryPaginationDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException($"missing {nameof(dto)}");

        return await _countryRepository.GetAllAsync(dto);
    }

    public async Task<Country?> GetCountryByIsoCodeAsync(string? isoCode)
    {
        if (isoCode == null)
            throw new ArgumentNullException($"missing {nameof(isoCode)}");

        return await _countryRepository.GetByIsoCodeAsync(isoCode);
    }

    public async Task<PagedResult<Country>> SearchCountryByNameAsync(ContrySearchByNameDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException($"missing {nameof(dto)}");
        if (dto.Name == null)
            throw new ArgumentNullException($"missing {nameof(dto.Name)}");

        return await _countryRepository.SearchByNameAsync(dto);
    }
}
