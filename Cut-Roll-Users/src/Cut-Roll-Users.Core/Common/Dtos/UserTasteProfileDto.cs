namespace Cut_Roll_Users.Core.Common.Dtos;

/// <summary>
/// DTO representing a user's taste profile
/// 
/// Note: This is generated on-demand from user's movie interactions.
/// No persistent storage is required - the profile is computed dynamically.
/// </summary>
public class UserTasteProfileDto
{
    public string UserId { get; set; } = string.Empty;
    public List<string> PreferredGenres { get; set; } = new();
    public List<string> PreferredKeywords { get; set; } = new();
    public List<string> PreferredActors { get; set; } = new();
    public List<string> PreferredDirectors { get; set; } = new();
    public double AverageRating { get; set; }
    public int TotalMoviesWatched { get; set; }
    public int TotalMoviesLiked { get; set; }
    public DateTime LastUpdated { get; set; }
    public List<float>? TasteVector { get; set; }
}
