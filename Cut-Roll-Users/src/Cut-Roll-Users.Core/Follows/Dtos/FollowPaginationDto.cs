using Cut_Roll_Users.Core.Common.Dtos;

namespace Cut_Roll_Users.Core.Follows.Dtos;

public class FollowPaginationDto
{
    public required string UserId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public FollowType Type { get; set; } = FollowType.Following;
}

public enum FollowType
{
    Followers,
    Following
}
