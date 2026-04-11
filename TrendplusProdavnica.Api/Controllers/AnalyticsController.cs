#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrendplusProdavnica.Api.Infrastructure.Auth;
using TrendplusProdavnica.Application.Analytics.DTOs;
using TrendplusProdavnica.Application.Analytics.Services;
using TrendplusProdavnica.Domain.Analytics;
using Microsoft.Extensions.Logging;

namespace TrendplusProdavnica.Api.Controllers
{
    [ApiController]
    [Route("api/analytics")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly IDemandPredictionService _demandPredictionService;
        private readonly ILogger<AnalyticsController> _logger;

        public AnalyticsController(
            IAnalyticsService analyticsService,
            IDemandPredictionService demandPredictionService,
            ILogger<AnalyticsController> logger)
        {
            _analyticsService = analyticsService;
           _demandPredictionService = demandPredictionService;
            _logger = logger;
        }

        [HttpPost("track")]
        [AllowAnonymous]
        public async Task<ActionResult<AnalyticsEventDto>> TrackEvent(
            [FromBody] CreateAnalyticsEventRequest request)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value != null
                    ? (long?)long.Parse(User.FindFirst("sub")!.Value)
                    : null;

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = Request.Headers["User-Agent"].ToString();

                var eventDto = await _analyticsService.TrackEventAsync(
                    request, userId, ipAddress, userAgent);

                return Ok(eventDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Analytics error");
                return StatusCode(500, new { error = "Analytics error" });
            }
        }

        [HttpPost("demand-prediction/predict")]
        [Authorize(Policy = ApiAuthorizationPolicies.Admin)]
        public async Task<ActionResult<DemandPredictionDto>> PredictDemand(
            [FromBody] DemandPredictionRequest request)
        {
            try
            {
                var prediction = await _demandPredictionService.PredictDemandAsync(request);
                return Ok(prediction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Demand prediction error");
                return StatusCode(500, new { error = "Demand prediction error" });
            }
        }

        [HttpPost("demand-prediction/predict-bulk")]
        [Authorize(Policy = ApiAuthorizationPolicies.Admin)]
        public async Task<ActionResult<BulkDemandPredictionResponse>> PredictDemandBulk(
            [FromBody] BulkDemandPredictionRequest request)
        {
            try
            {
                var response = await _demandPredictionService.PredictDemandBulkAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bulk prediction error");
                return StatusCode(500, new { error = "Bulk prediction error" });
            }
        }

        [HttpGet("demand-prediction/procurement/{productId}")]
        [Authorize(Policy = ApiAuthorizationPolicies.Admin)]
        public async Task<ActionResult<List<SizeDistributionData>>> GetProcurementRecommendations(
            long productId,
            [FromQuery] decimal safetyStockPercentage = 20m)
        {
            try
            {
                var recommendations = await _demandPredictionService
                    .GetProcurementRecommendationsAsync(productId, safetyStockPercentage);
                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Procurement error");
                return StatusCode(500, new { error = "Procurement error" });
            }
        }

        [HttpGet("demand-prediction/seasonality/{categoryId}")]
        [Authorize(Policy = ApiAuthorizationPolicies.Admin)]
        public async Task<ActionResult<List<SeasonalityData>>> GetCategorySeasonality(long categoryId)
        {
            try
            {
                var seasonality = await _demandPredictionService
                    .GetCategorySeasonalityAsync(categoryId);
                return Ok(seasonality);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Seasonality error");
                return StatusCode(500, new { error = "Seasonality error" });
            }
        }

        [HttpGet("demand-prediction/top-products")]
        [Authorize(Policy = ApiAuthorizationPolicies.Admin)]
        public async Task<ActionResult<List<DemandPredictionDto>>> GetTopDemandProducts(
            [FromQuery] long? categoryId = null,
            [FromQuery] int limit = 10)
        {
            try
            {
                var topProducts = await _demandPredictionService
                    .GetTopDemandProductsAsync(categoryId, limit);
                return Ok(topProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Top products error");
                return StatusCode(500, new { error = "Top products error" });
            }
        }
    }
}
