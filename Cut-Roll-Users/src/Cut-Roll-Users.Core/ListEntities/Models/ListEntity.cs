using Cut_Roll_Users.Core.ListLikes.Models;
using Cut_Roll_Users.Core.ListMovies.Models;
using Cut_Roll_Users.Core.Users.Models;

namespace Cut_Roll_Users.Core.ListEntities.Models
{
    public class ListEntity
    {
        public Guid Id { get; set; }
        public required string UserId { get; set; }

        public required string Title { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
        public ICollection<ListMovie> Movies { get; set; } = [];
        public ICollection<ListLike> Likes { get; set; } = [];
}
}