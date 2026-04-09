#nullable enable
using System;

namespace TrendplusProdavnica.Application.Admin.Dtos
{
    public class UpsertBrandPageContentRequest
    {
        public long BrandId { get; set; }
        public bool IsPublished { get; set; }
        public string? HeroTitle { get; set; }
        public string? HeroSubtitle { get; set; }
        public string? IntroTitle { get; set; }
        public string? IntroText { get; set; }
        public string? SeoText { get; set; }
        public string? HeroImageUrl { get; set; }
        public FaqItemAdminDto[]? Faq { get; set; }
        public FeaturedLinkAdminDto[]? FeaturedLinks { get; set; }
        public MerchBlockAdminDto[]? MerchBlocks { get; set; }
        public SeoAdminDto? Seo { get; set; }
    }

    public class UpsertCollectionPageContentRequest
    {
        public long CollectionId { get; set; }
        public bool IsPublished { get; set; }
        public string? HeroTitle { get; set; }
        public string? HeroSubtitle { get; set; }
        public string? IntroTitle { get; set; }
        public string? IntroText { get; set; }
        public string? SeoText { get; set; }
        public string? HeroImageUrl { get; set; }
        public FaqItemAdminDto[]? Faq { get; set; }
        public FeaturedLinkAdminDto[]? FeaturedLinks { get; set; }
        public MerchBlockAdminDto[]? MerchBlocks { get; set; }
        public SeoAdminDto? Seo { get; set; }
    }

    public class UpsertStorePageContentRequest
    {
        public long StoreId { get; set; }
        public bool IsPublished { get; set; }
        public string? HeroTitle { get; set; }
        public string? HeroSubtitle { get; set; }
        public string? IntroTitle { get; set; }
        public string? IntroText { get; set; }
        public string? SeoText { get; set; }
        public string? HeroImageUrl { get; set; }
        public FaqItemAdminDto[]? Faq { get; set; }
        public FeaturedLinkAdminDto[]? FeaturedLinks { get; set; }
        public MerchBlockAdminDto[]? MerchBlocks { get; set; }
        public SeoAdminDto? Seo { get; set; }
    }

    public record BrandPageContentAdminDto(
        long BrandId,
        bool IsPublished,
        string? HeroTitle,
        string? HeroSubtitle,
        string? IntroTitle,
        string? IntroText,
        string? SeoText,
        string? HeroImageUrl,
        FaqItemAdminDto[]? Faq,
        FeaturedLinkAdminDto[]? FeaturedLinks,
        MerchBlockAdminDto[]? MerchBlocks,
        SeoAdminDto? Seo,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset UpdatedAtUtc);

    public record CollectionPageContentAdminDto(
        long CollectionId,
        bool IsPublished,
        string? HeroTitle,
        string? HeroSubtitle,
        string? IntroTitle,
        string? IntroText,
        string? SeoText,
        string? HeroImageUrl,
        FaqItemAdminDto[]? Faq,
        FeaturedLinkAdminDto[]? FeaturedLinks,
        MerchBlockAdminDto[]? MerchBlocks,
        SeoAdminDto? Seo,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset UpdatedAtUtc);

    public record StorePageContentAdminDto(
        long StoreId,
        bool IsPublished,
        string? HeroTitle,
        string? HeroSubtitle,
        string? IntroTitle,
        string? IntroText,
        string? SeoText,
        string? HeroImageUrl,
        FaqItemAdminDto[]? Faq,
        FeaturedLinkAdminDto[]? FeaturedLinks,
        MerchBlockAdminDto[]? MerchBlocks,
        SeoAdminDto? Seo,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset UpdatedAtUtc);
}
