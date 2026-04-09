#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using TrendplusProdavnica.Domain.Enums;

namespace TrendplusProdavnica.Application.Admin.Dtos
{
    public class CreateCollectionRequest
    {
        [Required]
        [MaxLength(140)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(160)]
        public string Slug { get; set; } = string.Empty;

        public CollectionType CollectionType { get; set; } = CollectionType.Manual;

        [MaxLength(1000)]
        public string? ShortDescription { get; set; }

        [MaxLength(4000)]
        public string? LongDescription { get; set; }

        [MaxLength(500)]
        public string? CoverImageUrl { get; set; }

        [MaxLength(500)]
        public string? ThumbnailImageUrl { get; set; }

        [MaxLength(40)]
        public string? BadgeText { get; set; }

        public DateTimeOffset? StartAtUtc { get; set; }
        public DateTimeOffset? EndAtUtc { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
        public SeoAdminDto? Seo { get; set; }
    }

    public class UpdateCollectionRequest
    {
        [Required]
        [MaxLength(140)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(160)]
        public string Slug { get; set; } = string.Empty;

        public CollectionType CollectionType { get; set; } = CollectionType.Manual;

        [MaxLength(1000)]
        public string? ShortDescription { get; set; }

        [MaxLength(4000)]
        public string? LongDescription { get; set; }

        [MaxLength(500)]
        public string? CoverImageUrl { get; set; }

        [MaxLength(500)]
        public string? ThumbnailImageUrl { get; set; }

        [MaxLength(40)]
        public string? BadgeText { get; set; }

        public DateTimeOffset? StartAtUtc { get; set; }
        public DateTimeOffset? EndAtUtc { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
        public SeoAdminDto? Seo { get; set; }
    }

    public record CollectionAdminDto(
        long Id,
        string Name,
        string Slug,
        CollectionType CollectionType,
        string? ShortDescription,
        string? LongDescription,
        string? CoverImageUrl,
        string? ThumbnailImageUrl,
        string? BadgeText,
        DateTimeOffset? StartAtUtc,
        DateTimeOffset? EndAtUtc,
        bool IsFeatured,
        bool IsActive,
        int SortOrder,
        SeoAdminDto? Seo,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset UpdatedAtUtc);
}
