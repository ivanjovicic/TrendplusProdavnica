#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Analytics.DTOs;
using TrendplusProdavnica.Domain.Analytics;

namespace TrendplusProdavnica.Application.Analytics.Services
{
    /// <summary>
    /// Service za upravljanje analitičkim događajima i metrikama
    /// </summary>
    public interface IAnalyticsService
    {
        /// <summary>
        /// Prikuplja analitički događaj
        /// </summary>
        Task<AnalyticsEventDto> TrackEventAsync(
            CreateAnalyticsEventRequest request,
            long? userId = null,
            string? ipAddress = null,
            string? userAgent = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Vraća conversion rate metriku za vremenski period
        /// </summary>
        Task<ConversionRateMetric> GetConversionRateAsync(
            DateTime? from = null,
            DateTime? to = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Vraća top proizvode po različitim metrикama
        /// </summary>
        Task<List<TopProductMetric>> GetTopProductsAsync(
            int limit = 10,
            DateTime? from = null,
            DateTime? to = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Vraća revenue po kategoriji
        /// </summary>
        Task<List<CategoryRevenueMetric>> GetCategoryRevenueAsync(
            DateTime? from = null,
            DateTime? to = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Vraća kompletan dashboard sa svim metrikama
        /// </summary>
        Task<AnalyticsDashboardMetrics> GetDashboardMetricsAsync(
            DateTime? from = null,
            DateTime? to = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Dohvata sve događaje (sa paginacijom i filteriranjem)
        /// </summary>
        Task<(List<AnalyticsEventDto> events, int total)> GetEventsAsync(
            int page = 1,
            int pageSize = 50,
            AnalyticsEventType? eventType = null,
            DateTime? from = null,
            DateTime? to = null,
            CancellationToken cancellationToken = default);
    }
}
