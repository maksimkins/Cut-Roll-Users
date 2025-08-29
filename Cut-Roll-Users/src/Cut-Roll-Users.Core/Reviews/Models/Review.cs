using Cut_Roll_Users.Core.Comments.Models;
using Cut_Roll_Users.Core.Movies.Models;
using Cut_Roll_Users.Core.ReviewLikes.Models;
using Cut_Roll_Users.Core.Users.Models;

namespace Cut_Roll_Users.Core.Reviews.Models;
public class Review
{
    public Guid Id { get; set; }
    public required string UserId { get; set; }
    public Guid MovieId { get; set; }

    public required string Content { get; set; }
    public float Rating { get; set; } // 0-5
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Movie Movie { get; set; } = null!;
    public ICollection<ReviewLike> Likes { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
}