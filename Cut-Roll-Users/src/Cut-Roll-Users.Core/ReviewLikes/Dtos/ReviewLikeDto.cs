namespace Cut_Roll_Users.Core.ReviewLikes.Dtos;

public class ReviewLikeDto
{
    public required string UserId { get; set; }
    public required Guid ReviewId { get; set; }
}
