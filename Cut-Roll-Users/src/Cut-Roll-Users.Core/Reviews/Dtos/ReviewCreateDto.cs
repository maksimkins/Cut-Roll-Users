namespace Cut_Roll_Users.Core.Reviews.Dtos;

public class ReviewCreateDto
{
    public required string UserId { get; set; }
    public required Guid MovieId { get; set; }
    public required string Content { get; set; }
    public int Rating { get; set; }
}
