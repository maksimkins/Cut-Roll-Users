using Cut_Roll_Users.Core.Common.Repositories.Base;
using Cut_Roll_Users.Core.ListMovies.Dtos;

namespace Cut_Roll_Users.Core.ListMovies.Repositories;

public interface IListMovieRepository :
    ICreateAsync<ListMovieDto, Guid?>,
    IDeleteAsync<ListMovieDto, Guid?>,
    IBulkCreateAsync<ListMovieDto, bool>,
    IBulkDeleteAsync<ListMovieDto, bool>
{
    Task<bool> IsMovieInListAsync(Guid listId, Guid movieId);
    Task<int> GetMovieCountByListIdAsync(Guid listId);
}
