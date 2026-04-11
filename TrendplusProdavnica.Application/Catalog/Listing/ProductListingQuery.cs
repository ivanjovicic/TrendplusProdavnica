#nullable enable
using System.Collections.Generic;

namespace TrendplusProdavnica.Application.Catalog.Listing
{
    public sealed record ProductListingQuery(
        string? CategorySlug,
        string? BrandSlug,
        string? CollectionSlug,
        decimal? MinPrice,
        decimal? MaxPrice,
        IReadOnlyList<decimal>? Sizes,
        IReadOnlyList<string>? Colors,
        IReadOnlyList<long>? Brands,
        bool? IsOnSale,
        bool? IsNew,
        bool? InStockOnly,
        int Page = 1,
        int PageSize = 24,
        string? Sort = "popular");
}
