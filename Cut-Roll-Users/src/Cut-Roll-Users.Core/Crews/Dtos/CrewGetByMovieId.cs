
namespace Cut_Roll_Users.Core.Crews.Dtos;

public class CrewGetByMovieId
{
    public Guid MovieId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
