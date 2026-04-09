#nullable enable
using System;
using System.ComponentModel.DataAnnotations;

namespace TrendplusProdavnica.Application.Admin.Dtos
{
    public class CreateBrandRequest
    {
        [Required]
        [MaxLength(120)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(140)]
        public string Slug { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? ShortDescription { get; set; }

        [MaxLength(4000)]
        public string? LongDescription { get; set; }

        [MaxLength(500)]
        public string? LogoUrl { get; set; }

        [MaxLength(500)]
        public string? CoverImageUrl { get; set; }

        [MaxLength(500)]
        public string? WebsiteUrl { get; set; }

        public bool IsFeatured { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
        public SeoAdminDto? Seo { get; set; }
    }

    public class UpdateBrandRequest
    {
        [Required]
        [MaxLength(120)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(140)]
        public string Slug { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? ShortDescription { get; set; }

        [MaxLength(4000)]
        public string? LongDescription { get; set; }

        [MaxLength(500)]
        public string? LogoUrl { get; set; }

        [MaxLength(500)]
        public string? CoverImageUrl { get; set; }

        [MaxLength(500)]
        public string? WebsiteUrl { get; set; }

        public bool IsFeatured { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
        public SeoAdminDto? Seo { get; set; }
    }

    public record BrandAdminDto(
        long Id,
        string Name,
        string Slug,
        string? ShortDescription,
        string? LongDescription,
        string? LogoUrl,
        string? CoverImageUrl,
        string? WebsiteUrl,
        bool IsFeatured,
        bool IsActive,
        int SortOrder,
        SeoAdminDto? Seo,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset UpdatedAtUtc);
}
