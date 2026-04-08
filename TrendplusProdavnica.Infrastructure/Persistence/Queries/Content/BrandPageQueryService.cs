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
    public class BrandPageQueryService : IBrandPageQueryService
    {
        private readonly TrendplusDbContext _db;
        public BrandPageQueryService(TrendplusDbContext db) => _db = db;

        public async Task<BrandPageDto> GetBrandPageAsync(GetBrandPageQuery query)
        {
            var brand = await _db.Brands.AsNoTracking()
                .Where(b => b.Slug == query.Slug)
                .Select(b => new
                {
                    b.Id,
                    b.Name,
                    b.Slug,
                    b.Seo
                })
                .FirstOrDefaultAsync();

            if (brand is null) throw new System.Collections.Generic.KeyNotFoundException("Brand not found");

            var featured = await _db.Products.AsNoTracking()
                .Where(p =>
                    p.BrandId == brand.Id &&
                    p.IsVisible &&
                    p.IsPurchasable &&
                    p.Status == Domain.Enums.ProductStatus.Published)
                .OrderByDescending(p => p.IsBestseller)
                .Take(12)
                .Select(p => new TrendplusProdavnica.Application.Catalog.Dtos.ProductCardDto(
                    p.Id,
                    p.Slug,
                    brand.Name,
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

            var seo = new TrendplusProdavnica.Application.Catalog.Dtos.SeoDto(brand.Seo?.SeoTitle ?? brand.Name, brand.Seo?.SeoDescription ?? string.Empty, brand.Seo?.CanonicalUrl, null);

            return new BrandPageDto(brand.Name, brand.Slug, string.Empty, seo, featured, new object[0], null);
        }
    }
}
