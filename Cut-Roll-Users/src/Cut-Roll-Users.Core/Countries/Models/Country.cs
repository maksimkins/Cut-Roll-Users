using System.Text.Json.Serialization;
using Cut_Roll_Users.Core.ProductionCompanies.Models;

namespace Cut_Roll_Users.Core.Countries.Models;

public class Country
{
    public required string Iso3166_1 { get; set; }
    public required string Name { get; set; }
    [JsonIgnore]
    public ICollection<ProductionCompany> Companies { get; set; } = [];
}
