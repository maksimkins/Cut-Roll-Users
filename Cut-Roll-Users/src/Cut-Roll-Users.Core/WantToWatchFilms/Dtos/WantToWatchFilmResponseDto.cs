using Cut_Roll_Users.Core.Users.Dtos;
using Cut_Roll_Users.Core.Movies.Dtos;

namespace Cut_Roll_Users.Core.WantToWatchFilms.Dtos;

public class WantToWatchFilmResponseDto
{
    public DateTime AddedAt { get; set; }
    public UserSimlified User { get; set; } = null!;
    public MovieSimplifiedDto Movie { get; set; } = null!;
}
