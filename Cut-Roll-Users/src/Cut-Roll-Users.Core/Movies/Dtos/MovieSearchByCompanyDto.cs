namespace Cut_Roll_Users.Core.Movies.Dtos;

public class MovieSearchByCompanyDto
{
    public Guid? CompanyId { get; set; }
    public string? Name { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
