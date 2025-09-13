using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Follows.Dtos;
using Cut_Roll_Users.Core.Follows.Models;
using Cut_Roll_Users.Core.Follows.Repositories;
using Cut_Roll_Users.Core.Follows.Services;
using Cut_Roll_Users.Core.Users.Dtos;

namespace Cut_Roll_Users.Infrastructure.Follows.Services;

public class FollowService : IFollowService
{
    private readonly IFollowRepository _followRepository;

    public FollowService(IFollowRepository followRepository)
    {
        _followRepository = followRepository;
    }

    public async Task<string> CreateFollowAsync(FollowCreateDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "Follow data cannot be null.");
        
        if (string.IsNullOrWhiteSpace(dto.FollowerId))
            throw new ArgumentException("Follower ID cannot be null or empty.", nameof(dto.FollowerId));
        
        if (string.IsNullOrWhiteSpace(dto.FollowingId))
            throw new ArgumentException("Following ID cannot be null or empty.", nameof(dto.FollowingId));

        if (dto.FollowerId == dto.FollowingId)
            throw new ArgumentException("A user cannot follow themselves.", nameof(dto.FollowingId));

        var isAlreadyFollowing = await _followRepository.FollowExistsAsync(dto.FollowerId, dto.FollowingId);
        if (isAlreadyFollowing)
            throw new InvalidOperationException("User is already following this person.");

        var result = await _followRepository.CreateAsync(dto);
        return result ?? throw new InvalidOperationException("Failed to create follow relationship.");
    }



    public async Task<string> DeleteFollowAsync(FollowDeleteDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "Delete follow data cannot be null.");
        
        if (string.IsNullOrWhiteSpace(dto.FollowerId))
            throw new ArgumentException("Follower ID cannot be null or empty.", nameof(dto.FollowerId));
        
        if (string.IsNullOrWhiteSpace(dto.FollowingId))
            throw new ArgumentException("Following ID cannot be null or empty.", nameof(dto.FollowingId));

        var result = await _followRepository.DeleteFollowAsync(dto);
        if (result == null)
            throw new InvalidOperationException($"Follow relationship not found for FollowerId: {dto.FollowerId} and FollowingId: {dto.FollowingId}");
        
        return result;
    }

    public async Task<string> UnfollowUserAsync(string? followerId, string? followingId)
    {
        if (string.IsNullOrWhiteSpace(followerId))
            throw new ArgumentException("Follower ID cannot be null or empty.", nameof(followerId));
        
        if (string.IsNullOrWhiteSpace(followingId))
            throw new ArgumentException("Following ID cannot be null or empty.", nameof(followingId));

        var dto = new FollowDeleteDto
        {
            FollowerId = followerId,
            FollowingId = followingId
        };

        return await DeleteFollowAsync(dto);
    }

    public async Task<PagedResult<UserSimplified>> GetUserFollowsAsync(FollowPaginationDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "Pagination data cannot be null.");
        
        if (string.IsNullOrWhiteSpace(dto.UserId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(dto.UserId));

        return await _followRepository.GetUserFollowsAsync(dto);
    }

    public async Task<int> GetFollowersCountAsync(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

        return await _followRepository.GetFollowersCountAsync(userId);
    }

    public async Task<int> GetFollowingCountAsync(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

        return await _followRepository.GetFollowingCountAsync(userId);
    }

    public async Task<FollowStatusDto> GetFollowStatusAsync(string? userId, string? targetUserId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
        
        if (string.IsNullOrWhiteSpace(targetUserId))
            throw new ArgumentException("Target User ID cannot be null or empty.", nameof(targetUserId));

        return await _followRepository.GetFollowStatusAsync(userId, targetUserId);
    }

    public async Task<bool> IsFollowingAsync(string? followerId, string? followingId)
    {
        if (string.IsNullOrWhiteSpace(followerId))
            throw new ArgumentException("Follower ID cannot be null or empty.", nameof(followerId));
        
        if (string.IsNullOrWhiteSpace(followingId))
            throw new ArgumentException("Following ID cannot be null or empty.", nameof(followingId));

        return await _followRepository.FollowExistsAsync(followerId, followingId);
    }

    public async Task<PagedResult<FeedActivityDto>> GetUserFeedAsync(FeedPaginationDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "Feed pagination data cannot be null.");
        
        if (string.IsNullOrWhiteSpace(dto.UserId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(dto.UserId));

        return await _followRepository.GetUserFeedAsync(dto);
    }

    public async Task<List<string>> GetFollowingUserIdsAsync(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

        return await _followRepository.GetFollowingUserIdsAsync(userId);
    }

    public async Task<bool> IsFollowOwnedByUserAsync(string? followerId, string? followingId)
    {
        if (string.IsNullOrWhiteSpace(followerId))
            throw new ArgumentException("Follower ID cannot be null or empty.", nameof(followerId));
        
        if (string.IsNullOrWhiteSpace(followingId))
            throw new ArgumentException("Following ID cannot be null or empty.", nameof(followingId));

        return await _followRepository.IsFollowOwnedByUserAsync(followerId, followingId);
    }

    public async Task<bool> AreMutualFriendsAsync(string? userId1, string? userId2)
    {
        if (string.IsNullOrWhiteSpace(userId1))
            throw new ArgumentException("User ID 1 cannot be null or empty.", nameof(userId1));
        
        if (string.IsNullOrWhiteSpace(userId2))
            throw new ArgumentException("User ID 2 cannot be null or empty.", nameof(userId2));

        if (userId1 == userId2)
            throw new ArgumentException("User IDs must be different.", nameof(userId2));

        // Check if both users follow each other
        var user1FollowsUser2 = await _followRepository.FollowExistsAsync(userId1, userId2);
        var user2FollowsUser1 = await _followRepository.FollowExistsAsync(userId2, userId1);

        return user1FollowsUser2 && user2FollowsUser1;
    }

    public async Task<IQueryable<Follow>> GetFollowsAsQueryableAsync()
    {
        return await _followRepository.GetFollowsAsQueryableAsync();
    }

}
