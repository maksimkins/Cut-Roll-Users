using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Cut_Roll_Users.Core.MovieEmbeddings.Services;

namespace Cut_Roll_Users.Infrastructure;

/// <summary>
/// Helper class to reset all embeddings
/// Call this method to delete all vectors from Pinecone and reset database flags
/// </summary>
public static class ResetEmbeddingsHelper
{
    /// <summary>
    /// Resets all embeddings by:
    /// 1. Deleting all vectors from Pinecone
    /// 2. Setting HasEmbedding = false for all movies in database
    /// 3. Background service will regenerate all embeddings on next run
    /// </summary>
    public static async Task<bool> ResetAllEmbeddingsAsync(IServiceProvider serviceProvider)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var movieEmbeddingService = scope.ServiceProvider.GetRequiredService<IMovieEmbeddingService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<object>>();

            logger.LogWarning("STARTING EMBEDDING RESET PROCESS...");
            logger.LogWarning("This will delete ALL vectors from Pinecone and reset database flags!");
            logger.LogWarning("Background service will regenerate all embeddings on next run.");

            var success = await movieEmbeddingService.ResetAllEmbeddingsAsync();

            if (success)
            {
                logger.LogInformation("✅ Embedding reset completed successfully!");
                logger.LogInformation("🔄 Background service will regenerate all embeddings with consistent method.");
                logger.LogInformation("📊 You should see much better similarity scores after regeneration.");
            }
            else
            {
                logger.LogError("❌ Embedding reset failed!");
            }

            return success;
        }
        catch (Exception ex)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<object>>();
            logger.LogError(ex, "Error during embedding reset");
            return false;
        }
    }
}
