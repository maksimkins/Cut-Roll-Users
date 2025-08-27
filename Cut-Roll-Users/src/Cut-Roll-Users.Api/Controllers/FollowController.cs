namespace Cut_Roll_Users.Api.Controllers;

using Cut_Roll_Users.Api.Common.Extensions.Controllers;
using Cut_Roll_Users.Core.Follows.Dtos;
using Cut_Roll_Users.Core.Follows.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]")]
[ApiController]
public class FollowController : ControllerBase
{
    private readonly IFollowService _followService;

    public FollowController(IFollowService followService)
    {
        _followService = followService;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] FollowCreateDto? dto)
    {
        try
        {
            var result = await _followService.CreateFollowAsync(dto);
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
            var result = await _followService.GetFollowByIdAsync(id);
            return result is not null ? Ok(result) : NotFound("Follow not found.");
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> Delete([FromBody] FollowDeleteDto? dto)
    {
        try
        {
            var result = await _followService.DeleteFollowAsync(dto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [Authorize]
    [HttpDelete("unfollow")]
    public async Task<IActionResult> UnfollowUser([FromQuery] string? followerId, [FromQuery] string? followingId)
    {
        try
        {
            var result = await _followService.UnfollowUserAsync(followerId, followingId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpPost("by-user")]
    public async Task<IActionResult> GetUserFollows([FromBody] FollowPaginationDto? dto)
    {
        try
        {
            var result = await _followService.GetUserFollowsAsync(dto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("followers-count/{userId}")]
    public async Task<IActionResult> GetFollowersCount([FromRoute] string? userId)
    {
        try
        {
            var result = await _followService.GetFollowersCountAsync(userId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("following-count/{userId}")]
    public async Task<IActionResult> GetFollowingCount([FromRoute] string? userId)
    {
        try
        {
            var result = await _followService.GetFollowingCountAsync(userId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetFollowStatus([FromQuery] string? userId, [FromQuery] string? targetUserId)
    {
        try
        {
            var result = await _followService.GetFollowStatusAsync(userId, targetUserId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("is-following")]
    public async Task<IActionResult> IsFollowing([FromQuery] string? followerId, [FromQuery] string? followingId)
    {
        try
        {
            var result = await _followService.IsFollowingAsync(followerId, followingId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpPost("feed")]
    public async Task<IActionResult> GetUserFeed([FromBody] FeedPaginationDto? dto)
    {
        try
        {
            var result = await _followService.GetUserFeedAsync(dto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }
}
