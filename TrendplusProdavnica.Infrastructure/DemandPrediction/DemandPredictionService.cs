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
using TrendplusProdavnica.Infrastructure.Persistence;
using TrendplusProdavnica.Infrastructure.Persistence.Queries.Analytics;

namespace TrendplusProdavnica.Infrastructure.DemandPrediction
{
    /// <summary>
    /// Servis za predviđanje potražnje na osnovu sales history
    /// </summary>
    public class DemandPredictionService : IDemandPredictionService
    {
        private readonly TrendplusDbContext _db;
        private readonly DemandPredictionQueries _queries;
        private readonly ILogger<DemandPredictionService> _logger;

        public DemandPredictionService(
            TrendplusDbContext db,
            DemandPredictionQueries queries,
            ILogger<DemandPredictionService> logger)
        {
            _db = db;
            _queries = queries;
            _logger = logger;
        }

        /// <summary>
        /// Predviđa potražnju za jedan proizvod
        /// </summary>
        public async Task<DemandPredictionDto> PredictDemandAsync(
            DemandPredictionRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var product = await _db.Products
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

                if (product == null)
                {
                    throw new InvalidOperationException($"Proizvod sa ID {request.ProductId} nije pronađen");
                }

                // Prikupi sales podatke
                var monthlySalesData = await _queries.GetMonthlySalesDataAsync(
                    request.ProductId,
                    request.HistorymonthsCount,
                    cancellationToken);

                var sizeDistributionData = await _queries.GetSizeDistributionAsync(
                    request.ProductId,
                    request.HistorymonthsCount,
                    cancellationToken);

                var seasonalIndexes = await _queries.GetSeasonalIndexAsync(
                    request.ProductId,
                    cancellationToken);

                // Kalkuliši metrike
                var totalSalesUnits = monthlySalesData.Sum(x => x.UnitsSOld);
                var averageMonthlySales = monthlySalesData.Any() 
                    ? monthlySalesData.Average(x => x.UnitsSOld) 
                    : 0;

                // Izračunaj trend i predviđanje
                var trend = await _queries.GetSalesTrendAsync(
                    request.ProductId,
                    request.HistorymonthsCount,
                    cancellationToken: cancellationToken);

                var lastMonthAverage = trend.Any() 
                    ? trend.TakeLast(3).Average(x => x.Average) 
                    : averageMonthlySales;

                // Detektuj trend (raste/pada)
                var recentTrend = trend.Count > 5 
                    ? trend.TakeLast(5).Average(x => x.Average)
                    : lastMonthAverage;

                var olderTrend = trend.Count > 10
                    ? trend.Skip(5).Take(5).Average(x => x.Average)
                    : averageMonthlySales;

                var trendFactor = olderTrend > 0 
                    ? recentTrend / olderTrend 
                    : 1m;

                // Primeni trend faktor na predviđanje
                var forecastNextMonth = lastMonthAverage * trendFactor;

                // Izračunaj confidence score
                var confidenceScore = CalculateConfidenceScore(monthlySalesData, sizeDistributionData);

                // Kreiraj DTO
                return new DemandPredictionDto
                {
                    ProductId = request.ProductId,
                    ProductName = product.Name,
                    ExpectedMonthlySales = averageMonthlySales,
                    ForecastNextMonth = Math.Max(0, forecastNextMonth),
                    ConfidenceScore = confidenceScore,
                    AnalyzedAtUtc = DateTimeOffset.UtcNow,
                    Status = "COMPLETED",
                    MonthlySalesHistory = MapMonthlySalesHistory(monthlySalesData),
                    SizeDistribution = request.IsFootwear 
                        ? MapSizeDistribution(sizeDistributionData, forecastNextMonth)
                        : new List<SizeDistributionData>(),
                    SeasonalityIndex = MapSeasonalityIndex(seasonalIndexes)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri predviđanju potražnje za proizvod {ProductId}", request.ProductId);
                throw;
            }
        }

        /// <summary>
        /// Predviđa potražnju za više proizvoda
        /// </summary>
        public async Task<BulkDemandPredictionResponse> PredictDemandBulkAsync(
            BulkDemandPredictionRequest request,
            CancellationToken cancellationToken = default)
        {
            var response = new BulkDemandPredictionResponse();

            foreach (var productId in request.ProductIds)
            {
                try
                {
                    var prediction = await PredictDemandAsync(
                        new DemandPredictionRequest
                        {
                            ProductId = productId,
                            HistorymonthsCount = request.HistoryMonthsCount,
                            IsFootwear = true
                        },
                        cancellationToken);

                    response.Predictions.Add(prediction);
                    response.SuccessCount++;
                }
                catch (Exception ex)
                {
                    response.FailureCount++;
                    response.Errors.Add($"Proizvod {productId}: {ex.Message}");
                    _logger.LogWarning(ex, "Greška pri bulk predviđanju za proizvod {ProductId}", productId);
                }
            }

            return response;
        }

        /// <summary>
        /// Vraća preporuke za nabavku
        /// </summary>
        public async Task<List<SizeDistributionData>> GetProcurementRecommendationsAsync(
            long productId,
            decimal safetyStockPercentage = 20m,
            CancellationToken cancellationToken = default)
        {
            var prediction = await PredictDemandAsync(
                new DemandPredictionRequest { ProductId = productId },
                cancellationToken);

            // Primeni safety stock buffer
            var recommendations = prediction.SizeDistribution
                .Select(sd => new SizeDistributionData
                {
                    Size = sd.Size,
                    UnitsSold = sd.UnitsSold,
                    PercentageOfTotal = sd.PercentageOfTotal,
                    RecommendedStockQuantity = (int)(sd.RecommendedStockQuantity * (1 + safetyStockPercentage / 100))
                })
                .ToList();

            return recommendations;
        }

        /// <summary>
        /// Vraća sezonalne trendove
        /// </summary>
        public async Task<List<SeasonalityData>> GetCategorySeasonalityAsync(
            long categoryId,
            CancellationToken cancellationToken = default)
        {
            // Prikupi sve proizvode iz kategorije
            var productIds = await _db.Products
                .AsNoTracking()
                .Where(p => p.PrimaryCategoryId == categoryId)
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);

            if (!productIds.Any())
                return new List<SeasonalityData>();

            // Agreguj sezonalne podatke
            var seasonalAggregates = new Dictionary<string, List<decimal>>();

            foreach (var productId in productIds.Take(20)) // Limit za performansu
            {
                var indexes = await _queries.GetSeasonalIndexAsync(productId, cancellationToken);
                
                foreach (var (season, index) in indexes)
                {
                    if (!seasonalAggregates.ContainsKey(season))
                        seasonalAggregates[season] = new List<decimal>();
                    seasonalAggregates[season].Add(index);
                }
            }

            return seasonalAggregates
                .Select(kvp => new SeasonalityData
                {
                    Season = kvp.Key,
                    SeasonalIndex = kvp.Value.Any() ? kvp.Value.Average() : 1m,
                    AverageMonthlyUnits = 0 // Može se kalkulisati ako je potrebno
                })
                .ToList();
        }

        /// <summary>
        /// Vraća top proizvode po potražnji
        /// </summary>
        public async Task<List<DemandPredictionDto>> GetTopDemandProductsAsync(
            long? categoryId = null,
            int limit = 10,
            CancellationToken cancellationToken = default)
        {
            var query = _db.Products.AsNoTracking();

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.PrimaryCategoryId == categoryId.Value);
            }

            var productIds = await query
                .Where(p => p.IsVisible && p.IsPurchasable)
                .Take(limit * 3) // Uzmi više za filtriranje
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);

            var predictions = new List<DemandPredictionDto>();

            foreach (var productId in productIds)
            {
                try
                {
                    var prediction = await PredictDemandAsync(
                        new DemandPredictionRequest { ProductId = productId },
                        cancellationToken);

                    predictions.Add(prediction);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Greška pri predviđanju za proizvod {ProductId}", productId);
                }
            }

            return predictions
                .OrderByDescending(p => p.ExpectedMonthlySales * 12) // Godišnja potražnja
                .Take(limit)
                .ToList();
        }

        private decimal CalculateConfidenceScore(
            List<(string Month, decimal UnitsSOld, decimal Revenue)> monthlySales,
            List<(decimal Size, int UnitsSold)> sizeData)
        {
            if (!monthlySales.Any())
                return 0;

            // Confidence se povećava sa više podataka
            var dataPointsScore = Math.Min(100, monthlySales.Count * 8.33m); // Max 12 meseci

            // Confidence se smanjuje sa visokom volatilnošću
            var variance = CalculateVariance(monthlySales.Select(x => x.UnitsSOld).ToList());
            var average = monthlySales.Average(x => x.UnitsSOld);
            var volatility = average > 0 ? variance / average : 0;
            var volatilityPenalty = Math.Min(30, volatility * 30);

            // Size diversity - ako se prodaje u malim veličinama, manja je volatilnost
            var sizeCountScore = Math.Min(20, sizeData.Count * 2);

            return Math.Max(0, Math.Min(100, dataPointsScore - volatilityPenalty + (sizeCountScore / 2)));
        }

        private decimal CalculateVariance(List<decimal> values)
        {
            if (values.Count < 2)
                return 0;

            var average = values.Average();
            var squaredDifferences = values.Select(x => (x - average) * (x - average));
            return squaredDifferences.Average() > 0 
                ? (decimal)Math.Sqrt((double)squaredDifferences.Average())
                : 0;
        }

        private List<MonthlySalesData> MapMonthlySalesHistory(
            List<(string Month, decimal UnitsSOld, decimal Revenue)> data)
        {
            return data
                .Select(x => new MonthlySalesData
                {
                    Month = x.Month,
                    UnitsSOld = x.UnitsSOld,
                    Revenue = x.Revenue
                })
                .ToList();
        }

        private List<SizeDistributionData> MapSizeDistribution(
            List<(decimal Size, int UnitsSold)> sizeData,
            decimal forecastNextMonth)
        {
            var totalUnits = sizeData.Sum(x => x.UnitsSold);
            if (totalUnits == 0)
                return new List<SizeDistributionData>();

            return sizeData
                .Select(x => new SizeDistributionData
                {
                    Size = x.Size,
                    UnitsSold = x.UnitsSold,
                    PercentageOfTotal = totalUnits > 0 
                        ? (x.UnitsSold / (decimal)totalUnits) * 100 
                        : 0,
                    RecommendedStockQuantity = (int)(forecastNextMonth * (x.UnitsSold / (decimal)totalUnits))
                })
                .ToList();
        }

        private List<SeasonalityData> MapSeasonalityIndex(Dictionary<string, decimal> indexes)
        {
            return indexes
                .Select(x => new SeasonalityData
                {
                    Season = x.Key,
                    SeasonalIndex = x.Value,
                    AverageMonthlyUnits = 0
                })
                .ToList();
        }
    }
}
