namespace Cut_Roll_Users.Api.Controllers;

using Cut_Roll_Users.Api.Common.Extensions.Controllers;
using Cut_Roll_Users.Core.ListMovies.Dtos;
using Cut_Roll_Users.Core.ListMovies.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]")]
[ApiController]
public class ListMovieController : ControllerBase
{
    private readonly IListMovieService _listMovieService;

    public ListMovieController(IListMovieService listMovieService)
    {
        _listMovieService = listMovieService;
    }

    [Authorize]
    [HttpPost("add")]
    public async Task<IActionResult> AddMovieToList([FromBody] ListMovieDto? listMovieDto)
    {
        try
        {
            var result = await _listMovieService.AddMovieToListAsync(listMovieDto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [Authorize]
    [HttpPost("remove")]
    public async Task<IActionResult> RemoveMovieFromList([FromBody] ListMovieDto? listMovieDto)
    {
        try
        {
            var result = await _listMovieService.RemoveMovieFromListAsync(listMovieDto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [Authorize]
    [HttpPost("bulk-add")]
    public async Task<IActionResult> BulkAddMovies([FromBody] IEnumerable<ListMovieDto>? toCreate)
    {
        try
        {
            var result = await _listMovieService.BulkCreateAsync(toCreate);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [Authorize]
    [HttpPost("bulk-remove")]
    public async Task<IActionResult> BulkRemoveMovies([FromBody] IEnumerable<ListMovieDto>? toDelete)
    {
        try
        {
            var result = await _listMovieService.BulkDeleteAsync(toDelete);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("is-in-list")]
    public async Task<IActionResult> IsMovieInList([FromQuery] Guid? listId, [FromQuery] Guid? movieId)
    {
        try
        {
            var result = await _listMovieService.IsMovieInListAsync(listId, movieId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("count/{listId:guid}")]
    public async Task<IActionResult> GetMovieCountByListId([FromRoute] Guid? listId)
    {
        try
        {
            var result = await _listMovieService.GetMovieCountByListIdAsync(listId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }
}
