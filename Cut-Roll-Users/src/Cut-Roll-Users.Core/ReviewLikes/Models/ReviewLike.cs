
using Cut_Roll_Users.Core.Reviews.Models;
using Cut_Roll_Users.Core.Users.Models;

namespace Cut_Roll_Users.Core.ReviewLikes.Models;
public class ReviewLike
{
    public required string UserId { get; set; }
    public Guid ReviewId { get; set; }

    public DateTime LikedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public Review Review { get; set; } = null!;
}