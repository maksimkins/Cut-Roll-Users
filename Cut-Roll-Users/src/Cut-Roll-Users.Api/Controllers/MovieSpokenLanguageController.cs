namespace Cut_Roll_Users.Api.Controllers;

using Cut_Roll_Users.Api.Common.Extensions.Controllers;
using Cut_Roll_Users.Core.Movies.Dtos;
using Cut_Roll_Users.Core.MovieSpokenLanguages.Dtos;
using Cut_Roll_Users.Core.MovieSpokenLanguages.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]")]
[ApiController]
public class MovieSpokenLanguageController : ControllerBase
{
    private readonly IMovieSpokenLanguageService _movieSpokenLanguageService;

    public MovieSpokenLanguageController(IMovieSpokenLanguageService movieSpokenLanguageService)
    {
        _movieSpokenLanguageService = movieSpokenLanguageService;
    }

    [Authorize(Roles = "Admin,Publisher")]
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] MovieSpokenLanguageDto? dto)
    {
        try
        {
            var id = await _movieSpokenLanguageService.CreateMovieSpokenLanguageAsync(dto);
            return Ok(id);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [Authorize(Roles = "Admin,Publisher")]
    [HttpDelete("delete")]
    public async Task<IActionResult> Delete([FromBody] MovieSpokenLanguageDto? dto)
    {
        try
        {
            var id = await _movieSpokenLanguageService.DeleteMovieSpokenLanguageAsync(dto);
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
            var result = await _movieSpokenLanguageService.DeleteMovieSpokenLanguageRangeByMovieId(movieId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("by-movie/{movieId:guid}")]
    public async Task<IActionResult> GetSpokenLanguagesByMovieId(Guid? movieId)
    {
        try
        {
            var result = await _movieSpokenLanguageService.GetSpokenLanguagesByMovieIdAsync(movieId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpPost("movies-by-language")]
    public async Task<IActionResult> GetMoviesBySpokenLanguage([FromBody] MovieSearchBySpokenLanguageDto? dto)
    {
        try
        {
            var result = await _movieSpokenLanguageService.GetMoviesBySpokenLanguageIdAsync(dto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [Authorize(Roles = "Admin,Publisher")]
    [HttpPost("bulk-create")]
    public async Task<IActionResult> BulkCreate([FromBody] IEnumerable<MovieSpokenLanguageDto>? toCreate)
    {
        try
        {
            var result = await _movieSpokenLanguageService.BulkCreateMovieSpokenLaguageAsync(toCreate);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [Authorize(Roles = "Admin,Publisher")]
    [HttpPost("bulk-delete")]
    public async Task<IActionResult> BulkDelete([FromBody] IEnumerable<MovieSpokenLanguageDto>? toDelete)
    {
        try
        {
            var result = await _movieSpokenLanguageService.BulkDeleteeMovieSpokenLaguageAsync(toDelete);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }
}
