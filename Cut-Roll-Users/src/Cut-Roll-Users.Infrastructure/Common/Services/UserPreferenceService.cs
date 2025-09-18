using Microsoft.Extensions.Logging;
using Cut_Roll_Users.Core.Common.Services;
using Cut_Roll_Users.Core.Common.Dtos;
using Cut_Roll_Users.Core.MovieEmbeddings.Dtos;
using Cut_Roll_Users.Core.MovieEmbeddings.Services;
using Cut_Roll_Users.Core.Movies.Service;
using Cut_Roll_Users.Core.Movies.Models;
using Cut_Roll_Users.Core.Reviews.Services;
using Cut_Roll_Users.Core.Reviews.Dtos;
using Cut_Roll_Users.Core.Common.DataProcessing;
using Cut_Roll_Users.Core.WantToWatchFilms.Services;
using Cut_Roll_Users.Core.WantToWatchFilms.Dtos;
using Cut_Roll_Users.Core.Follows.Services;

namespace Cut_Roll_Users.Infrastructure.Common.Services;

/// <summary>
/// Service for analyzing user preferences and providing personalized movie recommendations
/// 
/// IMPORTANT: This service does NOT store user preferences persistently.
/// Instead, it generates user taste vectors dynamically by:
/// 1. Analyzing user's movie interactions (liked, watched, reviewed movies)
/// 2. Creating weighted average embeddings from their liked movies
/// 3. Using these taste vectors to query the vector database for similar movies
/// 4. Filtering out already watched/liked movies
/// 
/// This approach ensures recommendations are always up-to-date with user's current preferences
/// without requiring additional storage or complex preference management.
/// </summary>
public class UserPreferenceService : IUserPreferenceService
{
    private readonly IMovieEmbeddingService _movieEmbeddingService;
    private readonly ITextEmbeddingService _textEmbeddingService;
    private readonly IVectorMovieDatabaseService _vectorDatabaseService;
    private readonly IMovieService _movieService;
    private readonly IReviewService _reviewService;
    private readonly IWantToWatchFilmService _wantToWatchFilmService;
    private readonly IFollowService _followService;
    private readonly ILogger<UserPreferenceService> _logger;

    public UserPreferenceService(
        IMovieEmbeddingService movieEmbeddingService,
        ITextEmbeddingService textEmbeddingService,
        IVectorMovieDatabaseService vectorDatabaseService,
        IMovieService movieService,
        IReviewService reviewService,
        IWantToWatchFilmService wantToWatchFilmService,
        IFollowService followService,
        ILogger<UserPreferenceService> logger)
    {
        _movieEmbeddingService = movieEmbeddingService ?? throw new ArgumentNullException(nameof(movieEmbeddingService));
        _textEmbeddingService = textEmbeddingService ?? throw new ArgumentNullException(nameof(textEmbeddingService));
        _vectorDatabaseService = vectorDatabaseService ?? throw new ArgumentNullException(nameof(vectorDatabaseService));
        _movieService = movieService ?? throw new ArgumentNullException(nameof(movieService));
        _reviewService = reviewService ?? throw new ArgumentNullException(nameof(reviewService));
        _wantToWatchFilmService = wantToWatchFilmService ?? throw new ArgumentNullException(nameof(wantToWatchFilmService));
        _followService = followService ?? throw new ArgumentNullException(nameof(followService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<MovieRecommendationDto>> GetSimilarMoviesAsync(Guid movieId, int limit = 10)
    {
        try
        {
            if (movieId == Guid.Empty)
            {
                throw new ArgumentException("Movie ID cannot be empty", nameof(movieId));
            }

            if (limit <= 0)
            {
                _logger.LogWarning("Invalid limit {Limit} provided, using default limit of 10", limit);
                limit = 10; // Use a default limit instead of throwing
            }

            _logger.LogInformation("Getting similar movies for movie {MovieId} with limit {Limit}", movieId, limit);

            // Get movie data to generate query embedding
            _logger.LogInformation("Step 1: Fetching movie from database...");
            var movie = await _movieService.GetMovieByIdAsync(movieId);
            if (movie == null)
            {
                _logger.LogWarning("Movie {MovieId} not found in database", movieId);
                return new List<MovieRecommendationDto>();
            }
            _logger.LogInformation("Step 1: Movie found - {Title}", movie.Title);

            // Convert Movie to MovieDataForEmbeddingDto
            _logger.LogInformation("Step 2: Converting movie to embedding data...");
            var movieData = ConvertMovieToEmbeddingData(movie);
            if (movieData == null)
            {
                _logger.LogWarning("Could not convert movie {MovieId} to embedding data", movieId);
                return new List<MovieRecommendationDto>();
            }
            _logger.LogInformation("Step 2: Movie data converted successfully");

            // Generate embedding for the movie
            _logger.LogInformation("Step 3: Generating embedding for movie...");
            var movieEmbedding = await _textEmbeddingService.GenerateMovieEmbeddingAsync(movieData);
            if (movieEmbedding == null || !movieEmbedding.Any())
            {
                _logger.LogWarning("Failed to generate embedding for movie {MovieId}", movieId);
                return new List<MovieRecommendationDto>();
            }
            _logger.LogInformation("Step 3: Embedding generated - {Dimension} dimensions", movieEmbedding.Count);

            // Add some diversity by slightly perturbing the query vector
            var diversifiedEmbedding = AddDiversityToEmbedding(movieEmbedding, movieId);

            // Query vector database for similar movies with higher limit to allow for diversity filtering
            _logger.LogInformation("Step 4: Querying Pinecone for similar movies...");
            var rawRecommendations = await _vectorDatabaseService.FindSimilarMoviesAsync(
                diversifiedEmbedding, 
                limit * 2, // Get more results for diversity filtering
                new List<Guid> { movieId } // Exclude the original movie
            );

            _logger.LogInformation("Step 4: Found {Count} raw similar movies for movie {MovieId}", rawRecommendations.Count, movieId);

            // Apply diversity filtering to reduce similar recommendations
            var diversifiedRecommendations = ApplyDiversityFiltering(rawRecommendations, limit);

            _logger.LogInformation("Step 5: Applied diversity filtering, returning {Count} diverse recommendations", diversifiedRecommendations.Count);
            return diversifiedRecommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting similar movies for movie {MovieId}", movieId);
            return new List<MovieRecommendationDto>();
        }
    }

    public async Task<List<MovieRecommendationDto>> GetContentBasedRecommendationsAsync(string userId, int limit = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            }

            if (limit <= 0)
            {
                throw new ArgumentException("Limit must be greater than 0", nameof(limit));
            }

            _logger.LogDebug("Getting content-based recommendations for user {UserId} with limit {Limit}", userId, limit);

            // Analyze user preferences to get taste vector
            var userTasteVector = await AnalyzeUserPreferencesAsync(userId);
            if (userTasteVector == null || !userTasteVector.Any())
            {
                _logger.LogWarning("Could not generate taste vector for user {UserId}, falling back to popular movies", userId);
                return await GetFallbackRecommendationsAsync(userId, limit);
            }

            // Get user's already watched/liked movies to exclude
            var watchedMovies = await _movieService.GetWatchedMoviesByUserIdAsync(userId);
            var likedMovies = await _movieService.GetLikedMoviesByUserIdAsync(userId);
            
            // Get user's want-to-watch movies
            var wantToWatchPaginationDto = new WantToWatchFilmPaginationUserDto
            {
                UserId = userId,
                Page = 1,
                PageSize = 1000 // Get all want-to-watch movies
            };
            var wantToWatchResult = await _wantToWatchFilmService.GetWantToWatchFilmsByUserIdAsync(wantToWatchPaginationDto);
            var wantToWatchMovieIds = wantToWatchResult?.Data?.Select(m => m.MovieId).ToList() ?? new List<Guid>();

            var excludeMovieIds = watchedMovies.Select(m => m.Id)
                .Concat(likedMovies.Select(m => m.Id))
                .Concat(wantToWatchMovieIds)
                .Distinct()
                .ToList();

            // Find similar movies based on user taste
            var recommendations = await _vectorDatabaseService.FindSimilarMoviesAsync(
                userTasteVector,
                limit,
                excludeMovieIds
            );

            _logger.LogDebug("Found {Count} content-based recommendations for user {UserId}", recommendations.Count, userId);
            return recommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting content-based recommendations for user {UserId}", userId);
            return new List<MovieRecommendationDto>();
        }
    }

    public async Task<List<MovieRecommendationDto>> GetHybridRecommendationsAsync(string userId, int limit = 10)
    {
        try
        {
            _logger.LogDebug("Getting hybrid recommendations for user {UserId} with limit {Limit}", userId, limit);

            // Get both content-based and collaborative recommendations
            var contentBased = await GetContentBasedRecommendationsAsync(userId, limit);
            var collaborative = await GetCollaborativeRecommendationsAsync(userId, limit);

            // If we have no recommendations from either method, return fallback
            if (!contentBased.Any() && !collaborative.Any())
            {
                _logger.LogDebug("No recommendations from content-based or collaborative methods, using fallback for user {UserId}", userId);
                return await GetFallbackRecommendationsAsync(userId, limit);
            }

            // Combine and rank recommendations
            var hybridRecommendations = CombineRecommendations(contentBased, collaborative, limit);

            // If combined recommendations are still empty, use fallback
            if (!hybridRecommendations.Any())
            {
                _logger.LogDebug("Combined recommendations are empty, using fallback for user {UserId}", userId);
                return await GetFallbackRecommendationsAsync(userId, limit);
            }

            _logger.LogDebug("Generated {Count} hybrid recommendations for user {UserId}", hybridRecommendations.Count, userId);
            return hybridRecommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting hybrid recommendations for user {UserId}", userId);
            return await GetFallbackRecommendationsAsync(userId, limit);
        }
    }

    public async Task<List<float>?> AnalyzeUserPreferencesAsync(string userId)
    {
        try
        {
            _logger.LogDebug("Analyzing user preferences for user {UserId}", userId);

            // Get user's liked movies, watched movies, and reviews
            var likedMovies = await _movieService.GetLikedMoviesByUserIdAsync(userId);
            var watchedMovies = await _movieService.GetWatchedMoviesByUserIdAsync(userId);
            var userReviews = await _reviewService.GetReviewsByUserIdAsync(new ReviewPaginationUserDto 
            { 
                UserId = userId, 
                Page = 1, 
                PageSize = 100 
            });

            if (!likedMovies.Any() && !watchedMovies.Any() && !userReviews.Data.Any())
            {
                _logger.LogWarning("No movie data found for user {UserId}", userId);
                return null;
            }

            // Create weighted movie data with different weights for different interaction types
            var weightedMovieData = new List<(MovieDataForEmbeddingDto data, float weight, string interactionType)>();
            
            // Process liked movies with highest weight
            foreach (var movie in likedMovies)
            {
                var movieData = ConvertMovieToEmbeddingData(movie);
                if (movieData != null)
                {
                    var userReview = userReviews.Data.FirstOrDefault(r => r.MovieSimplified.MovieId == movie.Id);
                    var baseWeight = userReview?.Rating ?? (float)(movie.VoteAverage ?? 5.0f);
                    // Liked movies get 2x weight
                    var weight = Math.Max(1.0f, baseWeight * 2.0f);
                    weightedMovieData.Add((movieData, weight, "liked"));
                }
            }

            // Process watched movies with medium weight
            foreach (var movie in watchedMovies)
            {
                // Skip if already processed as liked
                if (likedMovies.Any(lm => lm.Id == movie.Id))
                    continue;
                    
                var movieData = ConvertMovieToEmbeddingData(movie);
                if (movieData != null)
                {
                    var userReview = userReviews.Data.FirstOrDefault(r => r.MovieSimplified.MovieId == movie.Id);
                    var baseWeight = userReview?.Rating ?? (float)(movie.VoteAverage ?? 5.0f);
                    // Watched movies get 1.5x weight
                    var weight = Math.Max(0.5f, baseWeight * 1.5f);
                    weightedMovieData.Add((movieData, weight, "watched"));
                }
            }

            // Process reviewed movies (even if not explicitly liked/watched)
            foreach (var review in userReviews.Data)
            {
                var movie = likedMovies.Concat(watchedMovies).FirstOrDefault(m => m.Id == review.MovieSimplified.MovieId);
                if (movie == null) continue; // Skip if already processed
                
                var movieData = ConvertMovieToEmbeddingData(movie);
                if (movieData != null)
                {
                    // Reviews get weight based on rating
                    var weight = Math.Max(0.3f, review.Rating);
                    weightedMovieData.Add((movieData, weight, "reviewed"));
                }
            }

            if (!weightedMovieData.Any())
            {
                _logger.LogWarning("No valid movie data found for user {UserId}", userId);
                return null;
            }

            _logger.LogDebug("User {UserId} has {LikedCount} liked, {WatchedCount} watched, {ReviewedCount} reviewed movies", 
                userId, 
                weightedMovieData.Count(w => w.interactionType == "liked"),
                weightedMovieData.Count(w => w.interactionType == "watched"),
                weightedMovieData.Count(w => w.interactionType == "reviewed"));

            // Generate embeddings for each movie
            var embeddings = new List<List<float>>();
            var weights = new List<float>();

            foreach (var (data, weight, interactionType) in weightedMovieData)
            {
                // Use the same text preparation method as stored movie embeddings
                var textToEmbed = PrepareTextForEmbedding(data);
                var embedding = await _textEmbeddingService.GenerateEmbeddingAsync(textToEmbed);
                if (embedding != null && embedding.Any())
                {
                    embeddings.Add(embedding.ToList());
                    weights.Add(weight);
                }
            }

            if (!embeddings.Any())
            {
                _logger.LogWarning("No valid embeddings generated for user {UserId}", userId);
                return null;
            }

            // Calculate weighted average embedding with user-specific normalization
            var tasteVector = CalculateUserSpecificWeightedAverageEmbedding(embeddings, weights, userId);

            _logger.LogDebug("Generated taste vector for user {UserId} with dimension {Dimension} from {MovieCount} movies", 
                userId, tasteVector.Count, embeddings.Count);
            return tasteVector;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing user preferences for user {UserId}", userId);
            return null;
        }
    }

    public async Task<UserTasteProfileDto> GetUserTasteProfileAsync(string userId)
    {
        try
        {
            _logger.LogDebug("Getting taste profile for user {UserId}", userId);

            var watchedMovies = await _movieService.GetWatchedMoviesByUserIdAsync(userId);
            var likedMovies = await _movieService.GetLikedMoviesByUserIdAsync(userId);
            var userReviews = await _reviewService.GetReviewsByUserIdAsync(new ReviewPaginationUserDto 
            { 
                UserId = userId, 
                Page = 1, 
                PageSize = 100 
            });

            var tasteVector = await AnalyzeUserPreferencesAsync(userId);

            // Analyze preferences
            var preferredGenres = AnalyzePreferredGenres(likedMovies);
            var preferredKeywords = AnalyzePreferredKeywords(likedMovies);
            var preferredActors = AnalyzePreferredActors(likedMovies);
            var preferredDirectors = AnalyzePreferredDirectors(likedMovies);
            var averageRating = userReviews.Data.Any() ? userReviews.Data.Average(r => r.Rating) : 0;

            var profile = new UserTasteProfileDto
            {
                UserId = userId,
                PreferredGenres = preferredGenres,
                PreferredKeywords = preferredKeywords,
                PreferredActors = preferredActors,
                PreferredDirectors = preferredDirectors,
                AverageRating = averageRating,
                TotalMoviesWatched = watchedMovies.Count,
                TotalMoviesLiked = likedMovies.Count,
                LastUpdated = DateTime.UtcNow,
                TasteVector = tasteVector
            };

            _logger.LogDebug("Generated taste profile for user {UserId}", userId);
            return profile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting taste profile for user {UserId}", userId);
            return new UserTasteProfileDto { UserId = userId };
        }
    }

    public async Task<bool> RefreshUserPreferencesAsync(string userId)
    {
        try
        {
            _logger.LogDebug("Refreshing user preferences for user {UserId}", userId);

            // Re-analyze user preferences (no storage needed - generated on-demand)
            var tasteVector = await AnalyzeUserPreferencesAsync(userId);
            
            // Note: We don't store preferences - they're generated dynamically from user's movie interactions
            // The taste vector is computed in real-time based on their liked movies and reviews
            _logger.LogInformation("User preferences refreshed for user {UserId} - taste vector dimension: {Dimension}", 
                userId, tasteVector?.Count ?? 0);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing user preferences for user {UserId}", userId);
            return false;
        }
    }

    public async Task<RecommendationExplanationDto> GetRecommendationExplanationAsync(string userId, Guid movieId)
    {
        try
        {
            _logger.LogDebug("Getting recommendation explanation for user {UserId} and movie {MovieId}", userId, movieId);

            var movie = await _movieService.GetMovieByIdAsync(movieId);
            if (movie == null)
            {
                return new RecommendationExplanationDto
                {
                    UserId = userId,
                    MovieId = movieId,
                    Explanation = "Movie not found"
                };
            }

            var userProfile = await GetUserTasteProfileAsync(userId);
            var similarMovies = await GetSimilarMoviesAsync(movieId, 3);

            // Generate explanation based on user profile and movie similarity
            var reasons = new List<string>();
            var matchingGenres = new List<string>();
            var matchingKeywords = new List<string>();

            // Check genre matches
            if (movie.MovieGenres != null)
            {
                matchingGenres = movie.MovieGenres
                    .Where(g => userProfile.PreferredGenres.Contains(g.Genre.Name))
                    .Select(g => g.Genre.Name)
                    .ToList();
                
                if (matchingGenres.Any())
                {
                    reasons.Add($"You like {string.Join(", ", matchingGenres)} movies");
                }
            }

            // Check keyword matches
            if (movie.Keywords != null)
            {
                matchingKeywords = movie.Keywords
                    .Where(k => userProfile.PreferredKeywords.Contains(k.Keyword.Name))
                    .Select(k => k.Keyword.Name)
                    .ToList();
                
                if (matchingKeywords.Any())
                {
                    reasons.Add($"This movie has themes you enjoy: {string.Join(", ", matchingKeywords)}");
                }
            }

            // Check actor/director matches
            var matchingActors = movie.Cast?
                .Where(c => userProfile.PreferredActors.Contains(c.Person.Name))
                .Select(c => c.Person.Name)
                .ToList() ?? new List<string>();

            if (matchingActors.Any())
            {
                reasons.Add($"You like movies with {string.Join(", ", matchingActors)}");
            }

            var explanation = reasons.Any() 
                ? $"We think you'll like this movie because: {string.Join("; ", reasons)}"
                : "This movie is similar to others you've enjoyed";

            return new RecommendationExplanationDto
            {
                UserId = userId,
                MovieId = movieId,
                MovieTitle = movie.Title,
                SimilarityScore = similarMovies.FirstOrDefault()?.SimilarityScore ?? 0,
                Reasons = reasons,
                SimilarMovies = similarMovies.Take(3).Select(m => m.Title).ToList(),
                MatchingGenres = matchingGenres,
                MatchingKeywords = matchingKeywords,
                Explanation = explanation
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recommendation explanation for user {UserId} and movie {MovieId}", userId, movieId);
            return new RecommendationExplanationDto
            {
                UserId = userId,
                MovieId = movieId,
                Explanation = "Unable to generate explanation"
            };
        }
    }

    #region Private Helper Methods

    private MovieDataForEmbeddingDto? ConvertMovieToEmbeddingData(Movie movie)
    {
        try
        {
            return new MovieDataForEmbeddingDto
            {
                Id = movie.Id,
                Title = movie.Title,
                Overview = movie.Overview ?? string.Empty,
                Genres = movie.MovieGenres?.Where(mg => mg.Genre != null).Select(mg => mg.Genre.Name).ToList() ?? new List<string>(),
                Keywords = movie.Keywords?.Where(mk => mk.Keyword != null).Select(mk => mk.Keyword.Name).ToList() ?? new List<string>(),
                Cast = movie.Cast?.Where(c => c.Person != null).Select(c => c.Person.Name).ToList() ?? new List<string>(),
                Crew = movie.Crew?.Where(c => c.Person != null).Select(c => c.Person.Name).ToList() ?? new List<string>(),
                ProductionCompanies = movie.ProductionCompanies?.Where(pc => pc.Company != null).Select(pc => pc.Company.Name).ToList() ?? new List<string>(),
                ProductionCountries = movie.ProductionCountries?.Where(pc => pc.Country != null).Select(pc => pc.Country.Name).ToList() ?? new List<string>(),
                SpokenLanguages = movie.SpokenLanguages?.Where(sl => sl.Language != null).Select(sl => sl.Language.EnglishName).ToList() ?? new List<string>(),
                // VoteAverage not available in MovieDataForEmbeddingDto
                ReleaseDate = movie.ReleaseDate,
                PosterPath = movie.Images?.FirstOrDefault(i => i.Type == "poster")?.FilePath
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting movie {MovieId} to embedding data", movie.Id);
            return null;
        }
    }

    private async Task<List<MovieRecommendationDto>> GetCollaborativeRecommendationsAsync(string userId, int limit)
    {
        try
        {
            _logger.LogDebug("Generating collaborative recommendations for user {UserId}", userId);

            // Get user's liked movies
            var userMovies = await _movieService.GetLikedMoviesByUserIdAsync(userId);
            if (!userMovies.Any())
            {
                _logger.LogDebug("No liked movies found for user {UserId}, skipping collaborative filtering", userId);
                return new List<MovieRecommendationDto>();
            }

            // For now, collaborative filtering is disabled due to complexity
            // In a real system, you'd implement sophisticated algorithms like:
            // - Cosine similarity on user-item matrices
            // - Matrix factorization (SVD, NMF)
            // - Deep learning approaches (neural collaborative filtering)
            _logger.LogDebug("Collaborative filtering disabled - returning empty recommendations for user {UserId}", userId);
            return new List<MovieRecommendationDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating collaborative recommendations for user {UserId}", userId);
            return new List<MovieRecommendationDto>();
        }
    }

    private List<MovieRecommendationDto> CombineRecommendations(
        List<MovieRecommendationDto> contentBased, 
        List<MovieRecommendationDto> collaborative, 
        int limit)
    {
        // Combine and deduplicate recommendations
        var combined = new Dictionary<Guid, MovieRecommendationDto>();

        // Add content-based recommendations with weight 0.7
        foreach (var rec in contentBased)
        {
            rec.SimilarityScore *= 0.7;
            combined[rec.MovieId] = rec;
        }

        // Add collaborative recommendations with weight 0.3
        foreach (var rec in collaborative)
        {
            rec.SimilarityScore *= 0.3;
            if (combined.ContainsKey(rec.MovieId))
            {
                combined[rec.MovieId].SimilarityScore += rec.SimilarityScore;
            }
            else
            {
                combined[rec.MovieId] = rec;
            }
        }

        return combined.Values
            .OrderByDescending(r => r.SimilarityScore)
            .Take(limit)
            .ToList();
    }


    /// <summary>
    /// Calculates user-specific weighted average embedding with enhanced personalization
    /// </summary>
    private List<float> CalculateUserSpecificWeightedAverageEmbedding(List<List<float>> embeddings, List<float> weights, string userId)
    {
        if (!embeddings.Any()) return new List<float>();

        var dimension = embeddings[0].Count;
        var weightedSum = new float[dimension];
        var totalWeight = weights.Sum();

        // Apply user-specific weighting and normalization
        var userHash = Math.Abs(userId.GetHashCode());
        var userSpecificFactor = 1.0f + (userHash % 100) / 1000.0f; // Small user-specific variation

        for (int i = 0; i < embeddings.Count; i++)
        {
            var embedding = embeddings[i];
            var weight = weights[i];

            // Apply user-specific weight adjustment
            var adjustedWeight = weight * userSpecificFactor;

            for (int j = 0; j < dimension; j++)
            {
                weightedSum[j] += embedding[j] * adjustedWeight;
            }
        }

        // Normalize by total weight
        for (int j = 0; j < dimension; j++)
        {
            weightedSum[j] /= totalWeight;
        }

        // Apply L2 normalization to prevent vector magnitude issues
        var norm = Math.Sqrt(weightedSum.Sum(x => x * x));
        if (norm > 0)
        {
            for (int j = 0; j < dimension; j++)
            {
                weightedSum[j] = (float)(weightedSum[j] / norm);
            }
        }

        return weightedSum.ToList();
    }

    private List<string> AnalyzePreferredGenres(List<Movie> movies)
    {
        return movies
            .SelectMany(m => m.MovieGenres?.Select(mg => mg.Genre.Name) ?? Enumerable.Empty<string>())
            .GroupBy(g => g)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => g.Key)
            .ToList();
    }

    private List<string> AnalyzePreferredKeywords(List<Movie> movies)
    {
        return movies
            .SelectMany(m => m.Keywords?.Select(mk => mk.Keyword.Name) ?? Enumerable.Empty<string>())
            .GroupBy(k => k)
            .OrderByDescending(k => k.Count())
            .Take(10)
            .Select(k => k.Key)
            .ToList();
    }

    private List<string> AnalyzePreferredActors(List<Movie> movies)
    {
        return movies
            .SelectMany(m => m.Cast?.Select(c => c.Person.Name) ?? Enumerable.Empty<string>())
            .GroupBy(a => a)
            .OrderByDescending(a => a.Count())
            .Take(10)
            .Select(a => a.Key)
            .ToList();
    }

    private List<string> AnalyzePreferredDirectors(List<Movie> movies)
    {
        return movies
            .SelectMany(m => m.Crew?.Where(c => c.Job == "Director").Select(c => c.Person.Name) ?? Enumerable.Empty<string>())
            .GroupBy(d => d)
            .OrderByDescending(d => d.Count())
            .Take(10)
            .Select(d => d.Key)
            .ToList();
    }

    #endregion

    #region Friend Recommendations

    public async Task<List<FriendRecommendationDto>> GetFriendRecommendationsAsync(FriendRecommendationRequestDto request)
    {
        try
        {
            _logger.LogInformation("Getting friend recommendations for users {UserId1} and {UserId2} with limit {Limit}", 
                request.UserId1, request.UserId2, request.Limit);

            // Validate request
            if (string.IsNullOrEmpty(request.UserId1) || string.IsNullOrEmpty(request.UserId2))
            {
                _logger.LogWarning("Friend recommendations failed: One or both user IDs are empty");
                throw new ArgumentException("Both user IDs must be provided");
            }

            if (request.UserId1 == request.UserId2)
            {
                _logger.LogWarning("Friend recommendations failed: User IDs are the same");
                throw new ArgumentException("User IDs must be different");
            }

            // Check if users are mutual friends
            _logger.LogDebug("Checking if users {UserId1} and {UserId2} are mutual friends", request.UserId1, request.UserId2);
            var areMutualFriends = await _followService.AreMutualFriendsAsync(request.UserId1, request.UserId2);
            _logger.LogInformation("Mutual friends check result: {AreMutualFriends}", areMutualFriends);
            
            if (!areMutualFriends)
            {
                _logger.LogWarning("Friend recommendations failed: Users {UserId1} and {UserId2} are not mutual friends", request.UserId1, request.UserId2);
                throw new InvalidOperationException("Users must be mutual friends to get friend recommendations. Both users need to follow each other.");
            }

            // Get both users' watched/liked movies
            _logger.LogDebug("Getting user interaction data for both users");
            var user1Watched = await _movieService.GetWatchedMoviesByUserIdAsync(request.UserId1);
            var user1Liked = await _movieService.GetLikedMoviesByUserIdAsync(request.UserId1);
            var user2Watched = await _movieService.GetWatchedMoviesByUserIdAsync(request.UserId2);
            var user2Liked = await _movieService.GetLikedMoviesByUserIdAsync(request.UserId2);

            _logger.LogInformation("User {UserId1} has {WatchedCount} watched and {LikedCount} liked movies", 
                request.UserId1, user1Watched.Count, user1Liked.Count);
            _logger.LogInformation("User {UserId2} has {WatchedCount} watched and {LikedCount} liked movies", 
                request.UserId2, user2Watched.Count, user2Liked.Count);

            // Get both users' want-to-watch movies
            var user1WantToWatch = await GetWantToWatchMoviesAsync(request.UserId1);
            var user2WantToWatch = await GetWantToWatchMoviesAsync(request.UserId2);
            
            _logger.LogInformation("User {UserId1} has {WantToWatchCount} want-to-watch movies", request.UserId1, user1WantToWatch.Count);
            _logger.LogInformation("User {UserId2} has {WantToWatchCount} want-to-watch movies", request.UserId2, user2WantToWatch.Count);

            // Create combined exclusion list (movies either user has interacted with)
            var excludeMovieIds = user1Watched.Select(m => m.Id)
                .Concat(user1Liked.Select(m => m.Id))
                .Concat(user2Watched.Select(m => m.Id))
                .Concat(user2Liked.Select(m => m.Id))
                .Concat(user1WantToWatch)
                .Concat(user2WantToWatch)
                .Distinct()
                .ToList();

            _logger.LogInformation("Excluding {Count} movies that either user has interacted with", excludeMovieIds.Count);

            // Generate individual taste vectors
            _logger.LogDebug("Generating taste vectors for both users");
            var user1TasteVector = await AnalyzeUserPreferencesAsync(request.UserId1);
            var user2TasteVector = await AnalyzeUserPreferencesAsync(request.UserId2);

            // Handle cases where both users have no interaction data
            if (user1TasteVector == null && user2TasteVector == null)
            {
                _logger.LogInformation("Both users have no interaction data - User1: {User1Vector}, User2: {User2Vector}. " +
                    "Returning popular movies recommendations as fallback.", 
                    user1TasteVector != null ? "Generated" : "NULL", user2TasteVector != null ? "Generated" : "NULL");
                
                // Return popular movies recommendations when both users have no data
                var popularMoviesVector = await GetPopularMoviesVectorAsync();
                if (popularMoviesVector == null)
                {
                    _logger.LogWarning("Could not generate popular movies vector, returning empty recommendations");
                    return new List<FriendRecommendationDto>();
                }

                // Get popular movies recommendations
                var popularRecommendations = await _vectorDatabaseService.FindSimilarMoviesAsync(
                    popularMoviesVector,
                    request.Limit * 2, // Get more to account for filtering
                    excludeMovieIds
                );

                // Apply diversity filtering and take the requested limit
                var popularFilteredRecommendations = ApplyDiversityFiltering(popularRecommendations, request.Limit);

                // Convert to friend recommendations with appropriate explanations
                var popularFriendRecommendations = await GenerateFriendRecommendationExplanationsAsync(
                    popularFilteredRecommendations.Take(request.Limit).ToList(),
                    popularMoviesVector,
                    popularMoviesVector
                );

                _logger.LogInformation("Successfully generated {Count} popular movies recommendations for users with no interaction data", 
                    popularFriendRecommendations.Count);

                return popularFriendRecommendations;
            }

            // If one user has no data, use only the other user's taste vector
            if (user1TasteVector == null && user2TasteVector != null)
            {
                _logger.LogInformation("User1 has no interaction data, using only User2's taste vector for recommendations");
                // Use only user2's taste vector
                user1TasteVector = user2TasteVector;
            }
            else if (user2TasteVector == null && user1TasteVector != null)
            {
                _logger.LogInformation("User2 has no interaction data, using only User1's taste vector for recommendations");
                // Use only user1's taste vector
                user2TasteVector = user1TasteVector;
            }

            _logger.LogInformation("Successfully generated taste vectors - User1: {User1Dimensions} dims, User2: {User2Dimensions} dims", 
                user1TasteVector?.Count ?? 0, user2TasteVector?.Count ?? 0);

            // Combine taste vectors (weighted average)
            if (user1TasteVector == null || user2TasteVector == null)
            {
                _logger.LogError("One or both taste vectors are null after processing - User1: {User1Vector}, User2: {User2Vector}", 
                    user1TasteVector != null ? "Generated" : "NULL", user2TasteVector != null ? "Generated" : "NULL");
                return new List<FriendRecommendationDto>();
            }
            
            var combinedTasteVector = CombineUserTasteVectors(user1TasteVector, user2TasteVector);
            _logger.LogInformation("Combined taste vector dimensions: {CombinedDimensions}", combinedTasteVector.Count);

            // Query vector database with combined vector
            _logger.LogDebug("Querying vector database for similar movies with limit {Limit}", request.Limit * 2);
            var recommendations = await _vectorDatabaseService.FindSimilarMoviesAsync(
                combinedTasteVector,
                request.Limit * 2, // Get more to account for filtering
                excludeMovieIds
            );
            
            _logger.LogInformation("Vector database returned {Count} recommendations before filtering", recommendations.Count);

            // Apply additional filters
            _logger.LogDebug("Applying additional filters to recommendations");
            var filteredRecommendations = await ApplyFriendRecommendationFiltersAsync(recommendations, request);
            _logger.LogInformation("After filtering: {FilteredCount} recommendations remain", filteredRecommendations.Count);

            // Generate recommendation explanations
            _logger.LogDebug("Generating recommendation explanations");
            var friendRecommendations = await GenerateFriendRecommendationExplanationsAsync(
                filteredRecommendations.Take(request.Limit).ToList(),
                user1TasteVector,
                user2TasteVector
            );

            _logger.LogInformation("Successfully generated {Count} friend recommendations for users {UserId1} and {UserId2}", 
                friendRecommendations.Count, request.UserId1, request.UserId2);

            return friendRecommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting friend recommendations for users {UserId1} and {UserId2}", 
                request.UserId1, request.UserId2);
            return new List<FriendRecommendationDto>();
        }
    }

    private async Task<List<Guid>> GetWantToWatchMoviesAsync(string userId)
    {
        try
        {
            var wantToWatchPaginationDto = new WantToWatchFilmPaginationUserDto
            {
                UserId = userId,
                Page = 1,
                PageSize = 1000 // Get all want-to-watch movies
            };
            var wantToWatchResult = await _wantToWatchFilmService.GetWantToWatchFilmsByUserIdAsync(wantToWatchPaginationDto);
            return wantToWatchResult?.Data?.Select(m => m.MovieId).ToList() ?? new List<Guid>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting want-to-watch movies for user {UserId}", userId);
            return new List<Guid>();
        }
    }

    private List<float> CombineUserTasteVectors(List<float> user1Vector, List<float> user2Vector)
    {
        if (user1Vector.Count != user2Vector.Count)
        {
            _logger.LogWarning("User taste vectors have different dimensions, using user1 vector");
            return user1Vector;
        }

        // Weighted average (50/50 split)
        return user1Vector.Zip(user2Vector, (a, b) => (a + b) / 2.0f).ToList();
    }

    private async Task<List<MovieRecommendationDto>> ApplyFriendRecommendationFiltersAsync(
        List<MovieRecommendationDto> recommendations, 
        FriendRecommendationRequestDto request)
    {
        var filteredRecommendations = new List<MovieRecommendationDto>();

        foreach (var movie in recommendations)
        {
            // Get movie details for filtering
            var movieDetails = await _movieService.GetMovieByIdAsync(movie.MovieId);
            if (movieDetails == null) continue;

            // Genre filtering
            if (request.PreferredGenres?.Any() == true)
            {
                var movieGenres = movieDetails.MovieGenres?.Select(mg => mg.Genre.Name).ToList() ?? new List<string>();
                if (!movieGenres.Any(g => request.PreferredGenres.Contains(g)))
                    continue;
            }

            // Rating filtering
            if (request.MinRating.HasValue)
            {
                if (movieDetails.VoteAverage < request.MinRating.Value)
                    continue;
            }

            // Vote count filtering (using RatingAverage as proxy for vote count)
            if (request.MinVoteCount.HasValue)
            {
                // Note: Movie model doesn't have VoteCount, using RatingAverage as alternative
                if (movieDetails.RatingAverage < request.MinVoteCount.Value)
                    continue;
            }

            // Release date filtering
            if (request.MinReleaseDate.HasValue)
            {
                if (movieDetails.ReleaseDate < request.MinReleaseDate.Value)
                    continue;
            }

            if (request.MaxReleaseDate.HasValue)
            {
                if (movieDetails.ReleaseDate > request.MaxReleaseDate.Value)
                    continue;
            }

            filteredRecommendations.Add(movie);
        }

        return filteredRecommendations;
    }

    private async Task<List<FriendRecommendationDto>> GenerateFriendRecommendationExplanationsAsync(
        List<MovieRecommendationDto> recommendations,
        List<float> user1Vector,
        List<float> user2Vector)
    {
        var friendRecommendations = new List<FriendRecommendationDto>();

        foreach (var rec in recommendations)
        {
            try
            {
                // Get movie details for explanation
                var movieDetails = await _movieService.GetMovieByIdAsync(rec.MovieId);
                if (movieDetails == null) continue;

                var friendRec = new FriendRecommendationDto
                {
                    MovieId = rec.MovieId,
                    Title = rec.Title,
                    PosterPath = rec.PosterPath,
                    SimilarityScore = rec.SimilarityScore,
                    VoteAverage = movieDetails.VoteAverage,
                    VoteCount = (int?)movieDetails.RatingAverage, // Using RatingAverage as proxy
                    Overview = movieDetails.Overview,
                    ReleaseDate = movieDetails.ReleaseDate,
                    RecommendationReason = GenerateFriendRecommendationReason(rec, user1Vector, user2Vector),
                    MatchingGenres = movieDetails.MovieGenres?.Select(mg => mg.Genre.Name).ToList() ?? new List<string>(),
                    MatchingKeywords = movieDetails.Keywords?.Select(mk => mk.Keyword.Name).ToList() ?? new List<string>()
                };

                friendRecommendations.Add(friendRec);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating explanation for movie {MovieId}", rec.MovieId);
            }
        }

        return friendRecommendations;
    }

    private string GenerateFriendRecommendationReason(MovieRecommendationDto recommendation, List<float> user1Vector, List<float> user2Vector)
    {
        var reasons = new List<string>();

        // Check if both vectors are the same (popular movies fallback)
        var isPopularMoviesFallback = user1Vector.Count == user2Vector.Count && 
                                     user1Vector.Zip(user2Vector, (a, b) => Math.Abs(a - b)).All(diff => diff < 0.001f);

        if (isPopularMoviesFallback)
        {
            // Both users have no data, using popular movies
            reasons.Add("Popular movie recommended for both users");
            if (recommendation.SimilarityScore > 0.7)
                reasons.Add("High-rated movie with wide appeal");
            else
                reasons.Add("Well-received movie that many people enjoy");
        }
        else
        {
            // Normal case with user taste vectors
            if (recommendation.SimilarityScore > 0.8)
                reasons.Add("Both users have very similar taste preferences");
            else if (recommendation.SimilarityScore > 0.6)
                reasons.Add("Both users have similar taste preferences");
            else
                reasons.Add("Movie matches both users' preferences");

            // Add genre-based reasoning
            if (recommendation.SimilarityScore > 0.7)
                reasons.Add("High compatibility with both users' movie preferences");
        }

        return string.Join(". ", reasons) + ".";
    }

    /// <summary>
    /// Adds diversity to embedding by slightly perturbing the vector
    /// </summary>
    private List<float> AddDiversityToEmbedding(List<float> embedding, Guid movieId)
    {
        var diversified = new List<float>(embedding);
        var random = new Random(movieId.GetHashCode()); // Deterministic but different per movie
        
        // Add small random perturbations to break ties
        for (int i = 0; i < diversified.Count; i++)
        {
            var perturbation = (float)(random.NextDouble() - 0.5) * 0.01f; // Small perturbation
            diversified[i] += perturbation;
        }
        
        // Normalize to maintain unit vector property
        var norm = Math.Sqrt(diversified.Sum(x => x * x));
        if (norm > 0)
        {
            for (int i = 0; i < diversified.Count; i++)
            {
                diversified[i] = (float)(diversified[i] / norm);
            }
        }
        
        return diversified;
    }

    /// <summary>
    /// Applies enhanced diversity filtering to reduce similar recommendations
    /// Works better with higher dimensional embeddings (768+ dimensions)
    /// </summary>
    private List<MovieRecommendationDto> ApplyDiversityFiltering(List<MovieRecommendationDto> recommendations, int targetLimit)
    {
        if (recommendations.Count <= targetLimit)
        {
            return recommendations;
        }

        var diversified = new List<MovieRecommendationDto>();
        var usedTitles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var usedGenres = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var usedDecades = new HashSet<string>();
        
        // First pass: Add high-scoring unique recommendations with genre and decade diversity
        foreach (var rec in recommendations.OrderByDescending(r => r.SimilarityScore))
        {
            if (diversified.Count >= targetLimit)
                break;
                
            // Enhanced similarity checking
            var titleWords = rec.Title.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var isTitleTooSimilar = usedTitles.Any(usedTitle => 
            {
                var usedWords = usedTitle.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var commonWords = titleWords.Intersect(usedWords).Count();
                return commonWords >= Math.Max(2, Math.Min(titleWords.Length, usedWords.Length) / 2);
            });
            
            // Extract genre and decade from title for diversity
            var genre = ExtractGenreFromTitle(rec.Title);
            var decade = ExtractDecadeFromTitle(rec.Title);
            
            // Check if we already have too many movies from the same genre/decade
            var genreCount = usedGenres.Count(g => g == genre);
            var decadeCount = usedDecades.Count(d => d == decade);
            
            // Allow some repetition but not too much
            var isDiverseEnough = !isTitleTooSimilar && 
                                 genreCount < Math.Max(2, targetLimit / 4) && 
                                 decadeCount < Math.Max(2, targetLimit / 3);
            
            if (isDiverseEnough)
            {
                diversified.Add(rec);
                usedTitles.Add(rec.Title);
                if (!string.IsNullOrEmpty(genre)) usedGenres.Add(genre);
                if (!string.IsNullOrEmpty(decade)) usedDecades.Add(decade);
            }
        }
        
        // Second pass: Fill remaining slots with diverse recommendations
        if (diversified.Count < targetLimit)
        {
            var remaining = recommendations.Except(diversified).ToList();
            var random = new Random(42); // Fixed seed for consistency
            
            while (diversified.Count < targetLimit && remaining.Any())
            {
                // Prefer recommendations that add diversity
                var bestCandidate = remaining
                    .OrderByDescending(r => CalculateDiversityScore(r, usedTitles, usedGenres, usedDecades))
                    .FirstOrDefault();
                
                if (bestCandidate != null)
                {
                    diversified.Add(bestCandidate);
                    remaining.Remove(bestCandidate);
                    
                    // Update tracking sets
                    var titleWords = bestCandidate.Title.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    usedTitles.Add(bestCandidate.Title);
                    
                    var genre = ExtractGenreFromTitle(bestCandidate.Title);
                    var decade = ExtractDecadeFromTitle(bestCandidate.Title);
                    if (!string.IsNullOrEmpty(genre)) usedGenres.Add(genre);
                    if (!string.IsNullOrEmpty(decade)) usedDecades.Add(decade);
                }
                else
                {
                    // Fallback to random selection
                    var randomIndex = random.Next(remaining.Count);
                    var selected = remaining[randomIndex];
                    diversified.Add(selected);
                    remaining.RemoveAt(randomIndex);
                }
            }
        }
        
        return diversified.OrderByDescending(r => r.SimilarityScore).ToList();
    }

    /// <summary>
    /// Extracts genre information from movie title for diversity filtering
    /// </summary>
    private string ExtractGenreFromTitle(string title)
    {
        var titleLower = title.ToLowerInvariant();
        
        // Common genre indicators in titles
        if (titleLower.Contains("horror") || titleLower.Contains("scary") || titleLower.Contains("nightmare"))
            return "horror";
        if (titleLower.Contains("comedy") || titleLower.Contains("funny") || titleLower.Contains("laugh"))
            return "comedy";
        if (titleLower.Contains("action") || titleLower.Contains("fight") || titleLower.Contains("war"))
            return "action";
        if (titleLower.Contains("romance") || titleLower.Contains("love") || titleLower.Contains("heart"))
            return "romance";
        if (titleLower.Contains("drama") || titleLower.Contains("story") || titleLower.Contains("life"))
            return "drama";
        if (titleLower.Contains("sci-fi") || titleLower.Contains("space") || titleLower.Contains("future"))
            return "sci-fi";
        if (titleLower.Contains("thriller") || titleLower.Contains("suspense") || titleLower.Contains("mystery"))
            return "thriller";
        
        return "unknown";
    }

    /// <summary>
    /// Extracts decade information from movie title for diversity filtering
    /// </summary>
    private string ExtractDecadeFromTitle(string title)
    {
        // Look for year patterns in title
        var yearMatch = System.Text.RegularExpressions.Regex.Match(title, @"\b(19|20)\d{2}\b");
        if (yearMatch.Success)
        {
            var year = int.Parse(yearMatch.Value);
            return $"{year / 10 * 10}s";
        }
        
        return "unknown";
    }

    /// <summary>
    /// Calculates diversity score for a recommendation
    /// </summary>
    private double CalculateDiversityScore(MovieRecommendationDto rec, HashSet<string> usedTitles, HashSet<string> usedGenres, HashSet<string> usedDecades)
    {
        var genre = ExtractGenreFromTitle(rec.Title);
        var decade = ExtractDecadeFromTitle(rec.Title);
        
        var genreScore = usedGenres.Contains(genre) ? 0.5 : 1.0;
        var decadeScore = usedDecades.Contains(decade) ? 0.5 : 1.0;
        var titleScore = usedTitles.Contains(rec.Title) ? 0.0 : 1.0;
        
        return (genreScore + decadeScore + titleScore) / 3.0;
    }

    /// <summary>
    /// Gets fallback recommendations for new users or users with no interaction data
    /// Returns popular movies with good ratings
    /// </summary>
    private async Task<List<MovieRecommendationDto>> GetFallbackRecommendationsAsync(string userId, int limit)
    {
        try
        {
            _logger.LogDebug("Getting fallback recommendations for user {UserId}", userId);

            // Get user's already watched/liked movies to exclude
            var watchedMovies = await _movieService.GetWatchedMoviesByUserIdAsync(userId);
            var likedMovies = await _movieService.GetLikedMoviesByUserIdAsync(userId);
            
            // Get user's want-to-watch movies
            var wantToWatchPaginationDto = new WantToWatchFilmPaginationUserDto
            {
                UserId = userId,
                Page = 1,
                PageSize = 1000
            };
            var wantToWatchResult = await _wantToWatchFilmService.GetWantToWatchFilmsByUserIdAsync(wantToWatchPaginationDto);
            var wantToWatchMovieIds = wantToWatchResult?.Data?.Select(m => m.MovieId).ToList() ?? new List<Guid>();

            var excludeMovieIds = watchedMovies.Select(m => m.Id)
                .Concat(likedMovies.Select(m => m.Id))
                .Concat(wantToWatchMovieIds)
                .Distinct()
                .ToList();

            // Query vector database for popular movies using a generic "popular movies" vector
            var popularMoviesVector = await GetPopularMoviesVectorAsync();
            if (popularMoviesVector == null || !popularMoviesVector.Any())
            {
                _logger.LogWarning("Could not generate popular movies vector, returning empty recommendations");
                return new List<MovieRecommendationDto>();
            }

            // Get more recommendations than needed to account for exclusions
            var recommendations = await _vectorDatabaseService.FindSimilarMoviesAsync(
                popularMoviesVector,
                limit * 3, // Get 3x more to account for exclusions
                excludeMovieIds
            );

            // Apply diversity filtering and take the requested limit
            var filteredRecommendations = ApplyDiversityFiltering(recommendations, limit);

            _logger.LogDebug("Generated {Count} fallback recommendations for user {UserId}", 
                filteredRecommendations.Count, userId);
            
            return filteredRecommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting fallback recommendations for user {UserId}", userId);
            return new List<MovieRecommendationDto>();
        }
    }

    /// <summary>
    /// Generates a vector representing popular movies with good ratings
    /// This is used as a fallback when users have no interaction data
    /// </summary>
    private async Task<List<float>?> GetPopularMoviesVectorAsync()
    {
        try
        {
            // Create a vector that represents popular, well-rated movies
            // This is a weighted combination of common movie characteristics
            var popularMovieData = new MovieDataForEmbeddingDto
            {
                Id = Guid.Empty, // Special ID for popular movies vector
                Title = "Popular Movies",
                Overview = "Popular movies with high ratings and wide appeal",
                Genres = new List<string> { "Drama", "Action", "Comedy", "Thriller", "Romance" },
                Keywords = new List<string> { "popular", "blockbuster", "award-winning", "critically-acclaimed" },
                Cast = new List<string>(), // Empty for generic vector
                Crew = new List<string>(), // Empty for generic vector
                ProductionCompanies = new List<string> { "major studio", "independent" },
                ProductionCountries = new List<string> { "United States", "United Kingdom", "Canada" },
                SpokenLanguages = new List<string> { "English" },
                ReleaseDate = DateTime.Now.AddYears(-5), // Recent movies
                PosterPath = null
            };

            // Use the same text preparation method as stored movie embeddings
            var textToEmbed = PrepareTextForEmbedding(popularMovieData);
            var embedding = await _textEmbeddingService.GenerateEmbeddingAsync(textToEmbed);
            return embedding?.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating popular movies vector");
            return null;
        }
    }

    /// <summary>
    /// Prepares movie data into a text string suitable for embedding (same as TextEmbeddingService)
    /// This ensures consistency between user taste vectors and stored movie embeddings
    /// </summary>
    private string PrepareTextForEmbedding(MovieDataForEmbeddingDto movieData)
    {
        var textParts = new List<string>();

        // Basic movie information with unique identifiers
        textParts.Add($"MOVIE_TITLE: {movieData.Title}");
        
        if (!string.IsNullOrEmpty(movieData.OriginalTitle) && movieData.OriginalTitle != movieData.Title)
        {
            textParts.Add($"ORIGINAL_TITLE: {movieData.OriginalTitle}");
        }

        // Add unique movie ID for better distinction
        textParts.Add($"MOVIE_ID: {movieData.Id}");

        if (!string.IsNullOrEmpty(movieData.Overview))
        {
            textParts.Add($"PLOT: {movieData.Overview}");
        }

        if (!string.IsNullOrEmpty(movieData.Tagline))
        {
            textParts.Add($"TAGLINE: {movieData.Tagline}");
        }

        // Genres with weighted importance
        if (movieData.Genres.Any())
        {
            var genreText = string.Join(" ", movieData.Genres.Select(g => $"GENRE_{g}"));
            textParts.Add(genreText);
        }

        // Keywords with weighted importance
        if (movieData.Keywords.Any())
        {
            var keywordText = string.Join(" ", movieData.Keywords.Select(k => $"KEYWORD_{k}"));
            textParts.Add(keywordText);
        }

        // Cast with character names and actor names
        if (movieData.Cast.Any())
        {
            var topCast = movieData.Cast.Take(15); // Increased from 10
            var castText = string.Join(" ", topCast.Select(c => $"ACTOR_{c}"));
            textParts.Add(castText);
        }

        // Crew with specific roles
        if (movieData.Crew.Any())
        {
            var topCrew = movieData.Crew.Take(15); // Increased from 10
            var crewText = string.Join(" ", topCrew.Select(c => $"CREW_{c}"));
            textParts.Add(crewText);
        }

        // Production companies with weighted importance
        if (movieData.ProductionCompanies.Any())
        {
            var companyText = string.Join(" ", movieData.ProductionCompanies.Select(pc => $"STUDIO_{pc}"));
            textParts.Add(companyText);
        }

        // Production countries
        if (movieData.ProductionCountries.Any())
        {
            var countryText = string.Join(" ", movieData.ProductionCountries.Select(pc => $"COUNTRY_{pc}"));
            textParts.Add(countryText);
        }

        // Spoken languages
        if (movieData.SpokenLanguages.Any())
        {
            var languageText = string.Join(" ", movieData.SpokenLanguages.Select(sl => $"LANGUAGE_{sl}"));
            textParts.Add(languageText);
        }

        // Additional metadata with specific formatting
        if (movieData.ReleaseDate.HasValue)
        {
            textParts.Add($"YEAR_{movieData.ReleaseDate.Value.Year}");
            textParts.Add($"DECADE_{movieData.ReleaseDate.Value.Year / 10 * 10}s");
        }

        if (movieData.Runtime.HasValue)
        {
            var runtimeCategory = movieData.Runtime.Value switch
            {
                < 90 => "SHORT_FILM",
                < 120 => "STANDARD_LENGTH",
                < 180 => "LONG_FILM",
                _ => "EPIC_LENGTH"
            };
            textParts.Add($"RUNTIME_{runtimeCategory}_{movieData.Runtime.Value}min");
        }

        if (movieData.Budget.HasValue && movieData.Budget.Value > 0)
        {
            var budgetCategory = movieData.Budget.Value switch
            {
                < 1000000 => "LOW_BUDGET",
                < 10000000 => "MEDIUM_BUDGET",
                < 100000000 => "HIGH_BUDGET",
                _ => "BLOCKBUSTER_BUDGET"
            };
            textParts.Add($"BUDGET_{budgetCategory}");
        }

        // Add movie uniqueness markers
        textParts.Add($"UNIQUE_MOVIE_{movieData.Id.ToString().Substring(0, 8)}");

        return string.Join(" ", textParts);
    }


    #endregion
}
