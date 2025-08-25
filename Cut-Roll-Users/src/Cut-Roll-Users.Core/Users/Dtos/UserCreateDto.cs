namespace Cut_Roll_Users.Core.Users.Dtos;

public class UserCreateDto
{
    public string Id { get; set; } = null!;
    public string RoleId { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? AvatarPath { get; set; }
}