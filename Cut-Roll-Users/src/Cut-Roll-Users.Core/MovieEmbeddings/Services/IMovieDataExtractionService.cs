namespace Cut_Roll_Users.Core.MovieEmbeddings.Services;

using Cut_Roll_Users.Core.MovieEmbeddings.Dtos;
public interface IMovieDataExtractionService
{
    Task<MovieDataForEmbeddingDto?> ExtractCompleteMovieDataAsync(Guid movieId);
    Task<List<MovieDataForEmbeddingDto>> ExtractMoviesDataBatchAsync(int offset, int limit);
    Task<int> GetTotalMovieCountAsync();
}