namespace Cut_Roll_Users.Api.Controllers;

using Cut_Roll_Users.Api.Common.Extensions.Controllers;
using Cut_Roll_Users.Core.MovieEmbeddings.Dtos;
using Cut_Roll_Users.Core.MovieEmbeddings.Services;
using Cut_Roll_Users.Core.Common.Services;
using Cut_Roll_Users.Core.Common.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Route("[controller]")]
[ApiController]
public class RecommendationController : ControllerBase
{
    private readonly IVectorMovieDatabaseService _vectorService;
    private readonly IMovieEmbeddingService _embeddingService;
    private readonly IUserPreferenceService _userPreferenceService;

    public RecommendationController(
        IVectorMovieDatabaseService vectorService,
        IMovieEmbeddingService embeddingService,
        IUserPreferenceService userPreferenceService)
    {
        _vectorService = vectorService;
        _embeddingService = embeddingService;
        _userPreferenceService = userPreferenceService;
    }

    [HttpPost("similar-movies")]
    public async Task<IActionResult> GetSimilarMovies([FromBody] SimilarMoviesRequestDto request)
    {
        try
        {
            var similarMovies = await _userPreferenceService.GetSimilarMoviesAsync(request.MovieId, request.Limit);
            return Ok(similarMovies);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpPost("recommendations/{movieId:guid}")]
    public async Task<IActionResult> GetMovieRecommendations([FromRoute] Guid movieId, [FromBody] RecommendationRequestDto request)
    {
        try
        {
            // Use the user preference service to get similar movies
            var similarMovies = await _userPreferenceService.GetSimilarMoviesAsync(movieId, request.Limit);

            return Ok(similarMovies);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [Authorize]
    [HttpPost("friend-recommendations")]
    public async Task<IActionResult> GetFriendRecommendations([FromBody] FriendRecommendationRequestDto request)
    {
        try
        {
            var recommendations = await _userPreferenceService.GetFriendRecommendationsAsync(request);
            return Ok(recommendations);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetEmbeddingStatus()
    {
        try
        {
            var status = await _embeddingService.GetEmbeddingStatusAsync();
            return Ok(status);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    [HttpGet("health")]
    public async Task<IActionResult> CheckRecommendationHealth()
    {
        try
        {
            // Check if vector database is accessible and has data
            var isVectorDbEmpty = await _vectorService.IsVectorDbEmptyAsync();
            var embeddedMoviesCount = await _vectorService.GetEmbeddedMoviesCountAsync();
            
            var health = new
            {
                IsHealthy = !isVectorDbEmpty && embeddedMoviesCount > 0,
                IsVectorDbEmpty = isVectorDbEmpty,
                EmbeddedMoviesCount = embeddedMoviesCount,
                Message = isVectorDbEmpty ? "Vector database is empty" : 
                         embeddedMoviesCount == 0 ? "No movies have been embedded" : 
                         "Recommendation system is healthy"
            };

            return Ok(health);
        }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }
}

// Additional DTO for similar movies request
public class SimilarMoviesRequestDto
{
    public Guid MovieId { get; set; }
    public int Limit { get; set; } = 10;
    public List<Guid> ExcludeMovieIds { get; set; } = new();
}
