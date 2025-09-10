using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Follows.Dtos;
using Cut_Roll_Users.Core.Follows.Models;
using Cut_Roll_Users.Core.Users.Dtos;

namespace Cut_Roll_Users.Core.Follows.Services;

public interface IFollowService
{
    Task<string> CreateFollowAsync(FollowCreateDto? dto);
    Task<string> DeleteFollowAsync(FollowDeleteDto? dto);
    Task<PagedResult<UserSimplified>> GetUserFollowsAsync(FollowPaginationDto? dto);
    Task<int> GetFollowersCountAsync(string? userId);
    Task<int> GetFollowingCountAsync(string? userId);
    Task<FollowStatusDto> GetFollowStatusAsync(string? userId, string? targetUserId);
    Task<bool> IsFollowingAsync(string? followerId, string? followingId);
    
    Task<PagedResult<FeedActivityDto>> GetUserFeedAsync(FeedPaginationDto? dto);
    Task<List<string>> GetFollowingUserIdsAsync(string? userId);
    Task<bool> IsFollowOwnedByUserAsync(string? followerId, string? followingId);
    Task<IQueryable<Follow>> GetFollowsAsQueryableAsync();
    
}

