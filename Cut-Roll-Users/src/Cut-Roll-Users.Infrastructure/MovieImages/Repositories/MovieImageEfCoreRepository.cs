namespace Cut_Roll_Users.Infrastructure.MovieImages.Repositories;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cut_Roll_Users.Core.MovieImages.Dtos;
using Cut_Roll_Users.Core.MovieImages.Models;
using Cut_Roll_Users.Core.MovieImages.Repositories;
using Cut_Roll_Users.Infrastructure.Common.Data;
using Microsoft.EntityFrameworkCore;

public class MovieImageEfCoreRepository : IMovieImageRepository
{
    private readonly UsersDbContext _dbContext;
    public MovieImageEfCoreRepository(UsersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> BulkCreateAsync(IEnumerable<MovieImageCreateDto> listToCreate)
    {
        var newList = listToCreate.Select(toCreate => new MovieImage
        {
            MovieId = toCreate.MovieId,
            Type = toCreate.Type,
            FilePath = toCreate.FilePath,
        });
        
        await _dbContext.Images.AddRangeAsync(newList);
        var res = await _dbContext.SaveChangesAsync();

        return res > 0;
    }

    public async Task<bool> BulkDeleteAsync(IEnumerable<MovieImageDeleteDto> listToDelete)
    {
        foreach (var item in listToDelete)
        {
            var image = await _dbContext.Images.FirstOrDefaultAsync(c =>
                c.Id == item.Id);

            if (image != null)
            {
                _dbContext.Images.Remove(image);
            }
        }

        var result = await _dbContext.SaveChangesAsync();
        return result > 0;
    }

    public async Task<Guid?> CreateAsync(MovieImageCreateDto entity)
    {
        var image = new MovieImage
        {
            MovieId = entity.MovieId,
            Type = entity.Type,
            FilePath = entity.FilePath,
        };

        await _dbContext.Images.AddAsync(image);
        var res = await _dbContext.SaveChangesAsync();

        return res > 0 ? entity.MovieId : null;
    }

    public async Task<Guid?> DeleteByIdAsync(Guid id)
    {
        var image = await _dbContext.Images.FirstOrDefaultAsync(i => i.Id == id);

        if (image != null)
            _dbContext.Images.Remove(image);

        var res = await _dbContext.SaveChangesAsync();
        return res > 0 ? id : null;
    }

    public async Task<bool> DeleteRangeById(Guid movieId)
    {
        var images = _dbContext.Images.Where(i => i.MovieId == movieId);
        _dbContext.RemoveRange(images);

        var res = await _dbContext.SaveChangesAsync();
        return res > 0;
    }

    public async Task<IEnumerable<MovieImage>> GetImagesByMovieIdAsync(Guid movieId)
    {
        return await _dbContext.Images.Where(g => g.MovieId == movieId).ToListAsync();
    }

    public async Task<IEnumerable<MovieImage>> GetImagesByTypeAsync(MovieImageSearchDto movieImageSearchDto)
    {
        return await _dbContext.Images.Where(g => g.MovieId == movieImageSearchDto.MovieId && g.Type == movieImageSearchDto.Type).ToListAsync();
    }
}
