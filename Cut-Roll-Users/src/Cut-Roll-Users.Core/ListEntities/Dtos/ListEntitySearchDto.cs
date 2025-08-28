namespace Cut_Roll_Users.Core.ListEntities.Dtos;

public class ListEntitySearchDto
{
    public string? UserId { get; set; }
    public string? Title { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public bool? SortByLikesAscending { get; set; }
}
