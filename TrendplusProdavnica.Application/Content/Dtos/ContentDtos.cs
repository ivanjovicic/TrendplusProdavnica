#nullable enable
using System;
using System.Collections.Generic;

namespace TrendplusProdavnica.Application.Content.Dtos
{
    public record EditorialArticleCardDto(string Title, string Slug, string Excerpt, string CoverImageUrl, DateTime PublishedAtUtc, string Topic);

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
        object[] RelatedProducts,
        object[] RelatedCollections,
        object[] RelatedCategories,
        object[] RelatedArticles
    );

    public record BrandPageDto(
        string BrandName,
        string Slug,
        string IntroText,
        TrendplusProdavnica.Application.Catalog.Dtos.SeoDto Seo,
        TrendplusProdavnica.Application.Catalog.Dtos.ProductCardDto[] FeaturedProducts,
        object[] CategoryLinks,
        object? Faq
    );

    public record CollectionPageDto(
        string Name,
        string Slug,
        string IntroText,
        TrendplusProdavnica.Application.Catalog.Dtos.SeoDto Seo,
        TrendplusProdavnica.Application.Catalog.Dtos.ProductCardDto[] FeaturedProducts,
        object[] MerchBlocks,
        object? Faq
    );
}
