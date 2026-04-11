#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrendplusProdavnica.Domain.Merchandising;

namespace TrendplusProdavnica.Application.Merchandising.Services
{
    /// <summary>
    /// Servis za upravljanje merchandising pravilima
    /// </summary>
    public interface IMerchandisingService
    {
        /// <summary>
        /// Dohvata sva aktuelna pravila
        /// </summary>
        Task<IReadOnlyList<MerchandisingRule>> GetActiveRulesAsync(
            int? categoryId = null,
            int? brandId = null,
            bool useCache = true);

        /// <summary>
        /// Dohvata sva pravila (uključujući neaktivna)
        /// </summary>
        Task<IReadOnlyList<MerchandisingRule>> GetAllRulesAsync(bool useCache = true);

        /// <summary>
        /// Dohvata pravilo po ID-u
        /// </summary>
        Task<MerchandisingRule?> GetRuleByIdAsync(long ruleId);

        /// <summary>
        /// Kreira novo pravilo
        /// </summary>
        Task<MerchandisingRule> CreateRuleAsync(MerchandisingRuleCreateRequest request, long userId);

        /// <summary>
        /// Ažurira postojeće pravilo
        /// </summary>
        Task<MerchandisingRule> UpdateRuleAsync(long ruleId, MerchandisingRuleUpdateRequest request, long userId);

        /// <summary>
        /// Briše pravilo
        /// </summary>
        Task DeleteRuleAsync(long ruleId);

        /// <summary>
        /// Evaluira primjenjiva pravila za proizvode u listi
        /// Vraća mapu product ID -> boost score
        /// </summary>
        Task<Dictionary<long, decimal>> EvaluateRulesAsync(
            IEnumerable<RuleEvaluationInput> products,
            DateTimeOffset? evaluationTimeUtc = null);

        /// <summary>
        /// Invalidira cache pravila
        /// </summary>
        Task InvalidateCacheAsync();
    }

    /// <summary>
    /// Input za evaluiranje pravila
    /// </summary>
    public class RuleEvaluationInput
    {
        public long ProductId { get; set; }
        public long CategoryId { get; set; }
        public long BrandId { get; set; }
        public decimal CurrentScore { get; set; }
    }

    /// <summary>
    /// DTO za kreiranje pravila
    /// </summary>
    public class MerchandisingRuleCreateRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public short RuleType { get; set; } = 1; // MerchandisingRuleType
        public long? CategoryId { get; set; }
        public long? BrandId { get; set; }
        public long? ProductId { get; set; }
        public decimal BoostScore { get; set; }
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }
        public int Priority { get; set; } = 100;
    }

    /// <summary>
    /// DTO za ažuriranje pravila
    /// </summary>
    public class MerchandisingRuleUpdateRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public short? RuleType { get; set; }
        public long? CategoryId { get; set; }
        public long? BrandId { get; set; }
        public long? ProductId { get; set; }
        public decimal? BoostScore { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsActive { get; set; }
        public int? Priority { get; set; }
    }
}
