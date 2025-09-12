namespace Cut_Roll_Users.Core.Common.Dtos;

/// <summary>
/// DTO explaining why a movie was recommended to a user
/// </summary>
public class RecommendationExplanationDto
{
    public string UserId { get; set; } = string.Empty;
    public Guid MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public double SimilarityScore { get; set; }
    public List<string> Reasons { get; set; } = new();
    public List<string> SimilarMovies { get; set; } = new();
    public List<string> MatchingGenres { get; set; } = new();
    public List<string> MatchingKeywords { get; set; } = new();
    public string Explanation { get; set; } = string.Empty;
}
