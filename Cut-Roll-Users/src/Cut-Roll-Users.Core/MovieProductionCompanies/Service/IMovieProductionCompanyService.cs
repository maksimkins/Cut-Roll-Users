namespace Cut_Roll_Users.Core.MovieProductionCompanies.Service;

using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.MovieProductionCompanies.Dtos;
using Cut_Roll_Users.Core.Movies.Dtos;
using Cut_Roll_Users.Core.ProductionCompanies.Models;

public interface IMovieProductionCompanyService
{
    Task<Guid> CreateMovieProductionCompanyAsync(MovieProductionCompanyDto? dto);
    Task<Guid> DeleteMovieProductionCompanyAsync(MovieProductionCompanyDto? dto);
    Task<bool> DeleteMovieProductionCompanyRangeByMovieIdAsync(Guid? movieId);
    Task<IEnumerable<ProductionCompany>> GetCompaniesByMovieIdAsync(Guid? movieId);
    Task<PagedResult<MovieSimplifiedDto>> GetMoviesByCompanyIdAsync(MovieSearchByCompanyDto? movieSearchByCompanyDto);
    Task<bool> BulkCreateMovieProductionCompanyAsync(IEnumerable<MovieProductionCompanyDto>? toCreate);
    Task<bool> BulkDeleteMovieProductionCompanyAsync(IEnumerable<MovieProductionCompanyDto>? toDelete);
}
