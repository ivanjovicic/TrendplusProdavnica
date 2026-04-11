#nullable enable
using System;
using Microsoft.Extensions.Options;
using TrendplusProdavnica.Application.Common.Caching;

namespace TrendplusProdavnica.Infrastructure.Caching
{
    public sealed class WebshopCacheKeys : IWebshopCacheKeys
    {
        private const string VersionToken = "v1";
        private readonly CacheSettings _settings;

        public WebshopCacheKeys(IOptions<CacheSettings> settings)
        {
            _settings = settings.Value;
        }

        public string HomePage() => BuildKey("home", VersionToken);

        public string ProductDetail(string slug) => BuildKey("pdp", VersionToken, NormalizeSlug(slug));

        public string BrandPage(string slug) => BuildKey("brand", VersionToken, NormalizeSlug(slug));

        public string CollectionPage(string slug) => BuildKey("collection", VersionToken, NormalizeSlug(slug));

        public string StorePage(string slug) => BuildKey("store", VersionToken, NormalizeSlug(slug));

        public string EditorialDetail(string slug) => BuildKey("editorial", VersionToken, NormalizeSlug(slug));

        public string EditorialList() => BuildKey("editorial-list", VersionToken);

        private string BuildKey(params string[] segments)
        {
            var normalizedPrefix = string.IsNullOrWhiteSpace(_settings.KeyPrefix)
                ? "tp"
                : _settings.KeyPrefix.Trim().ToLowerInvariant();

            return $"{normalizedPrefix}:{string.Join(':', segments)}";
        }

        private static string NormalizeSlug(string slug)
        {
            return slug.Trim().ToLowerInvariant();
        }
    }
}
