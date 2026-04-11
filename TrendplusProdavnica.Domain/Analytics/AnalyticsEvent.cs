#nullable enable
using System;
using TrendplusProdavnica.Domain.Common;

namespace TrendplusProdavnica.Domain.Analytics
{
    /// <summary>
    /// Analitički događaj (product view, add to cart, order completed, itd.)
    /// </summary>
    public class AnalyticsEvent : EntityBase
    {
        /// <summary>Tip događaja (ProductView, ProductClick, AddToCart, itd.)</summary>
        public AnalyticsEventType EventType { get; set; }

        /// <summary>ID proizvoda (ako je relevantan)</summary>
        public long? ProductId { get; set; }

        /// <summary>ID korisnika (ako je autentifikovan)</summary>
        public long? UserId { get; set; }

        /// <summary>Session ID za praćenje anonimnih korisnika</summary>
        public string? SessionId { get; set; }

        /// <summary>Vrijeme događaja</summary>
        public DateTimeOffset EventTimestamp { get; set; }

        /// <summary>IP adresa korisnika</summary>
        public string? IpAddress { get; set; }

        /// <summary>User agent (browser info)</summary>
        public string? UserAgent { get; set; }

        /// <summary>URL stranice gdje se dogodio event</summary>
        public string? PageUrl { get; set; }

        /// <summary>Referrer URL</summary>
        public string? ReferrerUrl { get; set; }

        /// <summary>Dodatni JSON podaci za događaj (npr. cart value za AddToCart)</summary>
        public string? EventData { get; set; }

        // EF Core parameterless constructor
        private AnalyticsEvent() { }

        public AnalyticsEvent(
            AnalyticsEventType eventType,
            long? productId = null,
            long? userId = null,
            string? sessionId = null)
        {
            EventType = eventType;
            ProductId = productId;
            UserId = userId;
            SessionId = sessionId;
            EventTimestamp = DateTimeOffset.UtcNow;
        }
    }
}
