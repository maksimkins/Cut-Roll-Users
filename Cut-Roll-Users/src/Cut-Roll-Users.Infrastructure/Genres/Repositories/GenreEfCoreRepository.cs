namespace Cut_Roll_Users.Infrastructure.Genres.Repositories;

using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Genres.Dtos;
using Cut_Roll_Users.Core.Genres.Models;
using Cut_Roll_Users.Core.Genres.Repositories;
using Cut_Roll_Users.Infrastructure.Common.Data;
using Microsoft.EntityFrameworkCore;

public class GenreEfCoreRepository : IGenreRepository
{
    private readonly UsersDbContext _dbContext;
    public GenreEfCoreRepository(UsersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid?> CreateAsync(GenreCreateDto entity)
    {
        var genre = new Genre
        {
            Name = entity.Name,
        };

        await _dbContext.Genres.AddAsync(genre);
        var res = await _dbContext.SaveChangesAsync();

        return res > 0 ? genre.Id : null;
    }

    public async Task<Guid?> DeleteByIdAsync(Guid id)
    {
        var toDelete = await _dbContext.Genres.FirstOrDefaultAsync(g => g.Id == id);
        if (toDelete != null)
            _dbContext.Genres.Remove(toDelete);

        var res = await _dbContext.SaveChangesAsync();
        return res > 0 ? id : null;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbContext.Genres.AnyAsync(g => g.Id == id);
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _dbContext.Genres.AnyAsync(g => g.Name == name);
    }


    public async Task<PagedResult<Genre>> GetAllAsync(GenrePaginationDto dto)
    {
        var query = _dbContext.Genres.AsQueryable();

        if (dto.PageNumber < 1) dto.PageNumber = 1;
        if (dto.PageSize < 1) dto.PageSize = 10;

        var totalCount = await query.CountAsync();

        query = query.
            Skip((dto.PageNumber - 1) * dto.PageSize)
            .Take(dto.PageSize);

        return new PagedResult<Genre>()
        {
            Data = await query.ToListAsync(),
            TotalCount = totalCount,
            Page = dto.PageNumber,
            PageSize = dto.PageSize
        };
    }

    public async Task<Genre?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Genres.FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<PagedResult<Genre>> SearchAsync(GenreSearchDto request)
    {
        var query = _dbContext.Genres.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            request.Name = request.Name.Trim();
            query = query.Where(g => EF.Functions.ILike(g.Name, $"%{request.Name}%"));
        }

        if (request.PageNumber < 1) request.PageNumber = 1;
        if (request.PageSize < 1) request.PageSize = 10;

        var totalCount = await query.CountAsync();

        query = query.
            Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize);

        return new PagedResult<Genre>()
        {
            Data = await query.ToListAsync(),
            TotalCount = totalCount,
            Page = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
