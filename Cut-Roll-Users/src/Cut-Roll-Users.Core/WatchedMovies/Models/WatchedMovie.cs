using Cut_Roll_Users.Core.Movies.Models;
using Cut_Roll_Users.Core.Users.Models;

namespace Cut_Roll_Users.Core.WatchedMovies.Models;
public class WatchedMovie
{
    public required string UserId { get; set; }
    public Guid MovieId { get; set; }
    public DateTime WatchedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public Movie Movie { get; set; } = null!;
}