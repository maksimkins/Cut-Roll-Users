namespace Cut_Roll_Users.Core.Common.Options;
public class LocalEmbeddingOptions
{
    public string ModelPath { get; set; } = string.Empty;
    public string TokenizerPath { get; set; } = string.Empty;
    public int BatchSize { get; set; } = 100;
}