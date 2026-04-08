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
    public class BrandPageQueryService : IBrandPageQueryService
    {
        private readonly TrendplusDbContext _db;
        public BrandPageQueryService(TrendplusDbContext db) => _db = db;

        public async Task<BrandPageDto> GetBrandPageAsync(GetBrandPageQuery query)
        {
            var brand = await _db.Brands.AsNoTracking()
                .Where(b => b.Slug == query.Slug)
                .Select(b => new { b.Id, b.Name, b.Slug, b.Seo })
                .FirstOrDefaultAsync();

            if (brand is null)
                throw new System.Collections.Generic.KeyNotFoundException("Brand not found");

            var pageContent = await _db.BrandPageContents.AsNoTracking()
                .Where(pc => pc.BrandId == brand.Id && pc.IsPublished)
                .Select(pc => new { pc.IntroText, pc.Faq, pc.Seo })
                .FirstOrDefaultAsync();

            var featuredProducts = await _db.Products.AsNoTracking()
                .Where(p => p.BrandId == brand.Id && p.IsVisible && p.IsPurchasable && p.Status == ProductStatus.Published)
                .OrderByDescending(p => p.IsBestseller)
                .ThenByDescending(p => p.SortRank)
                .Take(12)
                .Select(p => new ProductCardDto(
                    p.Id,
                    p.Slug,
                    brand.Name,
                    p.Name,
                    p.Media.Where(m => m.IsActive && m.IsPrimary).Select(m => m.Url).FirstOrDefault() ?? p.Media.Where(m => m.IsActive).OrderBy(m => m.SortOrder).Select(m => m.Url).FirstOrDefault() ?? string.Empty,
                    p.Media.Where(m => m.IsActive && !m.IsPrimary).OrderBy(m => m.SortOrder).Select(m => m.Url).FirstOrDefault(),
                    p.Variants.Where(v => v.IsActive && v.IsVisible).OrderBy(v => v.Price).Select(v => v.Price).FirstOrDefault(),
                    p.Variants.Where(v => v.IsActive && v.IsVisible).OrderBy(v => v.Price).Select(v => v.OldPrice).FirstOrDefault(),
                    p.Variants.Where(v => v.IsActive && v.IsVisible).Select(v => v.Currency).FirstOrDefault() ?? "RSD",
                    new[] { p.IsNew ? "Novi" : string.Empty }.Where(s => !string.IsNullOrEmpty(s)).ToArray(),
                    p.Variants.Any(v => v.IsActive && v.IsVisible && v.StockStatus != StockStatus.OutOfStock),
                    p.Variants.Count(v => v.IsActive && v.IsVisible),
                    p.PrimaryColorName
                ))
                .ToArrayAsync();

            var categoryEntities = await _db.Categories.AsNoTracking()
                .Where(c => c.IsActive && _db.Products.Any(p =>
                    p.BrandId == brand.Id &&
                    p.IsVisible &&
                    p.IsPurchasable &&
                    p.Status == ProductStatus.Published &&
                    (p.PrimaryCategoryId == c.Id || p.CategoryMaps.Any(cm => cm.CategoryId == c.Id))))
                .Select(c => new { c.Name, c.Slug })
                .Distinct()
                .OrderBy(c => c.Name)
                .ToArrayAsync();

            var categoryLinks = categoryEntities
                .Select(c => (object)new { Label = c.Name, Url = $"/kategorija/{c.Slug}" })
                .ToArray();

            var seo = new TrendplusProdavnica.Application.Catalog.Dtos.SeoDto(
                pageContent?.Seo?.SeoTitle ?? brand.Name,
                pageContent?.Seo?.SeoDescription ?? string.Empty,
                pageContent?.Seo?.CanonicalUrl,
                null);

            var introText = pageContent?.IntroText ?? string.Empty;
            object? faq = pageContent?.Faq?.Cast<object>().ToArray();

            return new BrandPageDto(brand.Name, brand.Slug, introText, seo, featuredProducts, categoryLinks, faq);
        }
    }
}
