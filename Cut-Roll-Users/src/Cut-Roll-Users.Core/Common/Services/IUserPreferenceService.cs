using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.MovieEmbeddings.Dtos;

namespace Cut_Roll_Users.Core.Common.Services;

/// <summary>
/// Service for analyzing user preferences and providing personalized movie recommendations
/// </summary>
public interface IUserPreferenceService
{
    /// <summary>
    /// Get similar movies based on a specific movie
    /// </summary>
    /// <param name="movieId">The movie ID to find similar movies for</param>
    /// <param name="limit">Maximum number of recommendations to return</param>
    /// <returns>List of similar movies with similarity scores</returns>
    Task<List<MovieRecommendationDto>> GetSimilarMoviesAsync(Guid movieId, int limit = 10);

    /// <summary>
    /// Get content-based recommendations for a specific user
    /// </summary>
    /// <param name="userId">The user ID to get recommendations for</param>
    /// <param name="limit">Maximum number of recommendations to return</param>
    /// <returns>List of personalized movie recommendations</returns>
    Task<List<MovieRecommendationDto>> GetContentBasedRecommendationsAsync(string userId, int limit = 10);

    /// <summary>
    /// Get hybrid recommendations combining content-based and collaborative filtering
    /// </summary>
    /// <param name="userId">The user ID to get recommendations for</param>
    /// <param name="limit">Maximum number of recommendations to return</param>
    /// <returns>List of hybrid movie recommendations</returns>
    Task<List<MovieRecommendationDto>> GetHybridRecommendationsAsync(string userId, int limit = 10);

    /// <summary>
    /// Analyze user preferences and generate a taste vector
    /// </summary>
    /// <param name="userId">The user ID to analyze</param>
    /// <returns>User taste vector as list of floats</returns>
    Task<List<float>?> AnalyzeUserPreferencesAsync(string userId);

    /// <summary>
    /// Get user's taste profile based on their movie interactions
    /// </summary>
    /// <param name="userId">The user ID to get taste profile for</param>
    /// <returns>User taste profile information</returns>
    Task<UserTasteProfileDto> GetUserTasteProfileAsync(string userId);

    /// <summary>
    /// Refresh user preferences by re-analyzing their movie interactions
    /// Note: This doesn't store preferences, just refreshes the analysis
    /// </summary>
    /// <param name="userId">The user ID to refresh preferences for</param>
    /// <returns>True if preferences were refreshed successfully</returns>
    Task<bool> RefreshUserPreferencesAsync(string userId);

    /// <summary>
    /// Get recommendation explanation for why a movie was recommended
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="movieId">The recommended movie ID</param>
    /// <returns>Explanation of why the movie was recommended</returns>
    Task<RecommendationExplanationDto> GetRecommendationExplanationAsync(string userId, Guid movieId);

    /// <summary>
    /// Get friend recommendations for two users to watch together
    /// Excludes movies that either user has watched, liked, or added to want-to-watch
    /// </summary>
    /// <param name="request">Friend recommendation request with user IDs and filters</param>
    /// <returns>List of movies suitable for both users to watch together</returns>
    Task<List<FriendRecommendationDto>> GetFriendRecommendationsAsync(FriendRecommendationRequestDto request);
}
