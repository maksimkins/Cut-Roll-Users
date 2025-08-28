using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.MovieLikes.Dtos;
using Cut_Roll_Users.Core.MovieLikes.Repositories;
using Cut_Roll_Users.Core.MovieLikes.Services;
using Cut_Roll_Users.Core.Movies.Dtos;

namespace Cut_Roll_Users.Infrastructure.MovieLikes.Services;

public class MovieLikeService : IMovieLikeService
{
    private readonly IMovieLikeRepository _movieLikeRepository;

    public MovieLikeService(IMovieLikeRepository movieLikeRepository)
    {
        _movieLikeRepository = movieLikeRepository;
    }

    public async Task<Guid> LikeMovieAsync(MovieLikeDto? likeDto)
    {
        if (likeDto == null)
            throw new ArgumentNullException(nameof(likeDto), "Movie like data cannot be null.");
        
        if (string.IsNullOrWhiteSpace(likeDto.UserId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(likeDto.UserId));
        
        if (likeDto.MovieId == Guid.Empty)
            throw new ArgumentException("Movie ID cannot be empty.", nameof(likeDto.MovieId));

        
        var isAlreadyLiked = await _movieLikeRepository.IsLikedByUserAsync(likeDto.UserId, likeDto.MovieId);
        if (isAlreadyLiked)
            return likeDto.MovieId; 

        var result = await _movieLikeRepository.CreateAsync(likeDto);
        return result ?? throw new InvalidOperationException("Failed to create movie like.");
    }

    public async Task<Guid> UnlikeMovieAsync(MovieLikeDto? likeDto)
    {
        if (likeDto == null)
            throw new ArgumentNullException(nameof(likeDto), "Movie unlike data cannot be null.");
        
        if (string.IsNullOrWhiteSpace(likeDto.UserId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(likeDto.UserId));
        
        if (likeDto.MovieId == Guid.Empty)
            throw new ArgumentException("Movie ID cannot be empty.", nameof(likeDto.MovieId));

        
        var isLiked = await _movieLikeRepository.IsLikedByUserAsync(likeDto.UserId, likeDto.MovieId);
        if (!isLiked)
            return likeDto.MovieId; 

        var result = await _movieLikeRepository.DeleteAsync(likeDto);
        if (result == null)
            throw new InvalidOperationException($"MovieLike not found for UserId: {likeDto.UserId} and MovieId: {likeDto.MovieId}");
        
        return result.Value;
    }

    public async Task<bool> IsMovieLikedByUserAsync(string? userId, Guid? movieId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
        
        if (!movieId.HasValue || movieId.Value == Guid.Empty)
            throw new ArgumentException("Movie ID cannot be null or empty.", nameof(movieId));

        return await _movieLikeRepository.IsLikedByUserAsync(userId, movieId.Value);
    }

    public async Task<int> GetMovieLikeCountAsync(Guid? movieId)
    {
        if (!movieId.HasValue || movieId.Value == Guid.Empty)
            throw new ArgumentException("Movie ID cannot be null or empty.", nameof(movieId));

        return await _movieLikeRepository.GetLikeCountByMovieIdAsync(movieId.Value);
    }

    public async Task<PagedResult<MovieSimplifiedDto>> GetLikedMoviesByUserIdAsync(MovieLikePaginationUserDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "Pagination data cannot be null.");
        
        if (string.IsNullOrWhiteSpace(dto.UserId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(dto.UserId));

        return await _movieLikeRepository.GetByUserIdAsync(dto);
    }
}
