namespace Cut_Roll_Users.Core.WatchedMovies.Dtos;

public class WatchedMovieSearchDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
