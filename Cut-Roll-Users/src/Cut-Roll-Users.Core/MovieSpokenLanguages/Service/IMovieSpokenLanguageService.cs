namespace Cut_Roll_Users.Core.MovieSpokenLanguages.Service;

using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Movies.Dtos;
using Cut_Roll_Users.Core.MovieSpokenLanguages.Dtos;
using Cut_Roll_Users.Core.SpokenLanguages.Models;

public interface IMovieSpokenLanguageService
{
    Task<Guid> CreateMovieSpokenLanguageAsync(MovieSpokenLanguageDto? dto);
    Task<Guid> DeleteMovieSpokenLanguageAsync(MovieSpokenLanguageDto? dto);
    Task<bool> DeleteMovieSpokenLanguageRangeByMovieId(Guid? movieId);
    Task<IEnumerable<SpokenLanguage>> GetSpokenLanguagesByMovieIdAsync(Guid? movieId);
    Task<PagedResult<MovieSimplifiedDto>> GetMoviesBySpokenLanguageIdAsync(MovieSearchBySpokenLanguageDto? movieSearchByCountryDto);
    Task<bool> BulkCreateMovieSpokenLaguageAsync(IEnumerable<MovieSpokenLanguageDto>? toCreate);
    Task<bool> BulkDeleteeMovieSpokenLaguageAsync(IEnumerable<MovieSpokenLanguageDto>? toDelete);
}

