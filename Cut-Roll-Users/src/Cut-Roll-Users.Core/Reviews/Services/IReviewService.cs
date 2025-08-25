using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Reviews.Dtos;

namespace Cut_Roll_Users.Core.Reviews.Services;

public interface IReviewService
{
    Task<Guid> CreateReviewAsync(ReviewCreateDto? reviewCreateDto);
    Task<ReviewResponseDto?> GetReviewByIdAsync(Guid? reviewId);
    Task<Guid> UpdateReviewAsync(ReviewUpdateDto? reviewUpdateDto);
    Task<Guid> DeleteReviewByIdAsync(Guid? reviewId);
    Task<ReviewResponseDto?> GetReviewByUserAndMovieAsync(string? userId, Guid? movieId);
    Task<PagedResult<ReviewResponseDto>> GetReviewsByMovieIdAsync(ReviewPaginationMovieDto dto);
    Task<PagedResult<ReviewResponseDto>> GetReviewsByUserIdAsync(ReviewPaginationUserDto dto);
    Task<double> GetAverageRatingByMovieIdAsync(Guid? movieId);
    Task<int> GetReviewCountByMovieIdAsync(Guid? movieId);
}
