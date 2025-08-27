namespace Cut_Roll_Users.Api.Controllers;

using Cut_Roll_Users.Api.Common.Extensions.Controllers;
using Cut_Roll_Users.Core.MovieKeywords.Dtos;
using Cut_Roll_Users.Core.MovieKeywords.Service;
using Cut_Roll_Users.Core.Movies.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


[Route("[controller]")]
[ApiController]
public class MovieKeywordController : ControllerBase
{
    private readonly IMovieKeywordService _movieKeywordService;

    public MovieKeywordController(IMovieKeywordService movieKeywordService)
    {
        _movieKeywordService = movieKeywordService;
    }

    [Authorize(Roles = "Admin,Publisher")]
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] MovieKeywordDto? dto)
    {
        try
        {
            var id = await _movieKeywordService.CreateMovieKeywordAsync(dto);
            return Ok(id);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [Authorize(Roles = "Admin,Publisher")]
    [HttpDelete("delete")]
    public async Task<IActionResult> Delete([FromBody] MovieKeywordDto? dto)
    {
        try
        {
            var id = await _movieKeywordService.DeleteMovieKeywordAsync(dto);
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
            var result = await _movieKeywordService.DeleteMovieKeywordRangeByMovieId(movieId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("by-movie/{movieId:guid}")]
    public async Task<IActionResult> GetKeywordsByMovieId(Guid? movieId)
    {
        try
        {
            var result = await _movieKeywordService.GetKeywordsByMovieIdAsync(movieId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpPost("movies-by-keyword")]
    public async Task<IActionResult> GetMoviesByKeyword([FromBody] MovieSearchByKeywordDto? searchDto)
    {
        try
        {
            var result = await _movieKeywordService.GetMoviesByKeywordIdAsync(searchDto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [Authorize(Roles = "Admin,Publisher")]
    [HttpPost("bulk-create")]
    public async Task<IActionResult> BulkCreate([FromBody] IEnumerable<MovieKeywordDto>? toCreate)
    {
        try
        {
            var result = await _movieKeywordService.BulkCreateMovieKeywordAsync(toCreate);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [Authorize(Roles = "Admin,Publisher")]
    [HttpPost("bulk-delete")]
    public async Task<IActionResult> BulkDelete([FromBody] IEnumerable<MovieKeywordDto>? toDelete)
    {
        try
        {
            var result = await _movieKeywordService.BulkDeleteMovieKeywordAsync(toDelete);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }
}