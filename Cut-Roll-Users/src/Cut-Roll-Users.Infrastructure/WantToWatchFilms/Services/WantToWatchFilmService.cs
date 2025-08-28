using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Movies.Dtos;
using Cut_Roll_Users.Core.WantToWatchFilms.Dtos;
using Cut_Roll_Users.Core.WantToWatchFilms.Repositories;
using Cut_Roll_Users.Core.WantToWatchFilms.Services;

namespace Cut_Roll_Users.Infrastructure.WantToWatchFilms.Services;

public class WantToWatchFilmService : IWantToWatchFilmService
{
    private readonly IWantToWatchFilmRepository _wantToWatchFilmRepository;

    public WantToWatchFilmService(IWantToWatchFilmRepository wantToWatchFilmRepository)
    {
        _wantToWatchFilmRepository = wantToWatchFilmRepository;
    }

    public async Task<Guid> AddMovieToWantToWatchAsync(WantToWatchFilmDto? wantToWatchDto)
    {
        if (wantToWatchDto == null)
            throw new ArgumentNullException(nameof(wantToWatchDto), "Want to watch data cannot be null.");
        
        if (string.IsNullOrWhiteSpace(wantToWatchDto.UserId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(wantToWatchDto.UserId));
        
        if (wantToWatchDto.MovieId == Guid.Empty)
            throw new ArgumentException("Movie ID cannot be empty.", nameof(wantToWatchDto.MovieId));

      
        var isAlreadyInWantToWatch = await _wantToWatchFilmRepository.IsMovieInWantToWatchByUserAsync(wantToWatchDto.UserId, wantToWatchDto.MovieId);
        if (isAlreadyInWantToWatch)
            return wantToWatchDto.MovieId; 

        var result = await _wantToWatchFilmRepository.CreateAsync(wantToWatchDto);
        return result ?? throw new InvalidOperationException("Failed to add movie to want to watch.");
    }

    public async Task<Guid> RemoveMovieFromWantToWatchAsync(WantToWatchFilmDto? wantToWatchDto)
    {
        if (wantToWatchDto == null)
            throw new ArgumentNullException(nameof(wantToWatchDto), "Remove from want to watch data cannot be null.");
        
        if (string.IsNullOrWhiteSpace(wantToWatchDto.UserId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(wantToWatchDto.UserId));
        
        if (wantToWatchDto.MovieId == Guid.Empty)
            throw new ArgumentException("Movie ID cannot be empty.", nameof(wantToWatchDto.MovieId));

        
        var isInWantToWatch = await _wantToWatchFilmRepository.IsMovieInWantToWatchByUserAsync(wantToWatchDto.UserId, wantToWatchDto.MovieId);
        if (!isInWantToWatch)
            return wantToWatchDto.MovieId;

        var result = await _wantToWatchFilmRepository.DeleteAsync(wantToWatchDto);
        if (result == null)
            throw new InvalidOperationException($"WantToWatchFilm not found for UserId: {wantToWatchDto.UserId} and MovieId: {wantToWatchDto.MovieId}");
        
        return result.Value;
    }

    public async Task<PagedResult<MovieSimplifiedDto>> GetWantToWatchFilmsByUserIdAsync(WantToWatchFilmPaginationUserDto? paginationDto)
    {
        if (paginationDto == null)
            throw new ArgumentNullException(nameof(paginationDto), "Pagination data cannot be null.");
        
        if (string.IsNullOrWhiteSpace(paginationDto.UserId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(paginationDto.UserId));

        return await _wantToWatchFilmRepository.GetByUserIdAsync(paginationDto);
    }

    public async Task<bool> IsMovieInWantToWatchAsync(string? userId, Guid? movieId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
        
        if (!movieId.HasValue || movieId.Value == Guid.Empty)
            throw new ArgumentException("Movie ID cannot be null or empty.", nameof(movieId));

        return await _wantToWatchFilmRepository.IsMovieInWantToWatchByUserAsync(userId, movieId.Value);
    }

    public async Task<int> GetWantToWatchCountByUserIdAsync(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

        return await _wantToWatchFilmRepository.GetWantToWatchCountByUserIdAsync(userId);
    }



    public async Task<bool> IsMovieInWantToWatchByUserAsync(string? userId, Guid? movieId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

        if (!movieId.HasValue || movieId.Value == Guid.Empty)
            throw new ArgumentException("Movie ID cannot be null or empty.", nameof(movieId));

        return await _wantToWatchFilmRepository.IsMovieInWantToWatchByUserAsync(userId, movieId.Value);
    }
}
