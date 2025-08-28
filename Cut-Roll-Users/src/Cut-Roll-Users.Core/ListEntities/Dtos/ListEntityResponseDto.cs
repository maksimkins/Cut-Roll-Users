using Cut_Roll_Users.Core.Users.Dtos;

namespace Cut_Roll_Users.Core.ListEntities.Dtos;

public class ListEntityResponseDto
{
    public required Guid Id { get; set; }
    public required UserSimplified UserSimplified { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public int MoviesCount { get; set; }
    public int LikesCount { get; set; }
}
