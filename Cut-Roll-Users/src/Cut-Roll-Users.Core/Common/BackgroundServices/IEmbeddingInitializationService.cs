using Cut_Roll_Users.Core.MovieEmbeddings.Dtos;

namespace Cut_Roll_Users.Core.Common.BackgroundServices;

public interface IEmbeddingInitializationService
{
    Task InitializeEmbeddingsAsync();
    Task<bool> IsVectorDbEmptyAsync();
    Task<bool> CheckSystemHealthAsync();
    Task<EmbeddingStatusDto> GetInitializationStatusAsync();
}