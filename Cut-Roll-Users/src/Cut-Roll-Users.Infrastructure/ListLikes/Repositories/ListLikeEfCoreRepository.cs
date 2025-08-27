using Cut_Roll_Users.Core.Common.Repositories.Base;
using Cut_Roll_Users.Core.ListLikes.Dtos;
using Cut_Roll_Users.Core.ListLikes.Models;
using Cut_Roll_Users.Core.ListLikes.Repositories;
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
}
