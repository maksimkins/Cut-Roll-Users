using Cut_Roll_Users.Core.ListLikes.Dtos;

namespace Cut_Roll_Users.Core.ListLikes.Services;

public interface IListLikeService
{
    Task<Guid> LikeListAsync(ListLikeDto? likeDto);
    Task<Guid> UnlikeListAsync(ListLikeDto? likeDto);
    Task<bool> IsListLikedByUserAsync(string? userId, Guid? listId);
    Task<int> GetListLikeCountAsync(Guid? listId);
}
