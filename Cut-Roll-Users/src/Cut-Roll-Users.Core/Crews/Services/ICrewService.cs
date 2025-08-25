namespace Cut_Roll_Users.Core.Crews.Services;

using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Crews.Dtos;

public interface ICrewService
{
    Task<Guid> UpdateCrewAsync(CrewUpdateDto? dto);
    Task<Guid> DeleteCrewByIdAsync(Guid? id);
    Task<Guid> CreateCrewAsync(CrewCreateDto? dto);
    Task<PagedResult<CrewGetDto>> SearchCrewAsync(CrewSearchDto? request);
    public Task<PagedResult<CrewGetDto>> GetCrewByMovieIdAsync(CrewGetByMovieId? dto);
    public Task<bool> BulkCreateCrewAsync(IEnumerable<CrewCreateDto>? crewList);
    public Task<bool> BulkDeleteCrewAsync(IEnumerable<Guid>? idsToDelete);
    public Task<PagedResult<CrewGetDto>> GetCrewByPersonIdAsync(CrewGetByPersonId? dto);
    public Task<bool> DeleteCrewRangeByMovieIdAsync(Guid? movieId);
}



