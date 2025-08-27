using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Countries.Dtos;
using Cut_Roll_Users.Core.Countries.Models;
using Cut_Roll_Users.Core.Countries.Repositories;
using Cut_Roll_Users.Infrastructure.Common.Data;
using Microsoft.EntityFrameworkCore;

namespace Cut_Roll_Users.Infrastructure.Countries.Repositories;

public class CountryEfCoreRepository : ICountryRepository
{
    private readonly UsersDbContext _dbContext;
    public CountryEfCoreRepository(UsersDbContext dbContext)
    {
        _dbContext = dbContext;
    }


    public async Task<PagedResult<Country>> GetAllAsync(ContryPaginationDto dto)
    {
        var query = _dbContext.Countries.AsQueryable();

        var totalCount = await query.CountAsync();

        query = query.
            Skip((dto.PageNumber - 1) * dto.PageSize)
            .Take(dto.PageSize);

        return new PagedResult<Country>()
        {
            Data = await query.ToListAsync(),
            TotalCount = totalCount,
            Page = dto.PageNumber,
            PageSize = dto.PageSize
        };
    }

    public async Task<Country?> GetByIsoCodeAsync(string isoCode)
    {
        return await _dbContext.Countries.FirstOrDefaultAsync(c => c.Iso3166_1 == isoCode);
    }


    public async Task<PagedResult<Country>> SearchByNameAsync(ContrySearchByNameDto dto)
    {
        var query = _dbContext.Countries.AsQueryable();

        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            var name = $"%{dto.Name.Trim()}%";
            query = query.Where(c => EF.Functions.ILike(c.Name, name));
        }
        
        var totalCount = await query.CountAsync();

        query = query.
            Skip((dto.PageNumber - 1) * dto.PageSize)
            .Take(dto.PageSize);

        return new PagedResult<Country>()
        {
            Data = await query.ToListAsync(),
            TotalCount = totalCount,
            Page = dto.PageNumber,
            PageSize = dto.PageSize
        };
    }
}
