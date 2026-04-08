#nullable enable
using System;
using System.Collections.Generic;
using TrendplusProdavnica.Domain.Common;
using TrendplusProdavnica.Domain.Enums;

namespace TrendplusProdavnica.Domain.Pricing
{
    public class Promotion : AggregateRoot
    {
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public PromotionDiscountType DiscountType { get; set; } = PromotionDiscountType.Percent;
        public decimal DiscountValue { get; set; }
        public bool AppliesToSalePrice { get; set; }
        public string? BadgeText { get; set; }
        public short Priority { get; set; }
        public DateTimeOffset? StartsAtUtc { get; set; }
        public DateTimeOffset? EndsAtUtc { get; set; }
        public bool IsActive { get; set; } = true;

        public IList<PromotionProduct> Products { get; } = new List<PromotionProduct>();
        public IList<PromotionCategory> Categories { get; } = new List<PromotionCategory>();
        public IList<PromotionBrand> Brands { get; } = new List<PromotionBrand>();
        public IList<PromotionCollection> Collections { get; } = new List<PromotionCollection>();
    }
}
