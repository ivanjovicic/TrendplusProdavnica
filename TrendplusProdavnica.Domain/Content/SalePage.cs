#nullable enable
using TrendplusProdavnica.Domain.Common;
using TrendplusProdavnica.Domain.ValueObjects;

namespace TrendplusProdavnica.Domain.Content
{
    public class SalePage : AggregateRoot
    {
        public string Slug { get; set; } = string.Empty;
        public long? CategoryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string? IntroText { get; set; }
        public string? SeoText { get; set; }
        public string? HeroImageUrl { get; set; }
        public IEnumerable<FaqItem>? Faq { get; set; }
        public bool IsPublished { get; set; }
        public SeoMetadata? Seo { get; set; }
    }
}
