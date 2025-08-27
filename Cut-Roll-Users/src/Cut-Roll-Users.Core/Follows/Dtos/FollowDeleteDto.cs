namespace Cut_Roll_Users.Core.Follows.Dtos;

public class FollowDeleteDto
{
    public required string FollowerId { get; set; }
    public required string FollowingId { get; set; }
}
