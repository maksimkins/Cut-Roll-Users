namespace Cut_Roll_Users.Core.Movies.Dtos;

public class MovieUpdateDto
{
    public required Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Tagline { get; set; }
    public string? Overview { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public int? Runtime { get; set; }
    public int? Budget { get; set; }
    public int? Revenue { get; set; }
    public string? Homepage { get; set; }
    public string? ImdbId { get; set; }
}
