#nullable enable
using System;

namespace TrendplusProdavnica.Infrastructure.Search.Models
{
    /// <summary>
    /// Represents a search index event (product created, updated, etc.)
    /// </summary>
    public sealed class SearchIndexEvent
    {
        public enum EventType
        {
            ProductCreated,
            ProductUpdated,
            ProductPriceChanged,
            InventoryChanged,
            ProductDeleted,
            FullReindex
        }

        public string Id { get; set; } = Guid.NewGuid().ToString();
        public EventType Type { get; set; }
        public long ProductId { get; set; }
        public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
        public int RetryCount { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTimeOffset? LastRetryAtUtc { get; set; }

        // For DLQ tracking
        public bool IsDeadLettered { get; set; }
        public DateTimeOffset? DeadLetteredAtUtc { get; set; }
        public string? DeadLetterReason { get; set; }
    }

    /// <summary>
    /// Configuration for retry and DLQ handling
    /// </summary>
    public sealed class SearchIndexEventConfig
    {
        public int MaxRetries { get; set; } = 3;
        public TimeSpan InitialRetryDelay { get; set; } = TimeSpan.FromSeconds(5);
        public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(60);
        public double RetryBackoffMultiplier { get; set; } = 2.0;
        public int DeadLetterQueueMaxSize { get; set; } = 1000;
    }
}
