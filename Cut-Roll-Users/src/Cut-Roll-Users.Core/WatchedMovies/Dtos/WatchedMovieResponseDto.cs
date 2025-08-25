using Cut_Roll_Users.Core.Movies.Dtos;

namespace Cut_Roll_Users.Core.WatchedMovies.Dtos;

public class WatchedMovieResponseDto
{
    public required Guid Id { get; set; }
    public required string UserId { get; set; }
    public required MovieSimplifiedDto Movie { get; set; }
    public DateTime WatchedAt { get; set; }
}
