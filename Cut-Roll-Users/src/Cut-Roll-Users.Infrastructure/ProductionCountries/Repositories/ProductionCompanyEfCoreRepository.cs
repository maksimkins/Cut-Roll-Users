namespace Cut_Roll_Users.Infrastructure.ProductionCountries.Repositories;

using System;
using System.Threading.Tasks;
using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.ProductionCompanies.Dtos;
using Cut_Roll_Users.Core.ProductionCompanies.Models;
using Cut_Roll_Users.Core.ProductionCompanies.Repositores;
using Cut_Roll_Users.Infrastructure.Common.Data;
using Microsoft.EntityFrameworkCore;

public class ProductionCompanyEfCoreRepository : IProductionCompanyRepository
{
    private readonly UsersDbContext _dbContext;
    public ProductionCompanyEfCoreRepository(UsersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid?> CreateAsync(ProductionCompanyCreateDto dto)
    {
        var toCreate = new ProductionCompany
        {
            Name = dto.Name,
            CountryCode = dto.CountryCode,
            LogoPath = dto.LogoPath

        };

        await _dbContext.ProductionCompanies.AddAsync(toCreate);
        var res = await _dbContext.SaveChangesAsync();

        return res > 0 ? toCreate.Id : null;
    }

    public async Task<Guid?> DeleteByIdAsync(Guid id)
    {
        var toDelete = await _dbContext.ProductionCompanies.FirstOrDefaultAsync(i => i.Id == id);

        if (toDelete != null)
            _dbContext.ProductionCompanies.Remove(toDelete);

        var res = await _dbContext.SaveChangesAsync();
        return res > 0 ? id : null;
    }

    public async Task<ProductionCompany?> GetByIdAsync(Guid id)
    {
        return await _dbContext.ProductionCompanies.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<PagedResult<ProductionCompany>> SearchAsync(ProductionCompanySearchRequest request)
    {
        var query = _dbContext.ProductionCompanies.AsQueryable();

        if (!string.IsNullOrEmpty(request.CountryCode))
        {
            query = query.Where(m => m.CountryCode == request.CountryCode);
        }
        else if (!string.IsNullOrWhiteSpace(request.Name))
        {
        var name = $"%{request.Name.Trim()}%";
        query = query.Where(m => EF.Functions.ILike(m.Name, name));
        }

        var totalCount = await query.CountAsync();
        query = query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize);

        return new PagedResult<ProductionCompany>()
        {
            Data = await query.ToListAsync(),
            TotalCount = totalCount,
            Page = request.PageNumber,
            PageSize =  request.PageSize,
        };
    }

    public async Task<Guid?> UpdateAsync(ProductionCompanyUpdateDto entity)
    {
        var toUpdate = await _dbContext.ProductionCompanies.FirstOrDefaultAsync(v => v.Id == entity.Id);

        if (toUpdate == null)
            throw new ArgumentException(message: $"caanot find company with id: {entity.Id}");

        toUpdate.Name = entity.Name ?? toUpdate.Name;
        toUpdate.CountryCode = entity.CountryCode ?? toUpdate.CountryCode;
        toUpdate.LogoPath = entity.LogoPath ?? toUpdate.LogoPath;

        _dbContext.ProductionCompanies.Update(toUpdate);
        var res = await _dbContext.SaveChangesAsync();

        return res > 0 ? toUpdate.Id : null;
    }
}
