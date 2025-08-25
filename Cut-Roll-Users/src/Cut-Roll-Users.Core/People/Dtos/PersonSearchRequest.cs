namespace Cut_Roll_Users.Core.People.Dtos;

public class PersonSearchRequest
{
    public required string Name { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
