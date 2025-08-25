namespace Cut_Roll_Users.Core.Users.Dtos;
public class UserSearchDto
{
    public string? SearchTerm { get; set; }
    public bool? IsBanned { get; set; }
    public bool? IsMuted { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}