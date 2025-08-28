using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Common.Repositories.Base;
using Cut_Roll_Users.Core.Reviews.Dtos;

namespace Cut_Roll_Users.Core.Reviews.Repositories;

public interface IReviewRepository :
    ICreateAsync<ReviewCreateDto, Guid?>,
    IGetByIdAsync<Guid, ReviewResponseDto?>,
    IUpdateAsync<ReviewUpdateDto, Guid?>,
    IDeleteByIdAsync<Guid, Guid?>
{
    Task<ReviewResponseDto?> GetByUserAndMovieAsync(string userId, Guid movieId);
    Task<PagedResult<ReviewResponseDto>> GetByMovieIdAsync(ReviewPaginationMovieDto dto);
    Task<PagedResult<ReviewResponseDto>> GetByUserIdAsync(ReviewPaginationUserDto dto);
    Task<double> GetAverageRatingByMovieIdAsync(Guid movieId);
    Task<int> GetReviewCountByMovieIdAsync(Guid movieId);
    Task<bool> IsReviewOwnedByUserAsync(Guid reviewId, string userId);
}
