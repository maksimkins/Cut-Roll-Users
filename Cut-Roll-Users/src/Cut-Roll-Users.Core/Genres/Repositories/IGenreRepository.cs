namespace Cut_Roll_Users.Core.Genres.Repositories;

using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Common.Repositories.Base;
using Cut_Roll_Users.Core.Genres.Dtos;
using Cut_Roll_Users.Core.Genres.Models;

public interface IGenreRepository : IDeleteByIdAsync<Guid, Guid?>, ICreateAsync<GenreCreateDto, Guid?>, IGetByIdAsync<Guid, Genre?>,
ISearchAsync<GenreSearchDto, PagedResult<Genre>>
{
    public Task<PagedResult<Genre>> GetAllAsync(GenrePaginationDto dto);
    public Task<bool> ExistsAsync(Guid id);
    public Task<bool> ExistsByNameAsync(string name);
}
