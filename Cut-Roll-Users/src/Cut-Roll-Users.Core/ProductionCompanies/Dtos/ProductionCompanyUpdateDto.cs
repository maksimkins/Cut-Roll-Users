namespace Cut_Roll_Users.Core.ProductionCompanies.Dtos;

public class ProductionCompanyUpdateDto
{
    public Guid Id { get; set; }
    
    public string? Name { get; set; }

    public string? CountryCode { get; set; }

    public string? LogoPath { get; set; }
}
