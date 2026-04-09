#nullable enable
using System.Collections.Generic;

namespace TrendplusProdavnica.Application.Catalog.Listing
{
    public sealed record ProductListingResponse(
        IReadOnlyList<ProductCardDto> Products,
        long TotalCount,
        int Page,
        int PageSize,
        ProductListingFacets Facets,
        string CanonicalUrl);

    public sealed record ProductListingFacets(
        IReadOnlyList<BrandFacetItem> Brands,
        IReadOnlyList<SizeFacetItem> Sizes,
        IReadOnlyList<ColorFacetItem> Colors,
        PriceRangeFacet PriceRange);

    public sealed record BrandFacetItem(long BrandId, string BrandName, int Count);

    public sealed record SizeFacetItem(decimal Size, int Count);

    public sealed record ColorFacetItem(string Color, int Count);

    public sealed record PriceRangeFacet(decimal? Min, decimal? Max);
}
