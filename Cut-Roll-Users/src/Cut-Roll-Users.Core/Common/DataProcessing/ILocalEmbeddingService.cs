namespace Cut_Roll_Users.Core.Common.DataProcessing;
public interface ILocalEmbeddingService
{
    Task<List<float>> GenerateEmbeddingAsync(string text);
    Task<List<List<float>>> GenerateEmbeddingsBatchAsync(List<string> texts);
    Task<bool> InitializeModelAsync();
    bool IsModelLoaded { get; }
}