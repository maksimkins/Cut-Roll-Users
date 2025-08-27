namespace Cut_Roll_Users.Infrastructure.MovieKeywords.Repositories;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Keywords.Models;
using Cut_Roll_Users.Core.MovieImages.Enums;
using Cut_Roll_Users.Core.MovieKeywords.Dtos;
using Cut_Roll_Users.Core.MovieKeywords.Models;
using Cut_Roll_Users.Core.MovieKeywords.Repositories;
using Cut_Roll_Users.Core.Movies.Dtos;
using Cut_Roll_Users.Core.Movies.Models;
using Cut_Roll_Users.Infrastructure.Common.Data;
using Microsoft.EntityFrameworkCore;

public class MovieKeywordEfCoreRepository : IMovieKeywordRepository
{
    private readonly UsersDbContext _dbContext;
    public MovieKeywordEfCoreRepository(UsersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> BulkCreateAsync(IEnumerable<MovieKeywordDto> listToCreate)
    {
        var newList = listToCreate.Select(toCreate => new MovieKeyword
        {
            MovieId = toCreate.MovieId,
            KeywordId = toCreate.KeywordId,
        });
        
        await _dbContext.MovieKeywords.AddRangeAsync(newList);
        var res = await _dbContext.SaveChangesAsync();

        return res > 0;
    }

    public async Task<bool> BulkDeleteAsync(IEnumerable<MovieKeywordDto> listToDelete)
    {
        foreach (var item in listToDelete)
        {
            var movieKeyword = await _dbContext.MovieKeywords.FirstOrDefaultAsync(c =>
                c.MovieId == item.MovieId && c.KeywordId == item.KeywordId);

            if (movieKeyword != null)
            {
                _dbContext.MovieKeywords.Remove(movieKeyword);
            }
        }

        var result = await _dbContext.SaveChangesAsync();
        return result > 0;
    }

    public async Task<Guid?> CreateAsync(MovieKeywordDto entity)
    {
        var movieKeyword = new MovieKeyword
        {
            MovieId = entity.MovieId,
            KeywordId = entity.KeywordId,
        };

        await _dbContext.MovieKeywords.AddAsync(movieKeyword);
        var res = await _dbContext.SaveChangesAsync();

        return res > 0 ? entity.MovieId : null;
    }

    public async Task<Guid?> DeleteAsync(MovieKeywordDto dto)
    {
        var toDelete = await _dbContext.MovieKeywords.FirstOrDefaultAsync(g => g.MovieId == dto.MovieId && g.KeywordId == dto.KeywordId);
        if (toDelete != null)
            _dbContext.MovieKeywords.Remove(toDelete);

        var res = await _dbContext.SaveChangesAsync();
        return res > 0 ? dto.MovieId : null;
    }

    public async Task<bool> DeleteRangeById(Guid movieId)
    {
        var toDeletes = _dbContext.MovieKeywords.Where(i => i.MovieId == movieId);
        _dbContext.RemoveRange(toDeletes);

        var res = await _dbContext.SaveChangesAsync();
        return res > 0;
    }

    public async Task<bool> ExistsAsync(MovieKeywordDto dto)
    {
        return await _dbContext.MovieKeywords.AnyAsync(g => g.MovieId == dto.MovieId && g.KeywordId == dto.KeywordId);
    }

    public async Task<IEnumerable<Keyword>> GetKeywordsByMovieIdAsync(Guid movieId)
    {
        return await _dbContext.MovieKeywords.Where(g => g.MovieId == movieId).
            Include(g => g.Keyword).Select(g => g.Keyword).ToListAsync();
    }

    public async Task<PagedResult<MovieSimplifiedDto>> GetMoviesByKeywordIdAsync(MovieSearchByKeywordDto searchDto)
    {
        var query = _dbContext.Movies
            .Include(m => m.Images)
            .Include(m => m.Keywords)
            .ThenInclude(mg => mg.Keyword)
            .AsQueryable();

        if (searchDto.KeywordId.HasValue)
        {
            query = query.Where(m => m.Keywords.Any(mg => mg.KeywordId == searchDto.KeywordId.Value));
        }

        else if (!string.IsNullOrWhiteSpace(searchDto.Name))
        {
            var name = $"%{searchDto.Name.Trim()}%";
            query = query.Where(m => m.Keywords.Any(k => EF.Functions.ILike(k.Keyword.Name, name)));
        }


        if (searchDto.PageNumber < 1) searchDto.PageNumber = 1;
        if (searchDto.PageSize < 1) searchDto.PageSize = 10;

        var totalCount = await query.CountAsync();

        query = query.
            Skip((searchDto.PageNumber - 1) * searchDto.PageSize)
            .Take(searchDto.PageSize);

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
            Page = searchDto.PageNumber,
            PageSize = searchDto.PageSize
        };
    }
}
