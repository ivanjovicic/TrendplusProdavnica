#nullable enable
using TrendplusProdavnica.Domain.Common;

namespace TrendplusProdavnica.Domain.Catalog
{
    public class ProductCategoryMap : EntityBase
    {
        public long ProductId { get; set; }
        public long CategoryId { get; set; }
        public int SortOrder { get; set; }
    }
}
