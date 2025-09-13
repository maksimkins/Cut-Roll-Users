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
                throw new ArgumentException("Limit must be greater than 0", nameof(limit));
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

            // Find similar movies using vector database
            _logger.LogInformation("Step 4: Querying Pinecone for similar movies...");
            var similarMovies = await _vectorDatabaseService.FindSimilarMoviesAsync(
                movieEmbedding, 
                limit + 1, // +1 to exclude the original movie
                new List<Guid> { movieId } // Exclude the original movie
            );

            _logger.LogInformation("Step 4: Found {Count} similar movies for movie {MovieId}", similarMovies.Count, movieId);
            return similarMovies;
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
                _logger.LogWarning("Could not generate taste vector for user {UserId}", userId);
                return new List<MovieRecommendationDto>();
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

            // Combine and rank recommendations
            var hybridRecommendations = CombineRecommendations(contentBased, collaborative, limit);

            _logger.LogDebug("Generated {Count} hybrid recommendations for user {UserId}", hybridRecommendations.Count, userId);
            return hybridRecommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting hybrid recommendations for user {UserId}", userId);
            return new List<MovieRecommendationDto>();
        }
    }

    public async Task<List<float>?> AnalyzeUserPreferencesAsync(string userId)
    {
        try
        {
            _logger.LogDebug("Analyzing user preferences for user {UserId}", userId);

            // Get user's liked movies with ratings
            var likedMovies = await _movieService.GetLikedMoviesByUserIdAsync(userId);
            var userReviews = await _reviewService.GetReviewsByUserIdAsync(new ReviewPaginationUserDto 
            { 
                UserId = userId, 
                Page = 1, 
                PageSize = 100 
            });

            if (!likedMovies.Any() && !userReviews.Data.Any())
            {
                _logger.LogWarning("No movie data found for user {UserId}", userId);
                return null;
            }

            // Create weighted movie data based on ratings
            var weightedMovieData = new List<(MovieDataForEmbeddingDto data, float weight)>();
            
            foreach (var movie in likedMovies)
            {
                var movieData = ConvertMovieToEmbeddingData(movie);
                if (movieData != null)
                {
                    // Weight by user's rating if available, otherwise use average rating
                    var userReview = userReviews.Data.FirstOrDefault(r => r.MovieSimplified.MovieId == movie.Id);
                    var weight = userReview?.Rating ?? (float)(movie.VoteAverage ?? 0);
                    weightedMovieData.Add((movieData, weight));
                }
            }

            if (!weightedMovieData.Any())
            {
                _logger.LogWarning("No valid movie data found for user {UserId}", userId);
                return null;
            }

            // Generate embeddings for each movie
            var embeddings = new List<List<float>>();
            var weights = new List<float>();

            foreach (var (data, weight) in weightedMovieData)
            {
                var embedding = await _textEmbeddingService.GenerateMovieEmbeddingAsync(data);
                if (embedding != null && embedding.Any())
                {
                    embeddings.Add(embedding);
                    weights.Add(weight);
                }
            }

            if (!embeddings.Any())
            {
                _logger.LogWarning("No valid embeddings generated for user {UserId}", userId);
                return null;
            }

            // Calculate weighted average embedding
            var tasteVector = CalculateWeightedAverageEmbedding(embeddings, weights);

            _logger.LogDebug("Generated taste vector for user {UserId} with dimension {Dimension}", userId, tasteVector.Count);
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

            // Step 1: Get user's liked movies and their ratings
            var userMovies = await GetUserLikedMoviesAsync(userId);
            if (!userMovies.Any())
            {
                _logger.LogDebug("No liked movies found for user {UserId}, skipping collaborative filtering", userId);
                return new List<MovieRecommendationDto>();
            }

            // Step 2: Find similar users based on movie preferences
            var similarUsers = await FindSimilarUsersAsync(userId, userMovies);
            if (!similarUsers.Any())
            {
                _logger.LogDebug("No similar users found for user {UserId}", userId);
                return new List<MovieRecommendationDto>();
            }

            // Step 3: Get movies liked by similar users that the current user hasn't seen
            var recommendations = await GetMoviesFromSimilarUsersAsync(userId, similarUsers, limit);
            
            _logger.LogDebug("Generated {Count} collaborative recommendations for user {UserId}", 
                recommendations.Count, userId);
            
            return recommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating collaborative recommendations for user {UserId}", userId);
            return new List<MovieRecommendationDto>();
        }
    }

    private async Task<List<Movie>> GetUserLikedMoviesAsync(string userId)
    {
        try
        {
            var likedMovies = await _movieService.GetLikedMoviesByUserIdAsync(userId);
            return likedMovies.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting liked movies for user {UserId}", userId);
            return new List<Movie>();
        }
    }

    private async Task<List<string>> FindSimilarUsersAsync(string userId, List<Movie> userMovies)
    {
        try
        {
            // This is a simplified implementation
            // In a real system, you'd use more sophisticated algorithms like:
            // - Cosine similarity on user-item matrices
            // - Matrix factorization
            // - Deep learning approaches
            
            var similarUsers = new List<string>();
            var userMovieIds = userMovies.Select(m => m.Id).ToHashSet();

            // Get all users who have liked at least one of the same movies
            // This is a basic approach - in production you'd use more sophisticated methods
            var allUsers = await GetAllUsersWithLikesAsync();
            
            foreach (var otherUserId in allUsers)
            {
                if (otherUserId == userId) continue;

                var otherUserMovies = await _movieService.GetLikedMoviesByUserIdAsync(otherUserId);
                var otherUserMovieIds = otherUserMovies.Select(m => m.Id).ToHashSet();

                // Calculate Jaccard similarity (intersection over union)
                var intersection = userMovieIds.Intersect(otherUserMovieIds).Count();
                var union = userMovieIds.Union(otherUserMovieIds).Count();
                
                if (union > 0)
                {
                    var similarity = (double)intersection / union;
                    
                    // Consider users similar if they share at least 20% of movies
                    if (similarity >= 0.2)
                    {
                        similarUsers.Add(otherUserId);
                    }
                }
            }

            _logger.LogDebug("Found {Count} similar users for user {UserId}", similarUsers.Count, userId);
            return similarUsers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding similar users for user {UserId}", userId);
            return new List<string>();
        }
    }

    private Task<List<string>> GetAllUsersWithLikesAsync()
    {
        try
        {
            // This is a placeholder - in a real system you'd have a proper user service
            // For now, we'll return an empty list to avoid breaking the system
            _logger.LogDebug("Getting all users with likes - placeholder implementation");
            return Task.FromResult(new List<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users with likes");
            return Task.FromResult(new List<string>());
        }
    }

    private async Task<List<MovieRecommendationDto>> GetMoviesFromSimilarUsersAsync(
        string userId, 
        List<string> similarUsers, 
        int limit)
    {
        try
        {
            var recommendations = new List<MovieRecommendationDto>();
            var userLikedMovieIds = (await _movieService.GetLikedMoviesByUserIdAsync(userId))
                .Select(m => m.Id).ToHashSet();

            foreach (var similarUserId in similarUsers.Take(5)) // Limit to top 5 similar users
            {
                var similarUserMovies = await _movieService.GetLikedMoviesByUserIdAsync(similarUserId);
                
                foreach (var movie in similarUserMovies)
                {
                    // Skip movies the user has already liked
                    if (userLikedMovieIds.Contains(movie.Id))
                        continue;

                    // Create recommendation
                    var recommendation = new MovieRecommendationDto
                    {
                        MovieId = movie.Id,
                        Title = movie.Title,
                        PosterPath = movie.Images?.FirstOrDefault(i => i.Type == "poster")?.FilePath,
                        SimilarityScore = 0.8 // Placeholder score
                    };

                    recommendations.Add(recommendation);
                }
            }

            // Remove duplicates and sort by score
            return recommendations
                .GroupBy(r => r.MovieId)
                .Select(g => g.First())
                .OrderByDescending(r => r.SimilarityScore)
                .Take(limit)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting movies from similar users for user {UserId}", userId);
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

    private List<float> CalculateWeightedAverageEmbedding(List<List<float>> embeddings, List<float> weights)
    {
        if (!embeddings.Any()) return new List<float>();

        var dimension = embeddings[0].Count;
        var weightedSum = new float[dimension];
        var totalWeight = weights.Sum();

        for (int i = 0; i < embeddings.Count; i++)
        {
            var embedding = embeddings[i];
            var weight = weights[i];

            for (int j = 0; j < dimension; j++)
            {
                weightedSum[j] += embedding[j] * weight;
            }
        }

        // Normalize by total weight
        for (int j = 0; j < dimension; j++)
        {
            weightedSum[j] /= totalWeight;
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
            _logger.LogDebug("Getting friend recommendations for users {UserId1} and {UserId2} with limit {Limit}", 
                request.UserId1, request.UserId2, request.Limit);

            // Validate request
            if (string.IsNullOrEmpty(request.UserId1) || string.IsNullOrEmpty(request.UserId2))
            {
                throw new ArgumentException("Both user IDs must be provided");
            }

            if (request.UserId1 == request.UserId2)
            {
                throw new ArgumentException("User IDs must be different");
            }

            // Check if users are mutual friends
            var areMutualFriends = await _followService.AreMutualFriendsAsync(request.UserId1, request.UserId2);
            if (!areMutualFriends)
            {
                throw new InvalidOperationException("Users must be mutual friends to get friend recommendations");
            }

            // Get both users' watched/liked movies
            var user1Watched = await _movieService.GetWatchedMoviesByUserIdAsync(request.UserId1);
            var user1Liked = await _movieService.GetLikedMoviesByUserIdAsync(request.UserId1);
            var user2Watched = await _movieService.GetWatchedMoviesByUserIdAsync(request.UserId2);
            var user2Liked = await _movieService.GetLikedMoviesByUserIdAsync(request.UserId2);

            // Get both users' want-to-watch movies
            var user1WantToWatch = await GetWantToWatchMoviesAsync(request.UserId1);
            var user2WantToWatch = await GetWantToWatchMoviesAsync(request.UserId2);

            // Create combined exclusion list (movies either user has interacted with)
            var excludeMovieIds = user1Watched.Select(m => m.Id)
                .Concat(user1Liked.Select(m => m.Id))
                .Concat(user2Watched.Select(m => m.Id))
                .Concat(user2Liked.Select(m => m.Id))
                .Concat(user1WantToWatch)
                .Concat(user2WantToWatch)
                .Distinct()
                .ToList();

            _logger.LogDebug("Excluding {Count} movies that either user has interacted with", excludeMovieIds.Count);

            // Generate individual taste vectors
            var user1TasteVector = await AnalyzeUserPreferencesAsync(request.UserId1);
            var user2TasteVector = await AnalyzeUserPreferencesAsync(request.UserId2);

            if (user1TasteVector == null || user2TasteVector == null)
            {
                _logger.LogWarning("Could not generate taste vectors for one or both users");
                return new List<FriendRecommendationDto>();
            }

            // Combine taste vectors (weighted average)
            var combinedTasteVector = CombineUserTasteVectors(user1TasteVector, user2TasteVector);

            // Query vector database with combined vector
            var recommendations = await _vectorDatabaseService.FindSimilarMoviesAsync(
                combinedTasteVector,
                request.Limit * 2, // Get more to account for filtering
                excludeMovieIds
            );

            // Apply additional filters
            var filteredRecommendations = await ApplyFriendRecommendationFiltersAsync(recommendations, request);

            // Generate recommendation explanations
            var friendRecommendations = await GenerateFriendRecommendationExplanationsAsync(
                filteredRecommendations.Take(request.Limit).ToList(),
                user1TasteVector,
                user2TasteVector
            );

            _logger.LogDebug("Generated {Count} friend recommendations for users {UserId1} and {UserId2}", 
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

        // Base similarity reason
        if (recommendation.SimilarityScore > 0.8)
            reasons.Add("Both users have very similar taste preferences");
        else if (recommendation.SimilarityScore > 0.6)
            reasons.Add("Both users have similar taste preferences");
        else
            reasons.Add("Movie matches both users' preferences");

        // Add genre-based reasoning
        if (recommendation.SimilarityScore > 0.7)
            reasons.Add("High compatibility with both users' movie preferences");

        return string.Join(". ", reasons) + ".";
    }

    #endregion
}
