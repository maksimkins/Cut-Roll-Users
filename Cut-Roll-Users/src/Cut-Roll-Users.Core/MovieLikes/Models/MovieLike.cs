using Cut_Roll_Users.Core.Movies.Models;
using Cut_Roll_Users.Core.Users.Models;

namespace Cut_Roll_Users.Core.MovieLikes.Models;

public class MovieLike
{
    public required string UserId { get; set; }
    public required Guid MovieId { get; set; }
    public DateTime LikedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public Movie Movie { get; set; } = null!;
}
