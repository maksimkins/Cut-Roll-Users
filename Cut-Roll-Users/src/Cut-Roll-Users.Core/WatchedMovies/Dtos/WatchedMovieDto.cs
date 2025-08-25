namespace Cut_Roll_Users.Core.WatchedMovies.Dtos;

public class WatchedMovieDto
{
    public required string UserId { get; set; }
    public required Guid MovieId { get; set; }
}
