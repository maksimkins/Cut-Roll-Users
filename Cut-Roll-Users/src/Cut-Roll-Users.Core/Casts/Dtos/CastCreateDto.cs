namespace Cut_Roll_Users.Core.Casts.Dtos;

public class CastCreateDto
{
    public Guid MovieId { get; set; }

    public Guid PersonId { get; set; }

    public string? Character { get; set; }

    public int CastOrder { get; set; }
}
