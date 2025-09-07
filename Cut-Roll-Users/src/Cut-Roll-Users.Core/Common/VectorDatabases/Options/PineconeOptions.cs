
namespace Cut_Roll_Users.Core.Common.VectorDatabases.Options;
public class PineconeOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string IndexName { get; set; } = string.Empty;
    public int VectorDimension { get; set; } = 384;
}