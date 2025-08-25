namespace Cut_Roll_Users.Core.ListEntities.Dtos;

public class ListEntityPaginationDto
{
    public required string UserId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
