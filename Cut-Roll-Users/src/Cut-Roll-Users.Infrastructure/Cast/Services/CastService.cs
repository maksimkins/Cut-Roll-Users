namespace Cut_Roll_Users.Infrastructure.Cast.Services;

using Cut_Roll_Users.Core.Casts.Dtos;
using Cut_Roll_Users.Core.Casts.Models;
using Cut_Roll_Users.Core.Casts.Repositories;
using Cut_Roll_Users.Core.Casts.Services;
using Cut_Roll_Users.Core.Common.Dtos;

public class CastService : ICastService
{
    private readonly ICastRepository _castRepository;
    public CastService(ICastRepository castRepository)
    {
        _castRepository = castRepository ?? throw new Exception($"{nameof(_castRepository)}");
    }
    public async Task<bool> BulkCreateCasteAsync(IEnumerable<CastCreateDto>? toCreate)
    {
        if (toCreate == null || !toCreate.Any())
            throw new ArgumentNullException($"there is no instances to create");

        foreach (var c in toCreate)
        {
            if (c == null)
                throw new ArgumentNullException("one of the object is null");
            if (c.MovieId == Guid.Empty)
                throw new ArgumentNullException($"missing {c.MovieId}");
            if (c.PersonId == Guid.Empty)
                throw new ArgumentNullException($"missing {c.PersonId}");
            
        }

        return await _castRepository.BulkCreateAsync(toCreate);
    }

    public async Task<bool> BulkDeleteCastAsync(IEnumerable<Guid>? idsToDelete)
    {
        if (idsToDelete == null || !idsToDelete.Any())
            throw new ArgumentNullException($"there is no instances to delete");
        
        foreach (var c in idsToDelete)
        {
            if (c == Guid.Empty)
                throw new ArgumentNullException($"missing {c}");

            var exists = await _castRepository.ExistsByIdAsync(c);

            if (!exists)
                throw new ArgumentException($"cannot find cast with Id: {c}");
            
        }

        return await _castRepository.BulkDeleteAsync(idsToDelete);
    }

    public async Task<Guid> CreateCastAsync(CastCreateDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException("nothing to create");
        if (dto.MovieId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(dto.MovieId)}");
        if (dto.PersonId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(dto.PersonId)}");
        if (dto.Character == null)
            throw new ArgumentNullException($"missing {nameof(dto.Character)}");

            
        return await _castRepository.CreateAsync(dto) ??
            throw new InvalidOperationException(message: "could not create cast");
    }


    public async Task<Guid> DeleteCastByIdAsync(Guid? id)
    {
        if (id == null || id == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(id)}");
        
        if (!await _castRepository.ExistsByIdAsync(id.Value))
            throw new ArgumentException($"cannot find cast with id: {id.Value}");

        return await _castRepository.DeleteByIdAsync(id.Value) ??
            throw new InvalidOperationException(message: "could not delete cast");
    }

    public async Task<bool> DeleteCastRangeByMovieIdAsync(Guid? movieId)
    {
        if (movieId == null || movieId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(movieId)}");
            
        return await _castRepository.DeleteRangeById(movieId.Value);
    }

    public async Task<PagedResult<CastGetDto>> GetCastByMovieIdAsync(CastGetByMovieIdDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException($"missing {nameof(dto)}");
        if (dto.MovieId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(dto.MovieId)}");

        return await _castRepository.GetByMovieIdAsync(dto);
    }

    public async Task<PagedResult<CastGetDto>> GetCastByPersonIdAsync(CastGetByPersonIdDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException($"missing {nameof(dto)}");
        if (dto.PersonId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(dto.PersonId)}");
        
        return await _castRepository.GetByPersonIdAsync(dto);
    }

    public async Task<PagedResult<CastGetDto>> SearchCastAsync(CastSearchDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException($"missing {nameof(dto)}");
        if (dto.CharacterName == null && dto.Name == null)
            throw new ArgumentNullException($"no arguments to search by ({nameof(dto.CharacterName)}, {dto.Name})");

        return await _castRepository.SearchAsync(dto);
    }

    public async Task<Guid> UpdateCastAsync(CastUpdateDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException($"missing {nameof(dto)}");
        if (dto.Id == Guid.Empty)
            throw new ArgumentNullException($"missing {dto.Id}");

        if (!await _castRepository.ExistsByIdAsync(dto.Id))
            throw new ArgumentException($"cannot find cast with id: {dto.Id}");

        return await _castRepository.UpdateAsync(dto) ??
            throw new InvalidOperationException(message: "could not update cast");
    }
}
