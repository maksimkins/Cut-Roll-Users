namespace Cut_Roll_Users.Core.SpokenLanguages.Repositories;

using System.Threading.Tasks;
using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.SpokenLanguages.Dtos;
using Cut_Roll_Users.Core.SpokenLanguages.Models;

public interface ISpokenLanguageRepository
{
    Task<PagedResult<SpokenLanguage>> GetAllAsync(SpokenLanguagePaginationDto dto);
    Task<PagedResult<SpokenLanguage>> SearchByNameAsync(SpokenLanguageSearchByNameDto dto);
    Task<SpokenLanguage?> GetByIsoCodeAsync(string isoCode);
}
