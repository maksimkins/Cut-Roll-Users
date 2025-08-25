namespace Cut_Roll_Users.Core.Comments.Dtos;

public class CommentPaginationReviewDto
{
    public required Guid ReviewId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
