namespace Cut_Roll_Users.Core.MovieImages.Dtos;

public class MovieImageCreateDto
{
    public Guid MovieId { get; set; }

    public required string Type { get; set; }
    
    public required string FilePath { get; set; }
}
