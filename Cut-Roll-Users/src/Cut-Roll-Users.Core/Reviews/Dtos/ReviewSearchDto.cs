namespace Cut_Roll_Users.Core.Reviews.Dtos;

public class ReviewSearchDto
{
    public string? UserId { get; set; }     
    public Guid? MovieId { get; set; }      
    public float? MinRating { get; set; }         
    public float? MaxRating { get; set; }         
    public DateTime? CreatedAfter { get; set; }   
    public DateTime? CreatedBefore { get; set; }  
    public ReviewSortBy SortBy { get; set; } = ReviewSortBy.CreatedAt;
    public bool SortDescending { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public enum ReviewSortBy
{
    CreatedAt,
    Rating,
    Likes
}
