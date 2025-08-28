namespace Cut_Roll_Users.Api.Controllers;

using Cut_Roll_Users.Api.Common.Extensions.Controllers;
using Cut_Roll_Users.Core.WantToWatchFilms.Dtos;
using Cut_Roll_Users.Core.WantToWatchFilms.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Route("[controller]")]
[ApiController]
public class WantToWatchFilmController : ControllerBase
{
    private readonly IWantToWatchFilmService _wantToWatchFilmService;

    public WantToWatchFilmController(IWantToWatchFilmService wantToWatchFilmService)
    {
        _wantToWatchFilmService = wantToWatchFilmService;
    }

    [Authorize]
    [HttpPost("add")]
    public async Task<IActionResult> AddToWantToWatch([FromBody] WantToWatchFilmDto? wantToWatchDto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            if (wantToWatchDto?.UserId != userId)
                throw new ArgumentException("User ID in request does not match authenticated user ID.");
            
            var result = await _wantToWatchFilmService.AddMovieToWantToWatchAsync(wantToWatchDto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [Authorize]
    [HttpPost("remove")]
    public async Task<IActionResult> RemoveFromWantToWatch([FromBody] WantToWatchFilmDto? wantToWatchDto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            if (wantToWatchDto?.UserId != userId)
                throw new ArgumentException("User ID in request does not match authenticated user ID.");
            
            var result = await _wantToWatchFilmService.RemoveMovieFromWantToWatchAsync(wantToWatchDto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpPost("by-user")]
    public async Task<IActionResult> GetWantToWatchMovies([FromBody] WantToWatchFilmPaginationUserDto? paginationDto)
    {
        try
        {
            var result = await _wantToWatchFilmService.GetWantToWatchFilmsByUserIdAsync(paginationDto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("is-in-want-to-watch")]
    public async Task<IActionResult> IsMovieInWantToWatch([FromQuery] string? userId, [FromQuery] Guid? movieId)
    {
        try
        {
            var result = await _wantToWatchFilmService.IsMovieInWantToWatchByUserAsync(userId, movieId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("count/{userId}")]
    public async Task<IActionResult> GetWantToWatchCountByUserId([FromRoute] string? userId)
    {
        try
        {
            var result = await _wantToWatchFilmService.GetWantToWatchCountByUserIdAsync(userId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }
}
