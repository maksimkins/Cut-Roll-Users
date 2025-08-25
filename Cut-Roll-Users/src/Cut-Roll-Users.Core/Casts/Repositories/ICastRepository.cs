namespace Cut_Roll_Users.Core.Casts.Repositories;

using Cut_Roll_Users.Core.Casts.Dtos;
using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Common.Repositories.Base;

public interface ICastRepository : IUpdateAsync<CastUpdateDto, Guid?>, IDeleteByIdAsync<Guid, Guid?>,
    ICreateAsync<CastCreateDto, Guid?>, ISearchAsync<CastSearchDto, PagedResult<CastGetDto>>, IBulkCreateAsync<CastCreateDto, bool>,
    IBulkDeleteAsync<Guid, bool>, IDeleteRangeById<Guid, bool>
{
    public Task<PagedResult<CastGetDto>> GetByMovieIdAsync(CastGetByMovieIdDto dto);
    public Task<PagedResult<CastGetDto>> GetByPersonIdAsync(CastGetByPersonIdDto dto);
    public Task<bool> ExistsByIdAsync(Guid id);
}
