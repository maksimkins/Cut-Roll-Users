using Cut_Roll_Users.Core.Users.Dtos;

namespace Cut_Roll_Users.Core.Reviews.Dtos;

public class ReviewSimplifiedDto
{
    public Guid Id { get; set; }
    public Guid MovieId { get; set; }
    public string Content { get; set; } = null!;
    public int Rating { get; set; }
    public DateTime CreatedAt { get; set; }
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
    public UserSimplified User { get; set; } = null!;
}
