namespace Cut_Roll_Users.Infrastructure.People.Services;

using System.Threading.Tasks;
using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Movies.Dtos;
using Cut_Roll_Users.Core.Movies.Models;
using Cut_Roll_Users.Core.People.Dtos;
using Cut_Roll_Users.Core.People.Models;
using Cut_Roll_Users.Core.People.Repositories;
using Cut_Roll_Users.Core.People.Service;

public class PersonService : IPersonService
{
    private readonly IPersonRepository _personRepository;
    public PersonService(IPersonRepository personRepository)
    {
        _personRepository = personRepository ?? throw new Exception(nameof(personRepository));
    }

    public async Task<Guid> CreatePersonAsync(PersonCreateDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException($"missing {nameof(dto)}");
        if (dto.Name == null)
            throw new ArgumentNullException($"missing {nameof(dto.Name)}");


        return await _personRepository.CreateAsync(dto)
            ?? throw new InvalidOperationException($"failed to create {nameof(Person)}");
    }

    public async Task<Guid> DeletePersonByIdAsync(Guid? id)
    {
        if (id == null || id == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(id)}");

        return await _personRepository.DeleteByIdAsync(id.Value)
            ?? throw new InvalidOperationException($"failed to delete {nameof(Person)}");
    }

    public async Task<PagedResult<MovieSimplifiedDto>> GetFilmographyAsync(MovieSearchByPesonIdDto? searchByPersonIdDto)
    {
        if (searchByPersonIdDto == null)
            throw new ArgumentNullException($"missing {nameof(searchByPersonIdDto)}");
        if (searchByPersonIdDto.PersonId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(searchByPersonIdDto.PersonId)}");

        return await _personRepository.GetFilmographyAsync(searchByPersonIdDto);
    }

    public async Task<Person?> GetPersonByIdAsync(Guid? id)
    {
        if (id == null || id == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(id)}");

        return await _personRepository.GetByIdAsync(id.Value);
    }

    public async Task<PagedResult<Person>> SearchPersonAsync(PersonSearchRequest? request)
    {
        if (request == null)
            throw new ArgumentNullException($"missing {nameof(request)}");
        if (string.IsNullOrEmpty(request.Name))
            throw new ArgumentNullException($"missing {nameof(request.Name)}");

        return await _personRepository.SearchAsync(request);
    }

    public async Task<Guid> UpdatePersonAsync(PersonUpdateDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException($"missing {nameof(dto)}");
        if (dto.Id == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(dto.Id)}");
        if (string.IsNullOrEmpty(dto.Name) && string.IsNullOrEmpty(dto.ProfilePath))
            throw new ArgumentNullException($"missing {nameof(dto.Name)} or {nameof(dto.ProfilePath)}");

        return await _personRepository.UpdateAsync(dto) ??
            throw new InvalidOperationException($"failed to update {nameof(Person)}");
    }
}
