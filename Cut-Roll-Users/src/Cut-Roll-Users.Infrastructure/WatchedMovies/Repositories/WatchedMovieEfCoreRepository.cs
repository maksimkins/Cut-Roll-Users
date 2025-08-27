using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Common.Repositories.Base;
using Cut_Roll_Users.Core.Movies.Dtos;
using Cut_Roll_Users.Core.MovieImages.Enums;
using Cut_Roll_Users.Core.WatchedMovies.Dtos;
using Cut_Roll_Users.Core.WatchedMovies.Models;
using Cut_Roll_Users.Core.WatchedMovies.Repositories;
using Cut_Roll_Users.Infrastructure.Common.Data;
using Microsoft.EntityFrameworkCore;

namespace Cut_Roll_Users.Infrastructure.WatchedMovies.Repositories;

public class WatchedMovieEfCoreRepository : IWatchedMovieRepository
{
    private readonly UsersDbContext _context;

    public WatchedMovieEfCoreRepository(UsersDbContext context)
    {
        _context = context;
    }

    public async Task<Guid?> CreateAsync(WatchedMovieDto entity)
    {
        var watchedMovie = new WatchedMovie
        {
            UserId = entity.UserId,
            MovieId = entity.MovieId,
            WatchedAt = DateTime.UtcNow
        };

        _context.WatchedMovies.Add(watchedMovie);
        var result = await _context.SaveChangesAsync();
        
        return result > 0 ? entity.MovieId : null;
    }

    public async Task<Guid?> DeleteAsync(WatchedMovieDto entity)
    {
        var watchedMovie = await _context.WatchedMovies
            .FirstOrDefaultAsync(w => w.UserId == entity.UserId && w.MovieId == entity.MovieId);
        
        if (watchedMovie == null) return null;

        _context.WatchedMovies.Remove(watchedMovie);
        var result = await _context.SaveChangesAsync();
        
        return result > 0 ? entity.MovieId : null;
    }

    public async Task<WatchedMovieResponseDto?> GetByUserAndMovieAsync(string userId, Guid movieId)
    {
        var watchedMovie = await _context.WatchedMovies
            .Include(w => w.Movie)
                .ThenInclude(m => m.Images)
            .FirstOrDefaultAsync(w => w.UserId == userId && w.MovieId == movieId);

        if (watchedMovie == null) return null;

        return new WatchedMovieResponseDto
        {
            Id = watchedMovie.MovieId,
            UserId = watchedMovie.UserId,
            Movie = new MovieSimplifiedDto
            {
                MovieId = watchedMovie.Movie.Id,
                Title = watchedMovie.Movie.Title,
                Poster = watchedMovie.Movie.Images.FirstOrDefault(i => i.Type == ImageTypes.poster.ToString())
            },
            WatchedAt = watchedMovie.WatchedAt
        };
    }

    public async Task<PagedResult<WatchedMovieResponseDto>> GetByUserIdAsync(WatchedMoviePaginationUserDto dto)
    {
        var query = _context.WatchedMovies
            .Include(w => w.Movie)
                .ThenInclude(m => m.Images)
            .Where(w => w.UserId == dto.UserId.ToString())
            .OrderByDescending(w => w.WatchedAt);

        var totalCount = await query.CountAsync();

        var pageNumber = dto.Page <= 0 ? 1 : dto.Page;
        var pageSize = dto.PageSize <= 0 ? 10 : dto.PageSize;

        var watchedMovies = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(w => new WatchedMovieResponseDto
            {
                Id = w.MovieId,
                UserId = w.UserId,
                Movie = new MovieSimplifiedDto
                {
                    MovieId = w.Movie.Id,
                    Title = w.Movie.Title,
                    Poster = w.Movie.Images.FirstOrDefault(i => i.Type == ImageTypes.poster.ToString())
                },
                WatchedAt = w.WatchedAt
            })
            .ToListAsync();

        return new PagedResult<WatchedMovieResponseDto>
        {
            Data = watchedMovies,
            TotalCount = totalCount,
            Page = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<int> GetWatchedCountByUserIdAsync(string userId)
    {
        return await _context.WatchedMovies
            .Where(w => w.UserId == userId)
            .CountAsync();
    }

    public async Task<bool> IsMovieWatchedByUserAsync(string userId, Guid movieId)
    {
        return await _context.WatchedMovies
            .AnyAsync(w => w.UserId == userId && w.MovieId == movieId);
    }

    public async Task<PagedResult<WatchedMovieResponseDto>> SearchAsync(WatchedMovieSearchDto request)
    {
        var query = _context.WatchedMovies
            .Include(w => w.Movie)
                .ThenInclude(m => m.Images)
            .AsQueryable();

        // Apply filters
        if (request.FromDate.HasValue)
        {
            query = query.Where(w => w.WatchedAt >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(w => w.WatchedAt <= request.ToDate.Value);
        }

        var totalCount = await query.CountAsync();

        var pageNumber = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        var watchedMovies = await query
            .OrderByDescending(w => w.WatchedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(w => new WatchedMovieResponseDto
            {
                Id = w.MovieId,
                UserId = w.UserId,
                Movie = new MovieSimplifiedDto
                {
                    MovieId = w.Movie.Id,
                    Title = w.Movie.Title,
                    Poster = w.Movie.Images.FirstOrDefault(i => i.Type == ImageTypes.poster.ToString())
                },
                WatchedAt = w.WatchedAt
            })
            .ToListAsync();

        return new PagedResult<WatchedMovieResponseDto>
        {
            Data = watchedMovies,
            TotalCount = totalCount,
            Page = pageNumber,
            PageSize = pageSize
        };
    }
}
