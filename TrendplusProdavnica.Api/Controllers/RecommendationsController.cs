#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrendplusProdavnica.Api.Infrastructure.Auth;
using TrendplusProdavnica.Application.Recommendations.Dtos;
using TrendplusProdavnica.Application.Recommendations.Services;

namespace TrendplusProdavnica.Api.Controllers
{
    [ApiController]
    [Route("api/recommendations")]
    public class RecommendationsController : ControllerBase
    {
        private readonly IRecommendationService _recommendationService;

        public RecommendationsController(IRecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }

        /// <summary>
        /// Dohvata preporučene proizvode za specifičan proizvod
        /// </summary>
        /// <remarks>
        /// Vraća liste povezanih, bestselera, trendingovog i novih proizvoda za trenutni proizvod
        /// </remarks>
        [HttpGet("product/{productId:long}")]
        [ProducesResponseType(typeof(RecommendationResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<RecommendationResponse>> GetProductRecommendations(
            long productId,
            [FromQuery] int limit = 8,
            [FromQuery] RecommendationType type = RecommendationType.RelatedProducts,
            CancellationToken cancellationToken = default)
        {
            if (productId <= 0)
                return BadRequest(new { error = "Invalid product ID" });

            if (limit < 1 || limit > 50)
                return BadRequest(new { error = "Limit must be between 1 and 50" });

            var recommendations = await _recommendationService.GetRecommendationsAsync(
                productId, limit, type, cancellationToken);

            if (recommendations == null || recommendations.Items.Count == 0)
                return NotFound(new { message = "No recommendations found" });

            return Ok(recommendations);
        }

        /// <summary>
        /// Dohvata preporučene proizvode za homepage
        /// </summary>
        /// <remarks>
        /// Vraća bestseller-e i top-rated proizvode za početnu stranicu
        /// </remarks>
        [HttpGet("homepage")]
        [ProducesResponseType(typeof(RecommendationResponse), 200)]
        public async Task<ActionResult<RecommendationResponse>> GetHomepageRecommendations(
            [FromQuery] int limit = 8,
            CancellationToken cancellationToken = default)
        {
            if (limit < 1 || limit > 50)
                return BadRequest(new { error = "Limit must be between 1 and 50" });

            var recommendations = await _recommendationService.GetHomepageRecommendationsAsync(
                limit, cancellationToken);

            return Ok(recommendations);
        }

        /// <summary>
        /// Dohvata scoring detalje produkata (ADMIN - za debug)
        /// </summary>
        [Authorize(Policy = ApiAuthorizationPolicies.Operational)]
        [HttpGet("debug/scoring/{productId:long}")]
        [ProducesResponseType(typeof(List<ProductScoringDetails>), 200)]
        public async Task<ActionResult<List<ProductScoringDetails>>> GetScoringDetails(
            long productId,
            CancellationToken cancellationToken = default)
        {
            if (productId <= 0)
                return BadRequest(new { error = "Invalid product ID" });

            var scoringDetails = await _recommendationService.GetScoringDetailsAsync(
                productId, cancellationToken);

            return Ok(scoringDetails);
        }

        /// <summary>
        /// Invalidira cache za proizvod (INTERNAL)
        /// </summary>
        [Authorize(Policy = ApiAuthorizationPolicies.Operational)]
        [HttpPost("admin/invalidate-cache/{productId:long}")]
        [ProducesResponseType(200)]
        public async Task<ActionResult> InvalidateCache(
            long productId,
            CancellationToken cancellationToken = default)
        {
            if (productId <= 0)
                return BadRequest(new { error = "Invalid product ID" });

            await _recommendationService.InvalidateCacheAsync(productId, cancellationToken);
            return Ok(new { message = "Cache invalidated" });
        }
    }
}
