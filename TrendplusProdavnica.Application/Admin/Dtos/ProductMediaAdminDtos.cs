#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using TrendplusProdavnica.Domain.Enums;

namespace TrendplusProdavnica.Application.Admin.Dtos
{
    public class CreateProductMediaRequest
    {
        [Required]
        public long ProductId { get; set; }

        public long? VariantId { get; set; }

        [Required]
        public string Url { get; set; } = string.Empty;

        public string? MobileUrl { get; set; }
        public string? AltText { get; set; }
        public string? Title { get; set; }
        public MediaType MediaType { get; set; } = MediaType.Image;
        public MediaRole MediaRole { get; set; } = MediaRole.Gallery;
        public int SortOrder { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateProductMediaRequest
    {
        [Required]
        public long ProductId { get; set; }

        public long? VariantId { get; set; }

        [Required]
        public string Url { get; set; } = string.Empty;

        public string? MobileUrl { get; set; }
        public string? AltText { get; set; }
        public string? Title { get; set; }
        public MediaType MediaType { get; set; } = MediaType.Image;
        public MediaRole MediaRole { get; set; } = MediaRole.Gallery;
        public int SortOrder { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public record ProductMediaAdminDto(
        long Id,
        long ProductId,
        long? VariantId,
        string Url,
        string? MobileUrl,
        string? AltText,
        string? Title,
        MediaType MediaType,
        MediaRole MediaRole,
        int SortOrder,
        bool IsPrimary,
        bool IsActive,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset UpdatedAtUtc);
}
