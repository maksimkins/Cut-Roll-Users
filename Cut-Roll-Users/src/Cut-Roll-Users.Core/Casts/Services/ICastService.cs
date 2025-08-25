using Cut_Roll_Users.Core.Casts.Dtos;
using Cut_Roll_Users.Core.Common.Dtos;

namespace Cut_Roll_Users.Core.Casts.Services;

public interface ICastService
{
    public Task<Guid> UpdateCastAsync(CastUpdateDto? dto);
    public Task<Guid> DeleteCastByIdAsync(Guid? id);
    public Task<Guid> CreateCastAsync(CastCreateDto? dto);
    public Task<PagedResult<CastGetDto>> SearchCastAsync(CastSearchDto? request);
    public Task<PagedResult<CastGetDto>> GetCastByMovieIdAsync(CastGetByMovieIdDto? dto);
    public Task<PagedResult<CastGetDto>> GetCastByPersonIdAsync(CastGetByPersonIdDto? dto);
    public Task<bool> BulkCreateCasteAsync(IEnumerable<CastCreateDto>? toCreate);
    public Task<bool> BulkDeleteCastAsync(IEnumerable<Guid>? toDeleteIds);
    public Task<bool> DeleteCastRangeByMovieIdAsync(Guid? movieId);
}


    
