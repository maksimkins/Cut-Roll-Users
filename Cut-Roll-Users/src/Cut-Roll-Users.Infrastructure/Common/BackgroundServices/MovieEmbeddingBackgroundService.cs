using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Cut_Roll_Users.Core.Common.BackgroundServices;
using Cut_Roll_Users.Core.MovieEmbeddings.Services;
using Cut_Roll_Users.Core.MovieEmbeddings.Dtos;
using Cut_Roll_Users.Core.Movies.Service;
using Cut_Roll_Users.Core.Common.DataProcessing;
using Cut_Roll_Users.Infrastructure.Common.Options;

namespace Cut_Roll_Users.Infrastructure.Common.BackgroundServices;

/// <summary>
/// Background service for automatically processing movie embeddings
/// </summary>
public class MovieEmbeddingBackgroundService : BackgroundService, IMovieEmbeddingBackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MovieEmbeddingBackgroundService> _logger;
    private readonly BackgroundServiceOptions _options;
    private readonly SemaphoreSlim _processingSemaphore;
    private volatile bool _isProcessing = false;
    private DateTime? _lastProcessedAt = null;
    private int _totalProcessed = 0;
    private int _totalFailed = 0;

    public MovieEmbeddingBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<MovieEmbeddingBackgroundService> logger,
        IOptions<BackgroundServiceOptions> options)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _processingSemaphore = new SemaphoreSlim(1, 1);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MovieEmbeddingBackgroundService started");

        // Wait for the application to fully start
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessNewMoviesAsync();
                
                // Wait for the configured interval before next processing
                var interval = TimeSpan.FromMinutes(_options.ProcessingIntervalMinutes);
                _logger.LogDebug("Waiting {Interval} minutes before next processing cycle", interval.TotalMinutes);
                await Task.Delay(interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("MovieEmbeddingBackgroundService is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MovieEmbeddingBackgroundService processing cycle");
                
                // Wait before retrying on error
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        _logger.LogInformation("MovieEmbeddingBackgroundService stopped");
    }

    public async Task ProcessNewMoviesAsync()
    {
        if (_isProcessing)
        {
            _logger.LogDebug("Processing already in progress, skipping this cycle");
            return;
        }

        await _processingSemaphore.WaitAsync();
        try
        {
            _isProcessing = true;
            _logger.LogInformation("Starting processing of new movies");

            using var scope = _serviceProvider.CreateScope();
            var movieEmbeddingService = scope.ServiceProvider.GetRequiredService<IMovieEmbeddingService>();
            var sqlDataReaderService = scope.ServiceProvider.GetRequiredService<ISqlDataReaderService>();
            var movieService = scope.ServiceProvider.GetRequiredService<IMovieService>();

            // Get count of movies without embeddings
            var totalMovies = await sqlDataReaderService.GetTotalMovieCountAsync();
            var moviesWithoutEmbeddings = await movieService.GetMoviesWithoutEmbeddingsCountAsync();
            var newMoviesCount = moviesWithoutEmbeddings;

            if (newMoviesCount <= 0)
            {
                _logger.LogDebug("No new movies to process");
                return;
            }

            _logger.LogInformation("Found {NewMoviesCount} movies without embeddings", newMoviesCount);

            // Process movies in batches
            var batchSize = _options.BatchSize;
            var offset = 0; // Start from the beginning since we're processing movies without embeddings
            var processedInCycle = 0;
            var failedInCycle = 0;

            while (offset < newMoviesCount)
            {
                var (successCount, failedCount) = await movieEmbeddingService.ProcessMoviesBatchAsync(offset, batchSize);
                
                processedInCycle += successCount;
                failedInCycle += failedCount;
                _totalProcessed += successCount;
                _totalFailed += failedCount;

                _logger.LogInformation("Processed batch {Offset}-{End}: Success={Success}, Failed={Failed}", 
                    offset, Math.Min(offset + batchSize, totalMovies), successCount, failedCount);

                offset += batchSize;

                // Small delay between batches to avoid overwhelming the system
                await Task.Delay(TimeSpan.FromMilliseconds(_options.BatchDelayMs));
            }

            _lastProcessedAt = DateTime.UtcNow;
            _logger.LogInformation("Completed processing cycle. Processed: {Processed}, Failed: {Failed}", 
                processedInCycle, failedInCycle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing new movies");
        }
        finally
        {
            _isProcessing = false;
            _processingSemaphore.Release();
        }
    }

    public async Task ProcessMoviesBatchAsync(int offset, int limit)
    {
        await _processingSemaphore.WaitAsync();
        try
        {
            _isProcessing = true;
            _logger.LogInformation("Processing movies batch {Offset}-{End}", offset, offset + limit - 1);

            using var scope = _serviceProvider.CreateScope();
            var movieEmbeddingService = scope.ServiceProvider.GetRequiredService<IMovieEmbeddingService>();

            var (successCount, failedCount) = await movieEmbeddingService.ProcessMoviesBatchAsync(offset, limit);
            
            _totalProcessed += successCount;
            _totalFailed += failedCount;
            _lastProcessedAt = DateTime.UtcNow;

            _logger.LogInformation("Completed batch processing. Success: {Success}, Failed: {Failed}", 
                successCount, failedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing movies batch {Offset}-{End}", offset, offset + limit - 1);
        }
        finally
        {
            _isProcessing = false;
            _processingSemaphore.Release();
        }
    }

    public async Task<int> GetNewMoviesCountAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var movieEmbeddingService = scope.ServiceProvider.GetRequiredService<IMovieEmbeddingService>();
            var sqlDataReaderService = scope.ServiceProvider.GetRequiredService<ISqlDataReaderService>();

            var totalMovies = await sqlDataReaderService.GetTotalMovieCountAsync();
            var processedMovies = await movieEmbeddingService.GetProcessedMovieCountAsync();
            
            return Math.Max(0, totalMovies - processedMovies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting new movies count");
            return 0;
        }
    }

    public Task<bool> IsProcessingAsync()
    {
        return Task.FromResult(_isProcessing);
    }

    public async Task<EmbeddingStatusDto> GetProcessingStatusAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var movieEmbeddingService = scope.ServiceProvider.GetRequiredService<IMovieEmbeddingService>();

            var status = await movieEmbeddingService.GetEmbeddingStatusAsync();
            
            // Add background service specific information
            status.IsProcessing = _isProcessing;
            status.LastProcessedAt = _lastProcessedAt;
            status.Status = _isProcessing ? "Processing" : 
                          _lastProcessedAt.HasValue ? "Idle" : "Not Started";

            return status;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting processing status");
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

    public override void Dispose()
    {
        _processingSemaphore?.Dispose();
        base.Dispose();
    }
}
