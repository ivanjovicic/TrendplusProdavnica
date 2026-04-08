#nullable enable
using TrendplusProdavnica.Domain.Common;

namespace TrendplusProdavnica.Domain.Pricing
{
    public class PromotionProduct : EntityBase
    {
        public long PromotionId { get; set; }
        public long ProductId { get; set; }
    }
}
