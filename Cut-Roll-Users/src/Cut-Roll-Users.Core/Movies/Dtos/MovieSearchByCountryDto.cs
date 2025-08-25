namespace Cut_Roll_Users.Core.Movies.Dtos;

public class MovieSearchByCountryDto
{
    public string? Iso3166_1 { get; set; }
    public string? Name { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
