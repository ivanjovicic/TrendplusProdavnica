#nullable enable
using System;
using System.ComponentModel.DataAnnotations;

namespace TrendplusProdavnica.Application.Admin.Dtos
{
    public class CreateStoreRequest
    {
        [Required]
        [MaxLength(160)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(180)]
        public string Slug { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [MaxLength(180)]
        public string AddressLine1 { get; set; } = string.Empty;

        [MaxLength(180)]
        public string? AddressLine2 { get; set; }

        [MaxLength(20)]
        public string? PostalCode { get; set; }

        [MaxLength(120)]
        public string? MallName { get; set; }

        [MaxLength(40)]
        public string? Phone { get; set; }

        [MaxLength(160)]
        public string? Email { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? WorkingHoursText { get; set; }
        public string? ShortDescription { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? DirectionsUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
        public SeoAdminDto? Seo { get; set; }
    }

    public class UpdateStoreRequest
    {
        [Required]
        [MaxLength(160)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(180)]
        public string Slug { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [MaxLength(180)]
        public string AddressLine1 { get; set; } = string.Empty;

        [MaxLength(180)]
        public string? AddressLine2 { get; set; }

        [MaxLength(20)]
        public string? PostalCode { get; set; }

        [MaxLength(120)]
        public string? MallName { get; set; }

        [MaxLength(40)]
        public string? Phone { get; set; }

        [MaxLength(160)]
        public string? Email { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? WorkingHoursText { get; set; }
        public string? ShortDescription { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? DirectionsUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
        public SeoAdminDto? Seo { get; set; }
    }

    public record StoreAdminDto(
        long Id,
        string Name,
        string Slug,
        string City,
        string AddressLine1,
        string? AddressLine2,
        string? PostalCode,
        string? MallName,
        string? Phone,
        string? Email,
        decimal? Latitude,
        decimal? Longitude,
        string? WorkingHoursText,
        string? ShortDescription,
        string? CoverImageUrl,
        string? DirectionsUrl,
        bool IsActive,
        int SortOrder,
        SeoAdminDto? Seo,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset UpdatedAtUtc);
}
