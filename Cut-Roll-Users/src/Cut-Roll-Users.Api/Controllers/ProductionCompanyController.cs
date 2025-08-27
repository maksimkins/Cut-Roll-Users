namespace Cut_Roll_Users.Api.Controllers;

using Cut_Roll_Users.Api.Common.Extensions.Controllers;
using Cut_Roll_Users.Core.ProductionCompanies.Dtos;
using Cut_Roll_Users.Core.ProductionCompanies.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]")]
[ApiController]
public class ProductionCompanyController : ControllerBase
{
    private readonly IProductionCompanyService _productionCompanyService;

    public ProductionCompanyController(IProductionCompanyService productionCompanyService)
    {
        _productionCompanyService = productionCompanyService;
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] ProductionCompanySearchRequest? request)
    {
        try
        {
            var result = await _productionCompanyService.SearchProductionCompanyAsync(request);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        try
        {
            var result = await _productionCompanyService.GetProductionCompanyByIdAsync(id);
            return result is not null ? Ok(result) : NotFound("Production company not found.");
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [Authorize(Roles = "Admin,Publisher")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductionCompanyCreateDto? dto)
    {
        try
        {
            var id = await _productionCompanyService.CreateProductionCompanyAsync(dto);
            return Ok(id);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [Authorize(Roles = "Admin,Publisher")]
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] ProductionCompanyUpdateDto? dto)
    {
        try
        {
            var id = await _productionCompanyService.UpdateProductionCompanyAsync(dto);
            return Ok(id);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [Authorize(Roles = "Admin,Publisher")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        try
        {
            var deletedId = await _productionCompanyService.DeleteProductionCompanyById(id);
            return Ok(deletedId);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }
}
