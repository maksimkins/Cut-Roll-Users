namespace Cut_Roll_Users.Infrastructure.SpokenLanguages.Services;

using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.SpokenLanguages.Dtos;
using Cut_Roll_Users.Core.SpokenLanguages.Models;
using Cut_Roll_Users.Core.SpokenLanguages.Repositories;
using Cut_Roll_Users.Core.SpokenLanguages.Service;

public class SpokenLanguageService : ISpokenLanguageService
{
    private readonly ISpokenLanguageRepository _spokenLanguageRepository;
    public SpokenLanguageService(ISpokenLanguageRepository spokenLanguageRepository)
    {
        _spokenLanguageRepository = spokenLanguageRepository ?? throw new Exception(nameof(spokenLanguageRepository));
    }

    public async Task<PagedResult<SpokenLanguage>> GetAllSpokenLanguageAsync(SpokenLanguagePaginationDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException($"missing {nameof(dto)}");

        return await _spokenLanguageRepository.GetAllAsync(dto);
    }

    public async Task<SpokenLanguage?> GetSpokenLanguageByIsoCodeAsync(string? isoCode)
    {
        if (string.IsNullOrEmpty(isoCode))
            throw new ArgumentNullException($"missing {nameof(isoCode)}");
        return await _spokenLanguageRepository.GetByIsoCodeAsync(isoCode);
    }

    public async Task<PagedResult<SpokenLanguage>> SearchSpokenLanguageByNameAsync(SpokenLanguageSearchByNameDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException($"missing {nameof(dto)}");
        if (dto.Name == null)
            throw new ArgumentNullException($"missing {nameof(dto.Name)}");

        return await _spokenLanguageRepository.SearchByNameAsync(dto);
    }
}
