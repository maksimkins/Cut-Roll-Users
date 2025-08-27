using Cut_Roll_Users.Core.Users.Dtos;

namespace Cut_Roll_Users.Core.Follows.Dtos;

public class FeedActivityDto
{
    public Guid Id { get; set; }
    public UserResponseDto User { get; set; } = null!;
    public ActivityType Type { get; set; }
    public string? MovieTitle { get; set; }
    public string? MovieId { get; set; }
    public string? ReviewContent { get; set; }
    public double? Rating { get; set; }
    public string? ListName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum ActivityType
{
    MovieLike,
    MovieReview,
    MovieWatched,
    WantToWatch,
    ListCreated,
    ListLiked,
    ReviewLiked
}
