using Cut_Roll_Users.Core.Common.Dtos;

namespace Cut_Roll_Users.Core.Follows.Dtos;

public class FeedPaginationDto
{
    public required string UserId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public ActivityType? FilterByType { get; set; }
    public DateTime? FromDate { get; set; }
}
