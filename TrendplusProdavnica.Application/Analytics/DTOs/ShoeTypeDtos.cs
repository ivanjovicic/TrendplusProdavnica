#nullable enable
using System;
using System.Collections.Generic;

namespace TrendplusProdavnica.Application.Analytics.DTOs
{
    /// <summary>
    /// Metrika po tipu obuće / kategoriji za analitiku
    /// </summary>
    public class ShoeTypeSalesStatsDto
    {
        public long? CategoryId { get; set; }
        public string? CategoryName { get; set; }

        // Order-related metrics
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int PendingOrders { get; set; }

        // Revenue metrics
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }

        // Product metrics
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
        public int SourceRecordCount { get; set; }
    }

    public class ShoeTypeSalesReportDto
    {
        public List<ShoeTypeSalesStatsDto> ShoeTypes { get; set; } = new();
        public DateTime ReportGeneratedAtUtc { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int TotalTypesIncluded { get; set; }
        public decimal TotalMarketRevenue { get; set; }
        public int TotalMarketOrders { get; set; }
    }
}
