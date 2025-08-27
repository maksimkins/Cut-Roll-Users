#pragma warning disable CS8618 

using System.ComponentModel.DataAnnotations;
using Cut_Roll_Users.Core.Users.Models;

namespace Cut_Roll_Users.Core.Follows.Models;

public class Follow
{
    [Key]
    public Guid Id { get; set; }

    public required string FollowerId { get; set; }
    public User Follower { get; set; } = null!;

    public required string FollowingId { get; set; }
    public User Following { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
