#nullable enable
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Common.Caching;
using TrendplusProdavnica.Application.Content.Dtos;
using TrendplusProdavnica.Application.Content.Queries;
using TrendplusProdavnica.Application.Content.Services;
using TrendplusProdavnica.Infrastructure.Caching;
using TrendplusProdavnica.Infrastructure.Persistence.Queries.Content;

namespace TrendplusProdavnica.Infrastructure.Persistence.Queries.Caching
{
    public sealed class CachedBrandPageQueryService : IBrandPageQueryService
    {
        private static readonly string[] Tags = { WebshopCacheTags.BrandPage };
        private readonly BrandPageQueryService _inner;
        private readonly IWebshopCache _cache;
        private readonly IWebshopCacheKeys _keys;

        public CachedBrandPageQueryService(
            BrandPageQueryService inner,
            IWebshopCache cache,
            IWebshopCacheKeys keys)
        {
            _inner = inner;
            _cache = cache;
            _keys = keys;
        }

        public Task<BrandPageDto> GetBrandPageAsync(GetBrandPageQuery query)
        {
            return _cache.GetOrSetAsync(
                _keys.BrandPage(query.Slug),
                WebshopCacheProfile.BrandPage,
                _ => _inner.GetBrandPageAsync(query),
                Tags);
        }
    }
}
