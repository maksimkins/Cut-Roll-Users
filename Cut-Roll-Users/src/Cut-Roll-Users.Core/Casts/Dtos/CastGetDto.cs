namespace Cut_Roll_Users.Core.Casts.Dtos;

using Cut_Roll_Users.Core.MovieImages.Models;
using Cut_Roll_Users.Core.People.Models;

public class CastGetDto
{
    public required Person Person { get; set; }
    public string? Character { get; set; }
    public int CastOrder { get; set; }
    public Guid MovieId { get; set; }
    public required string Title { get; set; }
    public MovieImage? Poster { get; set; }
}
