namespace Cut_Roll_Users.Api.Controllers;

using Cut_Roll_Users.Api.Common.Extensions.Controllers;
using Cut_Roll_Users.Core.Keywords.Dtos;
using Cut_Roll_Users.Core.Keywords.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]")]
[ApiController]
public class KeywordController : ControllerBase
{
    private readonly IKeywordService _keywordService;

    public KeywordController(IKeywordService keywordService)
    {
        _keywordService = keywordService;
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAll([FromQuery] KeywordPaginationDto? dto)
    {
        try
        {
            var result = await _keywordService.GetAllKeywordsAsync(dto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] KeywordSearchDto? dto)
    {
        try
        {
            var result = await _keywordService.SearchKeywordAsync(dto);
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
            var result = await _keywordService.GetKeywordByIdAsync(id);
            return result is not null ? Ok(result) : NotFound("Keyword not found.");
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [Authorize(Roles = "Admin,Publisher")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] KeywordCreateDto? dto)
    {
        try
        {
            var id = await _keywordService.CreateKeywordAsync(dto);
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
            var deletedId = await _keywordService.DeleteKeywordByIdAsync(id);
            return Ok(deletedId);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }
}
