namespace Cut_Roll_Users.Infrastructure.MovieSpokenLanguages.Services;

using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Movies.Dtos;
using Cut_Roll_Users.Core.Movies.Models;
using Cut_Roll_Users.Core.MovieSpokenLanguages.Dtos;
using Cut_Roll_Users.Core.MovieSpokenLanguages.Repositories;
using Cut_Roll_Users.Core.MovieSpokenLanguages.Service;
using Cut_Roll_Users.Core.SpokenLanguages.Models;

public class MovieSpokenLanguageService : IMovieSpokenLanguageService
{
    private readonly IMovieSpokenLanguageRepository _movieSpokenLanguageRepository;
    public MovieSpokenLanguageService(IMovieSpokenLanguageRepository movieSpokenLanguageRepository)
    {
        _movieSpokenLanguageRepository = movieSpokenLanguageRepository ?? throw new Exception($"{nameof(_movieSpokenLanguageRepository)}");
    }
    public async Task<bool> BulkCreateMovieSpokenLaguageAsync(IEnumerable<MovieSpokenLanguageDto>? toCreate)
    {
        if (toCreate == null || !toCreate.Any())
            throw new ArgumentNullException($"there is no instances to create");

        foreach (var c in toCreate)
        {
            if (c == null)
                throw new ArgumentNullException("one of the object is null");
            if (c.MovieId == Guid.Empty)
                throw new ArgumentNullException($"missing {c.MovieId}");
            if (string.IsNullOrEmpty(c.LanguageCode))
                throw new ArgumentNullException($"missing {c.LanguageCode}");

            var exists = await _movieSpokenLanguageRepository.ExistsAsync(c);


            if (exists)
                throw new ArgumentException($"movie with id: {c.MovieId} has already possess spoken language with code: {c.LanguageCode}");
        }

        return await _movieSpokenLanguageRepository.BulkCreateAsync(toCreate);
    }

    public async Task<bool> BulkDeleteeMovieSpokenLaguageAsync(IEnumerable<MovieSpokenLanguageDto>? toDelete)
    {
        if (toDelete == null || !toDelete.Any())
            throw new ArgumentNullException($"there is no instances to delete");
        
        foreach (var c in toDelete)
        {
            if (c == null)
                throw new ArgumentNullException("one of the object is null");
            if (c.MovieId == Guid.Empty)
                throw new ArgumentNullException($"missing {c.MovieId}");
            if (string.IsNullOrEmpty(c.LanguageCode))
                throw new ArgumentNullException($"missing {c.LanguageCode}");

            var exists = await _movieSpokenLanguageRepository.ExistsAsync(c);

            if (!exists)
                throw new ArgumentException($"movie with id: {c.MovieId} has not possess spoken language with code: {c.LanguageCode}"); 
        }

        return await _movieSpokenLanguageRepository.BulkDeleteAsync(toDelete);
    }

    public async Task<Guid> CreateMovieSpokenLanguageAsync(MovieSpokenLanguageDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException($"missing {nameof(dto)}");
        if (dto.MovieId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(dto.MovieId)}");
        if (string.IsNullOrEmpty(dto.LanguageCode))
            throw new ArgumentNullException($"missing {nameof(dto.LanguageCode)}");

        if (await _movieSpokenLanguageRepository.ExistsAsync(dto))
            throw new ArgumentException($"movie with id: {dto.MovieId} has already possess spoken language with code: {dto.LanguageCode}");
            
        return await _movieSpokenLanguageRepository.CreateAsync(dto) ??
            throw new InvalidOperationException(message: "could not add production country to movie");
    }

    public async Task<Guid> DeleteMovieSpokenLanguageAsync(MovieSpokenLanguageDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException($"missing {nameof(dto)}");
        if (dto.MovieId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(dto.MovieId)}");
        if (string.IsNullOrEmpty(dto.LanguageCode))
            throw new ArgumentNullException($"missing {nameof(dto.LanguageCode)}");

        if (!await _movieSpokenLanguageRepository.ExistsAsync(dto))
            throw new ArgumentException($"movie with id: {dto.MovieId} has not possess spoken language with code: {dto.LanguageCode}");
        
        return await _movieSpokenLanguageRepository.CreateAsync(dto) ??
            throw new InvalidOperationException(message: "could not delete production country from movie");
    }

    public async Task<bool> DeleteMovieSpokenLanguageRangeByMovieId(Guid? movieId)
    {
        if (movieId == null || movieId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(movieId)}");
            
        return await _movieSpokenLanguageRepository.DeleteRangeById(movieId.Value);
    }

    public async Task<PagedResult<MovieSimplifiedDto>> GetMoviesBySpokenLanguageIdAsync(MovieSearchBySpokenLanguageDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException($"missing {nameof(dto)}");
        if (string.IsNullOrEmpty(dto.Iso639_1))
            throw new ArgumentNullException($"missing {nameof(dto.Iso639_1)}");
        if (string.IsNullOrEmpty(dto.EnglishName))
            throw new ArgumentNullException($"missing {nameof(dto.EnglishName)}");


        return await _movieSpokenLanguageRepository.GetMoviesBySpokenLanguageIdAsync(dto);
    }

    public async Task<IEnumerable<SpokenLanguage>> GetSpokenLanguagesByMovieIdAsync(Guid? movieId)
    {
        if (movieId == null || movieId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(movieId)}");
            
        return await _movieSpokenLanguageRepository.GetSpokenLanguagesByMovieIdAsync(movieId.Value);
    }
}
