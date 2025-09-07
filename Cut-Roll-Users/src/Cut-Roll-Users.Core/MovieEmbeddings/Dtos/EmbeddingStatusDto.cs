namespace Cut_Roll_Users.Core.MovieEmbeddings.Dtos;

public class EmbeddingStatusDto
{
    public bool IsVectorDbEmpty { get; set; }
    public int TotalMoviesInDatabase { get; set; }
    public int TotalEmbeddingsInVectorDb { get; set; }
    public bool IsProcessing { get; set; }
    public DateTime? LastProcessedAt { get; set; }
    public string Status { get; set; } = string.Empty; 
}