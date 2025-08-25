#pragma warning disable CS8618

using System.Text.Json.Serialization;
using Cut_Roll_Users.Core.Movies.Models;

namespace Cut_Roll_Users.Core.MovieVideos.Models;

public class MovieVideo
{
    public Guid Id { get; set; }

    public Guid MovieId { get; set; }

    public required string Name { get; set; }

    public string? Type { get; set; }

    public required string Site { get; set; }

    public required string Key { get; set; }
    [JsonIgnore]

    public Movie Movie { get; set; }
}
