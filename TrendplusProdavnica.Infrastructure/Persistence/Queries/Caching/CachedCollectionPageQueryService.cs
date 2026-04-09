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
    public sealed class CachedCollectionPageQueryService : ICollectionPageQueryService
    {
        private static readonly string[] Tags = { WebshopCacheTags.CollectionPage };
        private readonly CollectionPageQueryService _inner;
        private readonly IWebshopCache _cache;
        private readonly IWebshopCacheKeys _keys;

        public CachedCollectionPageQueryService(
            CollectionPageQueryService inner,
            IWebshopCache cache,
            IWebshopCacheKeys keys)
        {
            _inner = inner;
            _cache = cache;
            _keys = keys;
        }

        public Task<CollectionPageDto> GetCollectionPageAsync(GetCollectionPageQuery query)
        {
            return _cache.GetOrSetAsync(
                _keys.CollectionPage(query.Slug),
                WebshopCacheProfile.CollectionPage,
                _ => _inner.GetCollectionPageAsync(query),
                Tags);
        }
    }
}
