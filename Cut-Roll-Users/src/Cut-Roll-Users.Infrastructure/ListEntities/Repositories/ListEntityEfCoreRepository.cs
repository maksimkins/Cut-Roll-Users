using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Common.Repositories.Base;
using Cut_Roll_Users.Core.ListEntities.Dtos;
using Cut_Roll_Users.Core.ListEntities.Models;
using Cut_Roll_Users.Core.ListEntities.Repositories;
using Cut_Roll_Users.Core.Users.Dtos;
using Cut_Roll_Users.Infrastructure.Common.Data;
using Microsoft.EntityFrameworkCore;

namespace Cut_Roll_Users.Infrastructure.ListEntities.Repositories;

public class ListEntityEfCoreRepository : IListEntityRepository
{
    private readonly UsersDbContext _context;

    public ListEntityEfCoreRepository(UsersDbContext context)
    {
        _context = context;
    }

    public async Task<Guid?> CreateAsync(ListEntityCreateDto entity)
    {
        var listEntity = new ListEntity
        {
            UserId = entity.UserId,
            Title = entity.Title,
            Description = entity.Description,
            CreatedAt = DateTime.UtcNow
        };

        _context.ListEntities.Add(listEntity);
        var result = await _context.SaveChangesAsync();
        
        return result > 0 ? listEntity.Id : null;
    }

    public async Task<ListEntityResponseDto?> GetByIdAsync(Guid id)
    {
        var listEntity = await _context.ListEntities
            .Include(l => l.User)
            .Include(l => l.Movies)
            .Include(l => l.Likes)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (listEntity == null) return null;

        return new ListEntityResponseDto
        {
            Id = listEntity.Id,
            userSimlified = new UserSimlified
            {
                Id = listEntity.User.Id,
                UserName = listEntity.User.UserName,
                Email = listEntity.User.Email,
                AvatarPath = listEntity.User.AvatarPath
            },
            Title = listEntity.Title,
            Description = listEntity.Description,
            CreatedAt = listEntity.CreatedAt,
            MoviesCount = listEntity.Movies.Count,
            LikesCount = listEntity.Likes.Count
        };
    }

    public async Task<Guid?> UpdateAsync(ListEntityUpdateDto entity)
    {
        var listEntity = await _context.ListEntities
            .FirstOrDefaultAsync(l => l.Id == entity.Id);
        
        if (listEntity == null) return null;

        if (!string.IsNullOrWhiteSpace(entity.Title))
            listEntity.Title = entity.Title;
        
        if (entity.Description != null)
            listEntity.Description = entity.Description;
        
        var result = await _context.SaveChangesAsync();
        return result > 0 ? entity.Id : null;
    }

    public async Task<Guid?> DeleteByIdAsync(Guid id)
    {
        var listEntity = await _context.ListEntities
            .FirstOrDefaultAsync(l => l.Id == id);
        
        if (listEntity == null) return null;

        _context.ListEntities.Remove(listEntity);
        var result = await _context.SaveChangesAsync();
        
        return result > 0 ? id : null;
    }

    public async Task<PagedResult<ListEntityResponseDto>> SearchAsync(ListEntitySearchDto request)
    {
        var query = _context.ListEntities
            .Include(l => l.User)
            .Include(l => l.Movies)
            .Include(l => l.Likes)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.UserId))
        {
            query = query.Where(l => l.UserId == request.UserId);
        }

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            var title = $"%{request.Title.Trim()}%";
            query = query.Where(l => EF.Functions.ILike(l.Title, title));
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(l => l.CreatedAt >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(l => l.CreatedAt <= request.ToDate.Value);
        }

        var totalCount = await query.CountAsync();

        var pageNumber = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        var listEntities = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new ListEntityResponseDto
            {
                Id = l.Id,
                userSimlified = new UserSimlified
                {
                    Id = l.User.Id,
                    UserName = l.User.UserName,
                    Email = l.User.Email,
                    AvatarPath = l.User.AvatarPath
                },
                Title = l.Title,
                Description = l.Description,
                CreatedAt = l.CreatedAt,
                MoviesCount = l.Movies.Count,
                LikesCount = l.Likes.Count
            })
            .ToListAsync();

        return new PagedResult<ListEntityResponseDto>
        {
            Data = listEntities,
            TotalCount = totalCount,
            Page = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<ListEntityResponseDto>> GetByUserIdAsync(ListEntityPaginationDto dto)
    {
        var query = _context.ListEntities
            .Include(l => l.User)
            .Include(l => l.Movies)
            .Include(l => l.Likes)
            .Where(l => l.UserId == dto.UserId)
            .OrderByDescending(l => l.CreatedAt);

        var totalCount = await query.CountAsync();

        var pageNumber = dto.Page <= 0 ? 1 : dto.Page;
        var pageSize = dto.PageSize <= 0 ? 10 : dto.PageSize;

        var listEntities = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new ListEntityResponseDto
            {
                Id = l.Id,
                userSimlified = new UserSimlified
                {
                    Id = l.User.Id,
                    UserName = l.User.UserName,
                    Email = l.User.Email,
                    AvatarPath = l.User.AvatarPath
                },
                Title = l.Title,
                Description = l.Description,
                CreatedAt = l.CreatedAt,
                MoviesCount = l.Movies.Count,
                LikesCount = l.Likes.Count
            })
            .ToListAsync();

        return new PagedResult<ListEntityResponseDto>
        {
            Data = listEntities,
            TotalCount = totalCount,
            Page = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<int> GetListCountByUserIdAsync(string userId)
    {
        return await _context.ListEntities
            .Where(l => l.UserId == userId)
            .CountAsync();
    }
}
