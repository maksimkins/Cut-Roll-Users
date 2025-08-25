using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Common.Repositories.Base;
using Cut_Roll_Users.Core.ProductionCompanies.Dtos;
using Cut_Roll_Users.Core.ProductionCompanies.Models;

namespace Cut_Roll_Users.Core.ProductionCompanies.Repositores;

public interface IProductionCompanyRepository: ISearchAsync<ProductionCompanySearchRequest, PagedResult<ProductionCompany>>,
ICreateAsync<ProductionCompanyCreateDto, Guid?>, IDeleteByIdAsync<Guid, Guid?>, IUpdateAsync<ProductionCompanyUpdateDto, Guid?>,
IGetByIdAsync<Guid, ProductionCompany?>
{
    
}
