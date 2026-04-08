#nullable enable
using TrendplusProdavnica.Domain.Common;

namespace TrendplusProdavnica.Domain.Catalog
{
    public class ProductCollectionMap : EntityBase
    {
        public long ProductId { get; set; }
        public long CollectionId { get; set; }
        public int SortOrder { get; set; }
        public bool Pinned { get; set; }
        public decimal? MerchandisingScore { get; set; }
    }
}
