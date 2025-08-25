namespace Cut_Roll_Users.Core.Crews.Dtos;

public class CrewCreateDto
{
    public Guid MovieId { get; set; }

    public Guid PersonId { get; set; }

    public string? Job { get; set; }

    public string? Department { get; set; }

}
