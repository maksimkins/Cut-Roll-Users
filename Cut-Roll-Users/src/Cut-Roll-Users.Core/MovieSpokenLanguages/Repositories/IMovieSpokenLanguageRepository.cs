namespace Cut_Roll_Users.Core.MovieSpokenLanguages.Repositories;

using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Common.Repositories.Base;
using Cut_Roll_Users.Core.Movies.Dtos;
using Cut_Roll_Users.Core.MovieSpokenLanguages.Dtos;
using Cut_Roll_Users.Core.SpokenLanguages.Models;

public interface IMovieSpokenLanguageRepository : ICreateAsync<MovieSpokenLanguageDto, Guid?>, IDeleteAsync<MovieSpokenLanguageDto, Guid?>,
IDeleteRangeById<Guid, bool>, IBulkCreateAsync<MovieSpokenLanguageDto, bool>, IBulkDeleteAsync<MovieSpokenLanguageDto, bool>
{
    Task<IEnumerable<SpokenLanguage>> GetSpokenLanguagesByMovieIdAsync(Guid movieId);
    Task<bool> ExistsAsync(MovieSpokenLanguageDto dto);
    Task<PagedResult<MovieSimplifiedDto>> GetMoviesBySpokenLanguageIdAsync(MovieSearchBySpokenLanguageDto movieSearchByCountryDto);
}
