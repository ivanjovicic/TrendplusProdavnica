#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrendplusProdavnica.Application.Content.Dtos;
using TrendplusProdavnica.Application.Content.Queries;
using TrendplusProdavnica.Application.Content.Services;
using TrendplusProdavnica.Infrastructure.Persistence;

namespace TrendplusProdavnica.Infrastructure.Persistence.Queries.Content
{
    public class EditorialQueryService : IEditorialQueryService
    {
        private readonly TrendplusDbContext _db;
        public EditorialQueryService(TrendplusDbContext db) => _db = db;

        public async Task<EditorialArticleDto> GetEditorialArticleAsync(GetEditorialArticleQuery query)
        {
            var art = await _db.EditorialArticles.AsNoTracking()
                .Where(a => a.Slug == query.Slug && a.Status == Domain.Enums.ContentStatus.Published)
                .Select(a => new EditorialArticleDto(
                    a.Title,
                    a.Slug,
                    a.Excerpt,
                    a.CoverImageUrl ?? string.Empty,
                    a.Body,
                    a.PublishedAtUtc.HasValue ? a.PublishedAtUtc.Value.UtcDateTime : DateTime.UtcNow,
                    a.Topic ?? string.Empty,
                    a.AuthorName ?? string.Empty,
                    new TrendplusProdavnica.Application.Catalog.Dtos.SeoDto(
                        a.Seo != null ? a.Seo.SeoTitle ?? a.Title : a.Title,
                        a.Seo != null ? a.Seo.SeoDescription ?? string.Empty : string.Empty,
                        a.Seo != null ? a.Seo.CanonicalUrl : null,
                        null),
                    a.Products.Select(p => p.ProductId).ToArray(),
                    a.Collections.Select(c => c.CollectionId).ToArray(),
                    a.Categories.Select(c => c.CategoryId).ToArray(),
                    Array.Empty<long>()
                ))
                .FirstOrDefaultAsync();

            if (art is null) throw new System.Collections.Generic.KeyNotFoundException("Article not found");
            return art;
        }
    }
}
