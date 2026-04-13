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

    /// <summary>
    /// Metrika: Supplier sales statistics - točni imutetni podaci
    /// </summary>
    public class SupplierSalesStatsDto
    {
        public long? BrandId { get; set; }
        public string? BrandName { get; set; }
        
        // Order-related metrics
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int PendingOrders { get; set; }
        
        // Revenue metrics
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        
        // Product metrics
        public int TotalProductsListed { get; set; }
        public int UnitsOrdered { get; set; }
        public decimal AverageUnitsPerOrder { get; set; }
        
        // Performance indicators
        public decimal ConversionRate { get; set; }  // percentage
        public int ProductViews { get; set; }
        
        // Temporal info
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        
        // Data integrity markers
        public DateTime CalculatedAtUtc { get; set; }
        public string DataVersion { get; set; } = "1.0";
        public bool IsAggregated { get; set; }
        public int SourceRecordCount { get; set; }  // Broj izvora koji je korišten
    }

    /// <summary>
    /// Aggregated supplier sales response
    /// </summary>
    public class SupplierSalesReportDto
    {
        public List<SupplierSalesStatsDto> Suppliers { get; set; } = new();
        public DateTime ReportGeneratedAtUtc { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int TotalSuppliersIncluded { get; set; }
        public decimal TotalMarketRevenue { get; set; }
        public int TotalMarketOrders { get; set; }
    }
}
