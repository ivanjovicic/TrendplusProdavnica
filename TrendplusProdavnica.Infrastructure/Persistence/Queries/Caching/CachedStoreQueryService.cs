#nullable enable
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Common.Caching;
using TrendplusProdavnica.Application.Stores.Dtos;
using TrendplusProdavnica.Application.Stores.Queries;
using TrendplusProdavnica.Application.Stores.Services;
using TrendplusProdavnica.Infrastructure.Caching;
using TrendplusProdavnica.Infrastructure.Persistence.Queries.Stores;

namespace TrendplusProdavnica.Infrastructure.Persistence.Queries.Caching
{
    public sealed class CachedStoreQueryService : IStoreQueryService
    {
        private static readonly string[] StoreTags = { WebshopCacheTags.StorePage };
        private readonly StoreQueryService _inner;
        private readonly IWebshopCache _cache;
        private readonly IWebshopCacheKeys _keys;

        public CachedStoreQueryService(
            StoreQueryService inner,
            IWebshopCache cache,
            IWebshopCacheKeys keys)
        {
            _inner = inner;
            _cache = cache;
            _keys = keys;
        }

        public Task<StoreCardDto[]> GetStoresAsync(GetStoresQuery query)
        {
            return _inner.GetStoresAsync(query);
        }

        public Task<StorePageDto> GetStorePageAsync(GetStorePageQuery query)
        {
            return _cache.GetOrSetAsync(
                _keys.StorePage(query.Slug),
                WebshopCacheProfile.StorePage,
                _ => _inner.GetStorePageAsync(query),
                StoreTags);
        }
    }
}
