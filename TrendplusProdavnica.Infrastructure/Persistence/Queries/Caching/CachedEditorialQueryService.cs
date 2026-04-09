#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Common.Caching;
using TrendplusProdavnica.Application.Content.Dtos;
using TrendplusProdavnica.Application.Content.Queries;
using TrendplusProdavnica.Application.Content.Services;
using TrendplusProdavnica.Infrastructure.Caching;
using TrendplusProdavnica.Infrastructure.Persistence.Queries.Content;

namespace TrendplusProdavnica.Infrastructure.Persistence.Queries.Caching
{
    public sealed class CachedEditorialQueryService : IEditorialQueryService
    {
        private static readonly string[] ListTags = { WebshopCacheTags.EditorialList };
        private static readonly string[] DetailTags = { WebshopCacheTags.EditorialDetail };
        private readonly EditorialQueryService _inner;
        private readonly IWebshopCache _cache;
        private readonly IWebshopCacheKeys _keys;

        public CachedEditorialQueryService(
            EditorialQueryService inner,
            IWebshopCache cache,
            IWebshopCacheKeys keys)
        {
            _inner = inner;
            _cache = cache;
            _keys = keys;
        }

        public Task<IReadOnlyList<EditorialArticleCardDto>> GetListAsync()
        {
            return _cache.GetOrSetAsync<IReadOnlyList<EditorialArticleCardDto>>(
                _keys.EditorialList(),
                WebshopCacheProfile.EditorialList,
                _ => _inner.GetListAsync(),
                ListTags);
        }

        public Task<EditorialArticleDto> GetEditorialArticleAsync(GetEditorialArticleQuery query)
        {
            return _cache.GetOrSetAsync(
                _keys.EditorialDetail(query.Slug),
                WebshopCacheProfile.EditorialDetail,
                _ => _inner.GetEditorialArticleAsync(query),
                DetailTags);
        }
    }
}
