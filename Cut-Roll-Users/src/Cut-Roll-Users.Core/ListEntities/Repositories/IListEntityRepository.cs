using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Common.Repositories.Base;
using Cut_Roll_Users.Core.ListEntities.Dtos;
using Cut_Roll_Users.Core.Movies.Dtos;

namespace Cut_Roll_Users.Core.ListEntities.Repositories;

public interface IListEntityRepository :
    ICreateAsync<ListEntityCreateDto, Guid?>,
    IGetByIdAsync<Guid, ListEntityResponseDto?>,
    IUpdateAsync<ListEntityUpdateDto, Guid?>,
    IDeleteByIdAsync<Guid, Guid?>,
    ISearchAsync<ListEntitySearchDto, PagedResult<ListEntityResponseDto>>
{
    Task<PagedResult<ListEntityResponseDto>> GetByUserIdAsync(ListEntityPaginationDto dto);
    Task<int> GetListCountByUserIdAsync(string userId);
    Task<PagedResult<MovieSimplifiedDto>> GetMoviesFromListAsync(ListEntityGetByIdDto dto);

}
