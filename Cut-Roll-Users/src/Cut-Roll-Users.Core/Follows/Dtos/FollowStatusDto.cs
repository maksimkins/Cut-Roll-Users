namespace Cut_Roll_Users.Core.Follows.Dtos;

public class FollowStatusDto
{
    public required string UserId { get; set; }
    public required string TargetUserId { get; set; }
    public bool IsFollowing { get; set; }
    public bool IsFollowedBy { get; set; }
    public bool IsMutualFollow { get; set; }
}
