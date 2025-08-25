using Cut_Roll_Users.Core.ListLikes.Dtos;

namespace Cut_Roll_Users.Core.ListLikes.Services;

public interface IListLikeService
{
    Task<bool> LikeListAsync(ListLikeDto? likeDto);
    Task<bool> UnlikeListAsync(ListLikeDto? likeDto);
    Task<bool> IsListLikedByUserAsync(string? userId, Guid? listId);
    Task<int> GetListLikeCountAsync(Guid? listId);
}
