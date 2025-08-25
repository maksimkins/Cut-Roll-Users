using System.Text.Json.Serialization;
using Cut_Roll_Users.Core.MovieGenres.Models;

namespace Cut_Roll_Users.Core.Genres.Models;

public class Genre
{
    public Guid Id { get; set; }
    
    public required string Name { get; set; }
    [JsonIgnore]

    public ICollection<MovieGenre> MovieGenres { get; set; } = [];
}
