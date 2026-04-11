#nullable enable
using System;
using TrendplusProdavnica.Domain.Common;
using TrendplusProdavnica.Domain.Enums;

namespace TrendplusProdavnica.Domain.Merchandising
{
    /// <summary>
    /// Pravilo za merchandising - ručno upravljanje redosledom proizvoda
    /// </summary>
    public class MerchandisingRule : AggregateRoot
    {
        // Identifikacija
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Tip pravila
        public MerchandisingRuleType RuleType { get; set; }

        // Targeting
        public long? CategoryId { get; set; }
        public long? BrandId { get; set; }
        public long? ProductId { get; set; }

        // Scoring
        public decimal BoostScore { get; set; }

        // Validnost
        public DateTimeOffset StartDateUtc { get; set; }
        public DateTimeOffset? EndDateUtc { get; set; }

        // Status
        public bool IsActive { get; set; } = true;
        public int Priority { get; set; } = 100; // Viši number = veća prioriteta

        // Auditovanje
        public long CreatedByUserId { get; set; }
        public long? UpdatedByUserId { get; set; }

        public MerchandisingRule() { }

        public MerchandisingRule(
            string name,
            MerchandisingRuleType ruleType,
            decimal boostScore,
            long createdByUserId)
        {
            Name = name;
            RuleType = ruleType;
            BoostScore = boostScore;
            CreatedByUserId = createdByUserId;
            StartDateUtc = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Provjerava da li je pravilo aktivno u datom vremenu
        /// </summary>
        public bool IsValidAtTime(DateTimeOffset timeUtc)
        {
            if (!IsActive)
                return false;

            if (timeUtc < StartDateUtc)
                return false;

            if (EndDateUtc.HasValue && timeUtc > EndDateUtc.Value)
                return false;

            return true;
        }

        /// <summary>
        /// Provjerava da li se pravilo primjenjuje na proizvod u kategoriji
        /// </summary>
        public bool AppliesToCategory(long categoryId)
        {
            if (!CategoryId.HasValue)
                return true; // Nema filtera = primjenjuje se na sve

            return CategoryId == categoryId;
        }

        /// <summary>
        /// Provjerava da li se pravilo primjenjuje na proizvod određene marke
        /// </summary>
        public bool AppliesToBrand(long brandId)
        {
            if (!BrandId.HasValue)
                return true; // Nema filtera = primjenjuje se na sve

            return BrandId == brandId;
        }

        /// <summary>
        /// Provjerava da li se pravilo primjenjuje direktno na proizvod
        /// </summary>
        public bool AppliesToProduct(long productId)
        {
            if (!ProductId.HasValue)
                return false; // Pin pravilo mora biti specifično za proizvod

            return ProductId == productId;
        }
    }
}
