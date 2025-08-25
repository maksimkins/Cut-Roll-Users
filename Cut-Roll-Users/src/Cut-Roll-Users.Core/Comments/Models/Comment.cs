using Cut_Roll_Users.Core.Reviews.Models;
using Cut_Roll_Users.Core.Users.Models;

namespace Cut_Roll_Users.Core.Comments.Models;
public class Comment
{
    public required string UserId { get; set; }
    public Guid ReviewId { get; set; }

    public required string Content { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public Review Review { get; set; } = null!;
}