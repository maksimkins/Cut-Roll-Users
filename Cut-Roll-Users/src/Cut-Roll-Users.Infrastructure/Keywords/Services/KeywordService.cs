namespace Cut_Roll_Users.Infrastructure.Keywords.Services;

using System.Threading.Tasks;
using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Keywords.Dtos;
using Cut_Roll_Users.Core.Keywords.Models;
using Cut_Roll_Users.Core.Keywords.Repositories;
using Cut_Roll_Users.Core.Keywords.Services;

public class KeywordService : IKeywordService
{
    private readonly IKeywordRepository _keywordRepository;

    public KeywordService(IKeywordRepository keywordRepository)
    {
        _keywordRepository = keywordRepository ?? throw new Exception($"{nameof(_keywordRepository)}");
    }

    public async Task<Guid> CreateKeywordAsync(KeywordCreateDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException($"missing {nameof(dto)}");
        if (dto.Name == null)
            throw new ArgumentNullException($"missing {nameof(dto.Name)}");

        var exists = await _keywordRepository.ExistsByNameAsync(dto.Name);
        if (exists)
            throw new ArgumentException($"keyword with name: {dto.Name} already exists");

        return await _keywordRepository.CreateAsync(dto)
            ?? throw new InvalidOperationException($"failed to create {nameof(Keyword)}");
    }

    public async Task<Guid> DeleteKeywordByIdAsync(Guid? id)
    {
        if (id == null)
            throw new ArgumentNullException($"missing {nameof(id)}");

        var exists = await _keywordRepository.ExistsAsync(id.Value);
        if (exists)
            throw new ArgumentException($"keyword with id: {id} does not exist");

        return await _keywordRepository.DeleteByIdAsync(id.Value)
            ?? throw new InvalidOperationException($"failed to delete {nameof(Keyword)}");
    }

    public async Task<PagedResult<Keyword>> GetAllKeywordsAsync(KeywordPaginationDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException($"missing {nameof(dto)}");

        return await _keywordRepository.GetAllAsync(dto);
    }

    public async Task<Keyword?> GetKeywordByIdAsync(Guid? id)
    {
        if (id == null || id == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(id)}");

        return await _keywordRepository.GetByIdAsync(id.Value);
    }

    public async Task<PagedResult<Keyword>> SearchKeywordAsync(KeywordSearchDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException($"missing {nameof(dto)}");
        if (dto.Name == null)
            throw new ArgumentNullException($"missing {nameof(dto.Name)}");

        return await _keywordRepository.SearchAsync(dto);
    }
}
