namespace Cut_Roll_Users.Api.Controllers;

using Cut_Roll_Users.Api.Common.Extensions.Controllers;
using Cut_Roll_Users.Core.Users.Dtos;
using Cut_Roll_Users.Core.Users.Services;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] string? id)
    {
        try
        {
            var result = await _userService.GetUserByIdAsync(id);
            return result is not null ? Ok(result) : NotFound("User not found.");
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("by-username/{username}")]
    public async Task<IActionResult> GetByUsername([FromRoute] string? username)
    {
        try
        {
            var result = await _userService.GetUserByUsernameAsync(username);
            return result is not null ? Ok(result) : NotFound("User not found.");
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("by-email/{email}")]
    public async Task<IActionResult> GetByEmail([FromRoute] string? email)
    {
        try
        {
            var result = await _userService.GetUserByEmailAsync(email);
            return result is not null ? Ok(result) : NotFound("User not found.");
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] UserSearchDto? dto)
    {
        try
        {
            var result = await _userService.SearchUsersAsync(dto);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("exists/username/{username}")]
    public async Task<IActionResult> UserExistsByUsername([FromRoute] string? username)
    {
        try
        {
            var result = await _userService.UserExistsByUsernameAsync(username);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("exists/email/{email}")]
    public async Task<IActionResult> UserExistsByEmail([FromRoute] string? email)
    {
        try
        {
            var result = await _userService.UserExistsByEmailAsync(email);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("review-count/{userId}")]
    public async Task<IActionResult> GetUserReviewCount([FromRoute] string? userId)
    {
        try
        {
            var result = await _userService.GetUserReviewCountAsync(userId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("watched-count/{userId}")]
    public async Task<IActionResult> GetUserWatchedCount([FromRoute] string? userId)
    {
        try
        {
            var result = await _userService.GetUserWatchedCountAsync(userId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("movie-like-count/{userId}")]
    public async Task<IActionResult> GetUserMovieLikeCount([FromRoute] string? userId)
    {
        try
        {
            var result = await _userService.GetUserMovieLikeCountAsync(userId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("want-to-watch-count/{userId}")]
    public async Task<IActionResult> GetUserWantToWatchCount([FromRoute] string? userId)
    {
        try
        {
            var result = await _userService.GetUserWantToWatchCountAsync(userId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("list-count/{userId}")]
    public async Task<IActionResult> GetUserListCount([FromRoute] string? userId)
    {
        try
        {
            var result = await _userService.GetUserListCountAsync(userId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("average-rating/{userId}")]
    public async Task<IActionResult> GetUserAverageRating([FromRoute] string? userId)
    {
        try
        {
            var result = await _userService.GetUserAverageRatingAsync(userId);
            return Ok(result);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }
}
