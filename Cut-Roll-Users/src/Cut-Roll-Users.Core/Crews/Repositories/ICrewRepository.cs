namespace Cut_Roll_Users.Core.Crews.Repositories;

using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Common.Repositories.Base;
using Cut_Roll_Users.Core.Crews.Dtos;

public interface ICrewRepository : IUpdateAsync<CrewUpdateDto, Guid?>, IDeleteByIdAsync<Guid, Guid?>,
    ICreateAsync<CrewCreateDto, Guid?>, ISearchAsync<CrewSearchDto, PagedResult<CrewGetDto>>, IBulkCreateAsync<CrewCreateDto, bool>,
    IBulkDeleteAsync<Guid, bool>, IDeleteRangeById<Guid, bool>
{
    public Task<PagedResult<CrewGetDto>> GetByMovieIdAsync(CrewGetByMovieId dto);
    public Task<PagedResult<CrewGetDto>> GetByPersonIdAsync(CrewGetByPersonId dto);
    public Task<bool> ExistsByIdAsync(Guid id);
}
