namespace Cut_Roll_Users.Core.Comments.Dtos;

public class CommentDeleteDto
{
    public required string UserId { get; set; }
    public required Guid ReviewId { get; set; }
}
