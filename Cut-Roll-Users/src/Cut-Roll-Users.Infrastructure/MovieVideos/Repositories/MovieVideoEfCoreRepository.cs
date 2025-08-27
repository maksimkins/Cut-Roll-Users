namespace Cut_Roll_Users.Infrastructure.MovieVideos.Repositories;

using Cut_Roll_Users.Core.MovieVideos.Dtos;
using Cut_Roll_Users.Core.MovieVideos.Models;
using Cut_Roll_Users.Core.MovieVideos.Repositories;
using Cut_Roll_Users.Infrastructure.Common.Data;
using Microsoft.EntityFrameworkCore;

public class MovieVideoEfCoreRepository : IMovieVideoRepository
{
    private readonly UsersDbContext _dbContext;
    public MovieVideoEfCoreRepository(UsersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid?> CreateAsync(MovieVideoCreateDto entity)
    {
        var image = new MovieVideo
        {
            MovieId = entity.MovieId,
            Type = entity.Type,
            Site = entity.Site,
            Key = entity.Key,
            Name = entity.Name,
        };

        await _dbContext.Videos.AddAsync(image);
        var res = await _dbContext.SaveChangesAsync();

        return res > 0 ? entity.MovieId : null;
    }

    public async Task<Guid?> DeleteByIdAsync(Guid id)
    {
        var video = await _dbContext.Videos.FirstOrDefaultAsync(i => i.Id == id);

        if (video != null)
            _dbContext.Videos.Remove(video);

        var res = await _dbContext.SaveChangesAsync();
        return res > 0 ? id : null;
    }

    public async Task<bool> DeleteRangeById(Guid movieId)
    {
        var videos = _dbContext.Videos.Where(i => i.MovieId == movieId);
        _dbContext.RemoveRange(videos);

        var res = await _dbContext.SaveChangesAsync();
        return res > 0;
    }

    public async Task<IEnumerable<MovieVideo>> GetVideosByMovieIdAsync(Guid movieId)
    {
        return await _dbContext.Videos.Where(g => g.MovieId == movieId).ToListAsync();
    }

    public async Task<IEnumerable<MovieVideo>> GetVideosByTypeAsync(MovieVideoSearchDto movieVideoSearchDto)
    {
        return await _dbContext.Videos.Where(g => g.MovieId == movieVideoSearchDto.MovieId && g.Type == movieVideoSearchDto.Type).ToListAsync();
    }

    public async Task<Guid?> UpdateAsync(MovieVideoUpdateDto entity)
    {
        var toUpdate = await _dbContext.Videos.FirstOrDefaultAsync(v => v.Id == entity.Id);

        if (toUpdate == null)
            throw new ArgumentException(message: $"caanot find video with id: {entity.Id}");

        toUpdate.Key = entity.Key ?? toUpdate.Key;
        toUpdate.Name = entity.Name ?? toUpdate.Name;
        toUpdate.Type = entity.Type ?? toUpdate.Type;
        toUpdate.Site = entity.Site ?? toUpdate.Site;

        _dbContext.Videos.Update(toUpdate);
        var res = await _dbContext.SaveChangesAsync();

        return res > 0 ? toUpdate.Id : null; 
    }
}
