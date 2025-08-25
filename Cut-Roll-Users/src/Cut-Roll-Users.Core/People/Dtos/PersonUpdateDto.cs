namespace Cut_Roll_Users.Core.People.Dtos;

public class PersonUpdateDto
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string? ProfilePath { get; set; }
}
