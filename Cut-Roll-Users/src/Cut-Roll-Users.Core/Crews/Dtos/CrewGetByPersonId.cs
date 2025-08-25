namespace Cut_Roll_Users.Core.Crews.Dtos;

public class CrewGetByPersonId
{
    public Guid PersonId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
