#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TrendplusProdavnica.Application.Common.Caching;

namespace TrendplusProdavnica.Infrastructure.Caching
{
    public sealed class WebshopCacheInvalidationService : IWebshopCacheInvalidationService
    {
        private readonly IWebshopCache _cache;
        private readonly IWebshopCacheKeys _keys;
        private readonly ILogger<WebshopCacheInvalidationService> _logger;

        public WebshopCacheInvalidationService(
            IWebshopCache cache,
            IWebshopCacheKeys keys,
            ILogger<WebshopCacheInvalidationService> logger)
        {
            _cache = cache;
            _keys = keys;
            _logger = logger;
        }

        public Task InvalidateHomePageAsync(CancellationToken cancellationToken = default)
        {
            return SafeInvalidateAsync(
                "home page",
                () => _cache.RemoveAsync(_keys.HomePage(), cancellationToken));
        }

        public async Task InvalidateProductBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            await SafeInvalidateAsync(
                $"product '{slug}'",
                () => _cache.RemoveAsync(_keys.ProductDetail(slug), cancellationToken));

            await SafeInvalidateAsync(
                "listing pages after product change",
                () => _cache.RemoveByTagAsync(WebshopCacheTags.Listing, cancellationToken));
        }

        public async Task InvalidateBrandBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            await SafeInvalidateAsync(
                $"brand page '{slug}'",
                () => _cache.RemoveAsync(_keys.BrandPage(slug), cancellationToken));

            await SafeInvalidateAsync(
                "brand listings",
                () => _cache.RemoveByTagAsync(WebshopCacheTags.Listing, cancellationToken));
        }

        public async Task InvalidateCollectionBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            await SafeInvalidateAsync(
                $"collection page '{slug}'",
                () => _cache.RemoveAsync(_keys.CollectionPage(slug), cancellationToken));

            await SafeInvalidateAsync(
                "collection listings",
                () => _cache.RemoveByTagAsync(WebshopCacheTags.Listing, cancellationToken));
        }

        public Task InvalidateStoreBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            return SafeInvalidateAsync(
                $"store page '{slug}'",
                () => _cache.RemoveAsync(_keys.StorePage(slug), cancellationToken));
        }

        public Task InvalidateEditorialBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            return SafeInvalidateAsync(
                $"editorial '{slug}'",
                () => _cache.RemoveAsync(_keys.EditorialDetail(slug), cancellationToken));
        }

        public Task InvalidateEditorialListAsync(CancellationToken cancellationToken = default)
        {
            return SafeInvalidateAsync(
                "editorial list",
                () => _cache.RemoveAsync(_keys.EditorialList(), cancellationToken));
        }

        private async Task SafeInvalidateAsync(string target, Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache invalidation failed for {Target}", target);
            }
        }
    }
}
