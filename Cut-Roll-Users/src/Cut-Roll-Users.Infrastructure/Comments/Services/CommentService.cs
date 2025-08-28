using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Comments.Dtos;
using Cut_Roll_Users.Core.Comments.Repositories;
using Cut_Roll_Users.Core.Comments.Services;

namespace Cut_Roll_Users.Infrastructure.Comments.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;

    public CommentService(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<Guid> CreateCommentAsync(CommentCreateDto? commentDto)
    {
        if (commentDto == null)
            throw new ArgumentNullException(nameof(commentDto), "Comment creation data cannot be null.");
        
        if (string.IsNullOrWhiteSpace(commentDto.UserId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(commentDto.UserId));
        
        if (commentDto.ReviewId == Guid.Empty)
            throw new ArgumentException("Review ID cannot be empty.", nameof(commentDto.ReviewId));
        
        if (string.IsNullOrWhiteSpace(commentDto.Content))
            throw new ArgumentException("Comment content cannot be null or empty.", nameof(commentDto.Content));

        var result = await _commentRepository.CreateAsync(commentDto);
        return result ?? throw new InvalidOperationException("Failed to create comment.");
    }

    public async Task<Guid> UpdateCommentAsync(CommentUpdateDto? commentDto)
    {
        if (commentDto == null)
            throw new ArgumentNullException(nameof(commentDto), "Comment update data cannot be null.");
        
        if (string.IsNullOrWhiteSpace(commentDto.UserId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(commentDto.UserId));
        
        if (commentDto.ReviewId == Guid.Empty)
            throw new ArgumentException("Review ID cannot be empty.", nameof(commentDto.ReviewId));
        
        if (string.IsNullOrWhiteSpace(commentDto.Content))
            throw new ArgumentException("Comment content cannot be null or empty.", nameof(commentDto.Content));

        var result = await _commentRepository.UpdateAsync(commentDto);
        if (result == null)
            throw new InvalidOperationException($"Comment not found for UserId: {commentDto.UserId} and ReviewId: {commentDto.ReviewId}");
        
        return result.Value;
    }

    public async Task<Guid> DeleteCommentAsync(CommentDeleteDto? commentDto)
    {
        if (commentDto == null)
            throw new ArgumentNullException(nameof(commentDto), "Comment deletion data cannot be null.");
        
        if (string.IsNullOrWhiteSpace(commentDto.UserId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(commentDto.UserId));
        
        if (commentDto.ReviewId == Guid.Empty)
            throw new ArgumentException("Review ID cannot be empty.", nameof(commentDto.ReviewId));


        var result = await _commentRepository.DeleteAsync(commentDto);
        if (result == null)
            throw new InvalidOperationException($"Comment not found for UserId: {commentDto.UserId} and ReviewId: {commentDto.ReviewId}");
        
        return result.Value;
    }

    public async Task<PagedResult<CommentResponseDto>> GetByReviewIdAsync(CommentPaginationReviewDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "Comment pagination data cannot be null.");
        
        if (dto.ReviewId == Guid.Empty)
            throw new ArgumentException("Review ID cannot be empty.", nameof(dto.ReviewId));

        return await _commentRepository.GetByReviewIdAsync(dto);
    }

    public async Task<PagedResult<CommentResponseDto>> GetByUserIdAsync(CommentPaginationUserDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "Comment pagination data cannot be null.");
        
        if (string.IsNullOrWhiteSpace(dto.UserId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(dto.UserId));

        return await _commentRepository.GetByUserIdAsync(dto);
    }

    public async Task<int> GetCommentCountByReviewIdAsync(Guid? reviewId)
    {
        if (!reviewId.HasValue || reviewId.Value == Guid.Empty)
            throw new ArgumentException("Review ID cannot be null or empty.", nameof(reviewId));

        return await _commentRepository.GetCommentCountByReviewIdAsync(reviewId.Value);
    }
}
