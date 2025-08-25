namespace Cut_Roll_Users.Core.MovieImages.Repositories;

using Cut_Roll_Users.Core.Common.Repositories.Base;
using Cut_Roll_Users.Core.MovieImages.Dtos;
using Cut_Roll_Users.Core.MovieImages.Models;

public interface IMovieImageRepository : ICreateAsync<MovieImageCreateDto, Guid?>, IDeleteByIdAsync<Guid, Guid?>, IDeleteRangeById<Guid, bool>, 
IBulkCreateAsync<MovieImageCreateDto, bool>, IBulkDeleteAsync<MovieImageDeleteDto, bool>
{
    Task<IEnumerable<MovieImage>> GetImagesByMovieIdAsync(Guid movieId);
    Task<IEnumerable<MovieImage>> GetImagesByTypeAsync(MovieImageSearchDto dto);
}
