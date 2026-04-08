#nullable enable
using TrendplusProdavnica.Domain.Common;

namespace TrendplusProdavnica.Domain.Pricing
{
    public class PromotionCategory : EntityBase
    {
        public long PromotionId { get; set; }
        public long CategoryId { get; set; }
    }
}
