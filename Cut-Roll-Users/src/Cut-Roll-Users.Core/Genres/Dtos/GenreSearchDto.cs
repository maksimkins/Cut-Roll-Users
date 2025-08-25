namespace Cut_Roll_Users.Core.Genres.Dtos;

public class GenreSearchDto
{
    public required string Name { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
