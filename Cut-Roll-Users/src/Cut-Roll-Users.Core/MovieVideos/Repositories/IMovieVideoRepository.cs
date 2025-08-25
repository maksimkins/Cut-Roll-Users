namespace Cut_Roll_Users.Core.MovieVideos.Repositories;

using Cut_Roll_Users.Core.Common.Repositories.Base;
using Cut_Roll_Users.Core.MovieVideos.Dtos;
using Cut_Roll_Users.Core.MovieVideos.Models;

public interface IMovieVideoRepository : ICreateAsync<MovieVideoCreateDto, Guid?>, IDeleteByIdAsync<Guid, Guid?>,
IDeleteRangeById<Guid, bool>, IUpdateAsync<MovieVideoUpdateDto, Guid?>
{
    Task<IEnumerable<MovieVideo>> GetVideosByMovieIdAsync(Guid movieId);
    Task<IEnumerable<MovieVideo>> GetVideosByTypeAsync(MovieVideoSearchDto dto);
}
