using Cut_Roll_Users.Core.ListMovies.Dtos;

namespace Cut_Roll_Users.Core.ListMovies.Services;

public interface IListMovieService
{
    Task<Guid> AddMovieToListAsync(ListMovieDto? listMovieDto);
    Task<Guid> RemoveMovieFromListAsync(ListMovieDto? listMovieDto);
    Task<bool> BulkCreateAsync(IEnumerable<ListMovieDto>? toCreate);
    Task<bool> BulkDeleteAsync(IEnumerable<ListMovieDto>? toDelete);
    Task<bool> IsMovieInListAsync(Guid? listId, Guid? movieId);
    Task<int> GetMovieCountByListIdAsync(Guid? listId);
}
