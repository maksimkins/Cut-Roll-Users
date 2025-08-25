#pragma warning disable CS8618

namespace Cut_Roll_Users.Core.MovieImages.Models;

using System.Text.Json.Serialization;
using Cut_Roll_Users.Core.Movies.Models;

public class MovieImage
{
    public Guid Id { get; set; }
    public Guid MovieId { get; set; }
    public required string Type { get; set; }
    public required string FilePath { get; set; }
    [JsonIgnore]
    public Movie Movie { get; set; }
}
