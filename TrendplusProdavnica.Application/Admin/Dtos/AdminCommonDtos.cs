#nullable enable
using System.Text.Json;

namespace TrendplusProdavnica.Application.Admin.Dtos
{
    /// <summary>
    /// Generic paginated response wrapper for admin list endpoints
    /// </summary>
    public class AdminListResponse<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
    }

    public record SeoAdminDto(
        string? SeoTitle,
        string? SeoDescription,
        string? CanonicalUrl,
        string? RobotsDirective,
        string? OgTitle,
        string? OgDescription,
        string? OgImageUrl,
        string? StructuredDataOverrideJson);

    public record FaqItemAdminDto(string Question, string Answer);

    public record FeaturedLinkAdminDto(string Title, string Url, string? ImageUrl);

    public record MerchBlockAdminDto(string Title, string? Html, string[]? ProductSlugs);

    public record HomeModuleAdminDto(string Type, JsonElement? Payload);
}
