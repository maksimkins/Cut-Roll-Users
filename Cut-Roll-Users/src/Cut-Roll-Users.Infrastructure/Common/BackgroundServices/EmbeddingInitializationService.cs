using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Cut_Roll_Users.Core.Common.BackgroundServices;
using Cut_Roll_Users.Core.MovieEmbeddings.Services;
using Cut_Roll_Users.Core.MovieEmbeddings.Dtos;
using Cut_Roll_Users.Core.Common.DataProcessing;

namespace Cut_Roll_Users.Infrastructure.Common.BackgroundServices;

/// <summary>
/// Service for initializing the embedding system on startup
/// </summary>
public class EmbeddingInitializationService : IEmbeddingInitializationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmbeddingInitializationService> _logger;
    private bool _isInitialized = false;
    private DateTime? _lastInitializationAt = null;

    public EmbeddingInitializationService(
        IServiceProvider serviceProvider,
        ILogger<EmbeddingInitializationService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InitializeEmbeddingsAsync()
    {
        if (_isInitialized)
        {
            _logger.LogInformation("Embedding system already initialized");
            return;
        }

        try
        {
            _logger.LogInformation("Starting embedding system initialization");

            using var scope = _serviceProvider.CreateScope();
            var vectorDatabaseService = scope.ServiceProvider.GetRequiredService<IVectorMovieDatabaseService>();
            var movieEmbeddingService = scope.ServiceProvider.GetRequiredService<IMovieEmbeddingService>();
            var textEmbeddingService = scope.ServiceProvider.GetRequiredService<ITextEmbeddingService>();

            // Step 1: Check if vector database is empty
            var isVectorDbEmpty = await vectorDatabaseService.IsVectorDbEmptyAsync();
            if (!isVectorDbEmpty)
            {
                _logger.LogInformation("Vector database is not empty, skipping initialization");
                _isInitialized = true;
                _lastInitializationAt = DateTime.UtcNow;
                return;
            }

            // Step 2: Initialize the text embedding model (if it implements ILocalEmbeddingService)
            _logger.LogInformation("Initializing text embedding model");
            // TextEmbeddingService initializes model in constructor

            // Step 3: Initialize vector database
            _logger.LogInformation("Initializing vector database");
            var vectorDbInitialized = await vectorDatabaseService.InitializeIndexAsync();
            if (!vectorDbInitialized)
            {
                _logger.LogError("Failed to initialize vector database");
                throw new InvalidOperationException("Vector database initialization failed");
            }

            // Step 4: Process all movies to generate embeddings
            _logger.LogInformation("Processing all movies to generate embeddings");
            await movieEmbeddingService.ProcessAllMoviesAsync();

            _isInitialized = true;
            _lastInitializationAt = DateTime.UtcNow;

            _logger.LogInformation("Embedding system initialization completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during embedding system initialization");
            throw;
        }
    }

    public async Task<bool> IsVectorDbEmptyAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var vectorDatabaseService = scope.ServiceProvider.GetRequiredService<IVectorMovieDatabaseService>();
            return await vectorDatabaseService.IsVectorDbEmptyAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if vector database is empty");
            return true; // Assume empty on error
        }
    }

    public async Task<bool> CheckSystemHealthAsync()
    {
        try
        {
            _logger.LogDebug("Checking embedding system health");

            using var scope = _serviceProvider.CreateScope();
            var vectorDatabaseService = scope.ServiceProvider.GetRequiredService<IVectorMovieDatabaseService>();
            var textEmbeddingService = scope.ServiceProvider.GetRequiredService<ITextEmbeddingService>();

            // Check vector database health
            var vectorDbHealthy = await vectorDatabaseService.CheckVectorDbHealthAsync();
            if (!vectorDbHealthy)
            {
                _logger.LogWarning("Vector database health check failed");
                return false;
            }

            // Check text embedding service (if it implements ILocalEmbeddingService)
            // TextEmbeddingService handles model loading internally

            _logger.LogDebug("Embedding system health check passed");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during system health check");
            return false;
        }
    }

    public async Task<EmbeddingStatusDto> GetInitializationStatusAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var movieEmbeddingService = scope.ServiceProvider.GetRequiredService<IMovieEmbeddingService>();

            var status = await movieEmbeddingService.GetEmbeddingStatusAsync();
            
            // Add initialization specific information
            status.Status = _isInitialized ? "Initialized" : "Not Initialized";
            status.LastProcessedAt = _lastInitializationAt;

            return status;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting initialization status");
            return new EmbeddingStatusDto
            {
                IsVectorDbEmpty = true,
                TotalMoviesInDatabase = 0,
                TotalEmbeddingsInVectorDb = 0,
                IsProcessing = false,
                LastProcessedAt = null,
                Status = "Error"
            };
        }
    }
}
