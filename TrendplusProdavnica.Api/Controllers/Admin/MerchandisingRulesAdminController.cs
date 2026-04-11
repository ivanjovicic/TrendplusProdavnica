#nullable enable
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrendplusProdavnica.Api.Infrastructure.Auth;
using TrendplusProdavnica.Application.Merchandising.Services;

namespace TrendplusProdavnica.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/merchandising")]
    [Authorize(Policy = ApiAuthorizationPolicies.Admin)]
    public class MerchandisingRulesAdminController : ControllerBase
    {
        private readonly IMerchandisingService _merchandisingService;
        private readonly ILogger<MerchandisingRulesAdminController> _logger;

        public MerchandisingRulesAdminController(
            IMerchandisingService merchandisingService,
            ILogger<MerchandisingRulesAdminController> logger)
        {
            _merchandisingService = merchandisingService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<MerchandisingRuleDto>>> GetAll(
            [FromQuery] int? categoryId,
            [FromQuery] int? brandId,
            [FromQuery] bool? onlyActive)
        {
            try
            {
                IReadOnlyList<TrendplusProdavnica.Domain.Merchandising.MerchandisingRule> rules =
                    onlyActive == true
                        ? await _merchandisingService.GetActiveRulesAsync(categoryId, brandId)
                        : await _merchandisingService.GetAllRulesAsync();

                var dtos = rules.Select(MapToDto).ToList();
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greska pri dohvatu merchandising pravila");
                return StatusCode(500, new { error = "Greska pri ucitavanju pravila" });
            }
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<MerchandisingRuleDto>> GetById(long id)
        {
            try
            {
                var rule = await _merchandisingService.GetRuleByIdAsync(id);
                if (rule == null)
                {
                    return NotFound(new { error = "Pravilo nije pronadjeno" });
                }

                return Ok(MapToDto(rule));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greska pri dohvatu merchandising pravila sa ID: {RuleId}", id);
                return StatusCode(500, new { error = "Greska pri ucitavanju pravila" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<MerchandisingRuleDto>> Create([FromBody] MerchandisingRuleCreateRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var rule = await _merchandisingService.CreateRuleAsync(request, userId);
                return CreatedAtAction(nameof(GetById), new { id = rule.Id }, MapToDto(rule));
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { error = "Admin korisnik nije autentifikovan." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greska pri kreiranju merchandising pravila");
                return StatusCode(500, new { error = "Greska pri kreiranju pravila" });
            }
        }

        [HttpPut("{id:long}")]
        public async Task<ActionResult<MerchandisingRuleDto>> Update(
            long id,
            [FromBody] MerchandisingRuleUpdateRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var rule = await _merchandisingService.UpdateRuleAsync(id, request, userId);
                return Ok(MapToDto(rule));
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { error = "Admin korisnik nije autentifikovan." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "Pravilo nije pronadjeno" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greska pri azuriranju merchandising pravila sa ID: {RuleId}", id);
                return StatusCode(500, new { error = "Greska pri azuriranju pravila" });
            }
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var rule = await _merchandisingService.GetRuleByIdAsync(id);
                if (rule == null)
                {
                    return NotFound(new { error = "Pravilo nije pronadjeno" });
                }

                await _merchandisingService.DeleteRuleAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greska pri brisanju merchandising pravila sa ID: {RuleId}", id);
                return StatusCode(500, new { error = "Greska pri brisanju pravila" });
            }
        }

        [HttpPost("cache/invalidate")]
        public async Task<IActionResult> InvalidateCache()
        {
            try
            {
                await _merchandisingService.InvalidateCacheAsync();
                return Ok(new { message = "Cache je invalidiran" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greska pri invalidaciji cache-a");
                return StatusCode(500, new { error = "Greska pri invalidaciji cache-a" });
            }
        }

        private static MerchandisingRuleDto MapToDto(TrendplusProdavnica.Domain.Merchandising.MerchandisingRule rule)
        {
            return new MerchandisingRuleDto
            {
                Id = rule.Id,
                Name = rule.Name,
                Description = rule.Description,
                RuleType = (short)rule.RuleType,
                RuleTypeName = rule.RuleType.ToString(),
                CategoryId = rule.CategoryId,
                BrandId = rule.BrandId,
                ProductId = rule.ProductId,
                BoostScore = rule.BoostScore,
                StartDate = rule.StartDateUtc.UtcDateTime,
                EndDate = rule.EndDateUtc?.UtcDateTime,
                IsActive = rule.IsActive,
                Priority = rule.Priority,
                CreatedByUserId = rule.CreatedByUserId,
                CreatedAtUtc = rule.CreatedAtUtc.UtcDateTime,
                UpdatedByUserId = rule.UpdatedByUserId,
                UpdatedAtUtc = rule.UpdatedAtUtc.UtcDateTime
            };
        }

        private long GetCurrentUserId()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier) ??
                User.FindFirst("sub") ??
                User.FindFirst("userid");

            if (long.TryParse(userIdClaim?.Value, out var userId))
            {
                return userId;
            }

            throw new UnauthorizedAccessException("Authenticated admin user id claim is missing.");
        }
    }
}
