#nullable enable
using System;
using System.ComponentModel.DataAnnotations;

namespace TrendplusProdavnica.Application.Admin.Dtos
{
    public class UpdateHomePageRequest
    {
        [Required]
        [MaxLength(160)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(40)]
        public string Slug { get; set; } = "/";

        public SeoAdminDto? Seo { get; set; }
        public HomeModuleAdminDto[] Modules { get; set; } = Array.Empty<HomeModuleAdminDto>();
    }

    public record HomePageAdminDto(
        long Id,
        string Title,
        string Slug,
        bool IsPublished,
        DateTimeOffset? PublishedAtUtc,
        SeoAdminDto? Seo,
        HomeModuleAdminDto[] Modules,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset UpdatedAtUtc);
}
