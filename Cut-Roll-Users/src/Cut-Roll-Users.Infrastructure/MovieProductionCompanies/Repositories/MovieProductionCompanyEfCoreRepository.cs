namespace Cut_Roll_Users.Infrastructure.MovieProductionCompanies.Repositories;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.MovieImages.Enums;
using Cut_Roll_Users.Core.MovieProductionCompanies.Dtos;
using Cut_Roll_Users.Core.MovieProductionCompanies.Models;
using Cut_Roll_Users.Core.MovieProductionCompanies.Repositories;
using Cut_Roll_Users.Core.Movies.Dtos;
using Cut_Roll_Users.Core.Movies.Models;
using Cut_Roll_Users.Core.ProductionCompanies.Models;
using Cut_Roll_Users.Infrastructure.Common.Data;
using Microsoft.EntityFrameworkCore;

public class MovieProductionCompanyEfCoreRepository : IMovieProductionCompanyRepository
{
    private readonly UsersDbContext _dbContext;
    public MovieProductionCompanyEfCoreRepository(UsersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> BulkCreateAsync(IEnumerable<MovieProductionCompanyDto> listToCreate)
    {
        var newList = listToCreate.Select(toCreate => new MovieProductionCompany
        {
            MovieId = toCreate.MovieId,
            CompanyId = toCreate.CompanyId,
        });
        
        await _dbContext.MovieProductionCompanies.AddRangeAsync(newList);
        var res = await _dbContext.SaveChangesAsync();

        return res > 0;
    }

    public async Task<bool> BulkDeleteAsync(IEnumerable<MovieProductionCompanyDto> listToDelete)
    {
        foreach (var item in listToDelete)
        {
            var movieProdComp = await _dbContext.MovieProductionCompanies.FirstOrDefaultAsync(c =>
                c.MovieId == item.MovieId && c.CompanyId == item.CompanyId);

            if (movieProdComp != null)
            {
                _dbContext.MovieProductionCompanies.Remove(movieProdComp);
            }
        }

        var result = await _dbContext.SaveChangesAsync();
        return result > 0;
    }

    public async Task<Guid?> CreateAsync(MovieProductionCompanyDto entity)
    {
        var movieProdComp = new MovieProductionCompany
        {
            MovieId = entity.MovieId,
            CompanyId = entity.CompanyId,
        };

        await _dbContext.MovieProductionCompanies.AddAsync(movieProdComp);
        var res = await _dbContext.SaveChangesAsync();

        return res > 0 ? entity.MovieId : null;
    }

    public async Task<Guid?> DeleteAsync(MovieProductionCompanyDto dto)
    {
        var toDelete = await _dbContext.MovieProductionCompanies.FirstOrDefaultAsync(g => g.MovieId == dto.MovieId && g.CompanyId == dto.CompanyId);
        if (toDelete != null)
            _dbContext.MovieProductionCompanies.Remove(toDelete);

        var res = await _dbContext.SaveChangesAsync();
        return res > 0 ? dto.MovieId : null;
    }

    public async Task<bool> DeleteRangeById(Guid movieId)
    {
        var toDeletes = _dbContext.MovieProductionCompanies.Where(i => i.MovieId == movieId);
        _dbContext.RemoveRange(toDeletes);

        var res = await _dbContext.SaveChangesAsync();
        return res > 0;
    }

    public async Task<bool> ExistsAsync(MovieProductionCompanyDto dto)
    {
        return await _dbContext.MovieProductionCompanies.AnyAsync(g => g.MovieId == dto.MovieId && g.CompanyId == dto.CompanyId);
    }

    public async Task<IEnumerable<ProductionCompany>> GetCompaniesByMovieIdAsync(Guid movieId)
    {
        return await _dbContext.MovieProductionCompanies.Where(g => g.MovieId == movieId).
            Include(g => g.Company).Select(g => g.Company).ToListAsync();
    }

    public async Task<PagedResult<MovieSimplifiedDto>> GetMoviesByCompanyIdAsync(MovieSearchByCompanyDto movieSearchByCompanyDto)
    {
        var query = _dbContext.Movies
            .Include(m => m.Images)
            .Include(m => m.ProductionCompanies)
            .ThenInclude(mg => mg.Company)
            .AsQueryable();

        if (movieSearchByCompanyDto.CompanyId != null)
        {
            query = query.Where(m => m.ProductionCompanies.Any(mg => mg.CompanyId == movieSearchByCompanyDto.CompanyId));
        }

        else if (!string.IsNullOrWhiteSpace(movieSearchByCompanyDto.Name))
        {
            var name = $"%{movieSearchByCompanyDto.Name.Trim()}%";
            query = query.Where(m => m.ProductionCompanies.Any(k => EF.Functions.ILike(k.Company.Name, name)));
        }


        if (movieSearchByCompanyDto.Page < 1) movieSearchByCompanyDto.Page = 1;
        if (movieSearchByCompanyDto.PageSize < 1) movieSearchByCompanyDto.PageSize = 10;

        var totalCount = await query.CountAsync();

        query = query.
            Skip((movieSearchByCompanyDto.Page - 1) * movieSearchByCompanyDto.PageSize)
            .Take(movieSearchByCompanyDto.PageSize);

        var result = await query.ToListAsync();

        return new PagedResult<MovieSimplifiedDto>()
        {
            Data = result.Select(m => new MovieSimplifiedDto
            {
                MovieId = m.Id,
                Title = m.Title,
                Poster = m.Images?.FirstOrDefault(i => i.Type == ImageTypes.poster.ToString()),
            }).ToList(),
            TotalCount = totalCount,
            Page = movieSearchByCompanyDto.Page,
            PageSize = movieSearchByCompanyDto.PageSize
        };
    }
}
