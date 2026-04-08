#nullable enable
using TrendplusProdavnica.Domain.Common;
using TrendplusProdavnica.Domain.ValueObjects;

namespace TrendplusProdavnica.Domain.Content
{
    public class StorePageContent : EntityBase
    {
        public long StoreId { get; set; }
        public bool IsPublished { get; set; }
        public string? HeroTitle { get; set; }
        public string? HeroSubtitle { get; set; }
        public string? IntroTitle { get; set; }
        public string? IntroText { get; set; }
        public string? SeoText { get; set; }
        public string? HeroImageUrl { get; set; }
        public IEnumerable<FaqItem>? Faq { get; set; }
        public IEnumerable<FeaturedLink>? FeaturedLinks { get; set; }
        public IEnumerable<MerchBlock>? MerchBlocks { get; set; }
        public SeoMetadata? Seo { get; set; }
    }
}
