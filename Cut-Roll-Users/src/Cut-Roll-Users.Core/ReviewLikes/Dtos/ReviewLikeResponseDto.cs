using Cut_Roll_Users.Core.Users.Dtos;

namespace Cut_Roll_Users.Core.ReviewLikes.Dtos;

public class ReviewLikeResponseDto
{
    public Guid ReviewId { get; set; }
    public DateTime LikedAt { get; set; }
    public UserSimplified User { get; set; } = null!;
}
