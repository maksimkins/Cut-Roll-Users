using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Common.Repositories.Base;
using Cut_Roll_Users.Core.Movies.Dtos;
using Cut_Roll_Users.Core.People.Dtos;
using Cut_Roll_Users.Core.People.Models;

namespace Cut_Roll_Users.Core.People.Repositories;

public interface IPersonRepository : ISearchAsync<PersonSearchRequest, PagedResult<Person>>, IGetByIdAsync<Guid, Person?>,
IUpdateAsync<PersonUpdateDto, Guid?>, IDeleteByIdAsync<Guid, Guid?>, ICreateAsync<PersonCreateDto, Guid?>
{
    Task<PagedResult<MovieSimplifiedDto>> GetFilmographyAsync(MovieSearchByPesonIdDto searchByPersonIdDto);
}
