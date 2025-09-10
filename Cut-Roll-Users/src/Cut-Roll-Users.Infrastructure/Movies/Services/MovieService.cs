namespace Cut_Roll_Users.Infrastructure.Movies.Services;

using System.Collections.Generic;
using Cut_Roll_Users.Core.Casts.Repositories;
using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Crews.Repositories;
using Cut_Roll_Users.Core.MovieGenres.Repositories;
using Cut_Roll_Users.Core.MovieImages.Repositories;
using Cut_Roll_Users.Core.MovieKeywords.Repositories;
using Cut_Roll_Users.Core.MovieOriginCountries.Repository;
using Cut_Roll_Users.Core.MovieProductionCompanies.Repositories;
using Cut_Roll_Users.Core.MovieProductionCountries.Repositories;
using Cut_Roll_Users.Core.Movies.Dtos;
using Cut_Roll_Users.Core.Movies.Models;
using Cut_Roll_Users.Core.Movies.Repositories;
using Cut_Roll_Users.Core.Movies.Service;
using Cut_Roll_Users.Core.MovieSpokenLanguages.Repositories;
using Cut_Roll_Users.Core.MovieVideos.Repositories;

public class MovieService : IMovieService
{
    private readonly IMovieRepository _movieRepository;
    private readonly ICastRepository _castRepository;
    private readonly ICrewRepository _crewRepository;
    private readonly IMovieGenreRepository _movieGenreRepository;
    private readonly IMovieKeywordRepository _movieKeywordRepository;
    private readonly IMovieProductionCompanyRepository _movieProductionCompanyRepository;
    private readonly IMovieProductionCountryRepository _movieProductionCountryRepository;
    private readonly IMovieOriginCountryRepository _movieOriginCountryRepository;
    private readonly IMovieSpokenLanguageRepository _movieSpokenLanguageRepository;
    private readonly IMovieVideoRepository _movieVideoRepository;
    private readonly IMovieImageRepository _movieImageRepository;

    public MovieService(
        IMovieRepository movieRepository,
        ICastRepository castRepository,
        ICrewRepository crewRepository,
        IMovieGenreRepository movieGenreRepository,
        IMovieKeywordRepository movieKeywordRepository,
        IMovieProductionCompanyRepository movieProductionCompanyRepository,
        IMovieProductionCountryRepository movieProductionCountryRepository,
        IMovieOriginCountryRepository movieOriginCountryRepository,
        IMovieSpokenLanguageRepository movieSpokenLanguageRepository,
        IMovieVideoRepository movieVideoRepository,
        IMovieImageRepository movieImageRepository)
    {
        _movieRepository = movieRepository;
        _castRepository = castRepository;
        _crewRepository = crewRepository;
        _movieGenreRepository = movieGenreRepository;
        _movieKeywordRepository = movieKeywordRepository;
        _movieProductionCompanyRepository = movieProductionCompanyRepository;
        _movieProductionCountryRepository = movieProductionCountryRepository;
        _movieOriginCountryRepository = movieOriginCountryRepository;
        _movieSpokenLanguageRepository = movieSpokenLanguageRepository;
        _movieVideoRepository = movieVideoRepository;
        _movieImageRepository = movieImageRepository;
    }
    public async Task<int> CountMoviesAsync()
    {
        return await _movieRepository.CountAsync();
    }

    public async Task<Guid> CreateMovieAsync(MovieCreateDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        var movieId = await _movieRepository.CreateAsync(dto);
        if (movieId == null)
            throw new InvalidOperationException("Movie creation failed.");

        return movieId.Value;
    }

    public async Task<Guid> DeleteMovieByIdAsync(Guid? id)
    {
        if (id == null || id == Guid.Empty)
            throw new ArgumentNullException(nameof(id));

        var movieId = await _movieRepository.DeleteByIdAsync(id.Value);
        if (movieId == null)
            throw new InvalidOperationException("Movie not found or deletion failed.");

        return movieId.Value;
    }

    public async Task<List<Movie>> GetLikedMoviesByUserIdAsync(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));

        return await _movieRepository.GetLikedMoviesByUserIdAsync(userId);
    }

    public async Task<double> GetMovieAverageRatingAsync(Guid? id)
    {
        if (id == null || id == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(id)}");

        return await _movieRepository.GetMovieAverageRatingAsync(id.Value);
    }

    public async Task<Movie?> GetMovieByIdAsync(Guid? id)
    {
        if (id == null || id == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(id)}");

        return await _movieRepository.GetByIdAsync(id.Value);
    }

    public async Task<int> GetMovieLikeCountAsync(Guid? movieId)
    {
        if (movieId == null || movieId == Guid.Empty)
            throw new ArgumentNullException(nameof(movieId));

        return await _movieRepository.GetMovieLikeCountAsync(movieId.Value);
    }

    public async Task<int> GetMovieReviewCountAsync(Guid? movieId)
    {
        if (movieId == null || movieId == Guid.Empty)
            throw new ArgumentNullException(nameof(movieId));

        return await _movieRepository.GetMovieReviewCountAsync(movieId.Value);
    }

    public async Task<List<Movie>> GetMoviesWithoutEmbeddingsAsync(int offset, int limit)
    {
        return await _movieRepository.GetMoviesWithoutEmbeddingsAsync(offset, limit);
    }

    public async Task<int> GetMoviesWithoutEmbeddingsCountAsync()
    {
        return await _movieRepository.GetMoviesWithoutEmbeddingsCountAsync();
    }

    public async Task<List<Movie>> GetMoviesWithPaginationAsync(int offset, int limit)
    {
        return await _movieRepository.GetMoviesWithPaginationAsync(offset, limit);
    }

    public async Task<int> GetMovieWantToWatchCountAsync(Guid? movieId)
    {
        if (movieId == null || movieId == Guid.Empty)
            throw new ArgumentNullException(nameof(movieId));

        return await _movieRepository.GetMovieWantToWatchCountAsync(movieId.Value);
    }

    public async Task<int> GetMovieWatchedCountAsync(Guid? movieId)
    {
        if (movieId == null || movieId == Guid.Empty)
            throw new ArgumentNullException(nameof(movieId));

        return await _movieRepository.GetMovieWatchedCountAsync(movieId.Value);
    }

    public async Task<List<Movie>> GetWatchedMoviesByUserIdAsync(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));

        return await _movieRepository.GetWatchedMoviesByUserIdAsync(userId);
    }

    public async Task<List<Movie>> GetLikedMoviesByUserIdAsync(string? userId, int offset, int limit)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));
        if (offset < 0)
            throw new ArgumentException("Offset cannot be negative.", nameof(offset));
        if (limit <= 0)
            throw new ArgumentException("Limit must be positive.", nameof(limit));

        return await _movieRepository.GetLikedMoviesByUserIdAsync(userId, offset, limit);
    }

    public async Task<List<Movie>> GetWatchedMoviesByUserIdAsync(string? userId, int offset, int limit)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));
        if (offset < 0)
            throw new ArgumentException("Offset cannot be negative.", nameof(offset));
        if (limit <= 0)
            throw new ArgumentException("Limit must be positive.", nameof(limit));

        return await _movieRepository.GetWatchedMoviesByUserIdAsync(userId, offset, limit);
    }

    public async Task<int> GetLikedMoviesCountByUserIdAsync(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));

        return await _movieRepository.GetLikedMoviesCountByUserIdAsync(userId);
    }

    public async Task<int> GetWatchedMoviesCountByUserIdAsync(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));

        return await _movieRepository.GetWatchedMoviesCountByUserIdAsync(userId);
    }

    public async Task<bool> IsMovieInUserWantToWatchAsync(Guid? movieId, string? userId)
    {
        if (movieId == null || movieId == Guid.Empty)
            throw new ArgumentNullException(nameof(movieId));
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));

        return await _movieRepository.IsMovieInUserWantToWatchAsync(movieId.Value, userId);
    }

    public async Task<bool> IsMovieLikedByUserAsync(Guid? movieId, string? userId)
    {
        if (movieId == null || movieId == Guid.Empty)
            throw new ArgumentNullException(nameof(movieId));
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));

        return await _movieRepository.IsMovieLikedByUserAsync(movieId.Value, userId);
    }

    public async Task<bool> IsMovieWatchedByUserAsync(Guid? movieId, string? userId)
    {
        if (movieId == null || movieId == Guid.Empty)
            throw new ArgumentNullException(nameof(movieId));
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));

        return await _movieRepository.IsMovieWatchedByUserAsync(movieId.Value, userId);
    }

    public async Task<bool> MarkMovieAsEmbeddedAsync(Guid movieId)
    {
        return await _movieRepository.MarkMovieAsEmbeddedAsync(movieId);
    }

    public async Task<bool> MarkMovieAsNotEmbeddedAsync(Guid movieId)
    {
        return await _movieRepository.MarkMovieAsNotEmbeddedAsync(movieId);
    }

    public async Task<PagedResult<MovieSimplifiedDto>> SearchMovieAsync(MovieSearchRequest? dto)
    {
        if (dto == null)
            throw new ArgumentNullException($"missing {nameof(dto)}");

        return await _movieRepository.SearchAsync(dto);
    }

    public async Task<Guid> UpdateMovieAsync(MovieUpdateDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException($"missing {nameof(dto)}");

        return await _movieRepository.UpdateAsync(dto)
            ?? throw new InvalidOperationException("Movie update failed.");
    }

    public async Task<string?> GetMoviePosterPathAsync(Guid movieId)
    {
        if (movieId == Guid.Empty)
            throw new ArgumentNullException(nameof(movieId));
            
        return await _movieRepository.GetMoviePosterPathAsync(movieId);
    }
}
