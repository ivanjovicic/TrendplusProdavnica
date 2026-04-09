#nullable enable
using System;
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Catalog.Dtos;
using TrendplusProdavnica.Application.Catalog.Queries;
using TrendplusProdavnica.Application.Catalog.Services;
using TrendplusProdavnica.Application.Common.Caching;
using TrendplusProdavnica.Infrastructure.Caching;
using TrendplusProdavnica.Infrastructure.Persistence.Queries.Catalog;

namespace TrendplusProdavnica.Infrastructure.Persistence.Queries.Caching
{
    public sealed class CachedProductListingQueryService : IProductListingQueryService
    {
        private static readonly string[] ListingTags = { WebshopCacheTags.Listing };
        private readonly ProductListingQueryService _inner;
        private readonly IWebshopCache _cache;
        private readonly IWebshopCacheKeys _keys;

        public CachedProductListingQueryService(
            ProductListingQueryService inner,
            IWebshopCache cache,
            IWebshopCacheKeys keys)
        {
            _inner = inner;
            _cache = cache;
            _keys = keys;
        }

        public Task<ProductListingPageDto> GetCategoryListingAsync(GetCategoryListingQuery query)
        {
            return GetCachedListingAsync(
                _keys.CategoryListing(query),
                () => _inner.GetCategoryListingAsync(query));
        }

        public Task<ProductListingPageDto> GetBrandListingAsync(GetBrandListingQuery query)
        {
            return GetCachedListingAsync(
                _keys.BrandListing(query),
                () => _inner.GetBrandListingAsync(query));
        }

        public Task<ProductListingPageDto> GetCollectionListingAsync(GetCollectionListingQuery query)
        {
            return GetCachedListingAsync(
                _keys.CollectionListing(query),
                () => _inner.GetCollectionListingAsync(query));
        }

        public Task<ProductListingPageDto> GetSaleListingAsync(GetSaleListingQuery query)
        {
            return GetCachedListingAsync(
                _keys.SaleListing(query),
                () => _inner.GetSaleListingAsync(query));
        }

        private Task<ProductListingPageDto> GetCachedListingAsync(
            string? key,
            Func<Task<ProductListingPageDto>> factory)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return factory();
            }

            return _cache.GetOrSetAsync(
                key,
                WebshopCacheProfile.ListingLanding,
                _ => factory(),
                ListingTags);
        }
    }
}
