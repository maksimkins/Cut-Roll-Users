using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.ReviewLikes.Dtos;
using Cut_Roll_Users.Core.Reviews.Dtos;
using Cut_Roll_Users.Core.Reviews.Models;

namespace Cut_Roll_Users.Core.ReviewLikes.Services;

public interface IReviewLikeService
{
    Task<bool> LikeReviewAsync(ReviewLikeDto? likeDto);
    Task<bool> UnlikeReviewAsync(ReviewLikeDto? likeDto);
    Task<bool> IsReviewLikedByUserAsync(string? userId, Guid? reviewId);
    Task<int> GetReviewLikeCountAsync(Guid? reviewId);
    Task<PagedResult<Review>> GetLikedReviewsByUserIdAsync(ReviewPaginationUserDto dto);
}
