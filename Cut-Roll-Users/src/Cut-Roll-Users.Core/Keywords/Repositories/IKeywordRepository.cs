namespace Cut_Roll_Users.Core.Keywords.Repositories;

using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Common.Repositories.Base;
using Cut_Roll_Users.Core.Keywords.Dtos;
using Cut_Roll_Users.Core.Keywords.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;


public interface IKeywordRepository : IDeleteByIdAsync<Guid, Guid?>, ICreateAsync<KeywordCreateDto, Guid?>,
ISearchAsync<KeywordSearchDto, PagedResult<Keyword>>, IGetByIdAsync<Guid, Keyword?>
{
    Task<PagedResult<Keyword>> GetAllAsync(KeywordPaginationDto dto);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> ExistsByNameAsync(string name);
}
