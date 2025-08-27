using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Common.Repositories.Base;
using Cut_Roll_Users.Core.ReviewLikes.Dtos;
using Cut_Roll_Users.Core.ReviewLikes.Models;
using Cut_Roll_Users.Core.ReviewLikes.Repositories;
using Cut_Roll_Users.Core.Reviews.Dtos;
using Cut_Roll_Users.Core.Reviews.Models;
using Cut_Roll_Users.Infrastructure.Common.Data;
using Microsoft.EntityFrameworkCore;

namespace Cut_Roll_Users.Infrastructure.ReviewLikes.Repositories;

public class ReviewLikeEfCoreRepository : IReviewLikeRepository
{
    private readonly UsersDbContext _context;

    public ReviewLikeEfCoreRepository(UsersDbContext context)
    {
        _context = context;
    }

    public async Task<Guid?> CreateAsync(ReviewLikeDto entity)
    {
        var reviewLike = new ReviewLike
        {
            UserId = entity.UserId,
            ReviewId = entity.ReviewId,
            LikedAt = DateTime.UtcNow
        };

        _context.ReviewLikes.Add(reviewLike);
        var result = await _context.SaveChangesAsync();
        
        return result > 0 ? entity.ReviewId : null;
    }

    public async Task<Guid?> DeleteAsync(ReviewLikeDto entity)
    {
        var reviewLike = await _context.ReviewLikes
            .FirstOrDefaultAsync(rl => rl.UserId == entity.UserId && rl.ReviewId == entity.ReviewId);
        
        if (reviewLike == null) return null;

        _context.ReviewLikes.Remove(reviewLike);
        var result = await _context.SaveChangesAsync();
        
        return result > 0 ? entity.ReviewId : null;
    }

    public async Task<bool> IsLikedByUserAsync(string userId, Guid reviewId)
    {
        return await _context.ReviewLikes
            .AnyAsync(rl => rl.UserId == userId && rl.ReviewId == reviewId);
    }

    public async Task<int> GetLikeCountByReviewIdAsync(Guid reviewId)
    {
        return await _context.ReviewLikes
            .Where(rl => rl.ReviewId == reviewId)
            .CountAsync();
    }

    public async Task<PagedResult<Review>> GetLikedReviewsByUserIdAsync(ReviewPaginationUserDto dto)
    {
        var query = _context.ReviewLikes
            .Include(rl => rl.Review)
                .ThenInclude(r => r.User)
            .Include(rl => rl.Review)
                .ThenInclude(r => r.Movie)
            .Where(rl => rl.UserId == dto.UserId)
            .OrderByDescending(rl => rl.LikedAt);

        var totalCount = await query.CountAsync();

        var pageNumber = dto.Page <= 0 ? 1 : dto.Page;
        var pageSize = dto.PageSize <= 0 ? 10 : dto.PageSize;

        var likedReviews = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(rl => rl.Review)
            .ToListAsync();

        return new PagedResult<Review>
        {
            Data = likedReviews,
            TotalCount = totalCount,
            Page = pageNumber,
            PageSize = pageSize
        };
    }
}

