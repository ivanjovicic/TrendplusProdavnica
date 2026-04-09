#nullable enable
using System.Text.Json;

namespace TrendplusProdavnica.Application.Admin.Dtos
{
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
