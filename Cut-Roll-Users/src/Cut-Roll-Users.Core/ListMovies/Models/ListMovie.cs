
using Cut_Roll_Users.Core.ListEntities.Models;
using Cut_Roll_Users.Core.Movies.Models;

namespace Cut_Roll_Users.Core.ListMovies.Models
{
public class ListMovie
{
    public Guid ListId { get; set; }
    public Guid MovieId { get; set; }

    public ListEntity List { get; set; } = null!;
    public Movie Movie { get; set; } = null!;
}
}