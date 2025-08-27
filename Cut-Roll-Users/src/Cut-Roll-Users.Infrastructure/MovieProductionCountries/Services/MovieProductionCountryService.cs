namespace Cut_Roll_Users.Infrastructure.MovieProductionCountries.Services;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Countries.Models;
using Cut_Roll_Users.Core.MovieProductionCountries.Dtos;
using Cut_Roll_Users.Core.MovieProductionCountries.Repositories;
using Cut_Roll_Users.Core.MovieProductionCountries.Service;
using Cut_Roll_Users.Core.Movies.Dtos;
using Cut_Roll_Users.Core.Movies.Models;

public class MovieProductionCountryService : IMovieProductionCountryService
{
    private readonly IMovieProductionCountryRepository _movieProductionCountryRepository;
    public MovieProductionCountryService(IMovieProductionCountryRepository movieProductionCountryRepository)
    {
        _movieProductionCountryRepository = movieProductionCountryRepository ?? throw new Exception($"{nameof(_movieProductionCountryRepository)}");
    }
    public async Task<bool> BulkCreateMovieProductionCountryAsync(IEnumerable<MovieProductionCountryDto>? toCreate)
    {
        if (toCreate == null || !toCreate.Any())
            throw new ArgumentNullException($"there is no instances to create");

        foreach (var c in toCreate)
        {
            if (c == null)
                throw new ArgumentNullException("one of the object is null");
            if (c.MovieId == Guid.Empty)
                throw new ArgumentNullException($"missing {c.MovieId}");
            if (string.IsNullOrEmpty(c.CountryCode))
                throw new ArgumentNullException($"missing {c.CountryCode}");

            var exists = await _movieProductionCountryRepository.ExistsAsync(c);


            if (exists)
                throw new ArgumentException($"movie with id: {c.MovieId} has already possess production country with code: {c.CountryCode}");
        }

        return await _movieProductionCountryRepository.BulkCreateAsync(toCreate);
    }

    public async Task<bool> BulkDeleteMovieProductionCountryAsync(IEnumerable<MovieProductionCountryDto>? toDelete)
    {
        if (toDelete == null || !toDelete.Any())
            throw new ArgumentNullException($"there is no instances to create");
        
        foreach (var c in toDelete)
        {
            if (c == null)
                throw new ArgumentNullException("one of the object is null");
            if (c.MovieId == Guid.Empty)
                throw new ArgumentNullException($"missing {c.MovieId}");
            if (string.IsNullOrEmpty(c.CountryCode))
                throw new ArgumentNullException($"missing {c.CountryCode}");

            var exists = await _movieProductionCountryRepository.ExistsAsync(c);

            if (!exists)
                throw new ArgumentException($"movie with id: {c.MovieId} has not possess production country with code: {c.CountryCode}"); 
        }

        return await _movieProductionCountryRepository.BulkDeleteAsync(toDelete);
    }

    public async Task<Guid> CreateMovieProductionCountryAsync(MovieProductionCountryDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException("nothing to create");
        if (dto.MovieId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(dto.MovieId)}");
        if (string.IsNullOrEmpty(dto.CountryCode))
            throw new ArgumentNullException($"missing {nameof(dto.CountryCode)}");

        if (await _movieProductionCountryRepository.ExistsAsync(dto))
            throw new ArgumentException($"movie with id: {dto.MovieId} has already possess production country with code: {dto.CountryCode}");
            
        return await _movieProductionCountryRepository.CreateAsync(dto) ??
            throw new InvalidOperationException(message: "could not add production country to movie");
    }

    public async Task<Guid> DeleteMovieProductionCountryAsyncMovieProductionCountryAsync(MovieProductionCountryDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException("nothing to create");
        if (dto.MovieId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(dto.MovieId)}");
        if (string.IsNullOrEmpty(dto.CountryCode))
            throw new ArgumentNullException($"missing {nameof(dto.CountryCode)}");

        if (!await _movieProductionCountryRepository.ExistsAsync(dto))
            throw new ArgumentException($"movie with id: {dto.MovieId} has not possess production country with code: {dto.CountryCode}");
        
        return await _movieProductionCountryRepository.CreateAsync(dto) ??
            throw new InvalidOperationException(message: "could not delete production country from movie");
    }

    public async Task<bool> DeleteMovieProductionCountryRangeByMovieId(Guid? movieId)
    {
        if (movieId == null || movieId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(movieId)}");
            
        return await _movieProductionCountryRepository.DeleteRangeById(movieId.Value);
    }

    public async Task<IEnumerable<Country>> GetCountriesByMovieIdAsync(Guid? movieId)
    {
        if (movieId == null || movieId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(movieId)}");
            
        return await _movieProductionCountryRepository.GetCountriesByMovieIdAsync(movieId.Value);
    }

    public async Task<PagedResult<MovieSimplifiedDto>> GetMoviesByCountryIdAsync(MovieSearchByCountryDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException($"missing {nameof(dto)}");
        if (string.IsNullOrEmpty(dto.Iso3166_1))
            throw new ArgumentNullException($"missing {nameof(dto.Iso3166_1)}");
        if (string.IsNullOrEmpty(dto.Name))
            throw new ArgumentNullException($"missing {nameof(dto.Name)}");


        return await _movieProductionCountryRepository.GetMoviesByCountryIdAsync(dto);
    }
}
