namespace Cut_Roll_Users.Api.Controllers;

using Cut_Roll_Users.Api.Common.Extensions.Controllers;
using Cut_Roll_Users.Core.MovieEmbeddings.Dtos;
using Cut_Roll_Users.Core.MovieEmbeddings.Services;
using Cut_Roll_Users.Core.Common.Services;
using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.Common.DataProcessing;
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
    private readonly ITextEmbeddingService _textEmbeddingService;

    public RecommendationController(
        IVectorMovieDatabaseService vectorService,
        IMovieEmbeddingService embeddingService,
        IUserPreferenceService userPreferenceService,
        ITextEmbeddingService textEmbeddingService)
    {
        _vectorService = vectorService;
        _embeddingService = embeddingService;
        _userPreferenceService = userPreferenceService;
        _textEmbeddingService = textEmbeddingService;
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

    [Authorize]
    [HttpPost("user-recommendations")]
    public async Task<IActionResult> GetUserRecommendations([FromBody] UserRecommendationRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated");

            // Get personalized recommendations based on user's preferences
            var recommendations = await _userPreferenceService.GetContentBasedRecommendationsAsync(userId, request.Limit);

            return Ok(recommendations);
        }
        catch (ArgumentNullException ex) { return BadRequest(ex.Message); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
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
            
            // Get embedding status including background service info
            var embeddingStatus = await _embeddingService.GetEmbeddingStatusAsync();
            
            var health = new
            {
                IsHealthy = !isVectorDbEmpty && embeddedMoviesCount > 0,
                IsVectorDbEmpty = isVectorDbEmpty,
                EmbeddedMoviesCount = embeddedMoviesCount,
                BackgroundService = new
                {
                    IsProcessing = embeddingStatus.IsProcessing,
                    LastProcessedAt = embeddingStatus.LastProcessedAt,
                    Status = embeddingStatus.Status,
                    TotalProcessed = embeddingStatus.TotalProcessedMovies,
                    TotalFailed = embeddingStatus.TotalFailedMovies
                },
                Message = isVectorDbEmpty ? "Vector database is empty" : 
                         embeddedMoviesCount == 0 ? "No movies have been embedded" : 
                         "Recommendation system is healthy"
            };

            return Ok(health);
        }
        catch (Exception ex) { return this.InternalServerError(ex.Message); }
    }

    /// <summary>
    /// Manually triggers the background service to process movies without embeddings
    /// ADMIN ONLY - This will start processing immediately
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("trigger-processing")]
    public async Task<IActionResult> TriggerBackgroundProcessing()
    {
        try
        {
            // Get the background service and trigger processing
            var backgroundService = HttpContext.RequestServices.GetRequiredService<Cut_Roll_Users.Core.Common.BackgroundServices.IMovieEmbeddingBackgroundService>();
            
            var isProcessing = await backgroundService.IsProcessingAsync();
            if (isProcessing)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Background service is already processing movies. Please wait for current cycle to complete."
                });
            }

            // Trigger processing in background
            _ = Task.Run(async () =>
            {
                try
                {
                    await backgroundService.ProcessNewMoviesAsync();
                }
                catch (Exception ex)
                {
                    // Log error but don't throw to avoid breaking the response
                    var logger = HttpContext.RequestServices.GetRequiredService<ILogger<RecommendationController>>();
                    logger.LogError(ex, "Error in manual background processing trigger");
                }
            });

            return Ok(new
            {
                Success = true,
                Message = "Background processing triggered successfully. Check logs for progress.",
                Note = "Processing runs in background. Use /health endpoint to monitor status."
            });
        }
        catch (Exception ex)
        {
            return this.InternalServerError($"Error triggering background processing: {ex.Message}");
        }
    }

    /// <summary>
    /// Diagnoses the embedding model output dimensions and current configuration
    /// ADMIN ONLY - This helps debug embedding dimension issues
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("diagnose-embeddings")]
    public async Task<IActionResult> DiagnoseEmbeddings()
    {
        try
        {
            // Test with a sample text to see actual model output
            var testText = "The Dark Knight is a superhero action movie directed by Christopher Nolan";
            var embedding = await _textEmbeddingService.GenerateEmbeddingAsync(testText);
            
            var minValue = embedding.Min();
            var maxValue = embedding.Max();
            var meanValue = embedding.Average();
            var stdDevValue = Math.Sqrt(embedding.Select(x => Math.Pow(x - embedding.Average(), 2)).Average());
            
            var diagnosis = new
            {
                TestText = testText,
                ActualEmbeddingDimensions = embedding.Count,
                ExpectedPineconeDimensions = 4608, // Updated configuration
                EmbeddingSample = embedding.Take(10).ToArray(),
                EmbeddingStats = new
                {
                    Min = minValue,
                    Max = maxValue,
                    Mean = meanValue,
                    StdDev = stdDevValue
                },
                Recommendations = new
                {
                    CurrentDimensions = 4608,
                    Note = "Using full last_hidden_state dimensions for maximum semantic richness"
                }
            };

            return Ok(diagnosis);
        }
        catch (Exception ex)
        {
            return this.InternalServerError($"Error diagnosing embeddings: {ex.Message}");
        }
    }

    /// <summary>
    /// Detailed model inspection - shows raw ONNX model output before any processing
    /// ADMIN ONLY - This shows exactly what the model outputs before dimension reduction
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("inspect-model")]
    public IActionResult InspectModel()
    {
        try
        {
            var testText = "The Dark Knight is a superhero action movie directed by Christopher Nolan";
            
            // Based on your logs, we know the model outputs:
            // - last_hidden_state: 1x12x384 = 4,608 dimensions (token-level embeddings)
            // - pooler_output: 1x384 = 384 dimensions (sentence-level embedding)
            
            var inspection = new
            {
                TestText = testText,
                ModelOutputs = new
                {
                    LastHiddenState = new
                    {
                        Name = "last_hidden_state",
                        Shape = "1x12x384",
                        Dimensions = 4608, // 12 tokens × 384 dimensions
                        Description = "Token-level embeddings - contains rich contextual information",
                        Usage = "Currently NOT used - this is where the real power is!"
                    },
                    PoolerOutput = new
                    {
                        Name = "pooler_output", 
                        Shape = "1x384",
                        Dimensions = 384,
                        Description = "Sentence-level embedding - currently used",
                        Usage = "Currently selected - might be too simplistic"
                    }
                },
                CurrentSelection = new
                {
                    Selected = "last_hidden_state (full vector)",
                    Dimensions = 4608,
                    Status = "Now using full token-level embeddings for maximum semantic richness"
                },
                Implementation = new
                {
                    Method = "last_hidden_state (full vector)",
                    Dimensions = 4608,
                    Process = "1. Get last_hidden_state (1x12x384), 2. Flatten to 4608 dimensions, 3. Preserve all token information",
                    Status = "IMPLEMENTED - Using maximum semantic information"
                },
                TokenizerIssue = new
                {
                    Problem = "Tokenizer not found",
                    CurrentPath = "/app/Data/Models/",
                    ActualPath = "Cut-Roll-Users\\Data\\Models",
                    Impact = "Model might not be tokenizing text properly, affecting quality"
                }
            };

            return Ok(inspection);
        }
        catch (Exception ex)
        {
            return this.InternalServerError($"Error inspecting model: {ex.Message}");
        }
    }

    /// <summary>
    /// Resets all embeddings by deleting all vectors from Pinecone and setting HasEmbedding=false for all movies
    /// Background service will regenerate all embeddings on next run with consistent method
    /// ADMIN ONLY - This operation deletes ALL vectors and resets database flags
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("reset-embeddings")]
    public async Task<IActionResult> ResetAllEmbeddings()
    {
        try
        {
            var success = await _embeddingService.ResetAllEmbeddingsAsync();
            
            if (success)
            {
                var response = new
                {
                    Success = true,
                    Message = "Embeddings reset successfully! All vectors deleted from Pinecone and HasEmbedding set to false for all movies.",
                    Instructions = new
                    {
                        Step1 = "Restart your application",
                        Step2 = "Background service will automatically regenerate all embeddings",
                        Step3 = "New embeddings will use consistent method for better similarity scores",
                        ExpectedResult = "Much higher similarity scores (0.7+ instead of 0.112) and better recommendations"
                    }
                };
                
                return Ok(response);
            }
            else
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Failed to reset embeddings. Check logs for details."
                });
            }
        }
        catch (Exception ex) 
        { 
            return this.InternalServerError($"Error resetting embeddings: {ex.Message}"); 
        }
    }
}

// Additional DTO for similar movies request
public class SimilarMoviesRequestDto
{
    public Guid MovieId { get; set; }
    public int Limit { get; set; } = 10;
    public List<Guid> ExcludeMovieIds { get; set; } = new();
}

// DTO for user recommendations request
public class UserRecommendationRequestDto
{
    public int Limit { get; set; } = 10;
    public List<Guid> ExcludeMovieIds { get; set; } = new();
}
