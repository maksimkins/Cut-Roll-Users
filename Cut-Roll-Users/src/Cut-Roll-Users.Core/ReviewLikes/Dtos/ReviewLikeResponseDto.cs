namespace Cut_Roll_Users.Core.ReviewLikes.Dtos;

public class ReviewLikeResponseDto
{
    public required string UserId { get; set; }
    public required Guid ReviewId { get; set; }
    public DateTime LikedAt { get; set; }
}
