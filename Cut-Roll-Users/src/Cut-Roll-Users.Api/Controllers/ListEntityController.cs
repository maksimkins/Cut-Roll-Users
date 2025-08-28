namespace Cut_Roll_Users.Api.Controllers;

using Cut_Roll_Users.Api.Common.Extensions.Controllers;
using Cut_Roll_Users.Core.ListEntities.Dtos;
using Cut_Roll_Users.Core.ListEntities.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Route("[controller]")]
[ApiController]
public class ListEntityController : ControllerBase
{
    private readonly IListEntityService _listEntityService;

    public ListEntityController(IListEntityService listEntityService)
    {
        _listEntityService = listEntityService;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ListEntityCreateDto? listCreateDto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            if (listCreateDto?.UserId != userId)
                throw new ArgumentException("User ID in request does not match authenticated user ID.");
            
            var result = await _listEntityService.CreateListAsync(listCreateDto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid? id)
    {
        try
        {
            var result = await _listEntityService.GetListByIdAsync(id);
            return result is not null ? Ok(result) : NotFound("List not found.");
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [Authorize]
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] ListEntityUpdateDto? listUpdateDto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            if (listUpdateDto?.UserId != userId)
                throw new ArgumentException("User ID in request does not match authenticated user ID.");
            
            var result = await _listEntityService.UpdateListAsync(listUpdateDto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid? id)
    {
        try
        {
            var result = await _listEntityService.DeleteListByIdAsync(id);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] ListEntitySearchDto? searchDto)
    {
        try
        {
            var result = await _listEntityService.SearchListsAsync(searchDto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpPost("by-user")]
    public async Task<IActionResult> GetListsByUserId([FromBody] ListEntityPaginationDto dto)
    {
        try
        {
            var result = await _listEntityService.GetListsByUserIdAsync(dto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("count/{userId}")]
    public async Task<IActionResult> GetListCountByUserId([FromRoute] string? userId)
    {
        try
        {
            var result = await _listEntityService.GetListCountByUserIdAsync(userId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }
}
