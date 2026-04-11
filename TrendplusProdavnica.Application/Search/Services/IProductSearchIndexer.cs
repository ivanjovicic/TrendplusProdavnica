#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TrendplusProdavnica.Infrastructure.Search.Services
{
    /// <summary>
    /// Service for managing product search index synchronization
    /// </summary>
    public interface IProductSearchIndexer
    {
        /// <summary>
        /// Queue a product for indexing
        /// </summary>
        Task QueueProductAsync(long productId, SearchIndexEventType eventType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Queue multiple products for indexing
        /// </summary>
        Task QueueProductsAsync(IEnumerable<long> productIds, SearchIndexEventType eventType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Process all queued events
        /// </summary>
        Task ProcessQueueAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get current queue size
        /// </summary>
        Task<int> GetQueueSizeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get dead letter queue size
        /// </summary>
        Task<int> GetDeadLetterQueueSizeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Retry failed events from dead letter queue
        /// </summary>
        Task RetryDeadLetterAsync(int maxAttempts = 10, CancellationToken cancellationToken = default);
    }

    public enum SearchIndexEventType
    {
        ProductCreated,
        ProductUpdated,
        ProductPriceChanged,
        InventoryChanged,
        ProductDeleted,
        FullReindex
    }
}
