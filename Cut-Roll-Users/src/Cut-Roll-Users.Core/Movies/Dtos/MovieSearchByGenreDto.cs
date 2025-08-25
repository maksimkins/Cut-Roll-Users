namespace Cut_Roll_Users.Core.Movies.Dtos;

public class MovieSearchByGenreDto
{
    public Guid? GenreId { get; set; }
    public string? Name { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
