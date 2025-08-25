#pragma warning disable CS8618

using System.Text.Json.Serialization;
using Cut_Roll_Users.Core.Movies.Models;
using Cut_Roll_Users.Core.People.Models;

namespace Cut_Roll_Users.Core.Crews.Models;

public class Crew
{
    public Guid Id { get; set; }
    public Guid MovieId { get; set; }
    public Guid PersonId { get; set; }
    public string? Job { get; set; }
    public string? Department { get; set; }
    [JsonIgnore]
    public Movie Movie { get; set; }
    public Person Person { get; set; }
}
