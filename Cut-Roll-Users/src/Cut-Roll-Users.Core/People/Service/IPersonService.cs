namespace Cut_Roll_Users.Core.People.Service;

using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Movies.Dtos;
using Cut_Roll_Users.Core.Movies.Models;
using Cut_Roll_Users.Core.People.Dtos;
using Cut_Roll_Users.Core.People.Models;

public interface IPersonService
{
    Task<PagedResult<Person>> SearchPersonAsync(PersonSearchRequest? request);
    Task<Person?> GetPersonByIdAsync(Guid? id);
    Task<Guid> UpdatePersonAsync(PersonUpdateDto? dto);
    Task<Guid> DeletePersonByIdAsync(Guid? id);
    Task<Guid> CreatePersonAsync(PersonCreateDto? dto);
    Task<PagedResult<MovieSimplifiedDto>> GetFilmographyAsync(MovieSearchByPesonIdDto? searchByPersonIdDto);


}

