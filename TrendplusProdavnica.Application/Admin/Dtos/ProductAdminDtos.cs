#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using TrendplusProdavnica.Domain.Enums;

namespace TrendplusProdavnica.Application.Admin.Dtos
{
    public class CreateProductRequest
    {
        [Required]
        public long BrandId { get; set; }

        [Required]
        public long PrimaryCategoryId { get; set; }

        public long? SizeGuideId { get; set; }

        [Required]
        [MaxLength(180)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(220)]
        public string Slug { get; set; } = string.Empty;

        [MaxLength(180)]
        public string? Subtitle { get; set; }

        [Required]
        public string ShortDescription { get; set; } = string.Empty;

        public string? LongDescription { get; set; }

        [MaxLength(80)]
        public string? PrimaryColorName { get; set; }

        [MaxLength(80)]
        public string? StyleTag { get; set; }

        [MaxLength(80)]
        public string? OccasionTag { get; set; }

        [MaxLength(80)]
        public string? SeasonTag { get; set; }

        public ProductStatus Status { get; set; } = ProductStatus.Draft;
        public bool IsVisible { get; set; } = true;
        public bool IsPurchasable { get; set; } = true;
        public bool IsNew { get; set; }
        public bool IsBestseller { get; set; }
        public int SortRank { get; set; }
        public SeoAdminDto? Seo { get; set; }
        public long[] SecondaryCategoryIds { get; set; } = Array.Empty<long>();
        public long[] CollectionIds { get; set; } = Array.Empty<long>();
    }

    public class UpdateProductRequest
    {
        [Required]
        public uint Version { get; set; }

        [Required]
        public long BrandId { get; set; }

        [Required]
        public long PrimaryCategoryId { get; set; }

        public long? SizeGuideId { get; set; }

        [Required]
        [MaxLength(180)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(220)]
        public string Slug { get; set; } = string.Empty;

        [MaxLength(180)]
        public string? Subtitle { get; set; }

        [Required]
        public string ShortDescription { get; set; } = string.Empty;

        public string? LongDescription { get; set; }

        [MaxLength(80)]
        public string? PrimaryColorName { get; set; }

        [MaxLength(80)]
        public string? StyleTag { get; set; }

        [MaxLength(80)]
        public string? OccasionTag { get; set; }

        [MaxLength(80)]
        public string? SeasonTag { get; set; }

        public ProductStatus Status { get; set; } = ProductStatus.Draft;
        public bool IsVisible { get; set; } = true;
        public bool IsPurchasable { get; set; } = true;
        public bool IsNew { get; set; }
        public bool IsBestseller { get; set; }
        public int SortRank { get; set; }
        public SeoAdminDto? Seo { get; set; }
        public long[] SecondaryCategoryIds { get; set; } = Array.Empty<long>();
        public long[] CollectionIds { get; set; } = Array.Empty<long>();
    }

    public record ProductAdminListDto(
        long Id,
        long BrandId,
        string BrandName,
        long PrimaryCategoryId,
        string Name,
        string Slug,
        decimal? Price,
        int StockQuantity,
        ProductStatus Status,
        bool IsActive,
        bool IsVisible,
        bool IsPurchasable,
        bool IsNew,
        bool IsBestseller,
        int SortRank,
        DateTimeOffset? PublishedAtUtc,
        DateTimeOffset UpdatedAtUtc,
        uint Version);

    public record ProductAdminDetailDto(
        long Id,
        long BrandId,
        long PrimaryCategoryId,
        long? SizeGuideId,
        string Name,
        string Slug,
        string? Subtitle,
        string ShortDescription,
        string? LongDescription,
        string? PrimaryColorName,
        string? StyleTag,
        string? OccasionTag,
        string? SeasonTag,
        ProductStatus Status,
        bool IsVisible,
        bool IsPurchasable,
        bool IsNew,
        bool IsBestseller,
        int SortRank,
        SeoAdminDto? Seo,
        long[] SecondaryCategoryIds,
        long[] CollectionIds,
        DateTimeOffset? PublishedAtUtc,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset UpdatedAtUtc,
        uint Version);
}
