namespace Cut_Roll_Users.Core.Movies.Dtos;

public class MovieSearchBySpokenLanguageDto
{
    public string? Iso639_1 { get; set; }
    public string? EnglishName { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
