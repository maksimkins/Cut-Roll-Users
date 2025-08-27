namespace Cut_Roll_Users.Api.Controllers;

using Cut_Roll_Users.Api.Common.Extensions.Controllers;
using Cut_Roll_Users.Core.SpokenLanguages.Dtos;
using Cut_Roll_Users.Core.SpokenLanguages.Service;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]")]
[ApiController]
public class SpokenLanguageController : ControllerBase
{
    private readonly ISpokenLanguageService _spokenLanguageService;

    public SpokenLanguageController(ISpokenLanguageService spokenLanguageService)
    {
        _spokenLanguageService = spokenLanguageService;
    }

    [HttpPost("all")]
    public async Task<IActionResult> GetAll([FromBody] SpokenLanguagePaginationDto? dto)
    {
        try
        {
            var result = await _spokenLanguageService.GetAllSpokenLanguageAsync(dto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpPost("search")]
    public async Task<IActionResult> SearchByName([FromBody] SpokenLanguageSearchByNameDto? dto)
    {
        try
        {
            var result = await _spokenLanguageService.SearchSpokenLanguageByNameAsync(dto);
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
            var result = await _spokenLanguageService.GetSpokenLanguageByIsoCodeAsync(isoCode);
            return result is not null ? Ok(result) : NotFound("Spoken language not found.");
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }
}