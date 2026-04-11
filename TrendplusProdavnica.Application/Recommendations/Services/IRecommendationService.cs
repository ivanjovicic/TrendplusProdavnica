#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Recommendations.Dtos;

namespace TrendplusProdavnica.Application.Recommendations.Services
{
    /// <summary>
    /// Servis za preporuke produkata
    /// </summary>
    public interface IRecommendationService
    {
        /// <summary>
        /// Dohvata preporučene proizvode za specifičan proizvod
        /// </summary>
        Task<RecommendationResponse> GetRecommendationsAsync(long productId, int limit = 8, RecommendationType type = RecommendationType.RelatedProducts, CancellationToken cancellationToken = default);

        /// <summary>
        /// Dohvata preporučene proizvode za homepage
        /// </summary>
        Task<RecommendationResponse> GetHomepageRecommendationsAsync(int limit = 8, CancellationToken cancellationToken = default);

        /// <summary>
        /// Invalidira cache za proizvod
        /// </summary>
        Task InvalidateCacheAsync(long productId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Dohvata scoring detalje (za debug)
        /// </summary>
        Task<List<ProductScoringDetails>> GetScoringDetailsAsync(long productId, CancellationToken cancellationToken = default);
    }
}
