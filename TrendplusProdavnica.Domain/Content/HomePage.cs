#nullable enable
using System.Collections.Generic;
using TrendplusProdavnica.Domain.Common;
using TrendplusProdavnica.Domain.ValueObjects;

namespace TrendplusProdavnica.Domain.Content
{
    public class HomePage : AggregateRoot
    {
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = "/";
        public bool IsPublished { get; set; }
        public System.DateTimeOffset? PublishedAtUtc { get; set; }
        public SeoMetadata? Seo { get; set; }
        public IEnumerable<HomeModule> Modules { get; set; } = new List<HomeModule>();
    }
}
