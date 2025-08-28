using Cut_Roll_Users.Core.Users.Dtos;

namespace Cut_Roll_Users.Core.ListLikes.Dtos;

public class ListLikeResponseDto
{
    public Guid ListId { get; set; }
    public DateTime LikedAt { get; set; }
    public UserSimplified User { get; set; } = null!;
}
