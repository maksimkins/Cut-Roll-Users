namespace Cut_Roll_Users.Api.Controllers;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cut_Roll_Users.Api.Common.Extensions.Controllers;
using Cut_Roll_Users.Core.MovieProductionCompanies.Dtos;
using Cut_Roll_Users.Core.MovieProductionCompanies.Service;
using Cut_Roll_Users.Core.Movies.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]")]
[ApiController]
public class MovieProductionCompanyController : ControllerBase
{
    private readonly IMovieProductionCompanyService _movieProductionCompanyService;

    public MovieProductionCompanyController(IMovieProductionCompanyService movieProductionCompanyService)
    {
        _movieProductionCompanyService = movieProductionCompanyService;
    }

    [Authorize(Roles = "Admin,Publisher")]
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] MovieProductionCompanyDto? dto)
    {
        try
        {
            var id = await _movieProductionCompanyService.CreateMovieProductionCompanyAsync(dto);
            return Ok(id);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [Authorize(Roles = "Admin,Publisher")]
    [HttpDelete("delete")]
    public async Task<IActionResult> Delete([FromBody] MovieProductionCompanyDto? dto)
    {
        try
        {
            var id = await _movieProductionCompanyService.DeleteMovieProductionCompanyAsync(dto);
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
            var result = await _movieProductionCompanyService.DeleteMovieProductionCompanyRangeByMovieIdAsync(movieId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("by-movie/{movieId:guid}")]
    public async Task<IActionResult> GetCompaniesByMovieId(Guid? movieId)
    {
        try
        {
            var result = await _movieProductionCompanyService.GetCompaniesByMovieIdAsync(movieId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpPost("movies-by-company")]
    public async Task<IActionResult> GetMoviesByCompany([FromBody] MovieSearchByCompanyDto? dto)
    {
        try
        {
            var result = await _movieProductionCompanyService.GetMoviesByCompanyIdAsync(dto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [Authorize(Roles = "Admin,Publisher")]
    [HttpPost("bulk-create")]
    public async Task<IActionResult> BulkCreate([FromBody] IEnumerable<MovieProductionCompanyDto>? toCreate)
    {
        try
        {
            var result = await _movieProductionCompanyService.BulkCreateMovieProductionCompanyAsync(toCreate);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [Authorize(Roles = "Admin,Publisher")]
    [HttpPost("bulk-delete")]
    public async Task<IActionResult> BulkDelete([FromBody] IEnumerable<MovieProductionCompanyDto>? toDelete)
    {
        try
        {
            var result = await _movieProductionCompanyService.BulkDeleteMovieProductionCompanyAsync(toDelete);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }
}
