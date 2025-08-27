namespace Cut_Roll_Users.Infrastructure.MovieGenres.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Genres.Models;
using Cut_Roll_Users.Core.MovieGenres.Dtos;
using Cut_Roll_Users.Core.MovieGenres.Repositories;
using Cut_Roll_Users.Core.MovieGenres.Services;
using Cut_Roll_Users.Core.Movies.Dtos;
using Cut_Roll_Users.Core.Movies.Models;

public class MovieGenreService : IMovieGenreService
{
    private readonly IMovieGenreRepository _movieGenreRepository;

    public MovieGenreService(IMovieGenreRepository movieGenreRepository)
    {
        _movieGenreRepository = movieGenreRepository ?? throw new Exception($"{nameof(_movieGenreRepository)}");
    }

    public async Task<bool> BulkCreateMovieGenreAsync(IEnumerable<MovieGenreDto>? toCreate)
    {
        if (toCreate == null || !toCreate.Any())
            throw new ArgumentNullException($"there is no instances to create");

        foreach (var c in toCreate)
        {
            if (c == null)
                throw new ArgumentNullException("one of the object is null");
            if (c.MovieId == Guid.Empty)
                throw new ArgumentException($"missing {c.MovieId}");
            if (c.GenreId == Guid.Empty)
                throw new ArgumentException($"missing {c.GenreId}");

            var exists = await _movieGenreRepository.ExistsAsync(c);


            if (exists)
                throw new ArgumentException($"movie with id: {c.MovieId} has already possess genre with id: {c.GenreId}");
        }

        return await _movieGenreRepository.BulkCreateAsync(toCreate);
    }

    public async Task<bool> BulkDeleteMovieGenreAsync(IEnumerable<MovieGenreDto>? toDelete)
    {
        if (toDelete == null || !toDelete.Any())
            throw new ArgumentNullException($"there is no instances to create");
        
                foreach (var c in toDelete)
        {
            if (c == null)
                throw new ArgumentNullException("one of the object is null");
            if (c.MovieId == Guid.Empty)
                throw new ArgumentNullException($"missing {c.MovieId}");
            if (c.GenreId == Guid.Empty)
                throw new ArgumentNullException($"missing {c.GenreId}");

            var exists = await _movieGenreRepository.ExistsAsync(c);

            if (!exists)
                throw new ArgumentException($"movie with id: {c.MovieId} has not possess genre with id: {c.GenreId}");
            
        }

        return await _movieGenreRepository.BulkDeleteAsync(toDelete);
    }

    public async Task<Guid> CreateMovieGenreAsync(MovieGenreDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException("nothing to create");
        if (dto.MovieId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(dto.MovieId)}");
        if (dto.GenreId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(dto.GenreId)}");

        if (await _movieGenreRepository.ExistsAsync(dto))
            throw new ArgumentException($"movie with id: {dto.MovieId} has already possess genre with id: {dto.GenreId}");
            
        return await _movieGenreRepository.CreateAsync(dto) ??
            throw new InvalidOperationException(message: "could not add genre to movie");
    }

    public async Task<Guid?> DeleteMovieGenreAsync(MovieGenreDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException("nothing to delete");
        if (dto.MovieId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(dto.MovieId)}");
        if (dto.GenreId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(dto.GenreId)}");
        
        if (!await _movieGenreRepository.ExistsAsync(dto))
            throw new ArgumentException($"movie with id: {dto.MovieId} has not possess genre with id: {dto.GenreId}");

        return await _movieGenreRepository.DeleteAsync(dto) ??
            throw new InvalidOperationException(message: "could not remove genre from movie");
    }

    public async Task<bool> DeleteMovieGenreRangeByMovieId(Guid? movieId)
    {
        if (movieId == null || movieId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(movieId)}");
            
        return await _movieGenreRepository.DeleteRangeById(movieId.Value);
    }

    public async Task<IEnumerable<Genre>> GetGenresByMovieIdAsync(Guid? movieId)
    {
        if (movieId == null || movieId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(movieId)}");
            
        return await _movieGenreRepository.GetGenresByMovieIdAsync(movieId.Value);
    }

    public async Task<PagedResult<MovieSimplifiedDto>> GetMoviesByGenreIdAsync(MovieSearchByGenreDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException($"missing {nameof(dto)}");
        if (dto.GenreId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(dto.GenreId)}");
        if (dto.Name == null)
            throw new ArgumentNullException($"missing {nameof(dto.Name)}");


        return await _movieGenreRepository.GetMoviesByGenreIdAsync(dto);
    }
}
