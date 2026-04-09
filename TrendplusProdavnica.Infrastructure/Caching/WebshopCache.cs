#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TrendplusProdavnica.Application.Common.Caching;
using ZiggyCreatures.Caching.Fusion;

namespace TrendplusProdavnica.Infrastructure.Caching
{
    public sealed class WebshopCache : IWebshopCache
    {
        private readonly IFusionCache _fusionCache;
        private readonly CacheSettings _settings;

        public WebshopCache(IFusionCache fusionCache, IOptions<CacheSettings> settings)
        {
            _fusionCache = fusionCache;
            _settings = settings.Value;
        }

        public Task<TValue> GetOrSetAsync<TValue>(
            string key,
            WebshopCacheProfile profile,
            Func<CancellationToken, Task<TValue>> factory,
            IReadOnlyCollection<string>? tags = null,
            CancellationToken cancellationToken = default)
        {
            var options = CreateEntryOptions(profile);

            return _fusionCache.GetOrSetAsync(
                key,
                (_, token) => factory(token),
                MaybeValue<TValue>.None,
                options,
                tags,
                cancellationToken).AsTask();
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            return _fusionCache.RemoveAsync(
                key,
                options: null,
                token: cancellationToken).AsTask();
        }

        public Task RemoveByTagAsync(string tag, CancellationToken cancellationToken = default)
        {
            return _fusionCache.RemoveByTagAsync(
                tag,
                options: null,
                token: cancellationToken).AsTask();
        }

        private FusionCacheEntryOptions CreateEntryOptions(WebshopCacheProfile profile)
        {
            var duration = ResolveDuration(profile);

            return new FusionCacheEntryOptions
            {
                Duration = duration,
                IsFailSafeEnabled = _settings.IsFailSafeEnabled,
                FailSafeMaxDuration = _settings.FailSafeMaxDuration,
                FailSafeThrottleDuration = _settings.FailSafeThrottleDuration,
                FactorySoftTimeout = _settings.FactorySoftTimeout,
                FactoryHardTimeout = _settings.FactoryHardTimeout,
                DistributedCacheDuration = duration,
                DistributedCacheSoftTimeout = _settings.DistributedCacheSoftTimeout,
                DistributedCacheHardTimeout = _settings.DistributedCacheHardTimeout
            };
        }

        private TimeSpan ResolveDuration(WebshopCacheProfile profile)
        {
            var durations = _settings.Durations;

            var configured = profile switch
            {
                WebshopCacheProfile.HomePage => durations.HomePage,
                WebshopCacheProfile.ProductDetail => durations.ProductDetail,
                WebshopCacheProfile.BrandPage => durations.BrandPage,
                WebshopCacheProfile.CollectionPage => durations.CollectionPage,
                WebshopCacheProfile.StorePage => durations.StorePage,
                WebshopCacheProfile.EditorialDetail => durations.EditorialDetail,
                WebshopCacheProfile.EditorialList => durations.EditorialList,
                WebshopCacheProfile.ListingLanding => durations.ListingLanding,
                WebshopCacheProfile.SearchResults => durations.SearchResults,
                _ => durations.ProductDetail
            };

            return configured <= TimeSpan.Zero ? TimeSpan.FromMinutes(2) : configured;
        }
    }
}
