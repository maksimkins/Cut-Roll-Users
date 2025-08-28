using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Common.Services;
using Cut_Roll_Users.Core.Reviews.Dtos;
using Cut_Roll_Users.Core.Reviews.Repositories;
using Cut_Roll_Users.Core.Reviews.Services;

namespace Cut_Roll_Users.Infrastructure.Reviews.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    IMessageBrokerService _messageBrokerService;

    public ReviewService(IReviewRepository reviewRepository, IMessageBrokerService messageBrokerService)
    {
        _reviewRepository = reviewRepository;
        _messageBrokerService = messageBrokerService;
    }

    public async Task<Guid> CreateReviewAsync(ReviewCreateDto? reviewCreateDto)
    {
        if (reviewCreateDto == null)
            throw new ArgumentNullException(nameof(reviewCreateDto), "Review create data cannot be null.");
        
        if (string.IsNullOrWhiteSpace(reviewCreateDto.UserId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(reviewCreateDto.UserId));
        
        if (reviewCreateDto.MovieId == Guid.Empty)
            throw new ArgumentException("Movie ID cannot be empty.", nameof(reviewCreateDto.MovieId));
        
        if (string.IsNullOrWhiteSpace(reviewCreateDto.Content))
            throw new ArgumentException("Content cannot be null or empty.", nameof(reviewCreateDto.Content));
        
        if (reviewCreateDto.Rating < 0 || reviewCreateDto.Rating > 5)
            throw new ArgumentException("Rating must be between 0 and 5.", nameof(reviewCreateDto.Rating));

        var result = await _reviewRepository.CreateAsync(reviewCreateDto)
            ?? throw new InvalidOperationException("Failed to create review.");

        var ratingAverage = await _reviewRepository.GetAverageRatingByMovieIdAsync(reviewCreateDto.MovieId);
        await _messageBrokerService.PushAsync("movie_update_rating_news", new
        {
            MovieId = reviewCreateDto.MovieId,
            RatingAverage = ratingAverage,
            
        });
        return result; 
    }

    public async Task<ReviewResponseDto?> GetReviewByIdAsync(Guid? reviewId)
    {
        if (!reviewId.HasValue || reviewId.Value == Guid.Empty)
            throw new ArgumentException("Review ID cannot be null or empty.", nameof(reviewId));

        return await _reviewRepository.GetByIdAsync(reviewId.Value);
    }

    public async Task<Guid> UpdateReviewAsync(ReviewUpdateDto? reviewUpdateDto)
    {
        if (reviewUpdateDto == null)
            throw new ArgumentNullException(nameof(reviewUpdateDto), "Review update data cannot be null.");
        
        if (reviewUpdateDto.Id == Guid.Empty)
            throw new ArgumentException("Review ID cannot be empty.", nameof(reviewUpdateDto.Id));
        
        if (reviewUpdateDto.Rating.HasValue && (reviewUpdateDto.Rating.Value < 0 || reviewUpdateDto.Rating.Value > 5))
            throw new ArgumentException("Rating must be between 0 and 5.", nameof(reviewUpdateDto.Rating));

        var result = await _reviewRepository.UpdateAsync(reviewUpdateDto)
            ?? throw new InvalidOperationException("Failed to create review.");

        var reviewUpdated = await _reviewRepository.GetByIdAsync(reviewUpdateDto.Id)
            ?? throw new Exception("smth went wrong with reviews");

        var ratingAverage = await _reviewRepository.GetAverageRatingByMovieIdAsync(reviewUpdated.MovieId);
        await _messageBrokerService.PushAsync("movie_update_rating_news", new
        {
            MovieId = reviewUpdated.MovieId,
            RatingAverage = ratingAverage,
            
        });
        return result; 
    }

    public async Task<Guid> DeleteReviewByIdAsync(Guid? reviewId)
    {
        if (!reviewId.HasValue || reviewId.Value == Guid.Empty)
            throw new ArgumentException("Review ID cannot be null or empty.", nameof(reviewId));

        var reviewToDelete = await _reviewRepository.GetByIdAsync(reviewId.Value)
            ?? throw new Exception("smth went wrong with reviews");

        var movieId = reviewToDelete.MovieId;
        var ratingAverage = await _reviewRepository.GetAverageRatingByMovieIdAsync(movieId);

        var result = await _reviewRepository.DeleteByIdAsync(reviewId.Value);
        if (result == null)
            throw new InvalidOperationException($"Review not found for ID: {reviewId.Value}");

        await _messageBrokerService.PushAsync("movie_update_rating_news", new
        {
            MovieId = movieId,
            RatingAverage = ratingAverage,
            
        });
        
        return result.Value;
    }

    public async Task<ReviewResponseDto?> GetReviewByUserAndMovieAsync(string? userId, Guid? movieId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
        
        if (!movieId.HasValue || movieId.Value == Guid.Empty)
            throw new ArgumentException("Movie ID cannot be null or empty.", nameof(movieId));

        return await _reviewRepository.GetByUserAndMovieAsync(userId, movieId.Value);
    }

    public async Task<PagedResult<ReviewResponseDto>> GetReviewsByMovieIdAsync(ReviewPaginationMovieDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "Pagination data cannot be null.");
        
        if (dto.MovieId == Guid.Empty)
            throw new ArgumentException("Movie ID cannot be empty.", nameof(dto.MovieId));

        return await _reviewRepository.GetByMovieIdAsync(dto);
    }

    public async Task<PagedResult<ReviewResponseDto>> GetReviewsByUserIdAsync(ReviewPaginationUserDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "Pagination data cannot be null.");
        
        if (string.IsNullOrWhiteSpace(dto.UserId))
            throw new ArgumentException("User ID cannot be empty.", nameof(dto.UserId));

        return await _reviewRepository.GetByUserIdAsync(dto);
    }

    public async Task<double> GetAverageRatingByMovieIdAsync(Guid? movieId)
    {
        if (!movieId.HasValue || movieId.Value == Guid.Empty)
            throw new ArgumentException("Movie ID cannot be null or empty.", nameof(movieId));

        return await _reviewRepository.GetAverageRatingByMovieIdAsync(movieId.Value);
    }

    public async Task<int> GetReviewCountByMovieIdAsync(Guid? movieId)
    {
        if (!movieId.HasValue || movieId.Value == Guid.Empty)
            throw new ArgumentException("Movie ID cannot be null or empty.", nameof(movieId));

        return await _reviewRepository.GetReviewCountByMovieIdAsync(movieId.Value);
    }
}
