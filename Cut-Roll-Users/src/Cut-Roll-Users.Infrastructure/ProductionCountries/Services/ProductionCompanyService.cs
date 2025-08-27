namespace Cut_Roll_Users.Infrastructure.ProductionCountries.Services;

using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.ProductionCompanies.Dtos;
using Cut_Roll_Users.Core.ProductionCompanies.Models;
using Cut_Roll_Users.Core.ProductionCompanies.Repositores;
using Cut_Roll_Users.Core.ProductionCompanies.Service;


public class ProductionCompanyService : IProductionCompanyService
{
    private readonly IProductionCompanyRepository _productionCompanyRepository;
    public ProductionCompanyService(IProductionCompanyRepository productionCompanyRepository)
    {
        _productionCompanyRepository = productionCompanyRepository ?? throw new Exception($"{nameof(_productionCompanyRepository)}");
    }
    public async Task<Guid> CreateProductionCompanyAsync(ProductionCompanyCreateDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException($"missing {nameof(dto)}");
        if (dto.Name == null)
            throw new ArgumentNullException($"missing {nameof(dto.Name)}");


        return await _productionCompanyRepository.CreateAsync(dto)
            ?? throw new InvalidOperationException($"failed to create {nameof(ProductionCompany)}");
    }

    public async Task<Guid> DeleteProductionCompanyById(Guid? id)
    {
        if (id == null || id == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(id)}");

        return await _productionCompanyRepository.DeleteByIdAsync(id.Value)
            ?? throw new InvalidOperationException($"failed to delete {nameof(ProductionCompany)}");
    }

    public async Task<ProductionCompany?> GetProductionCompanyByIdAsync(Guid? id)
    {
        if (id == null || id == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(id)}");

        return await _productionCompanyRepository.GetByIdAsync(id.Value);
    }

    public async Task<PagedResult<ProductionCompany>> SearchProductionCompanyAsync(ProductionCompanySearchRequest? request)
    {
        if (request == null)
            throw new ArgumentNullException($"missing {nameof(request)}");
        if (string.IsNullOrEmpty(request.Name) && string.IsNullOrEmpty(request.CountryCode))
            throw new ArgumentNullException($"missing {nameof(request.Name)} or {nameof(request.CountryCode)}");

        return await _productionCompanyRepository.SearchAsync(request);
    }

    public async Task<Guid> UpdateProductionCompanyAsync(ProductionCompanyUpdateDto? dto)
    {
        if (dto == null)
            throw new ArgumentNullException($"missing {nameof(dto)}");
        if (dto.Id == Guid.Empty)
            throw new ArgumentNullException($"missing {nameof(dto.Id)}");
        if (string.IsNullOrEmpty(dto.Name) && string.IsNullOrEmpty(dto.CountryCode) && string.IsNullOrEmpty(dto.LogoPath))
            throw new ArgumentNullException($"missing {nameof(dto.Name)} or {nameof(dto.CountryCode)} or {nameof(dto.LogoPath)}");

        return await _productionCompanyRepository.UpdateAsync(dto) ??
            throw new Exception($"failed to update {nameof(ProductionCompany)}");
    }
}
