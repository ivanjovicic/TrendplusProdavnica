#nullable enable
using System;
using System.Collections.Generic;

namespace TrendplusProdavnica.Application.Catalog.Dtos
{
    public record ProductAggregateRatingDto(
        decimal RatingValue,
        int ReviewCount,
        int? RatingCount,
        decimal BestRating,
        decimal WorstRating);

    public record ProductReviewDto(
        string? AuthorName,
        string? Title,
        string? ReviewBody,
        decimal RatingValue,
        DateTimeOffset? PublishedAtUtc);

    public record ListingSortOptionDto(string Label, string Value);

    public record ListingPriceRangeDto(decimal? Min, decimal? Max);

    public record ListingAvailableFiltersDto(
        string[] Brands,
        string[] Colors,
        decimal[] Sizes,
        ListingPriceRangeDto PriceRange);

    public record ProductListingPageDto(
        string Title,
        string Slug,
        string? Intro,
        BreadcrumbItemDto[] Breadcrumbs,
        ProductCardDto[] Products,
        long TotalCount,
        int Page,
        int PageSize,
        ListingSortOptionDto[] SortOptions,
        ListingAvailableFiltersDto AvailableFilters,
        object[] MerchBlocks,
        object? Faq,
        SeoDto Seo
    );

    public record ProductDetailDto(
        long Id,
        string Slug,
        long BrandId,
        string BrandSlug,
        string BrandName,
        long CategoryId,
        string CategorySlug,
        string CategoryName,
        string Name,
        string? Subtitle,
        string ShortDescription,
        string? LongDescription,
        string? PrimaryColorName,
        string? SecondaryColorName,
        long? SizeGuideId,
        string? Sku,
        string? Mpn,
        string? Gtin,
        string? Gtin13,
        decimal Price,
        decimal? OldPrice,
        string Currency,
        bool IsNew,
        bool IsBestseller,
        bool IsOnSale,
        string[] Badges,
        BreadcrumbItemDto[] Breadcrumbs,
        ProductMediaDto[] Media,
        ProductSizeOptionDto[] Sizes,
        object? StoreAvailabilitySummary,
        ProductCardDto[] RelatedByBrand,
        ProductCardDto[] RelatedBySimilarity,
        ProductCardDto[] RelatedProducts,
        ProductCardDto[] SimilarProducts,
        SeoDto Seo,
        string DeliveryInfo,
        string ReturnInfo,
        string? CareInstructions,
        object? SizeGuide,
        ProductAggregateRatingDto? AggregateRating,
        decimal? AverageRating,
        int? ReviewCount,
        int? RatingCount,
        ProductReviewDto[] Reviews
    );
}
