#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrendplusProdavnica.Application.Content.Dtos;
using TrendplusProdavnica.Application.Content.Queries;
using TrendplusProdavnica.Application.Content.Services;
using TrendplusProdavnica.Domain.Enums;
using TrendplusProdavnica.Infrastructure.Persistence;
using TrendplusProdavnica.Infrastructure.Persistence.Queries.Catalog;

namespace TrendplusProdavnica.Infrastructure.Persistence.Queries.Content
{
    public class EditorialQueryService : IEditorialQueryService
    {
        private readonly TrendplusDbContext _db;

        public EditorialQueryService(TrendplusDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<EditorialArticleCardDto>> GetListAsync()
        {
            var rows = await _db.EditorialArticles.AsNoTracking()
                .Where(article => article.Status == ContentStatus.Published)
                .OrderByDescending(article => article.PublishedAtUtc)
                .ThenByDescending(article => article.Id)
                .Select(article => new
                {
                    article.Title,
                    article.Slug,
                    article.Excerpt,
                    article.CoverImageUrl,
                    article.PublishedAtUtc,
                    article.Topic
                })
                .ToArrayAsync();

            return rows
                .Select(article => new EditorialArticleCardDto(
                    article.Title,
                    article.Slug,
                    article.Excerpt,
                    article.CoverImageUrl ?? string.Empty,
                    article.PublishedAtUtc?.UtcDateTime ?? DateTime.UtcNow,
                    article.Topic ?? string.Empty))
                .ToArray();
        }

        public async Task<EditorialArticleDto> GetEditorialArticleAsync(GetEditorialArticleQuery query)
        {
            var article = await _db.EditorialArticles.AsNoTracking()
                .Where(entity =>
                    entity.Slug == query.Slug &&
                    entity.Status == ContentStatus.Published)
                .Select(entity => new
                {
                    entity.Id,
                    entity.Title,
                    entity.Slug,
                    entity.Excerpt,
                    entity.CoverImageUrl,
                    entity.Body,
                    entity.PublishedAtUtc,
                    entity.Topic,
                    entity.AuthorName,
                    entity.Seo,
                    ProductIds = entity.Products
                        .OrderBy(item => item.SortOrder)
                        .Select(item => item.ProductId)
                        .Distinct()
                        .ToArray(),
                    CollectionIds = entity.Collections
                        .Select(item => item.CollectionId)
                        .Distinct()
                        .ToArray(),
                    CategoryIds = entity.Categories
                        .Select(item => item.CategoryId)
                        .Distinct()
                        .ToArray()
                })
                .FirstOrDefaultAsync();

            if (article is null)
            {
                throw new KeyNotFoundException($"Editorial article '{query.Slug}' was not found.");
            }

            var relatedArticleIds = await BuildRelatedArticleIdsAsync(article.Id, article.Topic);
            var seo = ProductQueryMappingHelper.MapSeo(article.Seo, article.Title, article.Excerpt);

            return new EditorialArticleDto(
                article.Title,
                article.Slug,
                article.Excerpt,
                article.CoverImageUrl ?? string.Empty,
                article.Body,
                article.PublishedAtUtc?.UtcDateTime ?? DateTime.UtcNow,
                article.Topic ?? string.Empty,
                article.AuthorName ?? string.Empty,
                seo,
                article.ProductIds,
                article.CollectionIds,
                article.CategoryIds,
                relatedArticleIds);
        }

        private async Task<long[]> BuildRelatedArticleIdsAsync(long articleId, string? topic)
        {
            var relatedIds = new List<long>();

            if (!string.IsNullOrWhiteSpace(topic))
            {
                var sameTopic = await _db.EditorialArticles.AsNoTracking()
                    .Where(article =>
                        article.Status == ContentStatus.Published &&
                        article.Id != articleId &&
                        article.Topic == topic)
                    .OrderByDescending(article => article.PublishedAtUtc)
                    .ThenByDescending(article => article.Id)
                    .Take(6)
                    .Select(article => article.Id)
                    .ToArrayAsync();

                relatedIds.AddRange(sameTopic);
            }

            if (relatedIds.Count < 6)
            {
                var fallback = await _db.EditorialArticles.AsNoTracking()
                    .Where(article =>
                        article.Status == ContentStatus.Published &&
                        article.Id != articleId &&
                        !relatedIds.Contains(article.Id))
                    .OrderByDescending(article => article.PublishedAtUtc)
                    .ThenByDescending(article => article.Id)
                    .Take(6 - relatedIds.Count)
                    .Select(article => article.Id)
                    .ToArrayAsync();

                relatedIds.AddRange(fallback);
            }

            return relatedIds
                .Distinct()
                .ToArray();
        }
    }
}
