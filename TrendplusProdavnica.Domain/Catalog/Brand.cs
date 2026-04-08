#nullable enable
using System.Collections.Generic;
using TrendplusProdavnica.Domain.Common;
using TrendplusProdavnica.Domain.ValueObjects;

namespace TrendplusProdavnica.Domain.Catalog
{
    public class Brand : AggregateRoot
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string? LongDescription { get; set; }
        public string? LogoUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? WebsiteUrl { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
        public SeoMetadata? Seo { get; set; }

        public IList<Collection> Collections { get; } = new List<Collection>();
    }
}
