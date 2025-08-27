using Cut_Roll_Users.Core.Users.Dtos;

namespace Cut_Roll_Users.Core.Follows.Dtos;

public class FollowResponseDto
{
    public UserResponseDto Follower { get; set; } = null!;
    public UserResponseDto Following { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
