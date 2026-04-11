#nullable enable
using System;
using TrendplusProdavnica.Domain.Analytics;

namespace TrendplusProdavnica.Application.Analytics.DTOs
{
    /// <summary>
    /// DTO za analitički događaj (response)
    /// </summary>
    public class AnalyticsEventDto
    {
        public long Id { get; set; }
        public AnalyticsEventType EventType { get; set; }
        public long? ProductId { get; set; }
        public long? UserId { get; set; }
        public string? SessionId { get; set; }
        public DateTime EventTimestamp { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? PageUrl { get; set; }
        public string? ReferrerUrl { get; set; }
        public string? EventData { get; set; }
    }

    /// <summary>
    /// Request za slanje analitičkog događaja
    /// </summary>
    public class CreateAnalyticsEventRequest
    {
        public AnalyticsEventType EventType { get; set; }
        public long? ProductId { get; set; }
        public string? SessionId { get; set; }
        public string? PageUrl { get; set; }
        public string? ReferrerUrl { get; set; }
        public string? EventData { get; set; }
    }

    /// <summary>
    /// Metrika: Conversion rate
    /// </summary>
    public class ConversionRateMetric
    {
        public decimal ConversionRate { get; set; } // percentage
        public int TotalProductViews { get; set; }
        public int TotalOrders { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }

    /// <summary>
    /// Metrika: Top proizvodi
    /// </summary>
    public class TopProductMetric
    {
        public long ProductId { get; set; }
        public string? ProductName { get; set; }
        public int ViewCount { get; set; }
        public int AddToCartCount { get; set; }
        public int OrderCount { get; set; }
        public decimal ConversionRate { get; set; }
    }

    /// <summary>
    /// Metrika: Revenue po kategoriji
    /// </summary>
    public class CategoryRevenueMetric
    {
        public long CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
    }

    /// <summary>
    /// Dashboard metrics - sve kombinovana
    /// </summary>
    public class AnalyticsDashboardMetrics
    {
        public ConversionRateMetric? ConversionRate { get; set; }
        public List<TopProductMetric> TopProducts { get; set; } = new();
        public List<CategoryRevenueMetric> CategoryRevenue { get; set; } = new();
        public int TotalEvents { get; set; }
        public DateTime GeneratedAtUtc { get; set; }
    }
}
