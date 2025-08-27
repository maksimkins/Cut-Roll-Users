namespace Cut_Roll_Users.Infrastructure.MovieSpokenLanguages.Repositories;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.MovieImages.Enums;
using Cut_Roll_Users.Core.Movies.Dtos;
using Cut_Roll_Users.Core.Movies.Models;
using Cut_Roll_Users.Core.MovieSpokenLanguages.Dtos;
using Cut_Roll_Users.Core.MovieSpokenLanguages.Models;
using Cut_Roll_Users.Core.MovieSpokenLanguages.Repositories;
using Cut_Roll_Users.Core.SpokenLanguages.Models;
using Cut_Roll_Users.Infrastructure.Common.Data;
using Microsoft.EntityFrameworkCore;

public class MovieSpokenLanguageEfCoreRepository : IMovieSpokenLanguageRepository
{
    private readonly UsersDbContext _dbContext;
    public MovieSpokenLanguageEfCoreRepository(UsersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> BulkCreateAsync(IEnumerable<MovieSpokenLanguageDto> listToCreate)
    {
        var newList = listToCreate.Select(toCreate => new MovieSpokenLanguage
        {
            MovieId = toCreate.MovieId,
            LanguageCode = toCreate.LanguageCode,
        });
        
        await _dbContext.MovieSpokenLanguages.AddRangeAsync(newList);
        var res = await _dbContext.SaveChangesAsync();

        return res > 0;
    }

    public async Task<bool> BulkDeleteAsync(IEnumerable<MovieSpokenLanguageDto> listToDelete)
    {
        foreach (var item in listToDelete)
        {
            var movieSpokenLanguage = await _dbContext.MovieSpokenLanguages.FirstOrDefaultAsync(c =>
                c.MovieId == item.MovieId && c.LanguageCode == item.LanguageCode);

            if (movieSpokenLanguage != null)
            {
                _dbContext.MovieSpokenLanguages.Remove(movieSpokenLanguage);
            }
        }

        var result = await _dbContext.SaveChangesAsync();
        return result > 0;
    }

    public async Task<Guid?> CreateAsync(MovieSpokenLanguageDto entity)
    {
        var movieSpokenLanguage = new MovieSpokenLanguage
        {
            MovieId = entity.MovieId,
            LanguageCode = entity.LanguageCode,
        };

        await _dbContext.MovieSpokenLanguages.AddAsync(movieSpokenLanguage);
        var res = await _dbContext.SaveChangesAsync();

        return res > 0 ? entity.MovieId : null;
    }

    public async Task<Guid?> DeleteAsync(MovieSpokenLanguageDto dto)
    {
        var toDelete = await _dbContext.MovieSpokenLanguages.FirstOrDefaultAsync(g => g.MovieId == dto.MovieId && g.LanguageCode == dto.LanguageCode);
        if (toDelete != null)
            _dbContext.MovieSpokenLanguages.Remove(toDelete);

        var res = await _dbContext.SaveChangesAsync();
        return res > 0 ? dto.MovieId : null;
    }

    public async Task<bool> DeleteRangeById(Guid movieId)
    {
        var toDeletes = _dbContext.MovieSpokenLanguages.Where(i => i.MovieId == movieId);
        _dbContext.RemoveRange(toDeletes);

        var res = await _dbContext.SaveChangesAsync();
        return res > 0;
    }

    public async Task<bool> ExistsAsync(MovieSpokenLanguageDto dto)
    {
        return await _dbContext.MovieSpokenLanguages.AnyAsync(g => g.MovieId == dto.MovieId && g.LanguageCode == dto.LanguageCode);
    }

    public async Task<PagedResult<MovieSimplifiedDto>> GetMoviesBySpokenLanguageIdAsync(MovieSearchBySpokenLanguageDto movieSearchByCountryDto)
    {
        var query = _dbContext.Movies
            .Include(m => m.Images)
            .Include(m => m.SpokenLanguages)
            .ThenInclude(mg => mg.Language)
            .AsQueryable();

        if (!string.IsNullOrEmpty(movieSearchByCountryDto.Iso639_1))
        {
            query = query.Where(m => m.SpokenLanguages.Any(mg => mg.LanguageCode == movieSearchByCountryDto.Iso639_1));
        }

        else if (!string.IsNullOrWhiteSpace(movieSearchByCountryDto.EnglishName))
        {
            var name = $"%{movieSearchByCountryDto.EnglishName.Trim()}%";
            query = query.Where(m => m.SpokenLanguages.Any(k => EF.Functions.ILike(k.Language.EnglishName, name)));
        }

        if (movieSearchByCountryDto.Page < 1) movieSearchByCountryDto.Page = 1;
        if (movieSearchByCountryDto.PageSize < 1) movieSearchByCountryDto.PageSize = 10;

        var totalCount = await query.CountAsync();

        query = query.
            Skip((movieSearchByCountryDto.Page - 1) * movieSearchByCountryDto.PageSize)
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
            Page = movieSearchByCountryDto.Page,
            PageSize = movieSearchByCountryDto.PageSize
        };
    }

    public async Task<IEnumerable<SpokenLanguage>> GetSpokenLanguagesByMovieIdAsync(Guid movieId)
    {
        return await _dbContext.MovieSpokenLanguages.Where(g => g.MovieId == movieId).
            Include(g => g.Language).Select(g => g.Language).ToListAsync();
    }
}
