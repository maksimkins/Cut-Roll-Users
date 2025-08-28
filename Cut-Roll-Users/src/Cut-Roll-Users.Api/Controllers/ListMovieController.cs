namespace Cut_Roll_Users.Api.Controllers;

using Cut_Roll_Users.Api.Common.Extensions.Controllers;
using Cut_Roll_Users.Core.ListEntities.Services;
using Cut_Roll_Users.Core.ListMovies.Dtos;
using Cut_Roll_Users.Core.ListMovies.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Route("[controller]")]
[ApiController]
public class ListMovieController : ControllerBase
{
    private readonly IListMovieService _listMovieService;
    private readonly IListEntityService _listService;

    public ListMovieController(IListMovieService listMovieService, IListEntityService listService)
    {
        _listMovieService = listMovieService;
        _listService = listService;
    }

    [Authorize]
    [HttpPost("add")]
    public async Task<IActionResult> AddMovieToList([FromBody] ListMovieDto? listMovieDto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var list = await _listService.GetListByIdAsync(listMovieDto?.ListId);
            if (list is null || list.UserSimplified.Id != userId)
                throw new ArgumentException("there is no list, or user doent posses it");
            
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
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var list = await _listService.GetListByIdAsync(listMovieDto?.ListId);
            if (list is null || list.UserSimplified.Id != userId)
                throw new ArgumentException("there is no list, or user doent posses it");
            
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
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var listId = toCreate?.FirstOrDefault()?.ListId;
            var list = await _listService.GetListByIdAsync(listId);
            if (toCreate != null)
            {
                foreach (var item in toCreate)
                {
                    if (toCreate.Any(lm => lm.ListId != listId))
                    {
                        throw new ArgumentException("All items must belong to the same list.");
                    }
                }
                if (list is null || list.UserSimplified.Id != userId)
                    throw new ArgumentException("there is no list, or user doent posses it");
            }
            
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
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var listId = toDelete?.FirstOrDefault()?.ListId;
            var list = await _listService.GetListByIdAsync(listId);
            if (toDelete != null)
            {
                foreach (var item in toDelete)
                {
                    if (toDelete.Any(lm => lm.ListId != listId))
                    {
                        throw new ArgumentException("All items must belong to the same list.");
                    }
                }
                if (list is null || list.UserSimplified.Id != userId)
                    throw new ArgumentException("there is no list, or user doent posses it");
            }
            
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
