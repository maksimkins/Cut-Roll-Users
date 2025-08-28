using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.ListEntities.Dtos;
using Cut_Roll_Users.Core.Movies.Dtos;

namespace Cut_Roll_Users.Core.ListEntities.Services;

public interface IListEntityService
{
    Task<Guid> CreateListAsync(ListEntityCreateDto? listCreateDto);
    Task<ListEntityResponseDto?> GetListByIdAsync(Guid? listId);
    Task<Guid> UpdateListAsync(ListEntityUpdateDto? listUpdateDto);
    Task<Guid> DeleteListByIdAsync(Guid? listId);
    Task<PagedResult<ListEntityResponseDto>> SearchListsAsync(ListEntitySearchDto? searchDto);
    Task<PagedResult<ListEntityResponseDto>> GetListsByUserIdAsync(ListEntityPaginationDto dto);

    Task<int> GetListCountByUserIdAsync(string? userId);
    Task<PagedResult<MovieSimplifiedDto>> GetMoviesFromListAsync(ListEntityGetByIdDto? dto);
}
