namespace Cut_Roll_Users.Core.MovieEmbeddings.Dtos;
public class RecommendationRequestDto
{
    public int Limit { get; set; } = 10;
    public List<Guid> ExcludeMovieIds { get; set; } = new();
}