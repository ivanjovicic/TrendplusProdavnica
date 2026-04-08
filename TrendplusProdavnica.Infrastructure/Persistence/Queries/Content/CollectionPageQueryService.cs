#nullable enable
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrendplusProdavnica.Application.Catalog.Dtos;
using TrendplusProdavnica.Application.Content.Dtos;
using TrendplusProdavnica.Application.Content.Queries;
using TrendplusProdavnica.Application.Content.Services;
using TrendplusProdavnica.Domain.Catalog;
using TrendplusProdavnica.Domain.Enums;
using TrendplusProdavnica.Infrastructure.Persistence;

namespace TrendplusProdavnica.Infrastructure.Persistence.Queries.Content
{
    public class CollectionPageQueryService : ICollectionPageQueryService
    {
        private readonly TrendplusDbContext _db;
        public CollectionPageQueryService(TrendplusDbContext db) => _db = db;

        public async Task<CollectionPageDto> GetCollectionPageAsync(GetCollectionPageQuery query)
        {
            var col = await _db.Collections.AsNoTracking()
                .Where(c => c.Slug == query.Slug)
                .Select(c => new { c.Id, c.Name, c.Slug, c.Seo })
                .FirstOrDefaultAsync();

            if (col is null)
                throw new System.Collections.Generic.KeyNotFoundException("Collection not found");

            var pageContent = await _db.CollectionPageContents.AsNoTracking()
                .Where(pc => pc.CollectionId == col.Id && pc.IsPublished)
                .Select(pc => new { pc.IntroText, pc.Faq, pc.MerchBlocks, pc.Seo })
                .FirstOrDefaultAsync();

            var featuredProducts = await _db.ProductCollectionMaps.AsNoTracking()
                .Where(cm => cm.CollectionId == col.Id)
                .Join(_db.Products.AsNoTracking(),
                    cm => cm.ProductId,
                    p => p.Id,
                    (cm, p) => new { cm, p })
                .Where(x => x.p.IsVisible && x.p.IsPurchasable && x.p.Status == ProductStatus.Published)
                .OrderByDescending(x => x.cm.Pinned)
                .ThenBy(x => x.cm.SortOrder)
                .Select(x => new ProductCardDto(
                    x.p.Id,
                    x.p.Slug,
                    _db.Brands.Where(b => b.Id == x.p.BrandId).Select(b => b.Name).FirstOrDefault() ?? string.Empty,
                    x.p.Name,
                    x.p.Media.Where(m => m.IsActive && m.IsPrimary).Select(m => m.Url).FirstOrDefault() ?? x.p.Media.Where(m => m.IsActive).OrderBy(m => m.SortOrder).Select(m => m.Url).FirstOrDefault() ?? string.Empty,
                    x.p.Media.Where(m => m.IsActive && !m.IsPrimary).OrderBy(m => m.SortOrder).Select(m => m.Url).FirstOrDefault(),
                    x.p.Variants.Where(v => v.IsActive && v.IsVisible).OrderBy(v => v.Price).Select(v => v.Price).FirstOrDefault(),
                    x.p.Variants.Where(v => v.IsActive && v.IsVisible).OrderBy(v => v.Price).Select(v => v.OldPrice).FirstOrDefault(),
                    x.p.Variants.Where(v => v.IsActive && v.IsVisible).Select(v => v.Currency).FirstOrDefault() ?? "RSD",
                    new[] { x.p.IsNew ? "Novi" : string.Empty }.Where(s => !string.IsNullOrEmpty(s)).ToArray(),
                    x.p.Variants.Any(v => v.IsActive && v.IsVisible && v.StockStatus != StockStatus.OutOfStock),
                    x.p.Variants.Count(v => v.IsActive && v.IsVisible),
                    x.p.PrimaryColorName
                ))
                .Take(12)
                .ToArrayAsync();

            var merchBlocks = pageContent?.MerchBlocks?.Select(mb => new MerchBlockDto(mb.Title ?? string.Empty, mb.Html ?? string.Empty, (mb.ProductSlugs ?? Array.Empty<string>()).ToArray())).ToArray() ?? Array.Empty<MerchBlockDto>();
            var faq = pageContent?.Faq?.Select(f => new FaqItemDto(f.Question ?? string.Empty, f.Answer ?? string.Empty)).ToArray();

            var seo = new TrendplusProdavnica.Application.Catalog.Dtos.SeoDto(
                pageContent?.Seo?.SeoTitle ?? col.Name,
                pageContent?.Seo?.SeoDescription ?? string.Empty,
                pageContent?.Seo?.CanonicalUrl,
                null);

            var introText = pageContent?.IntroText ?? string.Empty;

            return new CollectionPageDto(col.Name, col.Slug, introText, seo, featuredProducts, merchBlocks, faq);
        }
    }
}
