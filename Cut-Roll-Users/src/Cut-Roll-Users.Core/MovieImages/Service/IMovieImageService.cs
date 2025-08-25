namespace Cut_Roll_Users.Core.MovieImages.Service;

using Cut_Roll_Users.Core.MovieImages.Dtos;
using Cut_Roll_Users.Core.MovieImages.Models;

public interface IMovieImageService
{
    Task<Guid> DeleteMovieImageByIdAsync(Guid? id);
    Task<Guid> CreateMovieGenreAsync(MovieImageCreateDto? dto);
    Task<bool> DeleteMovieImageRangeByMovieId(Guid? movieId);
    Task<IEnumerable<MovieImage>> GetMovieImagesByTypeAsync(Guid? movieId, string? type);
    Task<IEnumerable<MovieImage>> GetMovieImagesByMovieIdAsync(Guid? movieId);
    Task<bool> BulkMovieImageCreateAsync(IEnumerable<MovieImageCreateDto?>? toCreate);
    Task<bool> BulkMovieImageDeleteAsync(IEnumerable<MovieImageDeleteDto?>? toDelete);
}


