#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using TrendplusProdavnica.Domain.Enums;

namespace TrendplusProdavnica.Application.Admin.Dtos
{
    public class CreateEditorialArticleRequest
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Slug { get; set; } = string.Empty;

        public string Excerpt { get; set; } = string.Empty;
        public string? CoverImageUrl { get; set; }

        [Required]
        public string Body { get; set; } = string.Empty;

        public string? Topic { get; set; }
        public string? AuthorName { get; set; }
        public ContentStatus Status { get; set; } = ContentStatus.Draft;
        public DateTimeOffset? PublishedAtUtc { get; set; }
        public SeoAdminDto? Seo { get; set; }
        public long[] RelatedProductIds { get; set; } = Array.Empty<long>();
        public long[] RelatedCategoryIds { get; set; } = Array.Empty<long>();
        public long[] RelatedBrandIds { get; set; } = Array.Empty<long>();
        public long[] RelatedCollectionIds { get; set; } = Array.Empty<long>();
    }

    public class UpdateEditorialArticleRequest
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Slug { get; set; } = string.Empty;

        public string Excerpt { get; set; } = string.Empty;
        public string? CoverImageUrl { get; set; }

        [Required]
        public string Body { get; set; } = string.Empty;

        public string? Topic { get; set; }
        public string? AuthorName { get; set; }
        public ContentStatus Status { get; set; } = ContentStatus.Draft;
        public DateTimeOffset? PublishedAtUtc { get; set; }
        public SeoAdminDto? Seo { get; set; }
        public long[] RelatedProductIds { get; set; } = Array.Empty<long>();
        public long[] RelatedCategoryIds { get; set; } = Array.Empty<long>();
        public long[] RelatedBrandIds { get; set; } = Array.Empty<long>();
        public long[] RelatedCollectionIds { get; set; } = Array.Empty<long>();
    }

    public record EditorialArticleAdminDto(
        long Id,
        string Title,
        string Slug,
        string Excerpt,
        string? CoverImageUrl,
        string Body,
        string? Topic,
        string? AuthorName,
        ContentStatus Status,
        DateTimeOffset? PublishedAtUtc,
        SeoAdminDto? Seo,
        long[] RelatedProductIds,
        long[] RelatedCategoryIds,
        long[] RelatedBrandIds,
        long[] RelatedCollectionIds,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset UpdatedAtUtc);
}
