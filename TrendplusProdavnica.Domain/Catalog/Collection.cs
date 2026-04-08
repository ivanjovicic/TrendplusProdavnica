#nullable enable
using System;
using System.Collections.Generic;
using TrendplusProdavnica.Domain.Common;
using TrendplusProdavnica.Domain.ValueObjects;
using TrendplusProdavnica.Domain.Enums;

namespace TrendplusProdavnica.Domain.Catalog
{
    public class Collection : AggregateRoot
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public CollectionType CollectionType { get; set; } = CollectionType.Manual;
        public string? ShortDescription { get; set; }
        public string? LongDescription { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? ThumbnailImageUrl { get; set; }
        public string? BadgeText { get; set; }
        public DateTimeOffset? StartAtUtc { get; set; }
        public DateTimeOffset? EndAtUtc { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
        public SeoMetadata? Seo { get; set; }

        public IList<ProductCollectionMap> ProductMaps { get; } = new List<ProductCollectionMap>();
    }
}
