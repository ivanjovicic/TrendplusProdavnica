#nullable enable
using TrendplusProdavnica.Domain.Common;

namespace TrendplusProdavnica.Domain.Catalog
{
    public class SizeGuideRow : EntityBase
    {
        public long SizeGuideId { get; set; }
        public decimal EuSize { get; set; }
        public decimal? FootLengthMinMm { get; set; }
        public decimal? FootLengthMaxMm { get; set; }
        public string? Note { get; set; }
        public int SortOrder { get; set; }
    }
}
