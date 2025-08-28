namespace Cut_Roll_Users.Api.Controllers;

using Cut_Roll_Users.Api.Common.Extensions.Controllers;
using Cut_Roll_Users.Core.Movies.Dtos;
using Cut_Roll_Users.Core.Movies.Service;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]")]
[ApiController]
public class MovieController : ControllerBase
{
    private readonly IMovieService _movieService;

    public MovieController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] MovieSearchRequest? movieSearchRequest)
    {
        try
        {
            var result = await _movieService.SearchMovieAsync(movieSearchRequest);
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
            var result = await _movieService.GetMovieByIdAsync(id);
            return result is not null ? Ok(result) : NotFound("Movie not found.");
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("count")]
    public async Task<IActionResult> CountMovies()
    {
        try
        {
            var result = await _movieService.CountMoviesAsync();
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("review-count/{movieId:guid}")]
    public async Task<IActionResult> GetMovieReviewCount([FromRoute] Guid? movieId)
    {
        try
        {
            var result = await _movieService.GetMovieReviewCountAsync(movieId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("watched-count/{movieId:guid}")]
    public async Task<IActionResult> GetMovieWatchedCount([FromRoute] Guid? movieId)
    {
        try
        {
            var result = await _movieService.GetMovieWatchedCountAsync(movieId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("like-count/{movieId:guid}")]
    public async Task<IActionResult> GetMovieLikeCount([FromRoute] Guid? movieId)
    {
        try
        {
            var result = await _movieService.GetMovieLikeCountAsync(movieId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("want-to-watch-count/{movieId:guid}")]
    public async Task<IActionResult> GetMovieWantToWatchCount([FromRoute] Guid? movieId)
    {
        try
        {
            var result = await _movieService.GetMovieWantToWatchCountAsync(movieId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("average-rating/{movieId:guid}")]
    public async Task<IActionResult> GetMovieAverageRating([FromRoute] Guid? movieId)
    {
        try
        {
            var result = await _movieService.GetMovieAverageRatingAsync(movieId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("is-watched-by-user")]
    public async Task<IActionResult> IsMovieWatchedByUser([FromQuery] Guid? movieId, [FromQuery] string? userId)
    {
        try
        {
            var result = await _movieService.IsMovieWatchedByUserAsync(movieId, userId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("is-liked-by-user")]
    public async Task<IActionResult> IsMovieLikedByUser([FromQuery] Guid? movieId, [FromQuery] string? userId)
    {
        try
        {
            var result = await _movieService.IsMovieLikedByUserAsync(movieId, userId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("is-in-user-want-to-watch")]
    public async Task<IActionResult> IsMovieInUserWantToWatch([FromQuery] Guid? movieId, [FromQuery] string? userId)
    {
        try
        {
            var result = await _movieService.IsMovieInUserWantToWatchAsync(movieId, userId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }
}
