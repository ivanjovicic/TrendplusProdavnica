#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Analytics.DTOs;

namespace TrendplusProdavnica.Application.Analytics.Services
{
    /// <summary>
    /// Servis za predviđanje potražnje na osnovu istorijskog podataka o prodaji
    /// </summary>
    public interface IDemandPredictionService
    {
        /// <summary>
        /// Predviđa potražnju za jedan proizvod
        /// </summary>
        /// <param name="request">Zahtev sa ProductId i parametrima analize</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Predviđanje potražnje sa metrikama</returns>
        Task<DemandPredictionDto> PredictDemandAsync(
            DemandPredictionRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Predviđa potražnju za više proizvoda odjednom
        /// </summary>
        /// <param name="request">Zahtev sa listom ProductIds</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Rezultati za sve proizvode</returns>
        Task<BulkDemandPredictionResponse> PredictDemandBulkAsync(
            BulkDemandPredictionRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Vraća preporuke za nabavku na osnovu predviđanja
        /// </summary>
        /// <param name="productId">ID proizvoda</param>
        /// <param name="safetyStockPercentage">Procenat sigurnosnog stoka (default: 20%)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Preporuke za svaku veličinu</returns>
        Task<List<SizeDistributionData>> GetProcurementRecommendationsAsync(
            long productId,
            decimal safetyStockPercentage = 20m,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Vraća sezonalne trendove za kategoriju
        /// </summary>
        /// <param name="categoryId">ID kategorije</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Sezonalni indeksi</returns>
        Task<List<SeasonalityData>> GetCategorySeasonalityAsync(
            long categoryId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Vraća top proizvode po predviđenoj potražnji
        /// </summary>
        /// <param name="categoryId">Opciono: filtriraj po kategoriji</param>
        /// <param name="limit">Broj proizvoda (default: 10)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Top proizvodi po predviđenoj godišnjoj potražnji</returns>
        Task<List<DemandPredictionDto>> GetTopDemandProductsAsync(
            long? categoryId = null,
            int limit = 10,
            CancellationToken cancellationToken = default);
    }
}
