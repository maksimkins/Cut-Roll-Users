namespace Cut_Roll_Users.Core.Common.VectorDatabases.Models;
public class PineconeQueryResponse
{
    public List<PineconeMatch> Matches { get; set; } = new();
}