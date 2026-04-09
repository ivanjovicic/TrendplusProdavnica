#nullable enable
using System;
using System.Collections.Generic;
using TrendplusProdavnica.Domain.Common;
using TrendplusProdavnica.Domain.ValueObjects;
using TrendplusProdavnica.Domain.Enums;

namespace TrendplusProdavnica.Domain.Catalog
{
    public class Product : AggregateRoot
    {
        public long BrandId { get; set; }
        public long PrimaryCategoryId { get; set; }
        public long? SizeGuideId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string ShortDescription { get; set; } = string.Empty;
        public string? LongDescription { get; set; }
        public string? PrimaryColorName { get; set; }
        public string? StyleTag { get; set; }
        public string? OccasionTag { get; set; }
        public string? SeasonTag { get; set; }

        public ProductStatus Status { get; set; } = ProductStatus.Draft;
        public bool IsVisible { get; set; } = true;
        public bool IsPurchasable { get; set; } = true;
        public bool IsNew { get; set; }
        public bool IsBestseller { get; set; }
        public int SortRank { get; set; }

        public string? SearchKeywords { get; set; }
        public string[]? SearchSynonyms { get; set; }
        public bool SearchHidden { get; set; }

        public SeoMetadata? Seo { get; set; }
        public DateTimeOffset? PublishedAtUtc { get; set; }

        // Navigation
        public Brand? Brand { get; set; }

        public IList<ProductVariant> Variants { get; } = new List<ProductVariant>();
        public IList<ProductMedia> Media { get; } = new List<ProductMedia>();
        public IList<ProductCategoryMap> CategoryMaps { get; } = new List<ProductCategoryMap>();
        public IList<ProductCollectionMap> CollectionMaps { get; } = new List<ProductCollectionMap>();
        public IList<ProductRelatedProduct> RelatedProducts { get; } = new List<ProductRelatedProduct>();
    }
}
