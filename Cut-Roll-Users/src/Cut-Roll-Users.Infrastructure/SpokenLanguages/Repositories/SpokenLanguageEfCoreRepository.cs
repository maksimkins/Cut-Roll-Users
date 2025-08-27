namespace Cut_Roll_Users.Infrastructure.SpokenLanguages.Repositories;

using System.Collections.Generic;
using System.Threading.Tasks;
using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.SpokenLanguages.Dtos;
using Cut_Roll_Users.Core.SpokenLanguages.Models;
using Cut_Roll_Users.Core.SpokenLanguages.Repositories;
using Cut_Roll_Users.Infrastructure.Common.Data;
using Microsoft.EntityFrameworkCore;

public class SpokenLanguageEfCoreRepository : ISpokenLanguageRepository
{
    private readonly UsersDbContext _dbContext;
    public SpokenLanguageEfCoreRepository(UsersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<SpokenLanguage>> GetAllAsync(SpokenLanguagePaginationDto dto)
    {
        var query = _dbContext.SpokenLanguages.AsQueryable();

        var totalCount = await query.CountAsync();

        query = query.
            Skip((dto.PageNumber - 1) * dto.PageSize)
            .Take(dto.PageSize);

        return new PagedResult<SpokenLanguage>()
        {
            Data = await query.ToListAsync(),
            TotalCount = totalCount,
            Page = dto.PageNumber,
            PageSize = dto.PageSize
        };
    }

    public async Task<SpokenLanguage?> GetByIsoCodeAsync(string isoCode)
    {
        return await _dbContext.SpokenLanguages.FirstOrDefaultAsync(l => l.Iso639_1 == isoCode);
    }

    public async Task<PagedResult<SpokenLanguage>> SearchByNameAsync(SpokenLanguageSearchByNameDto dto)
    {
        var query = _dbContext.SpokenLanguages.AsQueryable();

        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            var name = $"%{dto.Name.Trim()}%";
            query = query.Where(l => EF.Functions.ILike(l.EnglishName, name));
        }

        var totalCount = await query.CountAsync();

        query = query.
            Skip((dto.PageNumber - 1) * dto.PageSize)
            .Take(dto.PageSize);

        return new PagedResult<SpokenLanguage>()
        {
            Data = await query.ToListAsync(),
            TotalCount = totalCount,
            Page = dto.PageNumber,
            PageSize = dto.PageSize
        };
    }
}
