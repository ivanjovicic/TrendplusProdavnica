#nullable enable
using TrendplusProdavnica.Domain.Common;

namespace TrendplusProdavnica.Domain.Pricing
{
    public class PromotionCollection : EntityBase
    {
        public long PromotionId { get; set; }
        public long CollectionId { get; set; }
    }
}
