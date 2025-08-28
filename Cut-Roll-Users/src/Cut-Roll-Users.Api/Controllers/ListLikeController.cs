namespace Cut_Roll_Users.Api.Controllers;

using Cut_Roll_Users.Api.Common.Extensions.Controllers;
using Cut_Roll_Users.Core.ListLikes.Dtos;
using Cut_Roll_Users.Core.ListLikes.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Route("[controller]")]
[ApiController]
public class ListLikeController : ControllerBase
{
    private readonly IListLikeService _listLikeService;

    public ListLikeController(IListLikeService listLikeService)
    {
        _listLikeService = listLikeService;
    }

    [Authorize]
    [HttpPost("like")]
    public async Task<IActionResult> LikeList([FromBody] ListLikeDto? likeDto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            if (likeDto?.UserId != userId)
                throw new ArgumentException("User ID in request does not match authenticated user ID.");
            
            var result = await _listLikeService.LikeListAsync(likeDto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [Authorize]
    [HttpPost("unlike")]
    public async Task<IActionResult> UnlikeList([FromBody] ListLikeDto? likeDto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            if (likeDto?.UserId != userId)
                throw new ArgumentException("User ID in request does not match authenticated user ID.");
            
            var result = await _listLikeService.UnlikeListAsync(likeDto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpPost("liked-lists")]
    public async Task<IActionResult> GetLikedLists([FromBody] ListLikedDto dto)
    {
        try
        {
            var result = await _listLikeService.GetLikedLists(dto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("is-liked")]
    public async Task<IActionResult> IsListLikedByUser([FromQuery] string? userId, [FromQuery] Guid? listId)
    {
        try
        {
            var result = await _listLikeService.IsListLikedByUserAsync(userId, listId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("count/{listId:guid}")]
    public async Task<IActionResult> GetListLikeCount([FromRoute] Guid? listId)
    {
        try
        {
            var result = await _listLikeService.GetListLikeCountAsync(listId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }
}
