namespace Cut_Roll_Users.Core.People.Dtos;

public class PersonCreateDto
{
    public required string Name { get; set; }

    public string? ProfilePath { get; set; }
}
