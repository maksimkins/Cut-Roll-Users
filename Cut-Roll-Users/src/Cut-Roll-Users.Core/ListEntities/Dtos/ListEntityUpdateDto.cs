namespace Cut_Roll_Users.Core.ListEntities.Dtos;

public class ListEntityUpdateDto
{
    public required Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
}
