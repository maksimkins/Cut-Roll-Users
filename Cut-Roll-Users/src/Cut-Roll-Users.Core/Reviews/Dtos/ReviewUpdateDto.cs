namespace Cut_Roll_Users.Core.Reviews.Dtos;

public class ReviewUpdateDto
{
    public required Guid Id { get; set; }
    public required string UserId { get; set; }
    public string? Content { get; set; }
    public int? Rating { get; set; }
}
