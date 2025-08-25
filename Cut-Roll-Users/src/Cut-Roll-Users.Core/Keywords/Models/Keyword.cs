using System.Text.Json.Serialization;
using Cut_Roll_Users.Core.MovieKeywords.Models;

namespace Cut_Roll_Users.Core.Keywords.Models;

public class Keyword
{
    public Guid Id { get; set; }

    public required string Name { get; set; }
    [JsonIgnore]
    
    public ICollection<MovieKeyword> MovieKeywords { get; set; } = [];
}
