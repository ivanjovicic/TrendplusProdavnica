#nullable enable
using TrendplusProdavnica.Domain.Common;
using TrendplusProdavnica.Domain.Enums;

namespace TrendplusProdavnica.Domain.Catalog
{
    public class ProductRelatedProduct : EntityBase
    {
        public long ProductId { get; set; }
        public long RelatedProductId { get; set; }
        public ProductRelationType RelationType { get; set; } = ProductRelationType.Similar;
        public int SortOrder { get; set; }
    }
}
