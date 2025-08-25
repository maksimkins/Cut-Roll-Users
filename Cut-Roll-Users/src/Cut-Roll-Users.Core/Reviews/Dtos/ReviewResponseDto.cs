using Cut_Roll_Users.Core.Users.Dtos;

namespace Cut_Roll_Users.Core.Reviews.Dtos;

public class ReviewResponseDto
{
    public required Guid Id { get; set; }
    public required UserSimlified UserSimlified { get; set; }
    public required Guid MovieId { get; set; }
    public required string Content { get; set; }
    public int Rating { get; set; }
    public DateTime CreatedAt { get; set; }
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
}
