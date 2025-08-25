using System.ComponentModel.DataAnnotations;

namespace Cut_Roll_Users.Core.Movies.Dtos;

public class MovieSearchRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; set; } = 10;

    public string? Title { get; set; }
    public List<string>? Genres { get; set; } = new List<string>();
    public string? Actor { get; set; }
    public string? Director { get; set; }
    public List<string>? Keywords { get; set; } = new List<string>();
    public int? Year { get; set; }
    public float? MinRating { get; set; }
    public float? MaxRating { get; set; }
    public string? Country { get; set; }
    public string? Language { get; set; }
    public string? SortBy { get; set; } = "title"; // title, rating, releaseDate, revenue
    public bool SortDescending { get; set; } = false;
}
