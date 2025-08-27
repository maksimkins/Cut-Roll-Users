using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.ReviewLikes.Dtos;
using Cut_Roll_Users.Core.ReviewLikes.Repositories;
using Cut_Roll_Users.Core.ReviewLikes.Services;
using Cut_Roll_Users.Core.Reviews.Dtos;
using Cut_Roll_Users.Core.Reviews.Models;

namespace Cut_Roll_Users.Infrastructure.ReviewLikes.Services;

public class ReviewLikeService : IReviewLikeService
{
    private readonly IReviewLikeRepository _reviewLikeRepository;

    public ReviewLikeService(IReviewLikeRepository reviewLikeRepository)
    {
        _reviewLikeRepository = reviewLikeRepository;
    }

    public async Task<Guid> LikeReviewAsync(ReviewLikeDto? likeDto)
    {
        if (likeDto == null)
            throw new ArgumentNullException(nameof(likeDto), "Review like data cannot be null.");
        
        if (string.IsNullOrWhiteSpace(likeDto.UserId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(likeDto.UserId));
        
        if (likeDto.ReviewId == Guid.Empty)
            throw new ArgumentException("Review ID cannot be empty.", nameof(likeDto.ReviewId));

        // Check if already liked
        var isAlreadyLiked = await _reviewLikeRepository.IsLikedByUserAsync(likeDto.UserId, likeDto.ReviewId);
        if (isAlreadyLiked)
            return likeDto.ReviewId; // Already liked, return success

        var result = await _reviewLikeRepository.CreateAsync(likeDto);
        return result ?? throw new InvalidOperationException("Failed to create review like.");
    }

    public async Task<Guid> UnlikeReviewAsync(ReviewLikeDto? likeDto)
    {
        if (likeDto == null)
            throw new ArgumentNullException(nameof(likeDto), "Review unlike data cannot be null.");
        
        if (string.IsNullOrWhiteSpace(likeDto.UserId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(likeDto.UserId));
        
        if (likeDto.ReviewId == Guid.Empty)
            throw new ArgumentException("Review ID cannot be empty.", nameof(likeDto.ReviewId));

        // Check if not liked
        var isLiked = await _reviewLikeRepository.IsLikedByUserAsync(likeDto.UserId, likeDto.ReviewId);
        if (!isLiked)
            return likeDto.ReviewId; // Not liked, return success

        var result = await _reviewLikeRepository.DeleteAsync(likeDto);
        if (result == null)
            throw new InvalidOperationException($"ReviewLike not found for UserId: {likeDto.UserId} and ReviewId: {likeDto.ReviewId}");
        
        return result.Value;
    }

    public async Task<bool> IsReviewLikedByUserAsync(string? userId, Guid? reviewId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
        
        if (!reviewId.HasValue || reviewId.Value == Guid.Empty)
            throw new ArgumentException("Review ID cannot be null or empty.", nameof(reviewId));

        return await _reviewLikeRepository.IsLikedByUserAsync(userId, reviewId.Value);
    }

    public async Task<int> GetReviewLikeCountAsync(Guid? reviewId)
    {
        if (!reviewId.HasValue || reviewId.Value == Guid.Empty)
            throw new ArgumentException("Review ID cannot be null or empty.", nameof(reviewId));

        return await _reviewLikeRepository.GetLikeCountByReviewIdAsync(reviewId.Value);
    }

    public async Task<PagedResult<Review>> GetLikedReviewsByUserIdAsync(ReviewPaginationUserDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "Pagination data cannot be null.");
        
        if (string.IsNullOrWhiteSpace(dto.UserId))
            throw new ArgumentException("User ID cannot be empty.", nameof(dto.UserId));

        return await _reviewLikeRepository.GetLikedReviewsByUserIdAsync(dto);
    }
}
