#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrendplusProdavnica.Infrastructure.Persistence;

namespace TrendplusProdavnica.Infrastructure.Persistence.Queries.Analytics
{
    /// <summary>
    /// Queries za demand prediction - analiza sales history podataka
    /// </summary>
    public class DemandPredictionQueries
    {
        private readonly TrendplusDbContext _db;

        public DemandPredictionQueries(TrendplusDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Prikupi mesečjne prodajne podatke za proizvod
        /// </summary>
        public async Task<List<(string Month, decimal UnitsSOld, decimal Revenue)>> GetMonthlySalesDataAsync(
            long productId,
            int monthsBack = 12,
            CancellationToken cancellationToken = default)
        {
            var cutoffDate = DateTimeOffset.UtcNow.AddMonths(-monthsBack);

            var monthlySales = await _db.Orders
                .Where(o => o.PlacedAtUtc >= cutoffDate && o.PlacedAtUtc.HasValue &&
                            o.Status != Domain.Sales.OrderStatus.Cancelled)
                .SelectMany(o => o.Items)
                .Where(oi => oi.ProductId == productId)
                .GroupBy(oi => new
                {
                    Year = oi.Order!.PlacedAtUtc!.Value.Year,
                    Month = oi.Order.PlacedAtUtc.Value.Month
                })
                .Select(g => new
                {
                    YearMonth = $"{g.Key.Year:0000}-{g.Key.Month:00}",
                    UnitsSOld = (decimal)g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.LineTotal)
                })
                .OrderBy(x => x.YearMonth)
                .ToListAsync(cancellationToken);

            return monthlySales
                .Select(x => (x.YearMonth, x.UnitsSOld, x.Revenue))
                .ToList();
        }

        /// <summary>
        /// Prikupi distribuciju veličina za proizvod
        /// </summary>
        public async Task<List<(decimal Size, int UnitsSold)>> GetSizeDistributionAsync(
            long productId,
            int monthsBack = 12,
            CancellationToken cancellationToken = default)
        {
            var cutoffDate = DateTimeOffset.UtcNow.AddMonths(-monthsBack);

            var sizeDistribution = await _db.Orders
                .Where(o => o.PlacedAtUtc >= cutoffDate && o.PlacedAtUtc.HasValue &&
                            o.Status != Domain.Sales.OrderStatus.Cancelled)
                .SelectMany(o => o.Items)
                .Where(oi => oi.ProductId == productId)
                .GroupBy(oi => oi.SizeEuSnapshot)
                .Select(g => new
                {
                    Size = g.Key,
                    UnitsSold = g.Sum(oi => oi.Quantity)
                })
                .OrderBy(x => x.Size)
                .ToListAsync(cancellationToken);

            return sizeDistribution
                .Select(x => (x.Size, x.UnitsSold))
                .ToList();
        }

        /// <summary>
        /// Prikupi sezonalne podatke za proizvod
        /// </summary>
        public async Task<List<(string Season, decimal UnitsSOld)>> GetSeasonalSalesDataAsync(
            long productId,
            int yearsBack = 2,
            CancellationToken cancellationToken = default)
        {
            var cutoffDate = DateTimeOffset.UtcNow.AddYears(-yearsBack);

            var seasonalData = await _db.Orders
                .Where(o => o.PlacedAtUtc >= cutoffDate && o.PlacedAtUtc.HasValue &&
                            o.Status != Domain.Sales.OrderStatus.Cancelled)
                .SelectMany(o => o.Items)
                .Where(oi => oi.ProductId == productId)
                .ToListAsync(cancellationToken);

            // Grupiraj po sezoni
            var salesBySeason = seasonalData
                .GroupBy(oi => GetSeason(oi.Order!.PlacedAtUtc!.Value))
                .Select(g => new
                {
                    Season = g.Key,
                    UnitsSOld = (decimal)g.Sum(oi => oi.Quantity)
                })
                .ToList();

            return salesBySeason
                .Select(x => (x.Season, x.UnitsSOld))
                .ToList();
        }

        /// <summary>
        /// Vrati total sales za proizvod u periodu
        /// </summary>
        public async Task<decimal> GetTotalSalesAsync(
            long productId,
            int monthsBack = 12,
            CancellationToken cancellationToken = default)
        {
            var cutoffDate = DateTimeOffset.UtcNow.AddMonths(-monthsBack);

            return await _db.Orders
                .Where(o => o.PlacedAtUtc >= cutoffDate && o.PlacedAtUtc.HasValue &&
                            o.Status != Domain.Sales.OrderStatus.Cancelled)
                .SelectMany(o => o.Items)
                .Where(oi => oi.ProductId == productId)
                .SumAsync(oi => (decimal?)oi.Quantity, cancellationToken) ?? 0;
        }

        /// <summary>
        /// Vrati sales trend za proizvod - korisno za detektovanje sezonalnosti
        /// </summary>
        public async Task<List<(int YearMonth, decimal Average)>> GetSalesTrendAsync(
            long productId,
            int monthsBack = 12,
            int windowSize = 3, // Moving average window
            CancellationToken cancellationToken = default)
        {
            var monthlySales = await GetMonthlySalesDataAsync(productId, monthsBack, cancellationToken);

            if (monthlySales.Count < windowSize)
                return monthlySales.Select((x, i) => (i, x.UnitsSOld)).ToList();

            var trend = new List<(int, decimal)>();
            
            for (int i = 0; i < monthlySales.Count; i++)
            {
                var windowStart = Math.Max(0, i - windowSize / 2);
                var windowEnd = Math.Min(monthlySales.Count - 1, i + windowSize / 2);
                var average = monthlySales
                    .Skip(windowStart)
                    .Take(windowEnd - windowStart + 1)
                    .Average(x => x.UnitsSOld);

                trend.Add((i, average));
            }

            return trend;
        }

        /// <summary>
        /// Vrati sezonalni indeks za proizvod
        /// </summary>
        public async Task<Dictionary<string, decimal>> GetSeasonalIndexAsync(
            long productId,
            CancellationToken cancellationToken = default)
        {
            var seasonalData = await GetSeasonalSalesDataAsync(productId, 2, cancellationToken);
            
            if (!seasonalData.Any())
                return new Dictionary<string, decimal>();

            var overallAverage = seasonalData.Average(x => x.UnitsSOld);

            return seasonalData
                .ToDictionary(
                    x => x.Season,
                    x => overallAverage > 0 ? x.UnitsSOld / overallAverage : 1m
                );
        }

        private static string GetSeason(DateTimeOffset date)
        {
            return date.Month switch
            {
                12 or 1 or 2 => "WINTER",
                3 or 4 or 5 => "SPRING",
                6 or 7 or 8 => "SUMMER",
                9 or 10 or 11 => "FALL",
                _ => "UNKNOWN"
            };
        }
    }
}
