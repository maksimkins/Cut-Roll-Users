namespace Cut_Roll_Users.Infrastructure.People.Repositories;

using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.MovieImages.Enums;
using Cut_Roll_Users.Core.Movies.Dtos;
using Cut_Roll_Users.Core.Movies.Models;
using Cut_Roll_Users.Core.People.Dtos;
using Cut_Roll_Users.Core.People.Models;
using Cut_Roll_Users.Core.People.Repositories;
using Cut_Roll_Users.Infrastructure.Common.Data;
using Microsoft.EntityFrameworkCore;

public class PersonEfCoreRepository : IPersonRepository
{
    private readonly UsersDbContext _dbContext;
    public PersonEfCoreRepository(UsersDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<Guid?> CreateAsync(PersonCreateDto dto)
    {
        var toCreate = new Person
        {
            Name = dto.Name,
            ProfilePath = dto.ProfilePath,
        };

        await _dbContext.People.AddAsync(toCreate);
        var res = await _dbContext.SaveChangesAsync();

        return res > 0 ? toCreate.Id : null;
    }

    public async Task<Guid?> DeleteByIdAsync(Guid id)
    {
        var toDelete = await _dbContext.People.FirstOrDefaultAsync(i => i.Id == id);

        if (toDelete != null)
            _dbContext.People.Remove(toDelete);

        var res = await _dbContext.SaveChangesAsync();
        return res > 0 ? id : null;
    }

    public async Task<Person?> GetByIdAsync(Guid id)
    {
        return await _dbContext.People.FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<PagedResult<Person>> SearchAsync(PersonSearchRequest request)
    {
        var query = _dbContext.People.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var name = $"%{request.Name.Trim()}%";
            query = query.Where(m => EF.Functions.ILike(m.Name, name));
        }

        var totalCount = await query.CountAsync();
        query = query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize);

        return new PagedResult<Person>()
        {
            Data = await query.ToListAsync(),
            TotalCount = totalCount,
            Page = request.PageNumber,
            PageSize =  request.PageSize,
        };
    }

    public async Task<Guid?> UpdateAsync(PersonUpdateDto entity)
    {
        var toUpdate = await _dbContext.People.FirstOrDefaultAsync(v => v.Id == entity.Id);

        if (toUpdate == null)
            throw new ArgumentException(message: $"caanot find person with id: {entity.Id}");

        toUpdate.Name = entity.Name ?? toUpdate.Name;
        toUpdate.ProfilePath = entity.ProfilePath ?? toUpdate.ProfilePath;

        _dbContext.People.Update(toUpdate);
        var res = await _dbContext.SaveChangesAsync();

        return res > 0 ? toUpdate.Id :
            throw new InvalidOperationException(message: $"something went wrong while updating person with id:{entity.Id}");
    }

    public async Task<PagedResult<MovieSimplifiedDto>> GetFilmographyAsync(MovieSearchByPesonIdDto dto)
    {
        var query = _dbContext.Movies
            .Include(m => m.Images)
            .Include(m => m.Cast)
            .ThenInclude(mg => mg.Person)
            .Include(m => m.Crew)
            .ThenInclude(mg => mg.Person)
            .AsQueryable();


        query = query.Where(m =>
            m.Cast.Any(c => c.PersonId == dto.PersonId) ||
            m.Crew.Any(cr => cr.PersonId == dto.PersonId));

        if (dto.PageNumber < 1) dto.PageNumber = 1;
        if (dto.PageSize < 1) dto.PageSize = 10;
        
        var totalCount = await query.CountAsync();

        query = query.
            Skip((dto.PageNumber - 1) * dto.PageSize)
            .Take(dto.PageSize);

        var result = await query.ToListAsync();

        return new PagedResult<MovieSimplifiedDto>()
        {
            Data = result.Select(m => new MovieSimplifiedDto
            {
                MovieId = m.Id,
                Title = m.Title,
                Poster = m.Images?.FirstOrDefault(i => i.Type == ImageTypes.poster.ToString()),
            }).ToList(),
            TotalCount = totalCount,
            Page = dto.PageNumber,
            PageSize = dto.PageSize
        };
    }
}
