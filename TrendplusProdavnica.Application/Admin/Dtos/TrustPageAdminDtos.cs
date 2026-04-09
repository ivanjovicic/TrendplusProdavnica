#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using TrendplusProdavnica.Domain.Enums;

namespace TrendplusProdavnica.Application.Admin.Dtos
{
    public class CreateTrustPageRequest
    {
        [Required]
        public TrustPageKind PageKind { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Slug { get; set; } = string.Empty;

        [Required]
        public string Body { get; set; } = string.Empty;

        public bool IsPublished { get; set; }
        public SeoAdminDto? Seo { get; set; }
    }

    public class UpdateTrustPageRequest
    {
        [Required]
        public TrustPageKind PageKind { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Slug { get; set; } = string.Empty;

        [Required]
        public string Body { get; set; } = string.Empty;

        public bool IsPublished { get; set; }
        public SeoAdminDto? Seo { get; set; }
    }

    public record TrustPageAdminDto(
        long Id,
        TrustPageKind PageKind,
        string Title,
        string Slug,
        string Body,
        bool IsPublished,
        SeoAdminDto? Seo,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset UpdatedAtUtc);
}
