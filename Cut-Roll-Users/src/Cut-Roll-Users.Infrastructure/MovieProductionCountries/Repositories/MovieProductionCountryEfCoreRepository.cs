namespace Cut_Roll_Users.Infrastructure.MovieProductionCountries.Repositories;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Countries.Models;
using Cut_Roll_Users.Core.MovieImages.Enums;
using Cut_Roll_Users.Core.MovieProductionCountries.Dtos;
using Cut_Roll_Users.Core.MovieProductionCountries.Models;
using Cut_Roll_Users.Core.MovieProductionCountries.Repositories;
using Cut_Roll_Users.Core.Movies.Dtos;
using Cut_Roll_Users.Core.Movies.Models;
using Cut_Roll_Users.Infrastructure.Common.Data;
using Microsoft.EntityFrameworkCore;

public class MovieProductionCountryEfCoreRepository : IMovieProductionCountryRepository
{
    private readonly UsersDbContext _dbContext;
    public MovieProductionCountryEfCoreRepository(UsersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> BulkCreateAsync(IEnumerable<MovieProductionCountryDto> listToCreate)
    {
        var newList = listToCreate.Select(toCreate => new MovieProductionCountry
        {
            MovieId = toCreate.MovieId,
            CountryCode = toCreate.CountryCode,
        });
        
        await _dbContext.MovieProductionCountries.AddRangeAsync(newList);
        var res = await _dbContext.SaveChangesAsync();

        return res > 0;
    }

    public async Task<bool> BulkDeleteAsync(IEnumerable<MovieProductionCountryDto> listToDelete)
    {
        foreach (var item in listToDelete)
        {
            var movieProductionCountry = await _dbContext.MovieProductionCountries.FirstOrDefaultAsync(c =>
                c.MovieId == item.MovieId && c.CountryCode == item.CountryCode);

            if (movieProductionCountry != null)
            {
                _dbContext.MovieProductionCountries.Remove(movieProductionCountry);
            }
        }

        var result = await _dbContext.SaveChangesAsync();
        return result > 0;
    }

    public async Task<Guid?> CreateAsync(MovieProductionCountryDto entity)
    {
        var movieProductionCountry = new MovieProductionCountry
        {
            MovieId = entity.MovieId,
            CountryCode = entity.CountryCode,
        };

        await _dbContext.MovieProductionCountries.AddAsync(movieProductionCountry);
        var res = await _dbContext.SaveChangesAsync();

        return res > 0 ? entity.MovieId : null;
    }

    public async Task<Guid?> DeleteAsync(MovieProductionCountryDto dto)
    {
        var toDelete = await _dbContext.MovieProductionCountries.FirstOrDefaultAsync(g => g.MovieId == dto.MovieId && g.CountryCode == dto.CountryCode);
        if (toDelete != null)
            _dbContext.MovieProductionCountries.Remove(toDelete);

        var res = await _dbContext.SaveChangesAsync();
        return res > 0 ? dto.MovieId : null;
    }

    public async Task<bool> DeleteRangeById(Guid movieId)
    {
        var toDeletes = _dbContext.MovieProductionCountries.Where(i => i.MovieId == movieId);
        _dbContext.RemoveRange(toDeletes);

        var res = await _dbContext.SaveChangesAsync();
        return res > 0;
    }

    public async Task<bool> ExistsAsync(MovieProductionCountryDto dto)
    {
        return await _dbContext.MovieProductionCountries.AnyAsync(g => g.MovieId == dto.MovieId && g.CountryCode == dto.CountryCode);
    }

    public async Task<IEnumerable<Country>> GetCountriesByMovieIdAsync(Guid movieId)
    {
        return await _dbContext.MovieProductionCountries.Where(g => g.MovieId == movieId).
            Include(g => g.Country).Select(g => g.Country).ToListAsync();
    }

    public async Task<PagedResult<MovieSimplifiedDto>> GetMoviesByCountryIdAsync(MovieSearchByCountryDto movieSearchByCountryDto)
    {
        var query = _dbContext.Movies
            .Include(m => m.Images)
            .Include(m => m.ProductionCompanies)
            .ThenInclude(mg => mg.Company)
            .AsQueryable();

        if (!string.IsNullOrEmpty(movieSearchByCountryDto.Iso3166_1))
        {
            query = query.Where(m => m.ProductionCountries.Any(mg => mg.CountryCode == movieSearchByCountryDto.Iso3166_1));
        }

        else if (!string.IsNullOrWhiteSpace(movieSearchByCountryDto.Name))
        {
            var name = $"%{movieSearchByCountryDto.Name.Trim()}%";
            query = query.Where(m => m.ProductionCountries.Any(k => EF.Functions.ILike(k.Country.Name, name)));
        }

        if (movieSearchByCountryDto.PageNumber < 1) movieSearchByCountryDto.PageNumber = 1;
        if (movieSearchByCountryDto.PageSize < 1) movieSearchByCountryDto.PageSize = 10;

        var totalCount = await query.CountAsync();

        query = query.
            Skip((movieSearchByCountryDto.PageNumber - 1) * movieSearchByCountryDto.PageSize)
            .Take(movieSearchByCountryDto.PageSize);

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
            Page = movieSearchByCountryDto.PageNumber,
            PageSize = movieSearchByCountryDto.PageSize
        };
    }
}
