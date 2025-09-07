namespace Cut_Roll_Users.Core.MovieEmbeddings.Dtos;
public class MovieDataForEmbeddingDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Overview { get; set; }
    public string? Tagline { get; set; }
    public string? OriginalTitle { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public string? PosterPath { get; set; }
    public decimal? Budget { get; set; }
    public decimal? Revenue { get; set; }
    public int? Runtime { get; set; }
    public string? Status { get; set; }
    public string? OriginalLanguage { get; set; }
    public List<string> Genres { get; set; } = new();
    public List<string> Keywords { get; set; } = new();
    public List<string> Cast { get; set; } = new();
    public List<string> Crew { get; set; } = new();
    public List<string> ProductionCompanies { get; set; } = new();
    public List<string> ProductionCountries { get; set; } = new();
    public List<string> SpokenLanguages { get; set; } = new();
}