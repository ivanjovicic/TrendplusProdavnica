#nullable enable
using System;
using System.Collections.Generic;

namespace TrendplusProdavnica.Application.Catalog.Dtos
{
    public record BreadcrumbItemDto(string Label, string Url);

    public record ProductMediaDto(
        string Url,
        string? MobileUrl,
        string? AltText,
        string? Title,
        string MediaType,
        string MediaRole,
        int SortOrder,
        bool IsPrimary
    );

    public record ProductSizeOptionDto(
        long VariantId,
        decimal SizeEu,
        string Label,
        bool IsInStock,
        bool IsLowStock,
        int TotalStock,
        decimal Price,
        decimal? OldPrice
    );

    public record ProductCardDto(
        long Id,
        string Slug,
        string BrandName,
        string Name,
        string PrimaryImageUrl,
        string? SecondaryImageUrl,
        decimal Price,
        decimal? OldPrice,
        string Currency,
        string[] Badges,
        bool IsInStock,
        int AvailableSizesCount,
        string? ColorLabel
    );

    public record PaginationDto(int Page, int PageSize, long TotalItems)
    {
        public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling((double)TotalItems / PageSize);
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;
    }

    public record FilterOptionDto(string Value, string Label, int Count, bool Selected, bool Disabled);
    public record FilterFacetDto(string Key, string Label, string Type, FilterOptionDto[] Options);
    public record AppliedFilterDto(string Key, string Label, string Value, string DisplayValue);

    /// <summary>
    /// SEO metadata DTO
    /// </summary>
    public record SeoDto(string Title, string Description, string? CanonicalUrl, string[]? Keywords);

    /// <summary>
    /// Category card for home page and dynamic listings
    /// </summary>
    public record CategoryCardDto(string Name, string Slug, string? ImageUrl);

    /// <summary>
    /// Collection teaser for home page featured collections
    /// </summary>
    public record CollectionTeaserDto(string Name, string Slug, string? CoverImageUrl, string? Description);

    /// <summary>
    /// Brand wall item for home page brand section
    /// </summary>
    public record BrandWallItemDto(string BrandName, string Slug, string? LogoUrl);

    /// <summary>
    /// Announcement bar for home page top section
    /// </summary>
    public record AnnouncementBarDto(string Text, string? BackgroundColor, string? TextColor, string? CallToActionUrl);

    /// <summary>
    /// Hero section for home page
    /// </summary>
    public record HeroSectionDto(string Title, string Subtitle, string ImageUrl);

    /// <summary>
    /// Editorial statement for home page
    /// </summary>
    public record EditorialStatementDto(string Title, string Text);

    /// <summary>
    /// Store teaser for home page
    /// </summary>
    public record StoreTeaserDto(string Name, string Slug, string CoverImageUrl);

    /// <summary>
    /// Trust/confidence item for home page
    /// </summary>
    public record TrustItemDto(string Title, string Description);

    /// <summary>
    /// Newsletter subscription form for home page
    /// </summary>
    public record NewsletterDto(string Title, string Placeholder);

    /// <summary>
    /// Home page DTO with all sections
    /// </summary>
    public record HomePageDto(
        SeoDto Seo,
        AnnouncementBarDto? AnnouncementBar,
        HeroSectionDto HeroSection,
        CategoryCardDto[] CategoryCards,
        ProductCardDto[] NewArrivals,
        CollectionTeaserDto[] FeaturedCollections,
        ProductCardDto[] Bestsellers,
        BrandWallItemDto[] BrandWall,
        EditorialStatementDto? EditorialStatement,
        StoreTeaserDto? StoreTeaser,
        TrustItemDto[] TrustItems,
        NewsletterDto? Newsletter
    );
}
