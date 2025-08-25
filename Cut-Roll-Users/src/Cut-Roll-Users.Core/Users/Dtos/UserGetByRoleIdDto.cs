namespace Cut_Roll_Users.Core.Users.Dtos;

public class UserGetByRoleIdDto
{
    public required string RoleId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
