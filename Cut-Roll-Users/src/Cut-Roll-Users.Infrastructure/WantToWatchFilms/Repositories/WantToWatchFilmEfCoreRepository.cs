using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Common.Repositories.Base;
using Cut_Roll_Users.Core.Movies.Dtos;
using Cut_Roll_Users.Core.MovieImages.Enums;
using Cut_Roll_Users.Core.WantToWatchFilms.Dtos;
using Cut_Roll_Users.Core.WantToWatchFilms.Models;
using Cut_Roll_Users.Core.WantToWatchFilms.Repositories;
using Cut_Roll_Users.Infrastructure.Common.Data;
using Microsoft.EntityFrameworkCore;

namespace Cut_Roll_Users.Infrastructure.WantToWatchFilms.Repositories;

public class WantToWatchFilmEfCoreRepository : IWantToWatchFilmRepository
{
    private readonly UsersDbContext _context;

    public WantToWatchFilmEfCoreRepository(UsersDbContext context)
    {
        _context = context;
    }

    public async Task<Guid?> CreateAsync(WantToWatchFilmDto entity)
    {
        var wantToWatchFilm = new WantToWatchFilm
        {
            UserId = entity.UserId,
            MovieId = entity.MovieId,
            AddedAt = DateTime.UtcNow
        };

        _context.WantToWatchMovies.Add(wantToWatchFilm);
        var result = await _context.SaveChangesAsync();
        
        return result > 0 ? entity.MovieId : null;
    }

    public async Task<Guid?> DeleteAsync(WantToWatchFilmDto entity)
    {
        var wantToWatchFilm = await _context.WantToWatchMovies
            .FirstOrDefaultAsync(w => w.UserId == entity.UserId && w.MovieId == entity.MovieId);
        
        if (wantToWatchFilm == null) return null;

        _context.WantToWatchMovies.Remove(wantToWatchFilm);
        var result = await _context.SaveChangesAsync();
        
        return result > 0 ? entity.MovieId : null;
    }

    public async Task<bool> IsInWantToWatchAsync(WantToWatchFilmDto dto)
    {
        return await _context.WantToWatchMovies
            .AnyAsync(w => w.UserId == dto.UserId && w.MovieId == dto.MovieId);
    }

    public async Task<int> GetWantToWatchCountByUserIdAsync(string userId)
    {
        return await _context.WantToWatchMovies
            .Where(w => w.UserId == userId)
            .CountAsync();
    }

    public async Task<PagedResult<MovieSimplifiedDto>> GetWantToWatchMoviesAsync(WantToWatchFilmPaginationUserDto dto)
    {
        var query = _context.WantToWatchMovies
            .Include(w => w.Movie)
                .ThenInclude(m => m.Images)
            .Where(w => w.UserId == dto.UserId)
            .OrderByDescending(w => w.AddedAt);

        var totalCount = await query.CountAsync();

        var pageNumber = dto.Page <= 0 ? 1 : dto.Page;
        var pageSize = dto.PageSize <= 0 ? 10 : dto.PageSize;

        var wantToWatchMovies = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(w => new MovieSimplifiedDto
            {
                MovieId = w.Movie.Id,
                Title = w.Movie.Title,
                Poster = w.Movie.Images.FirstOrDefault(i => i.Type == ImageTypes.poster.ToString())
            })
            .ToListAsync();

        return new PagedResult<MovieSimplifiedDto>
        {
            Data = wantToWatchMovies,
            TotalCount = totalCount,
            Page = pageNumber,
            PageSize = pageSize
        };
    }
}
