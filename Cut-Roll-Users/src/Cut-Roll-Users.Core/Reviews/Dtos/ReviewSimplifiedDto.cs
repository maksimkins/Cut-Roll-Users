using Cut_Roll_Users.Core.Movies.Dtos;
using Cut_Roll_Users.Core.Users.Dtos;

namespace Cut_Roll_Users.Core.Reviews.Dtos;

public class ReviewSimplifiedDto
{
    public Guid Id { get; set; }
    public MovieSimplifiedDto MovieSimplified { get; set; } = null!;
    public string Content { get; set; } = null!;
    public float Rating { get; set; }
    public DateTime CreatedAt { get; set; }
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
    public UserSimplified User { get; set; } = null!;
}
