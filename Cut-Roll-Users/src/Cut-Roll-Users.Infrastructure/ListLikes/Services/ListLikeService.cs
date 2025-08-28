using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.ListEntities.Dtos;
using Cut_Roll_Users.Core.ListLikes.Dtos;
using Cut_Roll_Users.Core.ListLikes.Repositories;
using Cut_Roll_Users.Core.ListLikes.Services;

namespace Cut_Roll_Users.Infrastructure.ListLikes.Services;

public class ListLikeService : IListLikeService
{
    private readonly IListLikeRepository _listLikeRepository;

    public ListLikeService(IListLikeRepository listLikeRepository)
    {
        _listLikeRepository = listLikeRepository;
    }

    public async Task<Guid> LikeListAsync(ListLikeDto? likeDto)
    {
        if (likeDto == null)
            throw new ArgumentNullException(nameof(likeDto), "List like data cannot be null.");
        
        if (string.IsNullOrWhiteSpace(likeDto.UserId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(likeDto.UserId));
        
        if (likeDto.ListId == Guid.Empty)
            throw new ArgumentException("List ID cannot be empty.", nameof(likeDto.ListId));

        
        var isAlreadyLiked = await _listLikeRepository.IsLikedByUserAsync(likeDto.UserId, likeDto.ListId);
        if (isAlreadyLiked)
            return likeDto.ListId; 

        var result = await _listLikeRepository.CreateAsync(likeDto);
        return result ?? throw new InvalidOperationException("Failed to create list like.");
    }

    public async Task<Guid> UnlikeListAsync(ListLikeDto? likeDto)
    {
        if (likeDto == null)
            throw new ArgumentNullException(nameof(likeDto), "List unlike data cannot be null.");
        
        if (string.IsNullOrWhiteSpace(likeDto.UserId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(likeDto.UserId));
        
        if (likeDto.ListId == Guid.Empty)
            throw new ArgumentException("List ID cannot be empty.", nameof(likeDto.ListId));

  
        var isLiked = await _listLikeRepository.IsLikedByUserAsync(likeDto.UserId, likeDto.ListId);
        if (!isLiked)
            return likeDto.ListId; 

        var result = await _listLikeRepository.DeleteAsync(likeDto);
        if (result == null)
            throw new InvalidOperationException($"ListLike not found for UserId: {likeDto.UserId} and ListId: {likeDto.ListId}");
        
        return result.Value;
    }

    public async Task<bool> IsListLikedByUserAsync(string? userId, Guid? listId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
        
        if (!listId.HasValue || listId.Value == Guid.Empty)
            throw new ArgumentException("List ID cannot be null or empty.", nameof(listId));

        return await _listLikeRepository.IsLikedByUserAsync(userId, listId.Value);
    }

    public async Task<int> GetListLikeCountAsync(Guid? listId)
    {
        if (!listId.HasValue || listId.Value == Guid.Empty)
            throw new ArgumentException("List ID cannot be null or empty.", nameof(listId));

        return await _listLikeRepository.GetLikeCountByListIdAsync(listId.Value);
    }

    public Task<PagedResult<ListEntityResponseDto>> GetLikedLists(ListLikedDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "dto data cannot be null.");
        if (string.IsNullOrWhiteSpace(dto.UserId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(dto.UserId));

        return _listLikeRepository.GetLikedLists(dto);
    }
}
