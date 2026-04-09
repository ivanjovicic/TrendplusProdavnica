#nullable enable
using System;

namespace TrendplusProdavnica.Infrastructure.Search
{
    public sealed class ProductSearchDocument
    {
        public long ProductId { get; init; }
        public string Slug { get; init; } = string.Empty;
        public string BrandName { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string? ShortDescription { get; init; }
        public string? PrimaryCategory { get; init; }
        public string[] SecondaryCategories { get; init; } = Array.Empty<string>();
        public string? PrimaryColorName { get; init; }
        public bool IsNew { get; init; }
        public bool IsBestseller { get; init; }
        public bool IsOnSale { get; init; }
        public double? MinPrice { get; init; }
        public double? MaxPrice { get; init; }
        public double[] AvailableSizes { get; init; } = Array.Empty<double>();
        public bool InStock { get; init; }
        public string? PrimaryImageUrl { get; init; }
        public string[] SearchKeywords { get; init; } = Array.Empty<string>();
        public int SortRank { get; init; }
        public DateTimeOffset? PublishedAtUtc { get; init; }
    }
}
