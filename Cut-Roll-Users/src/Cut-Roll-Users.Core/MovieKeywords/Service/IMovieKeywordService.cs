namespace Cut_Roll_Users.Core.MovieKeywords.Service;

using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Keywords.Models;
using Cut_Roll_Users.Core.MovieKeywords.Dtos;
using Cut_Roll_Users.Core.Movies.Dtos;

public interface IMovieKeywordService
{
    Task<Guid> CreateMovieKeywordAsync(MovieKeywordDto? dto);
    Task<Guid> DeleteMovieKeywordAsync(MovieKeywordDto? dto);
    Task<bool> DeleteMovieKeywordRangeByMovieId(Guid? movieId);
    Task<IEnumerable<Keyword>> GetKeywordsByMovieIdAsync(Guid? movieId);
    Task<PagedResult<MovieSimplifiedDto>> GetMoviesByKeywordIdAsync(MovieSearchByKeywordDto? searchDto);
    Task<bool> BulkCreateMovieKeywordAsync(IEnumerable<MovieKeywordDto>? toCreate);
    Task<bool> BulkDeleteMovieKeywordAsync(IEnumerable<MovieKeywordDto>? toDelete);
}


