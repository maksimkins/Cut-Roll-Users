#pragma warning disable CS8618

using System.Text.Json.Serialization;
using Cut_Roll_Users.Core.Movies.Models;
using Cut_Roll_Users.Core.SpokenLanguages.Models;

namespace Cut_Roll_Users.Core.MovieSpokenLanguages.Models;

public class MovieSpokenLanguage
{
    public Guid MovieId { get; set; }

    public required string LanguageCode { get; set; }
    [JsonIgnore]

    public Movie Movie { get; set; }

    public SpokenLanguage Language { get; set; }
}
