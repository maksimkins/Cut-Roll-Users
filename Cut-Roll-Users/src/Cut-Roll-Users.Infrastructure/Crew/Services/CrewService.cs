namespace Cut_Roll_Users.Infrastructure.Crew.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Crews.Dtos;
using Cut_Roll_Users.Core.Crews.Models;
using Cut_Roll_Users.Core.Crews.Repositories;
using Cut_Roll_Users.Core.Crews.Services;

public class CrewService : ICrewService
{
    private readonly ICrewRepository _crewRepository;
    public CrewService(ICrewRepository crewRepository)
    {
        _crewRepository = crewRepository ?? throw new Exception($"{nameof(_crewRepository)}");
    }

    public async Task<bool> BulkCreateCrewAsync(IEnumerable<CrewCreateDto>? toCreate)
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

        return await _crewRepository.BulkCreateAsync(toCreate);
    }

    public async Task<bool> BulkDeleteCrewAsync(IEnumerable<Guid>? idsToDelete)
    {
        if (idsToDelete == null || !idsToDelete.Any())
            throw new ArgumentNullException($"there is no instances to delete");
        
        foreach (var c in idsToDelete)
        {
            if (c == Guid.Empty)
                throw new ArgumentNullException($"missing {c}");

            var exists = await _crewRepository.ExistsByIdAsync(c);

            if (!exists)
                throw new ArgumentException($"cannot find cast with Id: {c}");
            
        }

        return await _crewRepository.BulkDeleteAsync(idsToDelete);
    }

    public async Task<Guid> CreateCrewAsync(CrewCreateDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException("nothing to create");
        if (dto.MovieId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(dto.MovieId)}");
        if (dto.PersonId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(dto.PersonId)}");
        if (string.IsNullOrEmpty(dto.Job))
            throw new ArgumentNullException($"missing {nameof(dto.Job)}");
        if (string.IsNullOrEmpty(dto.Department))
            throw new ArgumentNullException($"missing {nameof(dto.Department)}");

            
        return await _crewRepository.CreateAsync(dto) ??
            throw new InvalidOperationException(message: "could not create crew");
    }

    public async Task<Guid> DeleteCrewByIdAsync(Guid? id)
    {
        if (id == null || id == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(id)}");
        
        if (!await _crewRepository.ExistsByIdAsync(id.Value))
            throw new ArgumentException($"cannot find crew with id: {id.Value}");

        return await _crewRepository.DeleteByIdAsync(id.Value) ??
            throw new InvalidOperationException(message: "could not delete crew");
    }

    public async Task<PagedResult<CrewGetDto>> GetCrewByPersonIdAsync(CrewGetByPersonId? dto)
    {
        if (dto == null)
            throw new ArgumentNullException($"missing {nameof(dto)}");
        if (dto.PersonId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(dto.PersonId)}");
        
        return await _crewRepository.GetByPersonIdAsync(dto);
    }

    public async Task<PagedResult<CrewGetDto>> GetCrewByMovieIdAsync(CrewGetByMovieId? dto)
    {
        if (dto == null)
            throw new ArgumentNullException($"missing {nameof(dto)}");
        if (dto.MovieId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(dto.MovieId)}");

        return await _crewRepository.GetByMovieIdAsync(dto);
    }

    public async Task<PagedResult<CrewGetDto>> SearchCrewAsync(CrewSearchDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException($"missing {nameof(dto)}");
        if (dto.JobTitle == null && dto.Name == null && dto.Department == null)
            throw new ArgumentNullException($"no arguments to search by ({nameof(dto.JobTitle)}, {dto.Name}, {dto.Department})");

        return await _crewRepository.SearchAsync(dto);
    }

    public async Task<Guid> UpdateCrewAsync(CrewUpdateDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException($"missing {nameof(dto)}");
        if (dto.Id == Guid.Empty)
            throw new ArgumentNullException($"missing {dto.Id}");
        if (dto.Job == null && dto.Department == null)
            throw new ArgumentNullException($"no arguments to update ({nameof(dto.Job)}, {dto.Department})");

        if (!await _crewRepository.ExistsByIdAsync(dto.Id))
            throw new ArgumentException($"cannot find crew with Id: {dto.Id}");

        return await _crewRepository.UpdateAsync(dto) ??
            throw new InvalidOperationException(message: "could not update crew");
    }

    public async Task<bool> DeleteCrewRangeByMovieIdAsync(Guid? movieId)
    {
        if (movieId == null || movieId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(movieId)}");
            
        return await _crewRepository.DeleteRangeById(movieId.Value);
    }
}
