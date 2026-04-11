#nullable enable
using TrendplusProdavnica.Application.Common.Caching;

namespace TrendplusProdavnica.Infrastructure.Caching
{
    internal sealed class NoOpWebshopCacheKeys : IWebshopCacheKeys
    {
        public string HomePage() => "home";

        public string ProductDetail(string slug) => $"pdp:{slug}";

        public string BrandPage(string slug) => $"brand:{slug}";

        public string CollectionPage(string slug) => $"collection:{slug}";

        public string StorePage(string slug) => $"store:{slug}";

        public string EditorialDetail(string slug) => $"editorial:{slug}";

        public string EditorialList() => "editorial:list";
    }
}
