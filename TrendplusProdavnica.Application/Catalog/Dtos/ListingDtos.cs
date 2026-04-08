#nullable enable
using System.Collections.Generic;

namespace TrendplusProdavnica.Application.Catalog.Dtos
{
    public record ProductListingPageDto(
        string Title,
        string Description,
        SeoDto Seo,
        BreadcrumbItemDto[] Breadcrumbs,
        string? IntroTitle,
        string? IntroText,
        ProductCardDto[] Products,
        FilterFacetDto[] Facets,
        AppliedFilterDto[] AppliedFilters,
        PaginationDto Pagination,
        object[] MerchBlocks,
        object? Faq
    );

    public record ProductDetailDto(
        long Id,
        string Slug,
        string BrandName,
        string Name,
        string? Subtitle,
        string ShortDescription,
        string? LongDescription,
        decimal Price,
        decimal? OldPrice,
        string Currency,
        string[] Badges,
        BreadcrumbItemDto[] Breadcrumbs,
        ProductMediaDto[] Media,
        ProductSizeOptionDto[] Sizes,
        object? StoreAvailabilitySummary,
        ProductCardDto[] RelatedProducts,
        ProductCardDto[] SimilarProducts,
        SeoDto Seo,
        string DeliveryInfo,
        string ReturnInfo,
        object? SizeGuide
    );
}
