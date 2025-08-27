namespace Cut_Roll_Users.Api.Controllers;

using Cut_Roll_Users.Api.Common.Extensions.Controllers;
using Cut_Roll_Users.Core.Casts.Dtos;
using Cut_Roll_Users.Core.Casts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]")]
[ApiController]
public class CastController : ControllerBase
{
    private readonly ICastService _castService;

    public CastController(ICastService castService)
    {
        _castService = castService;
    }

    [HttpPost("create")]
    [Authorize("Admin,Publisher")]
    public async Task<IActionResult> Create([FromBody] CastCreateDto? dto)
    {
        try
        {
            var id = await _castService.CreateCastAsync(dto);
            return Ok(id);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpPut("update")]
    [Authorize("Admin,Publisher")]
    public async Task<IActionResult> Update([FromBody] CastUpdateDto? dto)
    {
        try
        {
            var id = await _castService.UpdateCastAsync(dto);
            return Ok(id);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpDelete("delete/{id:guid}")]
    [Authorize("Admin,Publisher")]
    public async Task<IActionResult> DeleteById(Guid? id)
    {
        try
        {
            var result = await _castService.DeleteCastByIdAsync(id);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] CastSearchDto? request)
    {
        try
        {
            var result = await _castService.SearchCastAsync(request);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpPost("by-movie")]
    public async Task<IActionResult> GetByMovieId([FromBody] CastGetByMovieIdDto? dto)
    {
        try
        {
            var result = await _castService.GetCastByMovieIdAsync(dto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpPost("by-person")]
    public async Task<IActionResult> GetByPersonId([FromBody] CastGetByPersonIdDto? dto)
    {
        try
        {
            var result = await _castService.GetCastByPersonIdAsync(dto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpPost("bulk-create")]
    [Authorize("Admin,Publisher")]
    public async Task<IActionResult> BulkCreate([FromBody] IEnumerable<CastCreateDto>? toCreate)
    {
        try
        {
            var result = await _castService.BulkCreateCasteAsync(toCreate);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpPost("bulk-delete")]
    [Authorize("Admin,Publisher")]
    public async Task<IActionResult> BulkDelete([FromBody] IEnumerable<Guid>? toDeleteIds)
    {
        try
        {
            var result = await _castService.BulkDeleteCastAsync(toDeleteIds);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpDelete("delete-by-movie/{movieId:guid}")]
    [Authorize("Admin,Publisher")]
    public async Task<IActionResult> DeleteRangeByMovieId(Guid? movieId)
    {
        try
        {
            var result = await _castService.DeleteCastRangeByMovieIdAsync(movieId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }
}