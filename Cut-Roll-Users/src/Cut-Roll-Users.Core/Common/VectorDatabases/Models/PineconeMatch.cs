namespace Cut_Roll_Users.Core.Common.VectorDatabases.Models;
public class PineconeMatch
{
    public string Id { get; set; } = string.Empty;
    public double Score { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}