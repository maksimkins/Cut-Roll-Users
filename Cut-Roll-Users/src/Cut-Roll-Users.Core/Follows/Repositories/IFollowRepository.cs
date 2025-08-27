using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Common.Repositories.Base;
using Cut_Roll_Users.Core.Follows.Dtos;
using Cut_Roll_Users.Core.Follows.Models;

namespace Cut_Roll_Users.Core.Follows.Repositories;

public interface IFollowRepository :
    ICreateAsync<FollowCreateDto, string?>
{
    Task<bool> FollowExistsAsync(string followerId, string followingId);
    Task<string?> DeleteFollowAsync(FollowDeleteDto dto);
    Task<PagedResult<FollowResponseDto>> GetUserFollowsAsync(FollowPaginationDto dto);
    Task<int> GetFollowersCountAsync(string userId);
    Task<int> GetFollowingCountAsync(string userId);
    Task<FollowStatusDto> GetFollowStatusAsync(string userId, string targetUserId);
    Task<IQueryable<Follow>> GetFollowsAsQueryableAsync();
    
    Task<PagedResult<FeedActivityDto>> GetUserFeedAsync(FeedPaginationDto dto);
    Task<List<string>> GetFollowingUserIdsAsync(string userId);
}

