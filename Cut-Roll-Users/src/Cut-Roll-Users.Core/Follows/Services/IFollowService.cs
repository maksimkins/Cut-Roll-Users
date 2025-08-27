using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Follows.Dtos;

namespace Cut_Roll_Users.Core.Follows.Services;

public interface IFollowService
{
    Task<Guid> CreateFollowAsync(FollowCreateDto? dto);
    Task<FollowResponseDto?> GetFollowByIdAsync(Guid? id);
    Task<string> DeleteFollowAsync(FollowDeleteDto? dto);
    Task<string> UnfollowUserAsync(string? followerId, string? followingId);
    Task<PagedResult<FollowResponseDto>> GetUserFollowsAsync(FollowPaginationDto? dto);
    Task<int> GetFollowersCountAsync(string? userId);
    Task<int> GetFollowingCountAsync(string? userId);
    Task<FollowStatusDto> GetFollowStatusAsync(string? userId, string? targetUserId);
    Task<bool> IsFollowingAsync(string? followerId, string? followingId);
    
    Task<PagedResult<FeedActivityDto>> GetUserFeedAsync(FeedPaginationDto? dto);
}

