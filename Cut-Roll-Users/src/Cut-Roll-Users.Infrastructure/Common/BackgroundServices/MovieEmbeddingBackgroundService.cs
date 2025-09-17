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
    private CancellationToken _stoppingToken;

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
        _stoppingToken = stoppingToken;
        _logger.LogInformation("MovieEmbeddingBackgroundService started");

        // Wait for the application to fully start
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        var consecutiveErrors = 0;
        const int maxConsecutiveErrors = 5;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogDebug("Starting background service processing cycle");
                await ProcessNewMoviesAsync();
                
                // Reset error counter on successful processing
                consecutiveErrors = 0;
                
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
                consecutiveErrors++;
                _logger.LogError(ex, "Error in MovieEmbeddingBackgroundService processing cycle (Error #{ErrorCount})", consecutiveErrors);
                
                // If too many consecutive errors, wait longer before retrying
                var retryDelay = consecutiveErrors >= maxConsecutiveErrors 
                    ? TimeSpan.FromMinutes(30) // Wait 30 minutes if too many errors
                    : TimeSpan.FromMinutes(5);  // Wait 5 minutes for normal errors
                
                _logger.LogWarning("Waiting {Delay} minutes before retry (Error #{ErrorCount}/{MaxErrors})", 
                    retryDelay.TotalMinutes, consecutiveErrors, maxConsecutiveErrors);
                
                try
                {
                    await Task.Delay(retryDelay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Background service cancelled during retry delay");
                    break;
                }
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

        // Check if we have a cancellation token and if it's been cancelled
        if (_stoppingToken.IsCancellationRequested)
        {
            _logger.LogDebug("Processing cancelled, application is shutting down");
            return;
        }

        await _processingSemaphore.WaitAsync(_stoppingToken);
        try
        {
            _isProcessing = true;
            _logger.LogInformation("Starting processing of new movies");

            // Check if service provider is disposed
            if (_serviceProvider == null)
            {
                _logger.LogError("Service provider is null, cannot process movies");
                return;
            }

            IServiceScope scope;
            try
            {
                scope = _serviceProvider.CreateScope();
            }
            catch (ObjectDisposedException ex)
            {
                _logger.LogError(ex, "Service provider is disposed, cannot create scope. Application may be shutting down.");
                return;
            }

            using (scope)
            {
                var movieEmbeddingService = scope.ServiceProvider.GetRequiredService<IMovieEmbeddingService>();
                var sqlDataReaderService = scope.ServiceProvider.GetRequiredService<ISqlDataReaderService>();
                var movieService = scope.ServiceProvider.GetRequiredService<IMovieService>();

                // Get count of movies without embeddings with better error handling
                int totalMovies, moviesWithoutEmbeddings;
                try
                {
                    totalMovies = await sqlDataReaderService.GetTotalMovieCountAsync();
                    moviesWithoutEmbeddings = await movieService.GetMoviesWithoutEmbeddingsCountAsync();
                    _logger.LogDebug("Database stats - Total movies: {Total}, Without embeddings: {WithoutEmbeddings}", 
                        totalMovies, moviesWithoutEmbeddings);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to get movie counts from database");
                    throw; // Re-throw to trigger retry logic
                }

                if (moviesWithoutEmbeddings <= 0)
                {
                    _logger.LogDebug("No new movies to process (movies without embeddings: {Count})", moviesWithoutEmbeddings);
                    return;
                }

                _logger.LogInformation("Found {NewMoviesCount} movies without embeddings out of {TotalMovies} total movies", 
                    moviesWithoutEmbeddings, totalMovies);

                // Process movies in batches with improved error handling
                var batchSize = _options.BatchSize;
                var offset = 0;
                var processedInCycle = 0;
                var failedInCycle = 0;
                var batchNumber = 1;

                while (offset < moviesWithoutEmbeddings)
                {
                    // Check for cancellation before each batch
                    if (_stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("Processing cancelled during batch {BatchNumber}, stopping gracefully", batchNumber);
                        break;
                    }

                    try
                    {
                        _logger.LogDebug("Processing batch {BatchNumber} (offset: {Offset}, size: {BatchSize})", 
                            batchNumber, offset, batchSize);

                        var (successCount, failedCount) = await movieEmbeddingService.ProcessMoviesBatchAsync(offset, batchSize);
                        
                        processedInCycle += successCount;
                        failedInCycle += failedCount;
                        _totalProcessed += successCount;
                        _totalFailed += failedCount;

                        _logger.LogInformation("Batch {BatchNumber} completed: Success={Success}, Failed={Failed}, Total processed this cycle: {TotalProcessed}", 
                            batchNumber, successCount, failedCount, processedInCycle);

                        offset += batchSize;
                        batchNumber++;

                        // Small delay between batches to avoid overwhelming the system
                        if (offset < moviesWithoutEmbeddings) // Only delay if there are more batches
                        {
                            await Task.Delay(TimeSpan.FromMilliseconds(_options.BatchDelayMs));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing batch {BatchNumber} (offset: {Offset})", batchNumber, offset);
                        failedInCycle++;
                        _totalFailed++;
                        
                        // Continue with next batch instead of stopping completely
                        offset += batchSize;
                        batchNumber++;
                        
                        // Longer delay after batch error
                        await Task.Delay(TimeSpan.FromMilliseconds(_options.BatchDelayMs * 2));
                    }
                }

                _lastProcessedAt = DateTime.UtcNow;
                _logger.LogInformation("Completed processing cycle. Processed: {Processed}, Failed: {Failed}, Total processed: {TotalProcessed}, Total failed: {TotalFailed}", 
                    processedInCycle, failedInCycle, _totalProcessed, _totalFailed);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error in ProcessNewMoviesAsync - this will trigger retry logic");
            throw; // Re-throw to trigger the retry logic in ExecuteAsync
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
