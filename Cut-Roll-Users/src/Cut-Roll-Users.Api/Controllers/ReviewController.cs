namespace Cut_Roll_Users.Api.Controllers;

using Cut_Roll_Users.Api.Common.Extensions.Controllers;
using Cut_Roll_Users.Core.Reviews.Dtos;
using Cut_Roll_Users.Core.Reviews.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]")]
[ApiController]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ReviewCreateDto? reviewCreateDto)
    {
        try
        {
            var result = await _reviewService.CreateReviewAsync(reviewCreateDto);
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
            var result = await _reviewService.GetReviewByIdAsync(id);
            return result is not null ? Ok(result) : NotFound("Review not found.");
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [Authorize]
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] ReviewUpdateDto? reviewUpdateDto)
    {
        try
        {
            var result = await _reviewService.UpdateReviewAsync(reviewUpdateDto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid? id)
    {
        try
        {
            var result = await _reviewService.DeleteReviewByIdAsync(id);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("by-user-and-movie")]
    public async Task<IActionResult> GetReviewByUserAndMovie([FromQuery] string? userId, [FromQuery] Guid? movieId)
    {
        try
        {
            var result = await _reviewService.GetReviewByUserAndMovieAsync(userId, movieId);
            return result is not null ? Ok(result) : NotFound("Review not found.");
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpPost("by-movie")]
    public async Task<IActionResult> GetReviewsByMovieId([FromBody] ReviewPaginationMovieDto dto)
    {
        try
        {
            var result = await _reviewService.GetReviewsByMovieIdAsync(dto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpPost("by-user")]
    public async Task<IActionResult> GetReviewsByUserId([FromBody] ReviewPaginationUserDto dto)
    {
        try
        {
            var result = await _reviewService.GetReviewsByUserIdAsync(dto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("average-rating/{movieId:guid}")]
    public async Task<IActionResult> GetAverageRatingByMovieId([FromRoute] Guid? movieId)
    {
        try
        {
            var result = await _reviewService.GetAverageRatingByMovieIdAsync(movieId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("count/{movieId:guid}")]
    public async Task<IActionResult> GetReviewCountByMovieId([FromRoute] Guid? movieId)
    {
        try
        {
            var result = await _reviewService.GetReviewCountByMovieIdAsync(movieId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }
}
