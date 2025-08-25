namespace Cut_Roll_Users.Core.ListEntities.Dtos;

public class ListEntityCreateDto
{
    public required string UserId { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
}
