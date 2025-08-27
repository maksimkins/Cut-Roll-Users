using Cut_Roll_Users.Core.Users.Dtos;

namespace Cut_Roll_Users.Core.ListEntities.Dtos;

public class ListEntitySimplifiedDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public int MoviesCount { get; set; }
    public int LikesCount { get; set; }
    public UserSimlified User { get; set; } = null!;
}
