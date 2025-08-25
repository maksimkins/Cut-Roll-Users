namespace Cut_Roll_Users.Core.MovieLikes.Dtos;

public class MovieLikeCreateDto
{
    public required string UserId { get; set; }
    public required Guid MovieId { get; set; }
}
