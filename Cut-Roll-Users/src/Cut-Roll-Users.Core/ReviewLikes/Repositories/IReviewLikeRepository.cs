using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Common.Repositories.Base;
using Cut_Roll_Users.Core.ReviewLikes.Dtos;
using Cut_Roll_Users.Core.Reviews.Dtos;
using Cut_Roll_Users.Core.Reviews.Models;

namespace Cut_Roll_Users.Core.ReviewLikes.Repositories;

public interface IReviewLikeRepository :
    ICreateAsync<ReviewLikeDto, Guid>,
    IDeleteAsync<ReviewLikeDto, Guid>
{
    Task<bool> IsLikedByUserAsync(string userId, Guid reviewId);
    Task<int> GetLikeCountByReviewIdAsync(Guid reviewId);
    Task<PagedResult<Review>> GetLikedReviewsByUserIdAsync(ReviewPaginationUserDto dto);
}
