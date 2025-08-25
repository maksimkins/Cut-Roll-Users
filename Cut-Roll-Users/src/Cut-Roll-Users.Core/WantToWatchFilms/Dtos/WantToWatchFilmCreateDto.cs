namespace Cut_Roll_Users.Core.WantToWatchFilms.Dtos;

public class WantToWatchFilmDto
{
    public required string UserId { get; set; }
    public required Guid MovieId { get; set; }
}
