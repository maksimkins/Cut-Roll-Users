namespace Cut_Roll_Users.Core.Follows.Dtos;

public class FollowCreateDto
{
    public required string FollowerId { get; set; }
    public required string FollowingId { get; set; }
}
