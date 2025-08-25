namespace Cut_Roll_Users.Core.MovieOriginCountries.Dtos;

public class MovieOriginCountryDto
{
    public Guid MovieId { get; set; }

    public required string CountryCode { get; set; }
}
