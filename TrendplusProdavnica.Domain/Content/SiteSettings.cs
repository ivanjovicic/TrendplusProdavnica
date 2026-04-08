#nullable enable
using TrendplusProdavnica.Domain.Common;
using TrendplusProdavnica.Domain.ValueObjects;

namespace TrendplusProdavnica.Domain.Content
{
    public class SiteSettings : EntityBase
    {
        // Note: conceptual smallint Id in DB; domain uses long for consistency
        public string SiteName { get; set; } = string.Empty;
        public string? DefaultSeoTitleSuffix { get; set; }
        public string? DefaultOgImageUrl { get; set; }
        public string? SupportEmail { get; set; }
        public string? SupportPhone { get; set; }
        public IEnumerable<SocialLink>? SocialLinks { get; set; }
        public ContactInfo? ContactInfo { get; set; }
        public string? AnalyticsSettings { get; set; }
    }
}
