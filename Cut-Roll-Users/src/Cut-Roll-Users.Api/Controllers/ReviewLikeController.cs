namespace Cut_Roll_Users.Api.Controllers;

using Cut_Roll_Users.Api.Common.Extensions.Controllers;
using Cut_Roll_Users.Core.ReviewLikes.Dtos;
using Cut_Roll_Users.Core.ReviewLikes.Services;
using Cut_Roll_Users.Core.Reviews.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]")]
[ApiController]
public class ReviewLikeController : ControllerBase
{
    private readonly IReviewLikeService _reviewLikeService;

    public ReviewLikeController(IReviewLikeService reviewLikeService)
    {
        _reviewLikeService = reviewLikeService;
    }

    [Authorize]
    [HttpPost("like")]
    public async Task<IActionResult> LikeReview([FromBody] ReviewLikeDto? likeDto)
    {
        try
        {
            var result = await _reviewLikeService.LikeReviewAsync(likeDto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [Authorize]
    [HttpPost("unlike")]
    public async Task<IActionResult> UnlikeReview([FromBody] ReviewLikeDto? likeDto)
    {
        try
        {
            var result = await _reviewLikeService.UnlikeReviewAsync(likeDto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("is-liked")]
    public async Task<IActionResult> IsReviewLikedByUser([FromQuery] string? userId, [FromQuery] Guid? reviewId)
    {
        try
        {
            var result = await _reviewLikeService.IsReviewLikedByUserAsync(userId, reviewId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("count/{reviewId:guid}")]
    public async Task<IActionResult> GetReviewLikeCount([FromRoute] Guid? reviewId)
    {
        try
        {
            var result = await _reviewLikeService.GetReviewLikeCountAsync(reviewId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpPost("liked-by-user")]
    public async Task<IActionResult> GetLikedReviewsByUserId([FromBody] ReviewPaginationUserDto dto)
    {
        try
        {
            var result = await _reviewLikeService.GetLikedReviewsByUserIdAsync(dto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }
}
