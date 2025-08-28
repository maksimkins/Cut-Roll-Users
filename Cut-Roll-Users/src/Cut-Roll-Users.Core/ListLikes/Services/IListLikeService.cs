using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.ListEntities.Dtos;
using Cut_Roll_Users.Core.ListLikes.Dtos;

namespace Cut_Roll_Users.Core.ListLikes.Services;

public interface IListLikeService
{
    Task<Guid> LikeListAsync(ListLikeDto? likeDto);
    Task<Guid> UnlikeListAsync(ListLikeDto? likeDto);
    Task<bool> IsListLikedByUserAsync(string? userId, Guid? listId);
    Task<int> GetListLikeCountAsync(Guid? listId);
    Task<PagedResult<ListEntityResponseDto>> GetLikedLists(ListLikedDto dto);
    
}
