using Cut_Roll_Users.Core.MovieEmbeddings.Dtos;

namespace Cut_Roll_Users.Core.Common.BackgroundServices;

public interface IMovieEmbeddingBackgroundService
{
    Task ProcessNewMoviesAsync();
    Task ProcessMoviesBatchAsync(int offset, int limit);
    Task<int> GetNewMoviesCountAsync();
    Task<bool> IsProcessingAsync();
    Task<EmbeddingStatusDto> GetProcessingStatusAsync();
}