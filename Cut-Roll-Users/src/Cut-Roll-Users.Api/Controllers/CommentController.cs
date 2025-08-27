namespace Cut_Roll_Users.Api.Controllers;

using Cut_Roll_Users.Api.Common.Extensions.Controllers;
using Cut_Roll_Users.Core.Comments.Dtos;
using Cut_Roll_Users.Core.Comments.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]")]
[ApiController]
public class CommentController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CommentCreateDto? commentDto)
    {
        try
        {
            var result = await _commentService.CreateCommentAsync(commentDto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [Authorize]
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] CommentUpdateDto? commentDto)
    {
        try
        {
            var result = await _commentService.UpdateCommentAsync(commentDto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> Delete([FromBody] CommentUpdateDto? commentDto)
    {
        try
        {
            var result = await _commentService.DeleteCommentAsync(commentDto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpPost("by-review")]
    public async Task<IActionResult> GetByReviewId([FromBody] CommentPaginationReviewDto dto)
    {
        try
        {
            var result = await _commentService.GetByReviewIdAsync(dto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpPost("by-user")]
    public async Task<IActionResult> GetByUserId([FromBody] CommentPaginationUserDto dto)
    {
        try
        {
            var result = await _commentService.GetByUserIdAsync(dto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("count/{reviewId:guid}")]
    public async Task<IActionResult> GetCommentCountByReviewId([FromRoute] Guid? reviewId)
    {
        try
        {
            var result = await _commentService.GetCommentCountByReviewIdAsync(reviewId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }
}
