using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.ListEntities.Dtos;
using Cut_Roll_Users.Core.ListLikes.Dtos;
using Cut_Roll_Users.Core.ListLikes.Models;
using Cut_Roll_Users.Core.ListLikes.Repositories;
using Cut_Roll_Users.Core.Users.Dtos;
using Cut_Roll_Users.Infrastructure.Common.Data;
using Microsoft.EntityFrameworkCore;

namespace Cut_Roll_Users.Infrastructure.ListLikes.Repositories;

public class ListLikeEfCoreRepository : IListLikeRepository
{
    private readonly UsersDbContext _context;

    public ListLikeEfCoreRepository(UsersDbContext context)
    {
        _context = context;
    }

    public async Task<Guid?> CreateAsync(ListLikeDto entity)
    {
        var listLike = new ListLike
        {
            UserId = entity.UserId,
            ListId = entity.ListId,
            LikedAt = DateTime.UtcNow
        };

        _context.ListLikes.Add(listLike);
        var result = await _context.SaveChangesAsync();
        
        return result > 0 ? entity.ListId : null;
    }

    public async Task<Guid?> DeleteAsync(ListLikeDto entity)
    {
        var listLike = await _context.ListLikes
            .FirstOrDefaultAsync(ll => ll.UserId == entity.UserId && ll.ListId == entity.ListId);
        
        if (listLike == null) return null;

        _context.ListLikes.Remove(listLike);
        var result = await _context.SaveChangesAsync();
        
        return result > 0 ? entity.ListId : null;
    }

    public async Task<bool> IsLikedByUserAsync(string userId, Guid listId)
    {
        return await _context.ListLikes
            .AnyAsync(ll => ll.UserId == userId && ll.ListId == listId);
    }

    public async Task<int> GetLikeCountByListIdAsync(Guid listId)
    {
        return await _context.ListLikes
            .Where(ll => ll.ListId == listId)
            .CountAsync();
    }

    public async Task<PagedResult<ListEntityResponseDto>> GetLikedLists(ListLikedDto dto)
    {
        var query = _context.ListLikes
            .Include(like => like.List)
                .ThenInclude(list => list.User) 
            .Include(like => like.List)
                .ThenInclude(list => list.Movies) 
            .Where(like => like.UserId == dto.UserId)
            .Select(like => like.List) 
            .AsQueryable();

        var totalCount = await query.CountAsync();

        var pageNumber = dto.Page <= 0 ? 1 : dto.Page;
        var pageSize = dto.PageSize <= 0 ? 10 : dto.PageSize;

        var lists = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(list => new ListEntityResponseDto
            {
                Id = list.Id,
                Title = list.Title,
                Description = list.Description,
                CreatedAt = list.CreatedAt,
                MoviesCount = list.Movies.Count,
                LikesCount = list.Likes.Count,
                UserSimplified = new UserSimplified
                {
                    Id = list.User.Id,
                    UserName = list.User.UserName,
                    Email = list.User.Email,
                    AvatarPath = list.User.AvatarPath
                }
            })
            .ToListAsync();

        return new PagedResult<ListEntityResponseDto>
        {
            Data = lists,
            TotalCount = totalCount,
            Page = pageNumber,
            PageSize = pageSize
        };
    }
}
