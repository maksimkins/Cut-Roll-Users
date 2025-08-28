using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Common.Repositories.Base;
using Cut_Roll_Users.Core.Follows.Dtos;
using Cut_Roll_Users.Core.Follows.Models;
using Cut_Roll_Users.Core.Follows.Repositories;
using Cut_Roll_Users.Core.Users.Dtos;
using Cut_Roll_Users.Core.Movies.Dtos;
using Cut_Roll_Users.Core.Reviews.Dtos;
using Cut_Roll_Users.Core.ListEntities.Dtos;
using Cut_Roll_Users.Core.MovieImages.Enums;
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

    public async Task<string?> CreateAsync(FollowCreateDto entity)
    {
        var follow = new Follow
        {
            FollowerId = entity.FollowerId,
            FollowingId = entity.FollowingId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Follows.Add(follow);
        var result = await _context.SaveChangesAsync();
        

        return result > 0 ? entity.FollowingId : null;
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

    public async Task<PagedResult<UserSimplified>> GetUserFollowsAsync(FollowPaginationDto dto)
    {
        IQueryable<Follow> query;

        if (dto.Type == FollowType.Followers)
        {
            query = _context.Follows
                .Include(f => f.Follower)
                .Where(f => f.FollowingId == dto.UserId);
        }
        else
        {
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
            .Select(f => new UserSimplified
            { 
                Id = dto.Type == FollowType.Followers ? f.Follower.Id : f.Following.Id,
                UserName = dto.Type == FollowType.Followers ? f.Follower.UserName : f.Following.UserName,
                Email = dto.Type == FollowType.Followers ? f.Follower.Email : f.Following.Email,
                AvatarPath = dto.Type == FollowType.Followers ? f.Follower.AvatarPath : f.Following.AvatarPath,
            })
            .ToListAsync();

        return new PagedResult<UserSimplified>
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
        
        var followingUserIds = await _context.Follows
            .Where(f => f.FollowerId == dto.UserId)
            .Select(f => f.FollowingId)
            .ToListAsync();

        if (!followingUserIds.Any())
        {
            return new PagedResult<FeedActivityDto>
            {
                Data = new List<FeedActivityDto>(),
                TotalCount = 0,
                Page = dto.Page,
                PageSize = dto.PageSize
            };
        }

        var activities = new List<FeedActivityDto>();

        if (!dto.FilterByType.HasValue || dto.FilterByType == ActivityType.MovieLike)
        {
            var movieLikes = await _context.MovieLikes
                .Include(ml => ml.User)
                .Include(ml => ml.Movie)
                    .ThenInclude(m => m.Images)
                .Where(ml => followingUserIds.Contains(ml.UserId))
                .Where(ml => !dto.FromDate.HasValue || ml.LikedAt >= dto.FromDate.Value)
                .Select(ml => new FeedActivityDto
                {
                    User = new UserSimplified
                    {
                        Id = ml.User.Id,
                        UserName = ml.User.UserName,
                        Email = ml.User.Email,
                        AvatarPath = ml.User.AvatarPath
                    },
                    Type = ActivityType.MovieLike,
                    Movie = new MovieSimplifiedDto
                    {
                        MovieId = ml.Movie.Id,
                        Title = ml.Movie.Title,
                        Poster = ml.Movie.Images.FirstOrDefault(i => i.Type == ImageTypes.poster.ToString())
                    },
                    CreatedAt = ml.LikedAt
                })
                .ToListAsync();
            activities.AddRange(movieLikes);
        }

        if (!dto.FilterByType.HasValue || dto.FilterByType == ActivityType.MovieReview)
        {
            var movieReviews = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Movie)
                    .ThenInclude(m => m.Images)
                .Where(r => followingUserIds.Contains(r.UserId))
                .Where(r => !dto.FromDate.HasValue || r.CreatedAt >= dto.FromDate.Value)
                .Select(r => new FeedActivityDto
                {
                    User = new UserSimplified
                    {
                        Id = r.User.Id,
                        UserName = r.User.UserName,
                        Email = r.User.Email,
                        AvatarPath = r.User.AvatarPath
                    },
                    Type = ActivityType.MovieReview,
                    Movie = new MovieSimplifiedDto
                    {
                        MovieId = r.Movie.Id,
                        Title = r.Movie.Title,
                        Poster = r.Movie.Images.FirstOrDefault(i => i.Type == ImageTypes.poster.ToString())
                    },
                    Review = new ReviewSimplifiedDto
                    {
                        Id = r.Id,
                        Content = r.Content,
                        Rating = (int)r.Rating,
                        CreatedAt = r.CreatedAt
                    },
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();
            activities.AddRange(movieReviews);
        }

        if (!dto.FilterByType.HasValue || dto.FilterByType == ActivityType.MovieWatched)
        {
            var watchedMovies = await _context.WatchedMovies
                .Include(w => w.User)
                .Include(w => w.Movie)
                    .ThenInclude(m => m.Images)
                .Where(w => followingUserIds.Contains(w.UserId))
                .Where(w => !dto.FromDate.HasValue || w.WatchedAt >= dto.FromDate.Value)
                .Select(w => new FeedActivityDto
                {
                    User = new UserSimplified
                    {
                        Id = w.User.Id,
                        UserName = w.User.UserName,
                        Email = w.User.Email,
                        AvatarPath = w.User.AvatarPath
                    },
                    Type = ActivityType.MovieWatched,
                    Movie = new MovieSimplifiedDto
                    {
                        MovieId = w.Movie.Id,
                        Title = w.Movie.Title,
                        Poster = w.Movie.Images.FirstOrDefault(i => i.Type == ImageTypes.poster.ToString())
                    },
                    CreatedAt = w.WatchedAt
                })
                .ToListAsync();
            activities.AddRange(watchedMovies);
        }

        if (!dto.FilterByType.HasValue || dto.FilterByType == ActivityType.WantToWatch)
        {
            var wantToWatchMovies = await _context.WantToWatchMovies
                .Include(w => w.User)
                .Include(w => w.Movie)
                    .ThenInclude(m => m.Images)
                .Where(w => followingUserIds.Contains(w.UserId))
                .Where(w => !dto.FromDate.HasValue || w.AddedAt >= dto.FromDate.Value)
                .Select(w => new FeedActivityDto
                {
                    User = new UserSimplified
                    {
                        Id = w.User.Id,
                        UserName = w.User.UserName,
                        Email = w.User.Email,
                        AvatarPath = w.User.AvatarPath
                    },
                    Type = ActivityType.WantToWatch,
                    Movie = new MovieSimplifiedDto
                    {
                        MovieId = w.Movie.Id,
                        Title = w.Movie.Title,
                        Poster = w.Movie.Images.FirstOrDefault(i => i.Type == ImageTypes.poster.ToString())
                    },
                    CreatedAt = w.AddedAt
                })
                .ToListAsync();
            activities.AddRange(wantToWatchMovies);
        }

        if (!dto.FilterByType.HasValue || dto.FilterByType == ActivityType.ListCreated)
        {
            var listCreated = await _context.ListEntities
                .Include(l => l.User)
                .Where(l => followingUserIds.Contains(l.UserId))
                .Where(l => !dto.FromDate.HasValue || l.CreatedAt >= dto.FromDate.Value)
                .Select(l => new FeedActivityDto
                {
                    User = new UserSimplified
                    {
                        Id = l.User.Id,
                        UserName = l.User.UserName,
                        Email = l.User.Email,
                        AvatarPath = l.User.AvatarPath
                    },
                    Type = ActivityType.ListCreated,
                    List = new ListEntitySimplifiedDto
                    {
                        Id = l.Id,
                        Title = l.Title,
                        Description = l.Description,
                        CreatedAt = l.CreatedAt
                    },
                    CreatedAt = l.CreatedAt
                })
                .ToListAsync();
            activities.AddRange(listCreated);
        }

        if (!dto.FilterByType.HasValue || dto.FilterByType == ActivityType.ListLiked)
        {
            var listLiked = await _context.ListLikes
                .Include(ll => ll.User)
                .Include(ll => ll.List)
                .Where(ll => followingUserIds.Contains(ll.UserId))
                .Where(ll => !dto.FromDate.HasValue || ll.LikedAt >= dto.FromDate.Value)
                .Select(ll => new FeedActivityDto
                {
                    User = new UserSimplified
                    {
                        Id = ll.User.Id,
                        UserName = ll.User.UserName,
                        Email = ll.User.Email,
                        AvatarPath = ll.User.AvatarPath
                    },
                    Type = ActivityType.ListLiked,
                    List = new ListEntitySimplifiedDto
                    {
                        Id = ll.List.Id,
                        Title = ll.List.Title,
                        Description = ll.List.Description,
                        CreatedAt = ll.List.CreatedAt
                    },
                    CreatedAt = ll.LikedAt
                })
                .ToListAsync();
            activities.AddRange(listLiked);
        }

        if (!dto.FilterByType.HasValue || dto.FilterByType == ActivityType.ReviewLiked)
        {
            var reviewLiked = await _context.ReviewLikes
                .Include(rl => rl.User)
                .Include(rl => rl.Review)
                    .ThenInclude(r => r.Movie)
                        .ThenInclude(m => m.Images)
                .Where(rl => followingUserIds.Contains(rl.UserId))
                .Where(rl => !dto.FromDate.HasValue || rl.LikedAt >= dto.FromDate.Value)
                .Select(rl => new FeedActivityDto
                {
                    User = new UserSimplified
                    {
                        Id = rl.User.Id,
                        UserName = rl.User.UserName,
                        Email = rl.User.Email,
                        AvatarPath = rl.User.AvatarPath
                    },
                    Type = ActivityType.ReviewLiked,
                    Movie = new MovieSimplifiedDto
                    {
                        MovieId = rl.Review.Movie.Id,
                        Title = rl.Review.Movie.Title,
                        Poster = rl.Review.Movie.Images.FirstOrDefault(i => i.Type == ImageTypes.poster.ToString())
                    },
                    Review = new ReviewSimplifiedDto
                    {
                        Id = rl.Review.Id,
                        Content = rl.Review.Content,
                        Rating = (int)rl.Review.Rating,
                        CreatedAt = rl.Review.CreatedAt
                    },
                    CreatedAt = rl.LikedAt
                })
                .ToListAsync();
            activities.AddRange(reviewLiked);
        }

        var sortedActivities = activities.OrderByDescending(a => a.CreatedAt).ToList();

        var totalCount = sortedActivities.Count;
        var pageNumber = dto.Page <= 0 ? 1 : dto.Page;
        var pageSize = dto.PageSize <= 0 ? 20 : dto.PageSize;

        var pagedActivities = sortedActivities
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<FeedActivityDto>
        {
            Data = pagedActivities,
            TotalCount = totalCount,
            Page = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<List<string>> GetFollowingUserIdsAsync(string userId)
    {
        return await _context.Follows
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FollowingId)
            .ToListAsync();
    }

    public async Task<bool> IsFollowOwnedByUserAsync(string followerId, string followingId)
    {
        return await _context.Follows
            .AnyAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
    }

}
