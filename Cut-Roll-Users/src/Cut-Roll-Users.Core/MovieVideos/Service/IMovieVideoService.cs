namespace Cut_Roll_Users.Core.MovieVideos.Service;

using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.MovieVideos.Dtos;
using Cut_Roll_Users.Core.MovieVideos.Models;

public interface IMovieVideoService
{
    Task<Guid> CreateMovieVideoCreateAsync(MovieVideoCreateDto? dto);
    Task<Guid> DeleteMovieVideoByIdAsync(Guid? id);
    Task<bool> DeleteMovieVideoRangeByMovieId(Guid? movieId);
    Task<Guid> UpdateMovieVideoAsync(MovieVideoUpdateDto? dto);
    Task<IEnumerable<MovieVideo>> GetVideosByMovieIdAsync(Guid? movieId);
    Task<IEnumerable<MovieVideo>> GetVideosByTypeAsync(MovieVideoSearchDto? dto);
}
