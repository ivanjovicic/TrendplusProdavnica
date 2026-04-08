#nullable enable
using System;
using System.Collections.Generic;

namespace TrendplusProdavnica.Application.Content.Dtos
{
    /// <summary>
    /// FAQ item DTO for pages
    /// </summary>
    public record FaqItemDto(string Question, string Answer);

    /// <summary>
    /// Merch block for collection/brand pages
    /// </summary>
    public record MerchBlockDto(string Title, string Html, string[] ProductSlugs);

    /// <summary>
    /// Category card for home page
    /// </summary>
    public record CategoryCardDto(string Name, string Slug, string? ImageUrl);

    /// <summary>
    /// Collection teaser for home page
    /// </summary>
    public record CollectionTeaserDto(string Name, string Slug, string? CoverImageUrl, string? Description);

    /// <summary>
    /// Brand wall item for home page
    /// </summary>
    public record BrandWallItemDto(string BrandName, string Slug, string? LogoUrl);

    /// <summary>
    /// Editorial article card
    /// </summary>
    public record EditorialArticleCardDto(string Title, string Slug, string Excerpt, string CoverImageUrl, DateTime PublishedAtUtc, string Topic);

    /// <summary>
    /// Full editorial article
    /// </summary>
    public record EditorialArticleDto(
        string Title,
        string Slug,
        string Excerpt,
        string CoverImageUrl,
        string Body,
        DateTime PublishedAtUtc,
        string Topic,
        string AuthorName,
        TrendplusProdavnica.Application.Catalog.Dtos.SeoDto Seo,
        long[] RelatedProducts,
        long[] RelatedCollections,
        long[] RelatedCategories,
        long[] RelatedArticles
    );

    /// <summary>
    /// Brand page DTO
    /// </summary>
    public record BrandPageDto(
        string BrandName,
        string Slug,
        string IntroText,
        TrendplusProdavnica.Application.Catalog.Dtos.SeoDto Seo,
        TrendplusProdavnica.Application.Catalog.Dtos.ProductCardDto[] FeaturedProducts,
        TrendplusProdavnica.Application.Catalog.Dtos.BreadcrumbItemDto[] CategoryLinks,
        FaqItemDto[]? Faq
    );

    /// <summary>
    /// Collection page DTO
    /// </summary>
    public record CollectionPageDto(
        string Name,
        string Slug,
        string IntroText,
        TrendplusProdavnica.Application.Catalog.Dtos.SeoDto Seo,
        TrendplusProdavnica.Application.Catalog.Dtos.ProductCardDto[] FeaturedProducts,
        MerchBlockDto[] MerchBlocks,
        FaqItemDto[]? Faq
    );
}
