namespace Cut_Roll_Users.Api.Controllers;

using Cut_Roll_Users.Api.Common.Extensions.Controllers;
using Cut_Roll_Users.Core.WatchedMovies.Dtos;
using Cut_Roll_Users.Core.WatchedMovies.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Route("[controller]")]
[ApiController]
public class WatchedMovieController : ControllerBase
{
    private readonly IWatchedMovieService _watchedMovieService;

    public WatchedMovieController(IWatchedMovieService watchedMovieService)
    {
        _watchedMovieService = watchedMovieService;
    }

    [Authorize]
    [HttpPost("mark-watched")]
    public async Task<IActionResult> MarkMovieAsWatched([FromBody] WatchedMovieDto? watchedMovieDto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            if (watchedMovieDto?.UserId != userId)
                throw new ArgumentException("User ID in request does not match authenticated user ID.");
            
            var result = await _watchedMovieService.MarkMovieAsWatchedAsync(watchedMovieDto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [Authorize]
    [HttpPost("remove-watched")]
    public async Task<IActionResult> UnmarkMovieAsWatchedAsync([FromBody] WatchedMovieDto? watchedMovieDto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            if (watchedMovieDto?.UserId != userId)
                throw new ArgumentException("User ID in request does not match authenticated user ID.");
            
            var result = await _watchedMovieService.UnmarkMovieAsWatchedAsync(watchedMovieDto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }


    [HttpPost("by-user")]
    public async Task<IActionResult> GetWatchedMoviesByUserId([FromBody] WatchedMoviePaginationUserDto dto)
    {
        try
        {
            var result = await _watchedMovieService.GetWatchedMoviesByUserIdAsync(dto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("count/{userId}")]
    public async Task<IActionResult> GetWatchedCountByUserId([FromRoute] string? userId)
    {
        try
        {
            var result = await _watchedMovieService.GetWatchedCountByUserIdAsync(userId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("is-watched")]
    public async Task<IActionResult> IsMovieWatchedByUser([FromQuery] string? userId, [FromQuery] Guid? movieId)
    {
        try
        {
            var result = await _watchedMovieService.IsMovieWatchedByUserAsync(userId, movieId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }
}
