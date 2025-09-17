namespace Cut_Roll_Users.Core.MovieEmbeddings.Services;

using Cut_Roll_Users.Core.MovieEmbeddings.Dtos;

public interface IMovieEmbeddingService
{
    Task<bool> GenerateAndStoreMovieEmbeddingAsync(Guid movieId);
    Task<bool> UpdateMovieEmbeddingAsync(Guid movieId);
    
    Task ProcessAllMoviesAsync(int? batchSize = null);
    Task<(int successCount, int failedCount)> ProcessMoviesBatchAsync(int offset, int limit);
    
    Task<EmbeddingStatusDto> GetEmbeddingStatusAsync();
    Task<int> GetTotalMovieCountAsync();
    Task<int> GetProcessedMovieCountAsync();
    Task<bool> ResetAllEmbeddingsAsync();
}