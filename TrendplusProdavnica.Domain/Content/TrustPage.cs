#nullable enable
using TrendplusProdavnica.Domain.Common;
using TrendplusProdavnica.Domain.Enums;
using TrendplusProdavnica.Domain.ValueObjects;

namespace TrendplusProdavnica.Domain.Content
{
    public class TrustPage : AggregateRoot
    {
        public TrustPageKind PageKind { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty; // JSON or HTML
        public bool IsPublished { get; set; }
        public SeoMetadata? Seo { get; set; }
    }
}
