#pragma warning disable CS8618

namespace Cut_Roll_Users.Core.MovieProductionCompanies.Models;

using System.Text.Json.Serialization;
using Cut_Roll_Users.Core.Movies.Models;
using Cut_Roll_Users.Core.ProductionCompanies.Models;

public class MovieProductionCompany
{
    public Guid MovieId { get; set; }
    public Guid CompanyId { get; set; }
    [JsonIgnore]
    public Movie Movie { get; set; }
    public ProductionCompany Company { get; set; }
}
