#nullable enable
using System;
using TrendplusProdavnica.Domain.Common;

namespace TrendplusProdavnica.Domain.Search
{
    /// <summary>
    /// Domain entity for tracking search index events and dead letters
    /// </summary>
    public sealed class SearchIndexEventLog : AggregateRoot
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

        public string EventId { get; private set; }
        public EventType Type { get; private set; }
        public long ProductId { get; private set; }
        public int RetryCount { get; private set; }
        public string? LastErrorMessage { get; private set; }
        public DateTimeOffset? LastRetryAtUtc { get; private set; }
        
        // DLQ tracking
        public bool IsProcessed { get; private set; }
        public bool IsDeadLettered { get; private set; }
        public DateTimeOffset? DeadLetteredAtUtc { get; private set; }
        public string? DeadLetterReason { get; private set; }

        // Audit
        public DateTimeOffset? ProcessedAtUtc { get; private set; }

        public SearchIndexEventLog(
            string eventId,
            EventType type,
            long productId)
        {
            EventId = eventId;
            Type = type;
            ProductId = productId;
            CreatedAtUtc = DateTimeOffset.UtcNow;
            UpdatedAtUtc = CreatedAtUtc;
            RetryCount = 0;
            IsProcessed = false;
            IsDeadLettered = false;
        }

        public void MarkAsProcessed()
        {
            IsProcessed = true;
            ProcessedAtUtc = DateTimeOffset.UtcNow;
            UpdatedAtUtc = ProcessedAtUtc.Value;
        }

        public void MarkRetryAttempt(string? errorMessage)
        {
            RetryCount++;
            LastErrorMessage = errorMessage;
            LastRetryAtUtc = DateTimeOffset.UtcNow;
            UpdatedAtUtc = LastRetryAtUtc.Value;
        }

        public void MarkAsDeadLettered(string reason)
        {
            IsDeadLettered = true;
            DeadLetteredAtUtc = DateTimeOffset.UtcNow;
            DeadLetterReason = reason;
            UpdatedAtUtc = DeadLetteredAtUtc.Value;
        }

        public void ResetForRetry()
        {
            IsDeadLettered = false;
            DeadLetteredAtUtc = null;
            DeadLetterReason = null;
            RetryCount = 0;
            LastErrorMessage = null;
            LastRetryAtUtc = null;
            IsProcessed = false;
            ProcessedAtUtc = null;
            UpdatedAtUtc = DateTimeOffset.UtcNow;
        }

        public TimeSpan GetRetryDelay(TimeSpan initialDelay, TimeSpan maxDelay, double backoffMultiplier)
        {
            var exponentialDelay = initialDelay.TotalSeconds * Math.Pow(backoffMultiplier, RetryCount);
            var cappedDelay = Math.Min(exponentialDelay, maxDelay.TotalSeconds);
            return TimeSpan.FromSeconds(cappedDelay);
        }
    }
}
