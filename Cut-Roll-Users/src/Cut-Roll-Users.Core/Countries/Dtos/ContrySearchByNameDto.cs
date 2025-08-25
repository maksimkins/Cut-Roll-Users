namespace Cut_Roll_Users.Core.Countries.Dtos;

public class ContrySearchByNameDto
{
    public required string Name { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10; 
}
