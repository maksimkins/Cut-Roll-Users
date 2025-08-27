namespace Cut_Roll_Users.Api.Controllers;

using Cut_Roll_Users.Api.Common.Extensions.Controllers;
using Cut_Roll_Users.Core.WantToWatchFilms.Dtos;
using Cut_Roll_Users.Core.WantToWatchFilms.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
            var result = await _wantToWatchFilmService.AddToWantToWatchAsync(wantToWatchDto);
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
            var result = await _wantToWatchFilmService.RemoveFromWantToWatchAsync(wantToWatchDto);
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
            var result = await _wantToWatchFilmService.GetWantToWatchMoviesAsync(paginationDto);
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
            var result = await _wantToWatchFilmService.IsMovieInWantToWatchAsync(userId, movieId);
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
