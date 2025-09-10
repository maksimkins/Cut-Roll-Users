using Cut_Roll_Users.Core.Movies.Dtos;
using Cut_Roll_Users.Core.Users.Dtos;

namespace Cut_Roll_Users.Core.Reviews.Dtos;

public class ReviewResponseDto
{
    public required Guid Id { get; set; }
    public required UserSimplified UserSimplified { get; set; }
    public MovieSimplifiedDto MovieSimplified { get; set; } = null!;
    public required string Content { get; set; }
    public float Rating { get; set; }
    public DateTime CreatedAt { get; set; }
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
}
