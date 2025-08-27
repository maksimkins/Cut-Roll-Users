using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Common.Repositories.Base;
using Cut_Roll_Users.Core.MovieLikes.Dtos;
using Cut_Roll_Users.Core.MovieLikes.Models;
using Cut_Roll_Users.Core.MovieLikes.Repositories;
using Cut_Roll_Users.Core.Movies.Dtos;
using Cut_Roll_Users.Core.MovieImages.Enums;
using Cut_Roll_Users.Infrastructure.Common.Data;
using Microsoft.EntityFrameworkCore;

namespace Cut_Roll_Users.Infrastructure.MovieLikes.Repositories;

public class MovieLikeEfCoreRepository : IMovieLikeRepository
{
    private readonly UsersDbContext _context;

    public MovieLikeEfCoreRepository(UsersDbContext context)
    {
        _context = context;
    }

    public async Task<Guid?> CreateAsync(MovieLikeCreateDto entity)
    {
        var movieLike = new MovieLike
        {
            UserId = entity.UserId,
            MovieId = entity.MovieId,
            LikedAt = DateTime.UtcNow
        };

        _context.MovieLikes.Add(movieLike);
        var result = await _context.SaveChangesAsync();
        
        return result > 0 ? entity.MovieId : null;
    }

    public async Task<Guid?> DeleteAsync(MovieLikeCreateDto entity)
    {
        var movieLike = await _context.MovieLikes
            .FirstOrDefaultAsync(ml => ml.UserId == entity.UserId && ml.MovieId == entity.MovieId);
        
        if (movieLike == null) return null;

        _context.MovieLikes.Remove(movieLike);
        var result = await _context.SaveChangesAsync();
        
        return result > 0 ? entity.MovieId : null;
    }

    public async Task<bool> IsLikedByUserAsync(string userId, Guid movieId)
    {
        return await _context.MovieLikes
            .AnyAsync(ml => ml.UserId == userId && ml.MovieId == movieId);
    }

    public async Task<int> GetLikeCountByMovieIdAsync(Guid movieId)
    {
        return await _context.MovieLikes
            .Where(ml => ml.MovieId == movieId)
            .CountAsync();
    }

    public async Task<PagedResult<MovieSimplifiedDto>> GetLikedMovies(MovieLikePaginationUserDto dto)
    {
        var query = _context.MovieLikes
            .Include(ml => ml.Movie)
                .ThenInclude(m => m.Images)
            .Where(ml => ml.UserId == dto.UserId)
            .OrderByDescending(ml => ml.LikedAt);

        var totalCount = await query.CountAsync();

        var pageNumber = dto.Page <= 0 ? 1 : dto.Page;
        var pageSize = dto.PageSize <= 0 ? 10 : dto.PageSize;

        var likedMovies = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(ml => new MovieSimplifiedDto
            {
                MovieId = ml.Movie.Id,
                Title = ml.Movie.Title,
                Poster = ml.Movie.Images.FirstOrDefault(i => i.Type == ImageTypes.poster.ToString())
            })
            .ToListAsync();

        return new PagedResult<MovieSimplifiedDto>
        {
            Data = likedMovies,
            TotalCount = totalCount,
            Page = pageNumber,
            PageSize = pageSize
        };
    }
}
