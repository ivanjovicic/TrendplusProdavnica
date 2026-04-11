#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TrendplusProdavnica.Application.Search.Services;
using TrendplusProdavnica.Domain.Search;
using TrendplusProdavnica.Infrastructure.Persistence;
using TrendplusProdavnica.Infrastructure.Search.Models;

namespace TrendplusProdavnica.Infrastructure.Search.Services
{
    /// <summary>
    /// Event-driven product search indexer with retry logic and dead letter queue support
    /// </summary>
    public sealed class ProductSearchIndexer : IProductSearchIndexer
    {
        private readonly TrendplusDbContext _db;
        private readonly IProductSearchIndexService _indexService;
        private readonly SearchIndexEventConfig _config;
        private readonly ILogger<ProductSearchIndexer> _logger;

        public ProductSearchIndexer(
            TrendplusDbContext db,
            IProductSearchIndexService indexService,
            IOptions<SearchIndexEventConfig> config,
            ILogger<ProductSearchIndexer> logger)
        {
            _db = db;
            _indexService = indexService;
            _config = config.Value;
            _logger = logger;
        }

        public async Task QueueProductAsync(
            long productId,
            SearchIndexEventType eventType,
            CancellationToken cancellationToken = default)
        {
            await QueueProductsAsync(new[] { productId }, eventType, cancellationToken);
        }

        public async Task QueueProductsAsync(
            IEnumerable<long> productIds,
            SearchIndexEventType eventType,
            CancellationToken cancellationToken = default)
        {
            var events = productIds
                .Distinct()
                .Select(productId => new SearchIndexEventLog(
                    Guid.NewGuid().ToString(),
                    (SearchIndexEventLog.EventType)(int)eventType,
                    productId))
                .ToList();

            if (events.Count == 0)
                return;

            await _db.SearchIndexEventLogs.AddRangeAsync(events, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Queued {Count} products for search indexing. Event type: {EventType}",
                events.Count,
                eventType);
        }

        public async Task ProcessQueueAsync(CancellationToken cancellationToken = default)
        {
            var batchSize = 100;
            var pendingEvents = await _db.SearchIndexEventLogs
                .Where(e => !e.IsProcessed && !e.IsDeadLettered)
                .OrderBy(e => e.CreatedAtUtc)
                .Take(batchSize)
                .ToListAsync(cancellationToken);

            if (pendingEvents.Count == 0)
            {
                _logger.LogDebug("No pending search index events to process");
                return;
            }

            _logger.LogInformation("Processing {Count} search index events", pendingEvents.Count);

            foreach (var eventLog in pendingEvents)
            {
                try
                {
                    // Check if we should retry or mark as DLQ
                    if (eventLog.RetryCount > 0 && eventLog.LastRetryAtUtc.HasValue)
                    {
                        var retryDelay = eventLog.GetRetryDelay(
                            _config.InitialRetryDelay,
                            _config.MaxRetryDelay,
                            _config.RetryBackoffMultiplier);

                        var nextRetryTime = eventLog.LastRetryAtUtc.Value.Add(retryDelay);
                        if (DateTimeOffset.UtcNow < nextRetryTime)
                        {
                            continue; // Skip, not ready for retry yet
                        }
                    }

                    if (eventLog.RetryCount >= _config.MaxRetries)
                    {
                        eventLog.MarkAsDeadLettered(
                            $"Max retries exceeded ({_config.MaxRetries}). Last error: {eventLog.LastErrorMessage}");
                        _logger.LogError(
                            "Event moved to dead letter queue: {EventId}, Product: {ProductId}",
                            eventLog.EventId,
                            eventLog.ProductId);
                        continue;
                    }

                    // Process the event
                    await ProcessEventAsync(eventLog, cancellationToken);
                    eventLog.MarkAsProcessed();

                    _logger.LogInformation(
                        "Successfully processed search index event: {EventId}, Product: {ProductId}",
                        eventLog.EventId,
                        eventLog.ProductId);
                }
                catch (Exception ex)
                {
                    eventLog.MarkRetryAttempt(ex.Message);
                    _logger.LogWarning(
                        ex,
                        "Failed to process search index event: {EventId}. Retry attempt {RetryCount}/{MaxRetries}",
                        eventLog.EventId,
                        eventLog.RetryCount,
                        _config.MaxRetries);
                }
            }

            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> GetQueueSizeAsync(CancellationToken cancellationToken = default)
        {
            return await _db.SearchIndexEventLogs
                .Where(e => !e.IsProcessed && !e.IsDeadLettered)
                .CountAsync(cancellationToken);
        }

        public async Task<int> GetDeadLetterQueueSizeAsync(CancellationToken cancellationToken = default)
        {
            return await _db.SearchIndexEventLogs
                .Where(e => e.IsDeadLettered)
                .CountAsync(cancellationToken);
        }

        public async Task RetryDeadLetterAsync(int maxAttempts = 10, CancellationToken cancellationToken = default)
        {
            var dlqEvents = await _db.SearchIndexEventLogs
                .Where(e => e.IsDeadLettered)
                .OrderBy(e => e.DeadLetteredAtUtc)
                .Take(maxAttempts)
                .ToListAsync(cancellationToken);

            if (dlqEvents.Count == 0)
            {
                _logger.LogInformation("No dead letter queue events to retry");
                return;
            }

            _logger.LogInformation("Retrying {Count} dead letter queue events", dlqEvents.Count);

            foreach (var eventLog in dlqEvents)
            {
                try
                {
                    // Reset DLQ status and retry count
                    eventLog.ResetForRetry();

                    await ProcessEventAsync(eventLog, cancellationToken);
                    eventLog.MarkAsProcessed();

                    _logger.LogInformation(
                        "Successfully recovered dead letter event: {EventId}, Product: {ProductId}",
                        eventLog.EventId,
                        eventLog.ProductId);
                }
                catch (Exception ex)
                {
                    eventLog.MarkRetryAttempt(ex.Message);
                    _logger.LogWarning(
                        ex,
                        "Failed to retry dead letter event: {EventId}. Attempt {RetryCount}/{MaxRetries}",
                        eventLog.EventId,
                        eventLog.RetryCount,
                        _config.MaxRetries);

                    if (eventLog.RetryCount >= _config.MaxRetries)
                    {
                        eventLog.MarkAsDeadLettered(
                            $"Retry failed in recovery process. Last error: {ex.Message}");
                    }
                }
            }

            await _db.SaveChangesAsync(cancellationToken);
        }

        private async Task ProcessEventAsync(SearchIndexEventLog eventLog, CancellationToken cancellationToken)
        {
            switch (eventLog.Type)
            {
                case SearchIndexEventLog.EventType.FullReindex:
                    await _indexService.ReindexAllAsync(cancellationToken);
                    break;

                case SearchIndexEventLog.EventType.ProductCreated:
                case SearchIndexEventLog.EventType.ProductUpdated:
                case SearchIndexEventLog.EventType.ProductPriceChanged:
                case SearchIndexEventLog.EventType.InventoryChanged:
                    await _indexService.ReindexProductAsync(eventLog.ProductId, cancellationToken);
                    break;

                case SearchIndexEventLog.EventType.ProductDeleted:
                    await _indexService.DeleteProductAsync(eventLog.ProductId, cancellationToken);
                    break;

                default:
                    throw new InvalidOperationException($"Unknown event type: {eventLog.Type}");
            }
        }
    }
}
