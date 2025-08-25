using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Common.Repositories.Base;
using Cut_Roll_Users.Core.Comments.Dtos;

namespace Cut_Roll_Users.Core.Comments.Repositories;

public interface ICommentRepository :
    ICreateAsync<CommentCreateDto, Guid?>,
    IUpdateAsync<CommentUpdateDto, Guid?>,
    IDeleteAsync<CommentDeleteDto, Guid?>
{
    Task<PagedResult<CommentResponseDto>> GetByReviewIdAsync(CommentPaginationReviewDto dto);
    Task<PagedResult<CommentResponseDto>> GetByUserIdAsync(CommentPaginationUserDto dto);
    Task<int> GetCommentCountByReviewIdAsync(Guid reviewId);
}
