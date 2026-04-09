#nullable enable
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Catalog.Dtos;
using TrendplusProdavnica.Application.Catalog.Queries;
using TrendplusProdavnica.Application.Catalog.Services;
using TrendplusProdavnica.Application.Common.Caching;
using TrendplusProdavnica.Infrastructure.Caching;
using TrendplusProdavnica.Infrastructure.Persistence.Queries.Catalog;

namespace TrendplusProdavnica.Infrastructure.Persistence.Queries.Caching
{
    public sealed class CachedProductDetailQueryService : IProductDetailQueryService
    {
        private static readonly string[] Tags = { WebshopCacheTags.ProductDetail };
        private readonly ProductDetailQueryService _inner;
        private readonly IWebshopCache _cache;
        private readonly IWebshopCacheKeys _keys;

        public CachedProductDetailQueryService(
            ProductDetailQueryService inner,
            IWebshopCache cache,
            IWebshopCacheKeys keys)
        {
            _inner = inner;
            _cache = cache;
            _keys = keys;
        }

        public Task<ProductDetailDto> GetProductDetailAsync(GetProductDetailQuery query)
        {
            return _cache.GetOrSetAsync(
                _keys.ProductDetail(query.Slug),
                WebshopCacheProfile.ProductDetail,
                _ => _inner.GetProductDetailAsync(query),
                Tags);
        }
    }
}
