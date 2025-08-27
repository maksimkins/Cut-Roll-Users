namespace Cut_Roll_Users.Infrastructure.MovieVideos.Services;

using Cut_Roll_Users.Core.MovieVideos.Dtos;
using Cut_Roll_Users.Core.MovieVideos.Models;
using Cut_Roll_Users.Core.MovieVideos.Repositories;
using Cut_Roll_Users.Core.MovieVideos.Service;

public class MovieVideoService : IMovieVideoService
{
    private readonly IMovieVideoRepository _movieVideoRepository;
    public MovieVideoService(IMovieVideoRepository movieVideoRepository)
    {
        _movieVideoRepository = movieVideoRepository ?? throw new Exception($"{nameof(_movieVideoRepository)}");
    }

    public async Task<Guid> CreateMovieVideoCreateAsync(MovieVideoCreateDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException($"missing {nameof(dto)}");
        if (dto.MovieId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(dto.MovieId)}");
        if (string.IsNullOrEmpty(dto.Name))
            throw new ArgumentNullException($"missing {nameof(dto.Name)}");
        if (string.IsNullOrEmpty(dto.Name))
            throw new ArgumentNullException($"missing {nameof(dto.Type)}");
        if (string.IsNullOrEmpty(dto.Name))
            throw new ArgumentNullException($"missing {nameof(dto.Site)}");
        if (string.IsNullOrEmpty(dto.Name))
            throw new ArgumentNullException($"missing {nameof(dto.Key)}");

        return await _movieVideoRepository.CreateAsync(dto) ??
            throw new InvalidOperationException(message: "could not add video to movie");
    }

    public async Task<Guid> DeleteMovieVideoByIdAsync(Guid? id)
    {
        if (id == null || id == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(id)}");
        
        return await _movieVideoRepository.DeleteByIdAsync(id.Value) ??
            throw new InvalidOperationException(message: "could not delete video from movie");
    }

    public async Task<bool> DeleteMovieVideoRangeByMovieId(Guid? movieId)
    {
        if (movieId == null || movieId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(movieId)}");
            
        return await _movieVideoRepository.DeleteRangeById(movieId.Value);
    }

    public async Task<IEnumerable<MovieVideo>> GetVideosByMovieIdAsync(Guid? movieId)
    {
        if (movieId == null || movieId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(movieId)}");
            
        return await _movieVideoRepository.GetVideosByMovieIdAsync(movieId.Value);
    }

    public async Task<IEnumerable<MovieVideo>> GetVideosByTypeAsync(MovieVideoSearchDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException($"missing {nameof(dto)}");
        if (dto.MovieId == null || dto.MovieId == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(dto.MovieId)}");
        if (string.IsNullOrEmpty(dto.Type))
            throw new ArgumentNullException($"missing {nameof(dto.Type)}");
            
        return await _movieVideoRepository.GetVideosByTypeAsync(dto);
    }

    public async Task<Guid> UpdateMovieVideoAsync(MovieVideoUpdateDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException($"missing {nameof(dto)}");
        if (dto.Id == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(dto.Id)}");
        if (string.IsNullOrEmpty(dto.Name))
            throw new ArgumentNullException($"missing {nameof(dto.Name)}");
        if (string.IsNullOrEmpty(dto.Name))
            throw new ArgumentNullException($"missing {nameof(dto.Type)}");
        if (string.IsNullOrEmpty(dto.Name))
            throw new ArgumentNullException($"missing {nameof(dto.Site)}");
        if (string.IsNullOrEmpty(dto.Name))
            throw new ArgumentNullException($"missing {nameof(dto.Key)}");

        return await _movieVideoRepository.UpdateAsync(dto) ??
            throw new InvalidOperationException(message: "could not update video for movie");
    }
}
