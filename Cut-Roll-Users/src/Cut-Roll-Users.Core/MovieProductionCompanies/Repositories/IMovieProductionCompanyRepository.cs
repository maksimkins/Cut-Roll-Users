namespace Cut_Roll_Users.Core.MovieProductionCompanies.Repositories;

using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Common.Repositories.Base;
using Cut_Roll_Users.Core.MovieProductionCompanies.Dtos;
using Cut_Roll_Users.Core.Movies.Dtos;
using Cut_Roll_Users.Core.ProductionCompanies.Models;

public interface IMovieProductionCompanyRepository : ICreateAsync<MovieProductionCompanyDto, Guid?>, IDeleteAsync<MovieProductionCompanyDto, Guid?>,
IDeleteRangeById<Guid, bool>, IBulkCreateAsync<MovieProductionCompanyDto, bool>, IBulkDeleteAsync<MovieProductionCompanyDto, bool>
{
    Task<IEnumerable<ProductionCompany>> GetCompaniesByMovieIdAsync(Guid movieId);
    Task<PagedResult<MovieSimplifiedDto>> GetMoviesByCompanyIdAsync(MovieSearchByCompanyDto movieSearchByCompanyDto);
    Task<bool> ExistsAsync(MovieProductionCompanyDto dto);
}
