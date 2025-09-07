namespace Cut_Roll_Users.Core.MovieEmbeddings.Dtos;

public class MovieRecommendationDto
{
    public Guid MovieId { get; set; }
    public required string Title { get; set; }
    public double SimilarityScore { get; set; }  
    public string? PosterPath { get; set; }
}
