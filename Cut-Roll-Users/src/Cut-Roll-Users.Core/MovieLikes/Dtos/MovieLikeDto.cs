namespace Cut_Roll_Users.Core.MovieLikes.Dtos;

public class MovieLikeDto
{
    public required string UserId { get; set; }
    public required Guid MovieId { get; set; }
}
