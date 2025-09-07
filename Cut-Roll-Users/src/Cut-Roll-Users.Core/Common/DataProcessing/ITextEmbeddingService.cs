using Cut_Roll_Users.Core.MovieEmbeddings.Dtos;

namespace Cut_Roll_Users.Core.Common.DataProcessing;
public interface ITextEmbeddingService
{
    Task<List<float>> GenerateEmbeddingAsync(string text);
    Task<List<float>> GenerateMovieEmbeddingAsync(MovieDataForEmbeddingDto movieData);
    Task<List<List<float>>> GenerateMovieEmbeddingsBatchAsync(List<MovieDataForEmbeddingDto> moviesData);
}