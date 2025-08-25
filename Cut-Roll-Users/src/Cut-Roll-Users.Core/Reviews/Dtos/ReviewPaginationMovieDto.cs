namespace Cut_Roll_Users.Core.Reviews.Dtos;

public class ReviewPaginationMovieDto
{
    public required Guid MovieId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
