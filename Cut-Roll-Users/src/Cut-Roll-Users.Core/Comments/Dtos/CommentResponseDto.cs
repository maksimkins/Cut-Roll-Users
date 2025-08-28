using Cut_Roll_Users.Core.Users.Dtos;

namespace Cut_Roll_Users.Core.Comments.Dtos;

public class CommentResponseDto
{
    public required UserSimplified UserSimplified { get; set; }
    public required Guid ReviewId { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAt { get; set; }
}
