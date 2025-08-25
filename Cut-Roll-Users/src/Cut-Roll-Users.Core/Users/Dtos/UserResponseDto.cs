namespace Cut_Roll_Users.Core.Users.Dtos;

public class UserResponseDto
{
    public required string Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public bool IsBanned { get; set; }
    public bool IsMuted { get; set; }
    public string? AvatarPath { get; set; }
}