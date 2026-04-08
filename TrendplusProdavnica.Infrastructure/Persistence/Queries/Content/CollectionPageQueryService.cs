#nullable enable
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrendplusProdavnica.Application.Content.Dtos;
using TrendplusProdavnica.Application.Content.Queries;
using TrendplusProdavnica.Application.Content.Services;
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
                .Select(c => new { c.Name, c.Slug, c.Seo })
                .FirstOrDefaultAsync();

            if (col is null) throw new System.Collections.Generic.KeyNotFoundException("Collection not found");

            var featured = await _db.ProductCollectionMaps.AsNoTracking()
                .Where(m => m.Collection.Slug == query.Slug)
                .Select(m => m.Product)
                .OrderByDescending(p => p.IsBestseller)
                .Take(12)
                .Select(p => new TrendplusProdavnica.Application.Catalog.Dtos.ProductCardDto(
                    p.Id,
                    p.Slug,
                    _db.Brands.Where(b => b.Id == p.BrandId).Select(b => b.Name).FirstOrDefault() ?? string.Empty,
                    p.Name,
                    p.Media.Where(m => m.IsPrimary).Select(m => m.Url).FirstOrDefault() ?? string.Empty,
                    null,
                    p.Variants.OrderBy(v => v.Price).Select(v => v.Price).FirstOrDefault(),
                    p.Variants.OrderBy(v => v.Price).Select(v => v.OldPrice).FirstOrDefault(),
                    p.Variants.Select(v => v.Currency).FirstOrDefault() ?? "RSD",
                    new string[0],
                    p.Variants.Any(v => v.StockStatus != Domain.Enums.StockStatus.OutOfStock && v.IsActive),
                    p.Variants.Count(v => v.IsActive),
                    p.PrimaryColorName
                ))
                .ToArrayAsync();

            var seo = new TrendplusProdavnica.Application.Catalog.Dtos.SeoDto(col.Seo?.SeoTitle ?? col.Name, col.Seo?.SeoDescription ?? string.Empty, col.Seo?.CanonicalUrl, null);

            return new CollectionPageDto(col.Name, col.Slug, string.Empty, seo, featured, new object[0], null);
        }
    }
}
