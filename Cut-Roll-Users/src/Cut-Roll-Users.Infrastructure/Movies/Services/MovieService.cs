namespace Cut_Roll_Users.Infrastructure.Movies.Services;

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

    public Task<Guid> CreateMovieAsync(MovieCreateDto? dto)
    {
        throw new NotImplementedException();
    }

    public Task<Guid> DeleteMovieByIdAsync(Guid? id)
    {
        throw new NotImplementedException();
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
}
