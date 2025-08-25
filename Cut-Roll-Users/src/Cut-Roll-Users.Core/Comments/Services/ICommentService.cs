using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Comments.Dtos;

namespace Cut_Roll_Users.Core.Comments.Services;

public interface ICommentService
{
    Task<bool> CreateCommentAsync(CommentCreateDto? commentDto);
    Task<bool> UpdateCommentAsync(CommentUpdateDto? commentDto);
    Task<bool> DeleteCommentAsync(CommentUpdateDto? commentDto);
    Task<PagedResult<CommentResponseDto>> GetByReviewIdAsync(CommentPaginationReviewDto dto);
    Task<PagedResult<CommentResponseDto>> GetByUserIdAsync(CommentPaginationUserDto dto);
    Task<int> GetCommentCountByReviewIdAsync(Guid? reviewId);
}
