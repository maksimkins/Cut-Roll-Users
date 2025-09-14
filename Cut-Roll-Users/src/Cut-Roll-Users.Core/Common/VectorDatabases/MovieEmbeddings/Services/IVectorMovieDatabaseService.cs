using Cut_Roll_Users.Core.MovieEmbeddings.Dtos;

namespace Cut_Roll_Users.Core.MovieEmbeddings.Services;

public interface IVectorMovieDatabaseService
{
    Task<bool> UpsertMovieEmbeddingAsync(MovieEmbeddingDto embedding, bool hasEmbedding = false);
    Task<List<MovieRecommendationDto>> FindSimilarMoviesAsync(List<float> queryVector, int limit = 10, List<Guid>? excludeMovieIds = null);
    Task<bool> DeleteMovieEmbeddingAsync(Guid movieId, bool hasEmbedding = true);
    Task<bool> InitializeIndexAsync();
    Task<int> GetEmbeddedMoviesCountAsync();
    Task<bool> IsVectorDbEmptyAsync();
    Task<bool> CheckVectorDbHealthAsync();
}
