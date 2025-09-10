namespace Cut_Roll_Users.Core.MovieEmbeddings.Dtos;
public class MovieEmbeddingDto
{
    public Guid MovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? PosterPath { get; set; }
    public List<float> Embedding { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}