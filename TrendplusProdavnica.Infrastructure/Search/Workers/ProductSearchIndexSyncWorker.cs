#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TrendplusProdavnica.Infrastructure.Search.Services;

namespace TrendplusProdavnica.Infrastructure.Search.Workers
{
    /// <summary>
    /// Background worker that processes search index events at regular intervals
    /// </summary>
    public sealed class ProductSearchIndexSyncWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SearchIndexSyncWorkerConfig _config;
        private readonly ILogger<ProductSearchIndexSyncWorker> _logger;

        public ProductSearchIndexSyncWorker(
            IServiceProvider serviceProvider,
            IOptions<SearchIndexSyncWorkerConfig> config,
            ILogger<ProductSearchIndexSyncWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _config = config.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ProductSearchIndexSyncWorker started");

            // Initial delay to allow app startup
            await Task.Delay(_config.InitialDelayMs, stoppingToken);

            using var timer = new PeriodicTimer(
                TimeSpan.FromMilliseconds(_config.IntervalMs));

            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    try
                    {
                        await ProcessQueueAsync(stoppingToken);

                        // Periodically retry dead letter queue
                        if (DateTime.UtcNow.Ticks % 10 == 0) // Every 10th iteration
                        {
                            await RetryDeadLetterAsync(stoppingToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in ProductSearchIndexSyncWorker");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("ProductSearchIndexSyncWorker cancelled");
            }
        }

        private async Task ProcessQueueAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var indexer = scope.ServiceProvider.GetRequiredService<IProductSearchIndexer>();

            var queueSize = await indexer.GetQueueSizeAsync(cancellationToken);
            if (queueSize > 0)
            {
                _logger.LogInformation("Processing {QueueSize} search index events", queueSize);
                await indexer.ProcessQueueAsync(cancellationToken);
            }
        }

        private async Task RetryDeadLetterAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var indexer = scope.ServiceProvider.GetRequiredService<IProductSearchIndexer>();

            var dlqSize = await indexer.GetDeadLetterQueueSizeAsync(cancellationToken);
            if (dlqSize > 0)
            {
                _logger.LogWarning("Found {DLQSize} dead letter queue events, attempting recovery", dlqSize);
                await indexer.RetryDeadLetterAsync(
                    maxAttempts: _config.MaxDLQRetryAttempts,
                    cancellationToken: cancellationToken);
            }
        }
    }

    /// <summary>
    /// Configuration for ProductSearchIndexSyncWorker
    /// </summary>
    public sealed class SearchIndexSyncWorkerConfig
    {
        /// <summary>
        /// Initial delay before worker starts processing (ms)
        /// </summary>
        public int InitialDelayMs { get; set; } = 5000;

        /// <summary>
        /// Interval between processing runs (ms)
        /// </summary>
        public int IntervalMs { get; set; } = 10000;

        /// <summary>
        /// Maximum number of DLQ events to retry per attempt
        /// </summary>
        public int MaxDLQRetryAttempts { get; set; } = 10;

        /// <summary>
        /// Whether to enable the background worker
        /// </summary>
        public bool Enabled { get; set; } = true;
    }
}
