namespace Cut_Roll_Users.Api.Controllers;

using Cut_Roll_Users.Api.Common.Extensions.Controllers;
using Cut_Roll_Users.Core.MovieLikes.Dtos;
using Cut_Roll_Users.Core.MovieLikes.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]")]
[ApiController]
public class MovieLikeController : ControllerBase
{
    private readonly IMovieLikeService _movieLikeService;

    public MovieLikeController(IMovieLikeService movieLikeService)
    {
        _movieLikeService = movieLikeService;
    }

    [Authorize]
    [HttpPost("like")]
    public async Task<IActionResult> LikeMovie([FromBody] MovieLikeCreateDto? likeDto)
    {
        try
        {
            var result = await _movieLikeService.LikeMovieAsync(likeDto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [Authorize]
    [HttpPost("unlike")]
    public async Task<IActionResult> UnlikeMovie([FromBody] MovieLikeCreateDto? likeDto)
    {
        try
        {
            var result = await _movieLikeService.UnlikeMovieAsync(likeDto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpPost("liked-by-user")]
    public async Task<IActionResult> GetLikedMovies([FromBody] MovieLikePaginationUserDto dto)
    {
        try
        {
            var result = await _movieLikeService.GetLikedMovies(dto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("is-liked")]
    public async Task<IActionResult> IsMovieLikedByUser([FromQuery] string? userId, [FromQuery] Guid? movieId)
    {
        try
        {
            var result = await _movieLikeService.IsMovieLikedByUserAsync(userId, movieId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("count/{movieId:guid}")]
    public async Task<IActionResult> GetMovieLikeCount([FromRoute] Guid? movieId)
    {
        try
        {
            var result = await _movieLikeService.GetMovieLikeCountAsync(movieId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }
}
