namespace Cut_Roll_Users.Api.Controllers;

using Cut_Roll_Users.Api.Common.Extensions.Controllers;
using Cut_Roll_Users.Core.MovieGenres.Dtos;
using Cut_Roll_Users.Core.MovieGenres.Services;
using Cut_Roll_Users.Core.Movies.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]")]
[ApiController]
public class MovieGenreController : ControllerBase
{
    private readonly IMovieGenreService _movieGenreService;

    public MovieGenreController(IMovieGenreService movieGenreService)
    {
        _movieGenreService = movieGenreService;
    }

    [Authorize(Roles = "Admin,Publisher")]
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] MovieGenreDto? dto)
    {
        try
        {
            var id = await _movieGenreService.CreateMovieGenreAsync(dto);
            return Ok(id);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [Authorize(Roles = "Admin,Publisher")]
    [HttpDelete("delete")]
    public async Task<IActionResult> Delete([FromBody] MovieGenreDto? dto)
    {
        try
        {
            var id = await _movieGenreService.DeleteMovieGenreAsync(dto);
            return Ok(id);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [Authorize(Roles = "Admin,Publisher")]
    [HttpDelete("delete-by-movie/{movieId:guid}")]
    public async Task<IActionResult> DeleteByMovieId(Guid? movieId)
    {
        try
        {
            var result = await _movieGenreService.DeleteMovieGenreRangeByMovieId(movieId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("by-movie/{movieId:guid}")]
    public async Task<IActionResult> GetGenresByMovieId(Guid? movieId)
    {
        try
        {
            var result = await _movieGenreService.GetGenresByMovieIdAsync(movieId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpPost("movies-by-genre")]
    public async Task<IActionResult> GetMoviesByGenreId([FromBody] MovieSearchByGenreDto? dto)
    {
        try
        {
            var result = await _movieGenreService.GetMoviesByGenreIdAsync(dto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [Authorize(Roles = "Admin,Publisher")]
    [HttpPost("bulk-create")]
    public async Task<IActionResult> BulkCreate([FromBody] IEnumerable<MovieGenreDto>? toCreate)
    {
        try
        {
            var result = await _movieGenreService.BulkCreateMovieGenreAsync(toCreate);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [Authorize(Roles = "Admin,Publisher")]
    [HttpPost("bulk-delete")]
    public async Task<IActionResult> BulkDelete([FromBody] IEnumerable<MovieGenreDto>? toDelete)
    {
        try
        {
            var result = await _movieGenreService.BulkDeleteMovieGenreAsync(toDelete);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }
}