namespace Cut_Roll_Users.Core.Common.Dtos;

/// <summary>
/// DTO for friend recommendation results
/// </summary>
public class FriendRecommendationDto
{
    public Guid MovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? PosterPath { get; set; }
    public double SimilarityScore { get; set; }
    public string RecommendationReason { get; set; } = string.Empty;
    public List<string> MatchingGenres { get; set; } = new();
    public List<string> MatchingKeywords { get; set; } = new();
    public double? VoteAverage { get; set; }
    public int? VoteCount { get; set; }
    public string? Overview { get; set; }
    public DateTime? ReleaseDate { get; set; }
}

