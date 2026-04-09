#nullable enable
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Catalog.Dtos;
using TrendplusProdavnica.Application.Catalog.Services;
using TrendplusProdavnica.Application.Common.Caching;
using TrendplusProdavnica.Infrastructure.Caching;
using TrendplusProdavnica.Infrastructure.Persistence.Queries.Content;

namespace TrendplusProdavnica.Infrastructure.Persistence.Queries.Caching
{
    public sealed class CachedHomePageQueryService : IHomePageQueryService
    {
        private static readonly string[] Tags = { WebshopCacheTags.HomePage };
        private readonly HomePageQueryService _inner;
        private readonly IWebshopCache _cache;
        private readonly IWebshopCacheKeys _keys;

        public CachedHomePageQueryService(
            HomePageQueryService inner,
            IWebshopCache cache,
            IWebshopCacheKeys keys)
        {
            _inner = inner;
            _cache = cache;
            _keys = keys;
        }

        public Task<HomePageDto> GetHomePageAsync()
        {
            return _cache.GetOrSetAsync(
                _keys.HomePage(),
                WebshopCacheProfile.HomePage,
                _ => _inner.GetHomePageAsync(),
                Tags);
        }
    }
}
