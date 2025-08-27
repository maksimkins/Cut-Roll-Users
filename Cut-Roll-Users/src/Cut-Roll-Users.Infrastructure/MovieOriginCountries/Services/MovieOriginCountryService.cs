namespace Cut_Roll_Users.Infrastructure.MovieOriginCountries.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Countries.Models;
using Cut_Roll_Users.Core.MovieOriginCountries.Dtos;
using Cut_Roll_Users.Core.MovieOriginCountries.Repository;
using Cut_Roll_Users.Core.MovieOriginCountries.Service;
using Cut_Roll_Users.Core.Movies.Dtos;
using Cut_Roll_Users.Core.Movies.Models;

public class MovieOriginCountryService : IMovieOriginCountryService
{
    private readonly IMovieOriginCountryRepository _movieOriginCountryRepository;
    public MovieOriginCountryService(IMovieOriginCountryRepository movieOriginCountryRepository)
    {
        _movieOriginCountryRepository = movieOriginCountryRepository ?? throw new Exception($"{nameof(_movieOriginCountryRepository)}");
    }

    public async Task<bool> BulkCreateMovieOriginCountryAsync(IEnumerable<MovieOriginCountryDto>? toCreate)
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

            var exists = await _movieOriginCountryRepository.ExistsAsync(c);


            if (exists)
                throw new ArgumentException($"movie with id: {c.MovieId} has already possess origin country with code: {c.CountryCode}");
        }

        return await _movieOriginCountryRepository.BulkCreateAsync(toCreate);
    }

    public async Task<bool> BulkDeleteMovieOriginCountryAsync(IEnumerable<MovieOriginCountryDto>? toDelete)
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

            var exists = await _movieOriginCountryRepository.ExistsAsync(c);

            if (!exists)
                throw new ArgumentException($"movie with id: {c.MovieId} has not possess origin country with code: {c.CountryCode}");
            
        }

        return await _movieOriginCountryRepository.BulkDeleteAsync(toDelete);
    }

    public async Task<Guid> CreateMovieOriginCountryAsync(MovieOriginCountryDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException("nothing to create");
        if (dto.MovieId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(dto.MovieId)}");
        if (string.IsNullOrEmpty(dto.CountryCode))
            throw new ArgumentNullException($"missing {nameof(dto.CountryCode)}");

        if (await _movieOriginCountryRepository.ExistsAsync(dto))
            throw new ArgumentException($"movie with id: {dto.MovieId} has already possess origin country with code: {dto.CountryCode}");
            
        return await _movieOriginCountryRepository.CreateAsync(dto) ??
            throw new InvalidOperationException(message: "could not add origin country to movie");
    }

    public async Task<Guid> DeleteMovieOriginCountryAsync(MovieOriginCountryDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException("nothing to create");
        if (dto.MovieId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(dto.MovieId)}");
        if (string.IsNullOrEmpty(dto.CountryCode))
            throw new ArgumentNullException($"missing {nameof(dto.CountryCode)}");

        if (!await _movieOriginCountryRepository.ExistsAsync(dto))
            throw new ArgumentException($"movie with id: {dto.MovieId} has not possess origin country with code: {dto.CountryCode}");
        
        return await _movieOriginCountryRepository.CreateAsync(dto) ??
            throw new InvalidOperationException(message: "could not delete origin country from movie");
    }

    public async Task<bool> DeleteMovieOriginCountryRangeByMovieIdAsync(Guid? movieId)
    {
        if (movieId == null || movieId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(movieId)}");
            
        return await _movieOriginCountryRepository.DeleteRangeById(movieId.Value);
    }

    public async Task<IEnumerable<Country>> GetCountriesByMovieIdAsync(Guid? movieId)
    {
        if (movieId == null || movieId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(movieId)}");
            
        return await _movieOriginCountryRepository.GetCountriesByMovieIdAsync(movieId.Value);
    }

    public async Task<PagedResult<MovieSimplifiedDto>> GetMoviesByOriginCountryIdAsync(MovieSearchByCountryDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException($"missing {nameof(dto)}");
        if (string.IsNullOrEmpty(dto.Iso3166_1))
            throw new ArgumentNullException($"missing {nameof(dto.Iso3166_1)}");
        if (string.IsNullOrEmpty(dto.Name))
            throw new ArgumentNullException($"missing {nameof(dto.Name)}");


        return await _movieOriginCountryRepository.GetMoviesByOriginCountryIdAsync(dto);
    }
}
