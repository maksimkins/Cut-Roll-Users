namespace Cut_Roll_Users.Core.Comments.Dtos;

public class CommentUpdateDto
{
    public required string UserId { get; set; }
    public required Guid ReviewId { get; set; }
    public required string Content { get; set; }
}
