using Cut_Roll_Users.Core.Common.Repositories.Base;
using Cut_Roll_Users.Core.ListMovies.Dtos;
using Cut_Roll_Users.Core.ListMovies.Models;
using Cut_Roll_Users.Core.ListMovies.Repositories;
using Cut_Roll_Users.Core.Movies.Dtos;
using Cut_Roll_Users.Core.MovieImages.Enums;
using Cut_Roll_Users.Infrastructure.Common.Data;
using Microsoft.EntityFrameworkCore;

namespace Cut_Roll_Users.Infrastructure.ListMovies.Repositories;

public class ListMovieEfCoreRepository : IListMovieRepository
{
    private readonly UsersDbContext _context;

    public ListMovieEfCoreRepository(UsersDbContext context)
    {
        _context = context;
    }

    public async Task<Guid?> CreateAsync(ListMovieDto entity)
    {
        var listMovie = new ListMovie
        {
            ListId = entity.ListId,
            MovieId = entity.MovieId
        };

        _context.ListMovies.Add(listMovie);
        var result = await _context.SaveChangesAsync();
        
        return result > 0 ? entity.ListId : null;
    }

    public async Task<Guid?> DeleteAsync(ListMovieDto entity)
    {
        var listMovie = await _context.ListMovies
            .FirstOrDefaultAsync(lm => lm.ListId == entity.ListId && lm.MovieId == entity.MovieId);
        
        if (listMovie == null) return null;

        _context.ListMovies.Remove(listMovie);
        var result = await _context.SaveChangesAsync();
        
        return result > 0 ? entity.ListId : null;
    }

    public async Task<bool> BulkCreateAsync(IEnumerable<ListMovieDto> listToCreate)
    {
        var newListMovies = listToCreate.Select(toCreate => new ListMovie
        {
            ListId = toCreate.ListId,
            MovieId = toCreate.MovieId
        });
        
        await _context.ListMovies.AddRangeAsync(newListMovies);
        var result = await _context.SaveChangesAsync();

        return result > 0;
    }

    public async Task<bool> BulkDeleteAsync(IEnumerable<ListMovieDto> listToDelete)
    {
        var listMovieIds = listToDelete.Select(lm => new { lm.ListId, lm.MovieId }).ToList();
        
        var toDelete = await _context.ListMovies
            .Where(lm => listMovieIds.Any(id => id.ListId == lm.ListId && id.MovieId == lm.MovieId))
            .ToListAsync();

        if (toDelete.Any())
        {
            _context.ListMovies.RemoveRange(toDelete);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        return true; // Nothing to delete
    }

    public async Task<bool> IsMovieInListAsync(Guid listId, Guid movieId)
    {
        return await _context.ListMovies
            .AnyAsync(lm => lm.ListId == listId && lm.MovieId == movieId);
    }

    public async Task<int> GetMovieCountByListIdAsync(Guid listId)
    {
        return await _context.ListMovies
            .Where(lm => lm.ListId == listId)
            .CountAsync();
    }
}
