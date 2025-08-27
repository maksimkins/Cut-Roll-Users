namespace Cut_Roll_Users.Infrastructure.Genres.Services;

using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Genres.Dtos;
using Cut_Roll_Users.Core.Genres.Models;
using Cut_Roll_Users.Core.Genres.Repositories;
using Cut_Roll_Users.Core.Genres.Services;

public class GenreService : IGenreService
{
    private readonly IGenreRepository _genreRepository;
    public GenreService(IGenreRepository genreRepository)
    {
        _genreRepository = genreRepository ?? throw new Exception($"{nameof(_genreRepository)}");
    }
    public async Task<Guid> CreateGenreAsync(GenreCreateDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException($"missing {nameof(dto)}");
        if (dto.Name == null)
            throw new ArgumentNullException($"missing {nameof(dto.Name)}");

        var exists = await _genreRepository.ExistsByNameAsync(dto.Name);
        if (exists)
            throw new ArgumentException($"genre with name: {dto.Name} already exists");

        return await _genreRepository.CreateAsync(dto)
            ?? throw new InvalidOperationException($"failed to create {nameof(Genre)}");
    }

    public async Task<Guid> DeleteGenreByIdAsync(Guid? id)
    {
        if (id == null || id == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(id)}");

        var exists = await _genreRepository.ExistsAsync(id.Value);
        if (exists)
            throw new ArgumentException($"genre with id: {id} does not exist");

        return await _genreRepository.DeleteByIdAsync(id.Value)
            ?? throw new InvalidOperationException($"failed to delete {nameof(Genre)}");
    }

    public async Task<PagedResult<Genre>> GetAllGenresAsync(GenrePaginationDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException($"missing {nameof(dto)}");

        return await _genreRepository.GetAllAsync(dto);
    }

    public async Task<Genre?> GetGenreByIdAsync(Guid? id)
    {
        if (id == null || id == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(id)}");

        return await _genreRepository.GetByIdAsync(id.Value);
    }

    public async Task<PagedResult<Genre>> SearchGenreAsync(GenreSearchDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException($"missing {nameof(dto)}");
        if (dto.Name == null)
            throw new ArgumentNullException($"missing {nameof(dto.Name)}");

        return await _genreRepository.SearchAsync(dto);
    }
}
