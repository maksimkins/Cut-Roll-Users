using Cut_Roll_Users.Core.Users.Dtos;
using Cut_Roll_Users.Core.Movies.Dtos;

namespace Cut_Roll_Users.Core.MovieLikes.Dtos;

public class MovieLikeResponseDto
{
    public DateTime LikedAt { get; set; }
    public UserSimplified User { get; set; } = null!;
    public MovieSimplifiedDto Movie { get; set; } = null!;
}
