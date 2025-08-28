namespace Cut_Roll_Users.Core.ListLikes.Dtos;

public class ListLikedDto
{
    public required string UserId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
