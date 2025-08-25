using System.Text.Json.Serialization;
using Cut_Roll_Users.Core.MovieSpokenLanguages.Models;

namespace Cut_Roll_Users.Core.SpokenLanguages.Models;

public class SpokenLanguage
{
    public required string Iso639_1 { get; set; }

    public required string EnglishName { get; set; }

    public string? Name { get; set; }
    [JsonIgnore]

    public ICollection<MovieSpokenLanguage> MovieSpokenLanguages { get; set; } = [];
}