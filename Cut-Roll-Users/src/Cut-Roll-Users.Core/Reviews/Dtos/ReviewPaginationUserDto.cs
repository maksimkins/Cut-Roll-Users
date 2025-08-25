
namespace Cut_Roll_Users.Core.Reviews.Dtos;

public class ReviewPaginationUserDto
{
    public required Guid UserId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
