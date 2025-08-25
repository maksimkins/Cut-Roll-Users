namespace Cut_Roll_Users.Core.SpokenLanguages.Service;

using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.SpokenLanguages.Dtos;
using Cut_Roll_Users.Core.SpokenLanguages.Models;

public interface ISpokenLanguageService
{
    Task<PagedResult<SpokenLanguage>> GetAllSpokenLanguageAsync(SpokenLanguagePaginationDto? dto);
    Task<PagedResult<SpokenLanguage>> SearchSpokenLanguageByNameAsync(SpokenLanguageSearchByNameDto? dto);
    Task<SpokenLanguage?> GetSpokenLanguageByIsoCodeAsync(string? isoCode);
}
