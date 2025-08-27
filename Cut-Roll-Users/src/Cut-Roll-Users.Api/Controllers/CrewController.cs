namespace Cut_Roll_Users.Api.Controllers;

using Cut_Roll_Users.Api.Common.Extensions.Controllers;
using Cut_Roll_Users.Core.Crews.Dtos;
using Cut_Roll_Users.Core.Crews.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]")]
[ApiController]
public class CrewController : ControllerBase
{
    private readonly ICrewService _crewService;

    public CrewController(ICrewService crewService)
    {
        _crewService = crewService;
    }

    [HttpPost("create")]
    [Authorize("Admin,Publisher")]
    public async Task<IActionResult> Create([FromBody] CrewCreateDto? dto)
    {
        try
        {
            var id = await _crewService.CreateCrewAsync(dto);
            return Ok(id);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpPut("update")]
    [Authorize("Admin,Publisher")]
    public async Task<IActionResult> Update([FromBody] CrewUpdateDto? dto)
    {
        try
        {
            var id = await _crewService.UpdateCrewAsync(dto);
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
            var result = await _crewService.DeleteCrewByIdAsync(id);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] CrewSearchDto? request)
    {
        try
        {
            var result = await _crewService.SearchCrewAsync(request);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpPost("by-movie")]
    public async Task<IActionResult> GetByMovieId([FromBody] CrewGetByMovieId? dto)
    {
        try
        {
            var result = await _crewService.GetCrewByMovieIdAsync(dto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpPost("by-person")]
    public async Task<IActionResult> GetByPersonId([FromBody] CrewGetByPersonId? dto)
    {
        try
        {
            var result = await _crewService.GetCrewByPersonIdAsync(dto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpPost("bulk-create")]
    [Authorize("Admin,Publisher")]
    public async Task<IActionResult> BulkCreate([FromBody] IEnumerable<CrewCreateDto>? crewList)
    {
        try
        {
            var result = await _crewService.BulkCreateCrewAsync(crewList);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpPost("bulk-delete")]
    [Authorize("Admin,Publisher")]
    public async Task<IActionResult> BulkDelete([FromBody] IEnumerable<Guid>? idsToDelete)
    {
        try
        {
            var result = await _crewService.BulkDeleteCrewAsync(idsToDelete);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpDelete("delete-by-movie/{movieId:guid}")]
    [Authorize("Admin,Publisher")]
    public async Task<IActionResult> DeleteRangeByMovieId([FromRoute] Guid? movieId)
    {
        try
        {
            var result = await _crewService.DeleteCrewRangeByMovieIdAsync(movieId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }
}