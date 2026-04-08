#nullable enable
using TrendplusProdavnica.Domain.Common;

namespace TrendplusProdavnica.Domain.Pricing
{
    public class PromotionBrand : EntityBase
    {
        public long PromotionId { get; set; }
        public long BrandId { get; set; }
    }
}
