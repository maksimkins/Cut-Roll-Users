namespace Cut_Roll_Users.Api.Common.Extensions.WebApplication;

using Cut_Roll_Users.Core.Common.BackgroundServices;
using Microsoft.AspNetCore.Builder;

public static class InitEmbeddingBackgroundMethod
{
    public static async Task InitEmbeddingBackground(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("EmbeddingInit");

        try
        {
            logger.LogInformation("Starting embedding system initialization...");
            
            var embeddingInitService = services.GetRequiredService<IEmbeddingInitializationService>();
            
            // Check if system is already initialized
            var isVectorDbEmpty = await embeddingInitService.IsVectorDbEmptyAsync();
            if (!isVectorDbEmpty)
            {
                logger.LogInformation("Embedding system already initialized, skipping...");
                return;
            }

            // Initialize the embedding system
            await embeddingInitService.InitializeEmbeddingsAsync();
            
            logger.LogInformation("Embedding system initialization completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize embedding system");
            // Don't throw - let the app start even if embedding init fails
        }
    }
}
