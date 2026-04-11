#nullable enable
using System;

namespace TrendplusProdavnica.Application.Merchandising.Services
{
    /// <summary>
    /// DTO za prikaz merchandising pravila
    /// </summary>
    public class MerchandisingRuleDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public short RuleType { get; set; }
        public string RuleTypeName { get; set; } = string.Empty;
        public long? CategoryId { get; set; }
        public long? BrandId { get; set; }
        public long? ProductId { get; set; }
        public decimal BoostScore { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public int Priority { get; set; }

        // Auditovanje
        public long CreatedByUserId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public long? UpdatedByUserId { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }
}
