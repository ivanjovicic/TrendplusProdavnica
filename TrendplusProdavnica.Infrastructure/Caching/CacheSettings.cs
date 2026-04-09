#nullable enable
using System;

namespace TrendplusProdavnica.Infrastructure.Caching
{
    public sealed class CacheSettings
    {
        public string KeyPrefix { get; set; } = "tp";
        public bool IsFailSafeEnabled { get; set; } = true;
        public TimeSpan FailSafeMaxDuration { get; set; } = TimeSpan.FromMinutes(30);
        public TimeSpan FailSafeThrottleDuration { get; set; } = TimeSpan.FromSeconds(20);
        public TimeSpan FactorySoftTimeout { get; set; } = TimeSpan.FromMilliseconds(120);
        public TimeSpan FactoryHardTimeout { get; set; } = TimeSpan.FromSeconds(2);
        public TimeSpan DistributedCacheSoftTimeout { get; set; } = TimeSpan.FromMilliseconds(80);
        public TimeSpan DistributedCacheHardTimeout { get; set; } = TimeSpan.FromMilliseconds(350);
        public CacheDurationSettings Durations { get; set; } = new();
        public ListingCacheSettings Listing { get; set; } = new();
        public OutputCacheSettings OutputCache { get; set; } = new();
    }

    public sealed class CacheDurationSettings
    {
        public TimeSpan HomePage { get; set; } = TimeSpan.FromMinutes(5);
        public TimeSpan ProductDetail { get; set; } = TimeSpan.FromMinutes(3);
        public TimeSpan BrandPage { get; set; } = TimeSpan.FromMinutes(4);
        public TimeSpan CollectionPage { get; set; } = TimeSpan.FromMinutes(4);
        public TimeSpan StorePage { get; set; } = TimeSpan.FromMinutes(4);
        public TimeSpan EditorialDetail { get; set; } = TimeSpan.FromMinutes(12);
        public TimeSpan EditorialList { get; set; } = TimeSpan.FromMinutes(5);
        public TimeSpan ListingLanding { get; set; } = TimeSpan.FromMinutes(2);
        public TimeSpan SearchResults { get; set; } = TimeSpan.FromSeconds(45);
    }

    public sealed class ListingCacheSettings
    {
        public bool Enabled { get; set; } = true;
        public bool FirstPageOnly { get; set; } = true;
        public int MaxPageSize { get; set; } = 24;
        public bool CacheOnlyWithoutFilters { get; set; } = true;
    }

    public sealed class OutputCacheSettings
    {
        public bool Enabled { get; set; } = true;
        public TimeSpan HomePageDuration { get; set; } = TimeSpan.FromMinutes(5);
        public TimeSpan EntityPageDuration { get; set; } = TimeSpan.FromSeconds(30);
        public TimeSpan ProductDetailDuration { get; set; } = TimeSpan.FromSeconds(20);
    }
}
