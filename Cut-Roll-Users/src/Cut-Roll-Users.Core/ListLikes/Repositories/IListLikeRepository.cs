using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Common.Repositories.Base;
using Cut_Roll_Users.Core.ListEntities.Dtos;
using Cut_Roll_Users.Core.ListLikes.Dtos;

namespace Cut_Roll_Users.Core.ListLikes.Repositories;

public interface IListLikeRepository :
    ICreateAsync<ListLikeDto, Guid?>,
    IDeleteAsync<ListLikeDto, Guid?>
{
    Task<bool> IsLikedByUserAsync(string userId, Guid listId);
    Task<int> GetLikeCountByListIdAsync(Guid listId);
    Task<PagedResult<ListEntityResponseDto>> GetLikedLists(ListLikedDto dto);
}
