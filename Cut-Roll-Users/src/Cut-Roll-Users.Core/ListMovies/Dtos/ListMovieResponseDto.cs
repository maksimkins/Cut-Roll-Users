using Cut_Roll_Users.Core.Movies.Dtos;

namespace Cut_Roll_Users.Core.ListMovies.Dtos;

public class ListMovieResponseDto
{
    public required Guid ListId { get; set; }
    public required MovieSimplifiedDto movieSimplifiedDto { get; set; }
}
