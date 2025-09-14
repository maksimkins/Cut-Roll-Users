
namespace Cut_Roll_Users.Core.Common.VectorDatabases.Options;
public class PineconeOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string IndexName { get; set; } = string.Empty;
    public int VectorDimensions { get; set; } = 384;
    
    // Namespace configuration
    public string? Namespace { get; set; } = "default";
    
    // Search configuration
    public string[]? SearchFields { get; set; } = ["title", "posterPath", "movieId"];
    public string? VectorFieldName { get; set; } = "vector";
    
    // Test configuration
    public float TestVectorValue { get; set; } = 0.1f;
    
    // Proxy configuration for Traefik
    public string? ProxyHost { get; set; }
    public int ProxyPort { get; set; } = 0;
}