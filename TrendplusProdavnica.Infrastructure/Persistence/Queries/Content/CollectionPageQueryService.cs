#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrendplusProdavnica.Application.Content.Dtos;
using TrendplusProdavnica.Application.Content.Queries;
using TrendplusProdavnica.Application.Content.Services;
using TrendplusProdavnica.Infrastructure.Persistence;
using TrendplusProdavnica.Infrastructure.Persistence.Queries.Catalog;

namespace TrendplusProdavnica.Infrastructure.Persistence.Queries.Content
{
    public class CollectionPageQueryService : ICollectionPageQueryService
    {
        private readonly TrendplusDbContext _db;

        public CollectionPageQueryService(TrendplusDbContext db)
        {
            _db = db;
        }

        public async Task<CollectionPageDto> GetCollectionPageAsync(GetCollectionPageQuery query)
        {
            var collection = await _db.Collections.AsNoTracking()
                .Where(entity => entity.Slug == query.Slug && entity.IsActive)
                .Select(entity => new
                {
                    entity.Id,
                    entity.Name,
                    entity.Slug,
                    entity.ShortDescription,
                    entity.LongDescription,
                    entity.Seo
                })
                .FirstOrDefaultAsync();

            if (collection is null)
            {
                throw new KeyNotFoundException($"Collection '{query.Slug}' was not found.");
            }

            var content = await _db.CollectionPageContents.AsNoTracking()
                .Where(entity => entity.CollectionId == collection.Id && entity.IsPublished)
                .Select(entity => new
                {
                    entity.IntroText,
                    entity.Faq,
                    entity.MerchBlocks,
                    entity.Seo
                })
                .FirstOrDefaultAsync();

            var orderedProductIds = await (
                    from map in _db.ProductCollectionMaps.AsNoTracking()
                    join product in ProductQueryMappingHelper.ApplyBaseProductVisibility(_db.Products.AsNoTracking())
                        on map.ProductId equals product.Id
                    where map.CollectionId == collection.Id
                    orderby map.Pinned descending,
                            map.SortOrder ascending,
                            product.SortRank descending,
                            product.PublishedAtUtc descending,
                            product.Id descending
                    select map.ProductId)
                .Take(48)
                .ToArrayAsync();

            var featuredProducts = Array.Empty<TrendplusProdavnica.Application.Catalog.Dtos.ProductCardDto>();

            if (orderedProductIds.Length > 0)
            {
                var projections = await ProductQueryMappingHelper
                    .ToProductCardProjection(
                        ProductQueryMappingHelper.ApplyBaseProductVisibility(_db.Products.AsNoTracking())
                            .Where(product => orderedProductIds.Contains(product.Id)),
                        _db.Brands.AsNoTracking())
                    .ToArrayAsync();

                var indexById = orderedProductIds
                    .Select((id, index) => new { id, index })
                    .ToDictionary(item => item.id, item => item.index);

                featuredProducts = ProductQueryMappingHelper
                    .ToProductCardDtos(projections.OrderBy(item => indexById[item.Id]));
            }

            var introText = content?.IntroText ?? collection.LongDescription ?? collection.ShortDescription ?? string.Empty;
            var seo = ProductQueryMappingHelper.MapSeo(
                content?.Seo ?? collection.Seo,
                collection.Name,
                collection.ShortDescription ?? string.Empty);

            return new CollectionPageDto(
                collection.Name,
                collection.Slug,
                introText,
                seo,
                featuredProducts,
                MapMerchBlocks(content?.MerchBlocks),
                MapFaq(content?.Faq));
        }

        private static MerchBlockDto[] MapMerchBlocks(IEnumerable<Domain.ValueObjects.MerchBlock>? blocks)
        {
            if (blocks is null)
            {
                return Array.Empty<MerchBlockDto>();
            }

            return blocks
                .Select(block => new MerchBlockDto(
                    block.Title,
                    block.Html ?? string.Empty,
                    (block.ProductSlugs ?? Array.Empty<string>()).ToArray()))
                .ToArray();
        }

        private static FaqItemDto[]? MapFaq(IEnumerable<Domain.ValueObjects.FaqItem>? faq)
        {
            if (faq is null)
            {
                return null;
            }

            var mapped = faq
                .Select(item => new FaqItemDto(item.Question, item.Answer))
                .ToArray();

            return mapped.Length == 0 ? null : mapped;
        }
    }
}
