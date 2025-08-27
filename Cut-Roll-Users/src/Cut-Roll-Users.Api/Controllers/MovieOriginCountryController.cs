namespace Cut_Roll_Users.Api.Controllers;

using Cut_Roll_Users.Api.Common.Extensions.Controllers;
using Cut_Roll_Users.Core.MovieOriginCountries.Dtos;
using Cut_Roll_Users.Core.MovieOriginCountries.Service;
using Cut_Roll_Users.Core.Movies.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]")]
[ApiController]
public class MovieOriginCountryController : ControllerBase
{
    private readonly IMovieOriginCountryService _movieOriginCountryService;

    public MovieOriginCountryController(IMovieOriginCountryService movieOriginCountryService)
    {
        _movieOriginCountryService = movieOriginCountryService;
    }

    [Authorize(Roles = "Admin,Publisher")]
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] MovieOriginCountryDto? dto)
    {
        try
        {
            var id = await _movieOriginCountryService.CreateMovieOriginCountryAsync(dto);
            return Ok(id);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [Authorize(Roles = "Admin,Publisher")]
    [HttpDelete("delete")]
    public async Task<IActionResult> Delete([FromBody] MovieOriginCountryDto? dto)
    {
        try
        {
            var id = await _movieOriginCountryService.DeleteMovieOriginCountryAsync(dto);
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
            var result = await _movieOriginCountryService.DeleteMovieOriginCountryRangeByMovieIdAsync(movieId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("by-movie/{movieId:guid}")]
    public async Task<IActionResult> GetCountriesByMovieId(Guid? movieId)
    {
        try
        {
            var result = await _movieOriginCountryService.GetCountriesByMovieIdAsync(movieId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpPost("movies-by-country")]
    public async Task<IActionResult> GetMoviesByCountry([FromBody] MovieSearchByCountryDto? dto)
    {
        try
        {
            var result = await _movieOriginCountryService.GetMoviesByOriginCountryIdAsync(dto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [Authorize(Roles = "Admin,Publisher")]
    [HttpPost("bulk-create")]
    public async Task<IActionResult> BulkCreate([FromBody] IEnumerable<MovieOriginCountryDto>? toCreate)
    {
        try
        {
            var result = await _movieOriginCountryService.BulkCreateMovieOriginCountryAsync(toCreate);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [Authorize(Roles = "Admin,Publisher")]
    [HttpPost("bulk-delete")]
    public async Task<IActionResult> BulkDelete([FromBody] IEnumerable<MovieOriginCountryDto>? toDelete)
    {
        try
        {
            var result = await _movieOriginCountryService.BulkDeleteMovieOriginCountryAsync(toDelete);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }
}
