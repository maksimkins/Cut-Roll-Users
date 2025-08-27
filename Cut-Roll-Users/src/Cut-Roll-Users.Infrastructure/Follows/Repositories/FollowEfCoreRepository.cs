using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Common.Repositories.Base;
using Cut_Roll_Users.Core.Follows.Dtos;
using Cut_Roll_Users.Core.Follows.Models;
using Cut_Roll_Users.Core.Follows.Repositories;
using Cut_Roll_Users.Core.Users.Dtos;
using Cut_Roll_Users.Infrastructure.Common.Data;
using Microsoft.EntityFrameworkCore;

namespace Cut_Roll_Users.Infrastructure.Follows.Repositories;

public class FollowEfCoreRepository : IFollowRepository
{
    private readonly UsersDbContext _context;

    public FollowEfCoreRepository(UsersDbContext context)
    {
        _context = context;
    }

    public async Task<Guid?> CreateAsync(FollowCreateDto entity)
    {
        var follow = new Follow
        {
            FollowerId = entity.FollowerId,
            FollowingId = entity.FollowingId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Follows.Add(follow);
        var result = await _context.SaveChangesAsync();
        
        return result > 0 ? follow.Id : null;
    }

    public async Task<FollowResponseDto?> GetByIdAsync(Guid id)
    {
        var follow = await _context.Follows
            .Include(f => f.Follower)
            .Include(f => f.Following)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (follow == null) return null;

        return new FollowResponseDto
        {
            Id = follow.Id,
            Follower = new UserResponseDto
            {
                Id = follow.Follower.Id,
                Username = follow.Follower.UserName,
                Email = follow.Follower.Email,
                AvatarPath = follow.Follower.AvatarPath,
                IsBanned = follow.Follower.IsBanned,
                IsMuted = follow.Follower.IsMuted
            },
            Following = new UserResponseDto
            {
                Id = follow.Following.Id,
                Username = follow.Following.UserName,
                Email = follow.Following.Email,
                AvatarPath = follow.Following.AvatarPath,
                IsBanned = follow.Following.IsBanned,
                IsMuted = follow.Following.IsMuted
            },
            CreatedAt = follow.CreatedAt
        };
    }

    public async Task<string?> DeleteByIdAsync(Guid id)
    {
        var follow = await _context.Follows
            .FirstOrDefaultAsync(f => f.Id == id);
        
        if (follow == null) return null;

        _context.Follows.Remove(follow);
        var result = await _context.SaveChangesAsync();
        
        return result > 0 ? follow.FollowerId : null;
    }

    public async Task<bool> FollowExistsAsync(string followerId, string followingId)
    {
        return await _context.Follows
            .AnyAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
    }

    public async Task<string?> DeleteFollowAsync(FollowDeleteDto dto)
    {
        var follow = await _context.Follows
            .FirstOrDefaultAsync(f => f.FollowerId == dto.FollowerId && f.FollowingId == dto.FollowingId);
        
        if (follow == null) return null;

        _context.Follows.Remove(follow);
        var result = await _context.SaveChangesAsync();
        
        return result > 0 ? follow.FollowerId : null;
    }

    public async Task<PagedResult<FollowResponseDto>> GetUserFollowsAsync(FollowPaginationDto dto)
    {
        IQueryable<Follow> query;

        if (dto.Type == FollowType.Followers)
        {
            // Get users who follow the specified user
            query = _context.Follows
                .Include(f => f.Follower)
                .Where(f => f.FollowingId == dto.UserId);
        }
        else
        {
            // Get users that the specified user is following
            query = _context.Follows
                .Include(f => f.Following)
                .Where(f => f.FollowerId == dto.UserId);
        }

        var totalCount = await query.CountAsync();

        var pageNumber = dto.Page <= 0 ? 1 : dto.Page;
        var pageSize = dto.PageSize <= 0 ? 10 : dto.PageSize;

        var follows = await query
            .OrderByDescending(f => f.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new FollowResponseDto
            {
                Id = f.Id,
                Follower = dto.Type == FollowType.Followers ? new UserResponseDto
                {
                    Id = f.Follower.Id,
                    Username = f.Follower.UserName,
                    Email = f.Follower.Email,
                    AvatarPath = f.Follower.AvatarPath,
                    IsBanned = f.Follower.IsBanned,
                    IsMuted = f.Follower.IsMuted
                } : null!,
                Following = dto.Type == FollowType.Following ? new UserResponseDto
                {
                    Id = f.Following.Id,
                    Username = f.Following.UserName,
                    Email = f.Following.Email,
                    AvatarPath = f.Following.AvatarPath,
                    IsBanned = f.Following.IsBanned,
                    IsMuted = f.Following.IsMuted
                } : null!,
                CreatedAt = f.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<FollowResponseDto>
        {
            Data = follows,
            TotalCount = totalCount,
            Page = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<int> GetFollowersCountAsync(string userId)
    {
        return await _context.Follows
            .Where(f => f.FollowingId == userId)
            .CountAsync();
    }

    public async Task<int> GetFollowingCountAsync(string userId)
    {
        return await _context.Follows
            .Where(f => f.FollowerId == userId)
            .CountAsync();
    }

    public async Task<FollowStatusDto> GetFollowStatusAsync(string userId, string targetUserId)
    {
        var isFollowing = await _context.Follows
            .AnyAsync(f => f.FollowerId == userId && f.FollowingId == targetUserId);

        var isFollowedBy = await _context.Follows
            .AnyAsync(f => f.FollowerId == targetUserId && f.FollowingId == userId);

        return new FollowStatusDto
        {
            UserId = userId,
            TargetUserId = targetUserId,
            IsFollowing = isFollowing,
            IsFollowedBy = isFollowedBy,
            IsMutualFollow = isFollowing && isFollowedBy
        };
    }

    public async Task<IQueryable<Follow>> GetFollowsAsQueryableAsync()
    {
        return await Task.FromResult(_context.Follows.AsQueryable());
    }

    public async Task<PagedResult<FeedActivityDto>> GetUserFeedAsync(FeedPaginationDto dto)
    {
        // Get all users that the current user is following
        var followingUserIds = await _context.Follows
            .Where(f => f.FollowerId == dto.UserId)
            .Select(f => f.FollowingId)
            .ToListAsync();

        // Get activities from followed users (this would need to be expanded based on what activities you want to show)
        // For now, returning empty result as the feed logic would depend on what activities you want to track
        return new PagedResult<FeedActivityDto>
        {
            Data = new List<FeedActivityDto>(),
            TotalCount = 0,
            Page = dto.Page,
            PageSize = dto.PageSize
        };
    }

    public async Task<List<string>> GetFollowingUserIdsAsync(string userId)
    {
        return await _context.Follows
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FollowingId)
            .ToListAsync();
    }
}
