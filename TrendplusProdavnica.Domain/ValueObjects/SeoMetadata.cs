#nullable enable
namespace TrendplusProdavnica.Domain.ValueObjects
{
    public sealed class SeoMetadata
    {
        public string? SeoTitle { get; set; }
        public string? SeoDescription { get; set; }
        public string? CanonicalUrl { get; set; }
        public string? RobotsDirective { get; set; }
        public string? OgTitle { get; set; }
        public string? OgDescription { get; set; }
        public string? OgImageUrl { get; set; }
        public string? StructuredDataOverrideJson { get; set; }
    }
}
