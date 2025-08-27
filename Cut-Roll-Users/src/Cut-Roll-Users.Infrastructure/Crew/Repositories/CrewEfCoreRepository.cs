namespace Cut_Roll_Users.Infrastructure.Crew.Repositories;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Crews.Dtos;
using Cut_Roll_Users.Core.Crews.Models;
using Cut_Roll_Users.Core.Crews.Repositories;
using Cut_Roll_Users.Core.MovieImages.Enums;
using Cut_Roll_Users.Infrastructure.Common.Data;
using Microsoft.EntityFrameworkCore;

public class CrewEfCoreRepository : ICrewRepository
{
    private readonly UsersDbContext _dbContext;
    public CrewEfCoreRepository(UsersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> BulkCreateAsync(IEnumerable<CrewCreateDto> listToCreate)
    {
        var newCrew = listToCreate.Select(toCreate => new Crew
        {
            MovieId = toCreate.MovieId,
            PersonId = toCreate.PersonId,
            Job = toCreate.Job,
            Department = toCreate.Department,
        });

        await _dbContext.Crew.AddRangeAsync(newCrew);
        var res = await _dbContext.SaveChangesAsync();

        return res > 0;
    }

    public async Task<bool> BulkDeleteAsync(IEnumerable<Guid> idsToDelete)
    {
        foreach (var item in idsToDelete)
        {
            var crew = await _dbContext.Crew.FirstOrDefaultAsync(c =>
                c.Id == item);

            if (crew != null)
            {
                _dbContext.Crew.Remove(crew);
            }
        }

        var result = await _dbContext.SaveChangesAsync();
        return result > 0;
    }

    public async Task<Guid?> CreateAsync(CrewCreateDto entity)
    {
        var crew = new Crew
        {
            MovieId = entity.MovieId,
            PersonId = entity.PersonId,
            Job = entity.Job,
            Department = entity.Department
        };

        await _dbContext.Crew.AddAsync(crew);
        var res = await _dbContext.SaveChangesAsync();

        return res > 0 ? crew.Id : null;
    }

    public async Task<Guid?> DeleteByIdAsync(Guid id)
    {
        var crew = await _dbContext.Crew.FirstOrDefaultAsync(c =>
            c.Id == id);

        if (crew != null)
        {
            _dbContext.Crew.Remove(crew);
        }

        var res = await _dbContext.SaveChangesAsync();

        return res > 0 ? id : null;
    }


    public async Task<PagedResult<CrewGetDto>> GetByMovieIdAsync(CrewGetByMovieId dto)
    {
        var query = _dbContext.Crew.Include(c => c.Person).Include(c => c.Movie).ThenInclude(m => m.Images).AsQueryable();
        query = query.Where(c => c.PersonId == dto.MovieId);

        if (dto.PageNumber < 1) dto.PageNumber = 1;
        if (dto.PageSize < 1) dto.PageSize = 10;

        var totalCount = await query.CountAsync();

        query = query.
            Skip((dto.PageNumber - 1) * dto.PageSize)
            .Take(dto.PageSize);

        var result = await query.ToListAsync();

        return new PagedResult<CrewGetDto>()
        {
            Data = result.Select(c => new CrewGetDto
            {
                MovieId = c.MovieId,
                Title = c.Movie.Title,
                Poster = c.Movie.Images?.FirstOrDefault(i => i.Type == ImageTypes.poster.ToString()),
                Person = c.Person,
                Job = c.Job,
                Department = c.Department,

            }).ToList(),

            TotalCount = totalCount,
            Page = dto.PageNumber,
            PageSize = dto.PageSize
        };
    }

    public async Task<PagedResult<CrewGetDto>> GetByPersonIdAsync(CrewGetByPersonId dto)
    {
        var query = _dbContext.Crew.Include(c => c.Person).Include(c => c.Movie).ThenInclude(m => m.Images).AsQueryable();
        query = query.Where(c => c.PersonId == dto.PersonId);


        if (dto.PageNumber < 1) dto.PageNumber = 1;
        if (dto.PageSize < 1) dto.PageSize = 10;

        var totalCount = await query.CountAsync();

        query = query.
            Skip((dto.PageNumber - 1) * dto.PageSize)
            .Take(dto.PageSize);

        var result = await query.ToListAsync();

        return new PagedResult<CrewGetDto>()
        {
            Data = result.Select(c => new CrewGetDto
            {
                MovieId = c.MovieId,
                Title = c.Movie.Title,
                Poster = c.Movie.Images?.FirstOrDefault(i => i.Type == ImageTypes.poster.ToString()),
                Person = c.Person,
                Job = c.Job,
                Department = c.Department,

            }).ToList(),

            TotalCount = totalCount,
            Page = dto.PageNumber,
            PageSize = dto.PageSize
        };

    }

    public async Task<Guid?> UpdateAsync(CrewUpdateDto entity)
    {
        var crew = await _dbContext.Crew.FirstOrDefaultAsync(c => c.Id == entity.Id);
        if (crew != null)
        {
            crew.Job = entity.Job ?? crew.Job;
            crew.Department = entity.Department ?? crew.Department;

            _dbContext.Crew.Update(crew);
        }

        var res = await _dbContext.SaveChangesAsync();

        return res > 0 ? entity.Id : null;
    }

    public async Task<PagedResult<CrewGetDto>> SearchAsync(CrewSearchDto dto)
    {
        var query = _dbContext.Crew
            .Include(c => c.Person).Include(c => c.Movie).ThenInclude(m => m.Images)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            var name = $"%{dto.Name.Trim()}%";
            query = query.Where(c => EF.Functions.ILike(c.Person.Name, name));
        }

        if (!string.IsNullOrWhiteSpace(dto.JobTitle))
        {
            var job = $"%{dto.JobTitle.Trim()}%";
            query = query.Where(c => c.Job != null && EF.Functions.ILike(c.Job, job));
        }

        if (!string.IsNullOrWhiteSpace(dto.Department))
        {
            var department = $"%{dto.Department.Trim()}%";
            query = query.Where(c => c.Department != null && EF.Functions.ILike(c.Department, department));
        }


        if (dto.PageNumber < 1) dto.PageNumber = 1;
        if (dto.PageSize < 1) dto.PageSize = 10;

        var totalCount = await query.CountAsync();

        query = query.
            Skip((dto.PageNumber - 1) * dto.PageSize)
            .Take(dto.PageSize);

        var result = await query.ToListAsync();

        return new PagedResult<CrewGetDto>()
        {
            Data = result.Select(c => new CrewGetDto
            {
                MovieId = c.MovieId,
                Title = c.Movie.Title,
                Poster = c.Movie.Images?.FirstOrDefault(i => i.Type == ImageTypes.poster.ToString()),
                Person = c.Person,
                Job = c.Job,
                Department = c.Department,

            }).ToList(),

            TotalCount = totalCount,
            Page = dto.PageNumber,
            PageSize = dto.PageSize
        };
    }

    public async Task<bool> DeleteRangeById(Guid movieId)
    {
        var toDeletes = _dbContext.Crew.Where(g => g.MovieId == movieId);
        _dbContext.Crew.RemoveRange(toDeletes);

        var res = await _dbContext.SaveChangesAsync();
        return res > 0;
    }
    
    public async Task<bool> ExistsByIdAsync(Guid id)
    {
        return await _dbContext.Crew.AnyAsync(c => c.Id == id);
    }
}
