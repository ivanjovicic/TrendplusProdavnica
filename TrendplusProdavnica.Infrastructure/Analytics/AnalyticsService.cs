#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TrendplusProdavnica.Application.Analytics.DTOs;
using TrendplusProdavnica.Application.Analytics.Services;
using TrendplusProdavnica.Domain.Analytics;
using TrendplusProdavnica.Domain.Sales;
using TrendplusProdavnica.Infrastructure.Persistence;

namespace TrendplusProdavnica.Infrastructure.Analytics
{
    /// <summary>
    /// Implementacija analytics servisa za event collection i metriku generisanja
    /// </summary>
    public class AnalyticsService : IAnalyticsService
    {
        private readonly TrendplusDbContext _db;
        private readonly ILogger<AnalyticsService> _logger;

        public AnalyticsService(
            TrendplusDbContext db,
            ILogger<AnalyticsService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<AnalyticsEventDto> TrackEventAsync(
            CreateAnalyticsEventRequest request,
            long? userId = null,
            string? ipAddress = null,
            string? userAgent = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var analyticsEvent = new AnalyticsEvent(
                    request.EventType,
                    request.ProductId,
                    userId,
                    request.SessionId)
                {
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    PageUrl = request.PageUrl,
                    ReferrerUrl = request.ReferrerUrl,
                    EventData = request.EventData
                };

                _db.AnalyticsEvents.Add(analyticsEvent);
                await _db.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Analytics event tracked: {EventType} for product {ProductId} by session {SessionId}",
                    request.EventType,
                    request.ProductId,
                    request.SessionId);

                return MapToDto(analyticsEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking analytics event");
                throw;
            }
        }

        public async Task<ConversionRateMetric> GetConversionRateAsync(
            DateTime? from = null,
            DateTime? to = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var startDate = from ?? DateTime.UtcNow.AddDays(-30);
                var endDate = to ?? DateTime.UtcNow;

                var productViews = await _db.AnalyticsEvents
                    .CountAsync(e =>
                        e.EventType == AnalyticsEventType.ProductView &&
                        e.EventTimestamp >= startDate &&
                        e.EventTimestamp <= endDate,
                    cancellationToken);

                var completedOrders = await _db.AnalyticsEvents
                    .CountAsync(e =>
                        e.EventType == AnalyticsEventType.OrderCompleted &&
                        e.EventTimestamp >= startDate &&
                        e.EventTimestamp <= endDate,
                    cancellationToken);

                var conversionRate = productViews > 0
                    ? (completedOrders * 100m) / productViews
                    : 0m;

                return new ConversionRateMetric
                {
                    ConversionRate = conversionRate,
                    TotalProductViews = productViews,
                    TotalOrders = completedOrders,
                    PeriodStart = startDate,
                    PeriodEnd = endDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating conversion rate");
                throw;
            }
        }

        public async Task<List<TopProductMetric>> GetTopProductsAsync(
            int limit = 10,
            DateTime? from = null,
            DateTime? to = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var startDate = from ?? DateTime.UtcNow.AddDays(-30);
                var endDate = to ?? DateTime.UtcNow;

                var timeFilter = new DateTimeOffset(startDate).UtcDateTime;
                var endFilter = new DateTimeOffset(endDate).UtcDateTime;

                var topProducts = await _db.AnalyticsEvents
                    .Where(e =>
                        e.ProductId.HasValue &&
                        e.EventTimestamp >= timeFilter &&
                        e.EventTimestamp <= endFilter)
                    .GroupBy(e => e.ProductId)
                    .Select(g => new
                    {
                        ProductId = g.Key.Value,
                        ViewCount = g.Count(e => e.EventType == AnalyticsEventType.ProductView),
                        AddToCartCount = g.Count(e => e.EventType == AnalyticsEventType.AddToCart),
                        OrderCount = g.Count(e => e.EventType == AnalyticsEventType.OrderCompleted)
                    })
                    .OrderByDescending(x => x.ViewCount)
                    .ThenByDescending(x => x.OrderCount)
                    .Take(limit)
                    .ToListAsync(cancellationToken);

                var productIds = topProducts.Select(p => p.ProductId).ToList();
                var productNames = await _db.Products
                    .Where(p => productIds.Contains(p.Id))
                    .Select(p => new { p.Id, p.Name })
                    .ToListAsync(cancellationToken);

                var result = topProducts
                    .Select(p => new TopProductMetric
                    {
                        ProductId = p.ProductId,
                        ProductName = productNames.FirstOrDefault(n => n.Id == p.ProductId)?.Name,
                        ViewCount = p.ViewCount,
                        AddToCartCount = p.AddToCartCount,
                        OrderCount = p.OrderCount,
                        ConversionRate = p.ViewCount > 0
                            ? (p.OrderCount * 100m) / p.ViewCount
                            : 0m
                    })
                    .ToList();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top products");
                throw;
            }
        }

        public async Task<List<CategoryRevenueMetric>> GetCategoryRevenueAsync(
            DateTime? from = null,
            DateTime? to = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var startDate = from ?? DateTime.UtcNow.AddDays(-30);
                var endDate = to ?? DateTime.UtcNow;

                // Get category revenue from orders
                var categoryRevenue = await _db.Orders
                    .Where(o =>
                        o.CreatedAtUtc >= startDate &&
                        o.CreatedAtUtc <= endDate)
                    .Include(o => o.Items)
                    .GroupBy(o => o.Items.First().ProductId)
                    .Select(g => new
                    {
                        CategoryId = g.Key,
                        OrderCount = g.Count(),
                        TotalRevenue = g.Sum(o => o.TotalAmount)
                    })
                    .ToListAsync(cancellationToken);

                var categoryIds = categoryRevenue.Select(c => c.CategoryId).ToList();
                var categories = await _db.Categories
                    .Where(c => categoryIds.Contains(c.Id))
                    .Select(c => new { c.Id, c.Name })
                    .ToListAsync(cancellationToken);

                var result = categoryRevenue
                    .Select(cr => new CategoryRevenueMetric
                    {
                        CategoryId = cr.CategoryId,
                        CategoryName = categories.FirstOrDefault(c => c.Id == cr.CategoryId)?.Name,
                        OrderCount = cr.OrderCount,
                        TotalRevenue = cr.TotalRevenue,
                        AverageOrderValue = cr.OrderCount > 0
                            ? cr.TotalRevenue / cr.OrderCount
                            : 0m
                    })
                    .OrderByDescending(x => x.TotalRevenue)
                    .ToList();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category revenue");
                throw;
            }
        }

        public async Task<AnalyticsDashboardMetrics> GetDashboardMetricsAsync(
            DateTime? from = null,
            DateTime? to = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var conversionRate = await GetConversionRateAsync(from, to, cancellationToken);
                var topProducts = await GetTopProductsAsync(10, from, to, cancellationToken);
                var categoryRevenue = await GetCategoryRevenueAsync(from, to, cancellationToken);

                var startDate = from ?? DateTime.UtcNow.AddDays(-30);
                var endDate = to ?? DateTime.UtcNow;

                var totalEvents = await _db.AnalyticsEvents
                    .CountAsync(e =>
                        e.EventTimestamp >= startDate &&
                        e.EventTimestamp <= endDate,
                    cancellationToken);

                return new AnalyticsDashboardMetrics
                {
                    ConversionRate = conversionRate,
                    TopProducts = topProducts,
                    CategoryRevenue = categoryRevenue,
                    TotalEvents = totalEvents,
                    GeneratedAtUtc = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating dashboard metrics");
                throw;
            }
        }

        public async Task<(List<AnalyticsEventDto> events, int total)> GetEventsAsync(
            int page = 1,
            int pageSize = 50,
            AnalyticsEventType? eventType = null,
            DateTime? from = null,
            DateTime? to = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _db.AnalyticsEvents.AsQueryable();

                if (eventType.HasValue)
                    query = query.Where(e => e.EventType == eventType.Value);

                if (from.HasValue)
                    query = query.Where(e => e.EventTimestamp >= from.Value);

                if (to.HasValue)
                    query = query.Where(e => e.EventTimestamp <= to.Value);

                var total = await query.CountAsync(cancellationToken);

                var events = await query
                    .OrderByDescending(e => e.EventTimestamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                return (events.Select(MapToDto).ToList(), total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving events");
                throw;
            }
        }

        /// <summary>
        /// Vraća read-only snapshot supplier sales statistike
        /// Korisni za analitiku - točni, imutetni podaci
        /// </summary>
        public async Task<SupplierSalesReportDto> GetSupplierSalesStatsAsync(
            long? brandId = null,
            DateTime? from = null,
            DateTime? to = null,
            int limit = 100,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var startDate = from ?? DateTime.UtcNow.AddDays(-30);
                var endDate = to ?? DateTime.UtcNow;
                var generatedAt = DateTime.UtcNow;

                // Get all orders in period (readonly snapshot)
                var orders = await _db.Orders
                    .AsNoTracking()
                    .Where(o =>
                        o.CreatedAtUtc >= startDate &&
                        o.CreatedAtUtc <= endDate &&
                        (o.Status == OrderStatus.Pending || o.Status == OrderStatus.Completed))
                    .Include(o => o.Items)
                    .ToListAsync(cancellationToken);

                // Get product->brand mapping (readonly)
                var productIds = orders
                    .SelectMany(o => o.Items.Select(i => i.ProductId))
                    .Distinct()
                    .ToList();

                var productBrandMap = await _db.Products
                    .AsNoTracking()
                    .Where(p => productIds.Contains(p.Id))
                    .Select(p => new { p.Id, p.BrandId, p.Brand!.Name })
                    .ToListAsync(cancellationToken);

                // Get product views (readonly)
                var productViewsQuery = _db.AnalyticsEvents
                    .AsNoTracking()
                    .Where(e =>
                        e.EventType == AnalyticsEventType.ProductView &&
                        e.EventTimestamp >= startDate &&
                        e.EventTimestamp <= endDate);

                var productViews = await productViewsQuery.ToListAsync(cancellationToken);

                // Aggregate supplier stats (deterministic calculation)
                var supplierStats = new Dictionary<long, SupplierSalesStatsDto>();
                var ordersProcessedBySuppplier = new Dictionary<(long orderId, long brandId), bool>();

                foreach (var order in orders)
                {
                    var orderBrands = new HashSet<long>();
                    
                    foreach (var item in order.Items)
                    {
                        var brandInfo = productBrandMap.FirstOrDefault(pb => pb.Id == item.ProductId);
                        var bId = brandInfo?.BrandId ?? 0;
                        var bName = brandInfo?.Name ?? "Unknown";

                        if (!supplierStats.ContainsKey(bId))
                        {
                            supplierStats[bId] = new SupplierSalesStatsDto
                            {
                                BrandId = bId,
                                BrandName = bName,
                                CalculatedAtUtc = generatedAt,
                                PeriodStart = startDate,
                                PeriodEnd = endDate
                            };
                        }

                        var stat = supplierStats[bId];
                        stat.UnitsOrdered += item.Quantity;
                        stat.TotalRevenue += item.LineTotal;
                        stat.SourceRecordCount++;
                        
                        orderBrands.Add(bId);
                    }
                    
                    // Count each order only once per brand in that order
                    foreach (var supplierBrandId in orderBrands)
                    {
                        if (!ordersProcessedBySuppplier.ContainsKey((order.Id, supplierBrandId)))
                        {
                            var stat = supplierStats[supplierBrandId];
                            stat.TotalOrders++;
                            if (order.Status == OrderStatus.Completed)
                                stat.CompletedOrders++;
                            else if (order.Status == OrderStatus.Pending)
                                stat.PendingOrders++;
                            
                            ordersProcessedBySuppplier[(order.Id, supplierBrandId)] = true;
                        }
                    }
                }

                // Add product views (grouped by brand)
                var viewsByBrand = new Dictionary<long, int>();
                foreach (var view in productViews)
                {
                    if (view.ProductId.HasValue)
                    {
                        var brandInfo = productBrandMap.FirstOrDefault(pb => pb.Id == view.ProductId.Value);
                        if (brandInfo != null)
                        {
                            if (!viewsByBrand.ContainsKey(brandInfo.BrandId))
                            {
                                viewsByBrand[brandInfo.BrandId] = 0;
                            }
                            viewsByBrand[brandInfo.BrandId]++;
                        }
                    }
                }
                
                foreach (var kvp in viewsByBrand)
                {
                    if (supplierStats.ContainsKey(kvp.Key))
                    {
                        supplierStats[kvp.Key].ProductViews = kvp.Value;
                    }
                }

                // Calculate derived metrics (deterministic)
                var reportList = new List<SupplierSalesStatsDto>();
                foreach (var stat in supplierStats.Values)
                {
                    // Average order value
                    if (stat.TotalOrders > 0)
                    {
                        stat.AverageOrderValue = stat.TotalRevenue / stat.TotalOrders;
                        stat.AverageUnitsPerOrder = (decimal)stat.UnitsOrdered / stat.TotalOrders;
                    }

                    // Conversion rate
                    if (stat.ProductViews > 0)
                    {
                        stat.ConversionRate = (stat.TotalOrders * 100m) / stat.ProductViews;
                    }

                    stat.IsAggregated = true;
                    reportList.Add(stat);
                }

                // Apply filter if specified
                if (brandId.HasValue)
                {
                    reportList = reportList.Where(s => s.BrandId == brandId).ToList();
                }

                // Sort and limit
                var report = new SupplierSalesReportDto
                {
                    Suppliers = reportList
                        .OrderByDescending(s => s.TotalRevenue)
                        .Take(limit)
                        .ToList(),
                    ReportGeneratedAtUtc = generatedAt,
                    PeriodStart = startDate,
                    PeriodEnd = endDate,
                    TotalSuppliersIncluded = reportList.Count,
                    TotalMarketRevenue = reportList.Sum(s => s.TotalRevenue),
                    TotalMarketOrders = reportList.Sum(s => s.TotalOrders)
                };

                _logger.LogInformation(
                    "Generated supplier sales stats for {Count} suppliers from {Start} to {End}",
                    report.TotalSuppliersIncluded,
                    startDate,
                    endDate);

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating supplier sales stats");
                throw;
            }
        }

        private AnalyticsEventDto MapToDto(AnalyticsEvent entity)
        {
            return new AnalyticsEventDto
            {
                Id = entity.Id,
                EventType = entity.EventType,
                ProductId = entity.ProductId,
                UserId = entity.UserId,
                SessionId = entity.SessionId,
                EventTimestamp = entity.EventTimestamp.UtcDateTime,
                IpAddress = entity.IpAddress,
                UserAgent = entity.UserAgent,
                PageUrl = entity.PageUrl,
                ReferrerUrl = entity.ReferrerUrl,
                EventData = entity.EventData
            };
        }
    }
}
