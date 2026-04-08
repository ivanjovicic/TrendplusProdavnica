#nullable enable
using System.Collections.Generic;
using TrendplusProdavnica.Domain.Common;

namespace TrendplusProdavnica.Domain.Catalog
{
    public class SizeGuide : AggregateRoot
    {
        public long? BrandId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; } = true;

        public IList<SizeGuideRow> Rows { get; } = new List<SizeGuideRow>();
    }
}
