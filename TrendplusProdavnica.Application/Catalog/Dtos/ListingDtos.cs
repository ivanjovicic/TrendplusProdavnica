#nullable enable
using System.Collections.Generic;

namespace TrendplusProdavnica.Application.Catalog.Dtos
{
    public record HomePageDto(
        SeoDto Seo,
        string? AnnouncementBar,
        HeroSectionDto HeroSection,
        ProductCardDto[] CategoryCards,
        ProductCardDto[] NewArrivals,
        ProductCardDto[] FeaturedCollections,
        ProductCardDto[] Bestsellers,
        string[] BrandWall,
        EditorialStatementDto? EditorialStatement,
        StoreTeaserDto? StoreTeaser,
        TrustItemDto[] TrustItems,
        NewsletterDto? Newsletter
    );

    public record HeroSectionDto(string Title, string Subtitle, string ImageUrl);
    public record EditorialStatementDto(string Title, string Text);
    public record StoreTeaserDto(string Name, string Slug, string CoverImageUrl);
    public record TrustItemDto(string Title, string Description);
    public record NewsletterDto(string Title, string Placeholder);

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

    public record SeoDto(string Title, string Description, string? CanonicalUrl, string[]? Keywords);
}
