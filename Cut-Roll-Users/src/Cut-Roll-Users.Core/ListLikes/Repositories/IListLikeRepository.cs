using Cut_Roll_Users.Core.Common.Repositories.Base;
using Cut_Roll_Users.Core.ListLikes.Dtos;

namespace Cut_Roll_Users.Core.ListLikes.Repositories;

public interface IListLikeRepository :
    ICreateAsync<ListLikeDto, Guid?>,
    IDeleteAsync<ListLikeDto, Guid?>
{
    Task<bool> IsLikedByUserAsync(string userId, Guid listId);
    Task<int> GetLikeCountByListIdAsync(Guid listId);
}
