

using Cut_Roll_Users.Core.ListEntities.Models;
using Cut_Roll_Users.Core.Users.Models;

namespace Cut_Roll_Users.Core.ListLikes.Models;
public class ListLike
{
    public required string UserId { get; set; }  
    public Guid ListId { get; set; }            
    public DateTime LikedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public ListEntity List { get; set; } = null!;
}