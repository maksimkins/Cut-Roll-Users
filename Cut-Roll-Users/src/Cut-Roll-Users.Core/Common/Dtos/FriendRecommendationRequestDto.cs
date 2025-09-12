namespace Cut_Roll_Users.Core.Common.Dtos;

/// <summary>
/// DTO for friend recommendation requests
/// </summary>
public class FriendRecommendationRequestDto
{
    public string UserId1 { get; set; } = string.Empty;
    public string UserId2 { get; set; } = string.Empty;
    public int Limit { get; set; } = 10;
    public List<string>? PreferredGenres { get; set; }
    public int? MinRating { get; set; }
    public int? MinVoteCount { get; set; }
    public DateTime? MinReleaseDate { get; set; }
    public DateTime? MaxReleaseDate { get; set; }
}
