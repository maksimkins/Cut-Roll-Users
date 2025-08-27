namespace Cut_Roll_Users.Infrastructure.Cast.Repositories;

using Cut_Roll_Users.Core.Casts.Dtos;
using Cut_Roll_Users.Core.Casts.Models;
using Cut_Roll_Users.Core.Casts.Repositories;
using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Common.Repositories.Base;
using Cut_Roll_Users.Core.MovieImages.Enums;
using Cut_Roll_Users.Infrastructure.Common.Data;
using Microsoft.EntityFrameworkCore;

public class CastEfCoreRepository : ICastRepository
{
    private readonly UsersDbContext _dbContext;
    public CastEfCoreRepository(UsersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> BulkCreateAsync(IEnumerable<CastCreateDto> listToCreate)
    {
        var newCasts = listToCreate.Select(toCreate => new Cast
        {
            MovieId = toCreate.MovieId,
            PersonId = toCreate.PersonId,
            Character = toCreate.Character,
            CastOrder = toCreate.CastOrder,
        });
        
        await _dbContext.Cast.AddRangeAsync(newCasts);
        var res = await _dbContext.SaveChangesAsync();

        return res > 0;
    }

 public async Task<bool> BulkDeleteAsync(IEnumerable<Guid> idsToDelete)
    {
        foreach (var item in idsToDelete)
        {
            var cast = await _dbContext.Cast.FirstOrDefaultAsync(c =>
                c.Id == item);

            if (cast != null)
            {
                _dbContext.Cast.Remove(cast);
            }
        }

        var result = await _dbContext.SaveChangesAsync();
        return result > 0;
    }

    public async Task<Guid?> CreateAsync(CastCreateDto entity)
    {
        var cast = new Cast
        {
            MovieId = entity.MovieId,
            PersonId = entity.PersonId,
            Character = entity.Character,
            CastOrder = entity.CastOrder
        };

        await _dbContext.Cast.AddAsync(cast);
        var res = await _dbContext.SaveChangesAsync();

        return res > 0 ? cast.Id : null;
    }

    public async Task<Guid?> DeleteByIdAsync(Guid id)
    {
        var cast = await _dbContext.Cast.FirstOrDefaultAsync(c =>
            c.Id == id);

        if (cast != null)
            _dbContext.Remove(cast);

        var res = await _dbContext.SaveChangesAsync();
        return res > 0 ? id : null;
    }

    public async Task<PagedResult<CastGetDto>> GetByMovieIdAsync(CastGetByMovieIdDto dto)
    {
        var query = _dbContext.Cast.AsQueryable();
        query = query.Where(c => c.MovieId == dto.MovieId).Include(c => c.Person).Include(c => c.Movie).ThenInclude(m => m.Images);

        var totalCount = await query.CountAsync();

        query = query.
            Skip((dto.PageNumber - 1) * dto.PageSize)
            .Take(dto.PageSize);

        var result = await query.ToListAsync();

        return new PagedResult<CastGetDto>()
        {
            Data = result.Select(c => new CastGetDto
            {
                MovieId = c.MovieId,
                Title = c.Movie.Title,
                Poster = c.Movie.Images?.FirstOrDefault(i => i.Type == ImageTypes.poster.ToString()),
                Person = c.Person,
                Character = c.Character,
                CastOrder = c.CastOrder,

            }).ToList(),

            TotalCount = totalCount,
            Page = dto.PageNumber,
            PageSize = dto.PageSize
        };

    }

    public async Task<PagedResult<CastGetDto>> GetByPersonIdAsync(CastGetByPersonIdDto dto)
    {
        var query = _dbContext.Cast.Include(i => i.Person).Include(c => c.Movie).ThenInclude(m => m.Images).AsQueryable();

        query = query.Where(c => c.PersonId == dto.PersonId);

        var totalCount = await query.CountAsync();

        query = query.
            Skip((dto.PageNumber - 1) * dto.PageSize)
            .Take(dto.PageSize);

        var result = await query.ToListAsync();

        return new PagedResult<CastGetDto>()
        {
            Data = result.Select(c => new CastGetDto
            {
                MovieId = c.MovieId,
                Title = c.Movie.Title,
                Poster = c.Movie.Images?.FirstOrDefault(i => i.Type == ImageTypes.poster.ToString()),
                Person = c.Person,
                Character = c.Character,
                CastOrder = c.CastOrder,

            }).ToList(),

            TotalCount = totalCount,
            Page = dto.PageNumber,
            PageSize = dto.PageSize
        };
    }


    public async Task<Guid?> UpdateAsync(CastUpdateDto entity)
    {
        var cast = await _dbContext.Cast.FirstOrDefaultAsync(c => c.Id == entity.Id);
        if (cast != null)
        {
            cast.Character = entity.Character ?? cast.Character;
            cast.CastOrder = entity.CastOrder ?? cast.CastOrder;

            _dbContext.Cast.Update(cast);
        }   
        
        var res = await _dbContext.SaveChangesAsync();

        return res > 0 ? entity.Id : null;
    }

    public async Task<PagedResult<CastGetDto>> SearchAsync(CastSearchDto request)
    {
        var query = _dbContext.Cast
            .Include(c => c.Person) 
            .Include(c => c.Movie).ThenInclude(m => m.Images)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var name = $"%{request.Name.Trim()}%";
            query = query.Where(c => EF.Functions.ILike(c.Person.Name, name));
        }
        
        if (!string.IsNullOrWhiteSpace(request.CharacterName))
        {
            var character = $"%{request.CharacterName.Trim()}%";
            query = query.Where(c => c.Character != null && EF.Functions.ILike(c.Character, character));
        }

        if (request.PageNumber < 1) request.PageNumber = 1;
        if (request.PageSize < 1) request.PageSize = 10;
        
        var totalCount = await query.CountAsync();

        query = query.
            Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize);

        var result = await query.ToListAsync();

        return new PagedResult<CastGetDto>()
        {
            Data = result.Select(c => new CastGetDto
            {
                MovieId = c.MovieId,
                Title = c.Movie.Title,
                Poster = c.Movie.Images?.FirstOrDefault(i => i.Type == ImageTypes.poster.ToString()),
                Person = c.Person,
                Character = c.Character,
                CastOrder = c.CastOrder,

            }).ToList(),

            TotalCount = totalCount,
            Page = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<bool> DeleteRangeById(Guid movieId)
    {
        var toDeletes = _dbContext.Cast.Where(g => g.MovieId == movieId);
        _dbContext.Cast.RemoveRange(toDeletes);

        var res = await _dbContext.SaveChangesAsync();
        return res > 0;
    }

    public async Task<bool> ExistsByIdAsync(Guid id)
    {
        return await _dbContext.Cast.AnyAsync(c => c.Id == id);
    }
}
