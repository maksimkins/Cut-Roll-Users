namespace Cut_Roll_Users.Api.Controllers;

using Cut_Roll_Users.Api.Common.Extensions.Controllers;
using Cut_Roll_Users.Core.Countries.Dtos;
using Cut_Roll_Users.Core.Countries.Services;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]")]
[ApiController]
public class CountryController : ControllerBase
{
    private readonly ICountryService _countryService;

    public CountryController(ICountryService countryService)
    {
        _countryService = countryService;
    }

    [HttpPost("all")]
    public async Task<IActionResult> GetAll([FromBody] ContryPaginationDto? dto)
    {
        try
        {
            var result = await _countryService.GetAllCountriesAsync(dto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpPost("search")]
    public async Task<IActionResult> SearchByName([FromBody] ContrySearchByNameDto? dto)
    {
        try
        {
            var result = await _countryService.SearchCountryByNameAsync(dto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("{isoCode}")]
    public async Task<IActionResult> GetByIsoCode([FromRoute] string? isoCode)
    {
        try
        {
            var result = await _countryService.GetCountryByIsoCodeAsync(isoCode);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }
}
