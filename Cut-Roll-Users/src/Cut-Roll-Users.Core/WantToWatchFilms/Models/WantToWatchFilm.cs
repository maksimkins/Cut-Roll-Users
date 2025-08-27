using Cut_Roll_Users.Core.Movies.Models;
using Cut_Roll_Users.Core.Users.Models;

namespace Cut_Roll_Users.Core.WantToWatchFilms.Models;

public class WantToWatchFilm
{
    public required string UserId { get; set; }
    public required Guid MovieId { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Movie Movie { get; set; } = null!;
}
