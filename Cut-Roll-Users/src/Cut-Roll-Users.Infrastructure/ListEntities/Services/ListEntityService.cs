using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.ListEntities.Dtos;
using Cut_Roll_Users.Core.ListEntities.Repositories;
using Cut_Roll_Users.Core.ListEntities.Services;
using Cut_Roll_Users.Core.Movies.Dtos;

namespace Cut_Roll_Users.Infrastructure.ListEntities.Services;

public class ListEntityService : IListEntityService
{
    private readonly IListEntityRepository _listEntityRepository;

    public ListEntityService(IListEntityRepository listEntityRepository)
    {
        _listEntityRepository = listEntityRepository;
    }

    public async Task<Guid> CreateListAsync(ListEntityCreateDto? listCreateDto)
    {
        if (listCreateDto == null)
            throw new ArgumentNullException(nameof(listCreateDto), "List creation data cannot be null.");
        
        if (string.IsNullOrWhiteSpace(listCreateDto.UserId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(listCreateDto.UserId));
        
        if (string.IsNullOrWhiteSpace(listCreateDto.Title))
            throw new ArgumentException("List title cannot be null or empty.", nameof(listCreateDto.Title));

        var result = await _listEntityRepository.CreateAsync(listCreateDto);
        return result ?? throw new InvalidOperationException("Failed to create list.");
    }

    public async Task<ListEntityResponseDto?> GetListByIdAsync(Guid? listId)
    {
        if (!listId.HasValue || listId.Value == Guid.Empty)
            throw new ArgumentException("List ID cannot be null or empty.", nameof(listId));

        return await _listEntityRepository.GetByIdAsync(listId.Value);
    }

    public async Task<Guid> UpdateListAsync(ListEntityUpdateDto? listUpdateDto)
    {
        if (listUpdateDto == null)
            throw new ArgumentNullException(nameof(listUpdateDto), "List update data cannot be null.");
        
        if (listUpdateDto.Id == Guid.Empty)
            throw new ArgumentException("List ID cannot be empty.", nameof(listUpdateDto.Id));
        
        if (string.IsNullOrWhiteSpace(listUpdateDto.Title) && string.IsNullOrWhiteSpace(listUpdateDto.Description))
            throw new ArgumentException("At least one field (Title or Description) must be provided for update.");

        var list = await _listEntityRepository.GetByIdAsync(listUpdateDto.Id);
        if (list == null || list.UserSimplified.Id != listUpdateDto.UserId)
            throw new ArgumentException("user doesnt own this list");

        var result = await _listEntityRepository.UpdateAsync(listUpdateDto);
        if (result == null)
            throw new InvalidOperationException($"ListEntity not found with Id: {listUpdateDto.Id}");
        
        return result.Value;
    }

    public async Task<Guid> DeleteListByIdAsync(Guid? listId)
    {
        if (!listId.HasValue || listId.Value == Guid.Empty)
            throw new ArgumentException("List ID cannot be null or empty.", nameof(listId));

        var result = await _listEntityRepository.DeleteByIdAsync(listId.Value);
        if (result == null)
            throw new InvalidOperationException($"ListEntity not found with Id: {listId.Value}");
        
        return result.Value;
    }

    public async Task<PagedResult<ListEntityResponseDto>> SearchListsAsync(ListEntitySearchDto? searchDto)
    {
        if (searchDto == null)
            throw new ArgumentNullException(nameof(searchDto), "Search data cannot be null.");

        return await _listEntityRepository.SearchAsync(searchDto);
    }

    public async Task<PagedResult<ListEntityResponseDto>> GetListsByUserIdAsync(ListEntityPaginationDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "Pagination data cannot be null.");
        
        if (string.IsNullOrWhiteSpace(dto.UserId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(dto.UserId));

        return await _listEntityRepository.GetByUserIdAsync(dto);
    }

    public async Task<int> GetListCountByUserIdAsync(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

        return await _listEntityRepository.GetListCountByUserIdAsync(userId);
    }

    public async Task<PagedResult<MovieSimplifiedDto>> GetMoviesFromListAsync(ListEntityGetByIdDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "data cannot be null.");
        if (dto.ListId == Guid.Empty)
            throw new ArgumentException("List ID cannot be null or empty.", nameof(dto.ListId));

        return await _listEntityRepository.GetMoviesFromListAsync(dto);
    }
}
