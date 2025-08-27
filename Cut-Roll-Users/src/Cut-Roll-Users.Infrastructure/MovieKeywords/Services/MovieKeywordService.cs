namespace Cut_Roll_Users.Infrastructure.MovieKeywords.Services;

using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Keywords.Models;
using Cut_Roll_Users.Core.MovieKeywords.Dtos;
using Cut_Roll_Users.Core.MovieKeywords.Repositories;
using Cut_Roll_Users.Core.MovieKeywords.Service;
using Cut_Roll_Users.Core.Movies.Dtos;
using Cut_Roll_Users.Core.Movies.Models;

public class MovieKeywordService : IMovieKeywordService
{
    private readonly IMovieKeywordRepository _movieKeywordRepository;
    public MovieKeywordService(IMovieKeywordRepository movieKeywordRepository)
    {
        _movieKeywordRepository = movieKeywordRepository ?? throw new Exception($"{nameof(_movieKeywordRepository)}");
    }
    
    public async Task<bool> BulkCreateMovieKeywordAsync(IEnumerable<MovieKeywordDto>? toCreate)
    {
        if (toCreate == null || !toCreate.Any())
            throw new ArgumentNullException($"there is no instances to create");

        foreach (var c in toCreate)
        {
            if (c == null)
                throw new ArgumentNullException("one of the object is null");
            if (c.MovieId == Guid.Empty)
                throw new ArgumentNullException($"missing {c.MovieId}");
            if (c.KeywordId == Guid.Empty)
                throw new ArgumentNullException($"missing {c.KeywordId}");

            var exists = await _movieKeywordRepository.ExistsAsync(c);


            if (exists)
                throw new ArgumentException($"movie with id: {c.MovieId} has already possess keyword with id: {c.KeywordId}");
            
        }

        return await _movieKeywordRepository.BulkCreateAsync(toCreate);
    }

    public async Task<bool> BulkDeleteMovieKeywordAsync(IEnumerable<MovieKeywordDto>? toDelete)
    {
        if (toDelete == null || !toDelete.Any())
            throw new ArgumentNullException($"there is no instances to create");
        
                foreach (var c in toDelete)
        {
            if (c == null)
                throw new ArgumentNullException("one of the object is null");
            if (c.MovieId == Guid.Empty)
                throw new ArgumentNullException($"missing {c.MovieId}");
            if (c.KeywordId == Guid.Empty)
                throw new ArgumentNullException($"missing {c.KeywordId}");

            var exists = await _movieKeywordRepository.ExistsAsync(c);

            if (!exists)
                throw new ArgumentException($"movie with id: {c.MovieId} has not possess keyword with id: {c.KeywordId}");
            
        }

        return await _movieKeywordRepository.BulkDeleteAsync(toDelete);
    }

    public async Task<Guid> CreateMovieKeywordAsync(MovieKeywordDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException("nothing to create");
        if (dto.MovieId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(dto.MovieId)}");
        if (dto.KeywordId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(dto.KeywordId)}");

        if (await _movieKeywordRepository.ExistsAsync(dto))
            throw new ArgumentException($"movie with id: {dto.MovieId} has already possess keyword with id: {dto.KeywordId}");
            
        return await _movieKeywordRepository.CreateAsync(dto) ??
            throw new InvalidOperationException(message: "could not add keyword to movie");
    }

    public async Task<Guid> DeleteMovieKeywordAsync(MovieKeywordDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException("nothing to delete");
        if (dto.MovieId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(dto.MovieId)}");
        if (dto.KeywordId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(dto.KeywordId)}");
        
        if (!await _movieKeywordRepository.ExistsAsync(dto))
            throw new ArgumentException($"movie with id: {dto.MovieId} has not possess keyword with id: {dto.KeywordId}");

        return await _movieKeywordRepository.DeleteAsync(dto) ??
            throw new InvalidOperationException(message: "could not remove keyword from movie");
    }

    public async Task<bool> DeleteMovieKeywordRangeByMovieId(Guid? movieId)
    {
        if (movieId == null || movieId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(movieId)}");
            
        return await _movieKeywordRepository.DeleteRangeById(movieId.Value);
    }

    public async Task<IEnumerable<Keyword>> GetKeywordsByMovieIdAsync(Guid? movieId)
    {
        if (movieId == null || movieId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(movieId)}");
            
        return await _movieKeywordRepository.GetKeywordsByMovieIdAsync(movieId.Value);
    }

    public async Task<PagedResult<MovieSimplifiedDto>> GetMoviesByKeywordIdAsync(MovieSearchByKeywordDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException($"missing {nameof(dto)}");
        if (dto.KeywordId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(dto.KeywordId)}");
        if (dto.Name == null)
            throw new ArgumentNullException($"missing {nameof(dto.Name)}");


        return await _movieKeywordRepository.GetMoviesByKeywordIdAsync(dto);
    }
}
