namespace Cut_Roll_Users.Infrastructure.Common.Options;

/// <summary>
/// Configuration options for background services
/// </summary>
public class BackgroundServiceOptions
{
    /// <summary>
    /// Interval in minutes between processing cycles
    /// </summary>
    public int ProcessingIntervalMinutes { get; set; } = 30;

    /// <summary>
    /// Number of movies to process in each batch
    /// </summary>
    public int BatchSize { get; set; } = 32;

    /// <summary>
    /// Delay in milliseconds between batches
    /// </summary>
    public int BatchDelayMs { get; set; } = 1000;

    /// <summary>
    /// Maximum number of retry attempts for failed movies
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Whether to enable automatic processing on startup
    /// </summary>
    public bool EnableAutoProcessing { get; set; } = true;
}

