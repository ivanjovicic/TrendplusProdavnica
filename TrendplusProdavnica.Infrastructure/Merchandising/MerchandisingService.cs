#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using TrendplusProdavnica.Application.Merchandising.Services;
using TrendplusProdavnica.Domain.Merchandising;
using TrendplusProdavnica.Infrastructure.Persistence;
using ZiggyCreatures.Caching.Fusion;

namespace TrendplusProdavnica.Infrastructure.Merchandising
{
    /// <summary>
    /// Implementacija servisa za upravljanje merchandising pravilima
    /// </summary>
    public class MerchandisingService : IMerchandisingService
    {
        private readonly TrendplusDbContext _dbContext;
        private readonly IFusionCache _cache;
        private readonly ILogger<MerchandisingService> _logger;
        private readonly MerchandisingRuleEvaluator _evaluator;

        private const string CacheKey = "merchandising_rules_all";
        private const int CacheDurationMinutes = 30;

        public MerchandisingService(
            TrendplusDbContext dbContext,
            IFusionCache cache,
            ILogger<MerchandisingService> logger)
        {
            _dbContext = dbContext;
            _cache = cache;
            _logger = logger;
            _evaluator = new MerchandisingRuleEvaluator();
        }

        public async Task<IReadOnlyList<MerchandisingRule>> GetActiveRulesAsync(
            int? categoryId = null,
            int? brandId = null,
            bool useCache = true)
        {
            try
            {
                var allRules = await GetAllRulesAsync(useCache);
                var now = DateTimeOffset.UtcNow;

                var activeRules = allRules
                    .Where(r => r.IsValidAtTime(now))
                    .Where(r => categoryId == null || r.AppliesToCategory(categoryId.Value))
                    .Where(r => brandId == null || r.AppliesToBrand(brandId.Value))
                    .OrderByDescending(r => r.Priority)
                    .ThenByDescending(r => (int)r.RuleType) // Pin > Boost > Demote
                    .ToList();

                return activeRules;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri dohvati aktivnih merchandising pravila");
                return new List<MerchandisingRule>();
            }
        }

        public async Task<IReadOnlyList<MerchandisingRule>> GetAllRulesAsync(bool useCache = true)
        {
            try
            {
                if (useCache)
                {
                    return await _cache.GetOrSetAsync(
                        CacheKey,
                        async (_) =>
                        {
                            var rules = await _dbContext.MerchandisingRules
                                .AsNoTracking()
                                .ToListAsync();

                            return (IReadOnlyList<MerchandisingRule>)rules;
                        },
                        new FusionCacheEntryOptions 
                        { 
                            Duration = TimeSpan.FromMinutes(CacheDurationMinutes) 
                        });
                }

                var allRules = await _dbContext.MerchandisingRules
                    .AsNoTracking()
                    .ToListAsync();

                return allRules;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri dohvati svih merchandising pravila");
                return new List<MerchandisingRule>();
            }
        }

        public async Task<MerchandisingRule?> GetRuleByIdAsync(long ruleId)
        {
            try
            {
                return await _dbContext.MerchandisingRules
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == ruleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri dohvati merchandising pravila sa ID: {RuleId}", ruleId);
                return null;
            }
        }

        public async Task<MerchandisingRule> CreateRuleAsync(
            MerchandisingRuleCreateRequest request,
            long userId)
        {
            try
            {
                var rule = new MerchandisingRule(
                    request.Name,
                    (Domain.Enums.MerchandisingRuleType)request.RuleType,
                    request.BoostScore,
                    userId)
                {
                    Description = request.Description,
                    CategoryId = request.CategoryId,
                    BrandId = request.BrandId,
                    ProductId = request.ProductId,
                    StartDateUtc = new DateTimeOffset(request.StartDate, TimeSpan.Zero),
                    EndDateUtc = request.EndDate.HasValue 
                        ? new DateTimeOffset(request.EndDate.Value, TimeSpan.Zero) 
                        : null,
                    Priority = request.Priority
                };

                _dbContext.MerchandisingRules.Add(rule);
                await _dbContext.SaveChangesAsync();

                await InvalidateCacheAsync();

                _logger.LogInformation(
                    "Kreirano novo merchandising pravilo: {RuleName} (ID: {RuleId}) od strane korisnika {UserId}",
                    rule.Name,
                    rule.Id,
                    userId);

                return rule;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri kreiranju merchandising pravila: {RuleName}", request.Name);
                throw;
            }
        }

        public async Task<MerchandisingRule> UpdateRuleAsync(
            long ruleId,
            MerchandisingRuleUpdateRequest request,
            long userId)
        {
            try
            {
                var rule = await _dbContext.MerchandisingRules
                    .FirstOrDefaultAsync(r => r.Id == ruleId)
                    ?? throw new KeyNotFoundException($"Pravilo sa ID {ruleId} nije pronađeno");

                if (!string.IsNullOrEmpty(request.Name))
                    rule.Name = request.Name;

                if (request.Description != null)
                    rule.Description = request.Description;

                if (request.RuleType.HasValue)
                    rule.RuleType = (Domain.Enums.MerchandisingRuleType)request.RuleType.Value;

                if (request.CategoryId != null)
                    rule.CategoryId = request.CategoryId <= 0 ? null : request.CategoryId;

                if (request.BrandId != null)
                    rule.BrandId = request.BrandId <= 0 ? null : request.BrandId;

                if (request.ProductId != null)
                    rule.ProductId = request.ProductId <= 0 ? null : request.ProductId;

                if (request.BoostScore.HasValue)
                    rule.BoostScore = request.BoostScore.Value;

                if (request.StartDate.HasValue)
                    rule.StartDateUtc = new DateTimeOffset(request.StartDate.Value, TimeSpan.Zero);

                if (request.EndDate != null)
                    rule.EndDateUtc = new DateTimeOffset(request.EndDate.Value, TimeSpan.Zero);

                if (request.IsActive.HasValue)
                    rule.IsActive = request.IsActive.Value;

                if (request.Priority.HasValue)
                    rule.Priority = request.Priority.Value;

                rule.UpdatedByUserId = userId;
                rule.UpdatedAtUtc = DateTimeOffset.UtcNow;

                _dbContext.MerchandisingRules.Update(rule);
                await _dbContext.SaveChangesAsync();

                await InvalidateCacheAsync();

                _logger.LogInformation(
                    "Ažurirano merchandising pravilo: {RuleName} (ID: {RuleId}) od strane korisnika {UserId}",
                    rule.Name,
                    rule.Id,
                    userId);

                return rule;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri ažuriranju merchandising pravila sa ID: {RuleId}", ruleId);
                throw;
            }
        }

        public async Task DeleteRuleAsync(long ruleId)
        {
            try
            {
                var rule = await _dbContext.MerchandisingRules
                    .FirstOrDefaultAsync(r => r.Id == ruleId)
                    ?? throw new KeyNotFoundException($"Pravilo sa ID {ruleId} nije pronađeno");

                _dbContext.MerchandisingRules.Remove(rule);
                await _dbContext.SaveChangesAsync();

                await InvalidateCacheAsync();

                _logger.LogInformation(
                    "Obrisano merchandising pravilo: {RuleName} (ID: {RuleId})",
                    rule.Name,
                    rule.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri brisanju merchandising pravila sa ID: {RuleId}", ruleId);
                throw;
            }
        }

        public async Task<Dictionary<long, decimal>> EvaluateRulesAsync(
            IEnumerable<RuleEvaluationInput> products,
            DateTimeOffset? evaluationTimeUtc = null)
        {
            try
            {
                evaluationTimeUtc ??= DateTimeOffset.UtcNow;
                var rules = await GetActiveRulesAsync(useCache: true);

                var result = _evaluator.Evaluate(products, rules, evaluationTimeUtc.Value);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri evaluaciji merchandising pravila");
                return new Dictionary<long, decimal>();
            }
        }

        public async Task InvalidateCacheAsync()
        {
            try
            {
                await _cache.RemoveAsync(CacheKey);
                _logger.LogInformation("Invalidiran cache merchandising pravila");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Greška pri invalidaciji cache-a merchandising pravila");
            }
        }
    }
}
