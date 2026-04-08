#nullable enable
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrendplusProdavnica.Application.Catalog.Dtos;
using TrendplusProdavnica.Application.Catalog.Services;
using TrendplusProdavnica.Infrastructure.Persistence;

namespace TrendplusProdavnica.Infrastructure.Persistence.Queries.Content
{
    public class HomePageQueryService : IHomePageQueryService
    {
        private readonly TrendplusDbContext _db;
        public HomePageQueryService(TrendplusDbContext db) => _db = db;

        public async Task<HomePageDto> GetHomePageAsync()
        {
            var page = await _db.HomePages.AsNoTracking()
                .Where(h => h.IsPublished)
                .OrderByDescending(h => h.PublishedAtUtc)
                .Select(h => new
                {
                    h.Seo,
                    h.Title,
                    Modules = h.Modules
                })
                .FirstOrDefaultAsync();

            if (page is null)
            {
                return EmptyHomePage();
            }

            var seo = new SeoDto(page.Seo?.SeoTitle ?? page.Title, page.Seo?.SeoDescription ?? string.Empty, page.Seo?.CanonicalUrl, null);

            // New arrivals
            var newArrivals = await _db.Products.AsNoTracking()
                .Where(p => p.Status == Domain.Enums.ProductStatus.Published && p.IsVisible && p.IsPurchasable && p.IsNew)
                .OrderByDescending(p => p.PublishedAtUtc)
                .Take(12)
                .Select(p => MapToProductCard(p))
                .ToArrayAsync();

            // Bestsellers
            var bestsellers = await _db.Products.AsNoTracking()
                .Where(p => p.Status == Domain.Enums.ProductStatus.Published && p.IsVisible && p.IsPurchasable && p.IsBestseller)
                .OrderByDescending(p => p.SortRank)
                .Take(12)
                .Select(p => MapToProductCard(p))
                .ToArrayAsync();

            // Featured collections -> products from featured collections
            var featuredCollections = await _db.Products.AsNoTracking()
                .Where(p => p.Status == Domain.Enums.ProductStatus.Published && p.IsVisible && p.IsPurchasable && p.CollectionMaps.Any(cm => cm.Collection.IsFeatured && cm.Collection.IsActive))
                .OrderByDescending(p => p.SortRank)
                .Take(12)
                .Select(p => MapToProductCard(p))
                .ToArrayAsync();

            // Brand wall: featured active brands (return slugs)
            var brandWall = await _db.Brands.AsNoTracking()
                .Where(b => b.IsActive && b.IsFeatured)
                .OrderBy(b => b.SortOrder)
                .Select(b => b.Slug)
                .ToArrayAsync();

            // Store teaser: pick first active store
            var storeTeaser = await _db.Stores.AsNoTracking()
                .Where(s => s.IsActive)
                .OrderBy(s => s.SortOrder)
                .Select(s => new StoreTeaserDto(s.Name, s.Slug, s.CoverImageUrl ?? string.Empty))
                .FirstOrDefaultAsync();

            // Category cards: try to map from modules if payload contains category slugs; fallback to empty
            ProductCardDto[] categoryCards = Array.Empty<ProductCardDto>();
            try
            {
                var modules = page.Modules?.ToList();
                if (modules != null)
                {
                    // find modules that explicitly list product slugs or category slugs
                    var firstModule = modules.FirstOrDefault();
                    if (firstModule != null && firstModule.Payload is JsonElement je)
                    {
                        if (je.TryGetProperty("productSlugs", out var prodSlugs) && prodSlugs.ValueKind == JsonValueKind.Array)
                        {
                            var slugs = prodSlugs.EnumerateArray().Where(x => x.ValueKind == JsonValueKind.String).Select(x => x.GetString()!).ToArray();
                            if (slugs.Length > 0)
                            {
                                categoryCards = await _db.Products.AsNoTracking().Where(p => slugs.Contains(p.Slug) && p.IsVisible && p.IsPurchasable).Select(p => MapToProductCard(p)).ToArrayAsync();
                            }
                        }
                    }
                }
            }
            catch
            {
                categoryCards = Array.Empty<ProductCardDto>();
            }

            return new HomePageDto(
                seo,
                AnnouncementBar: null,
                new HeroSectionDto(page.Title, string.Empty, string.Empty),
                categoryCards,
                newArrivals,
                featuredCollections,
                bestsellers,
                brandWall,
                EditorialStatement: null,
                storeTeaser,
                TrustItems: Array.Empty<TrustItemDto>(),
                Newsletter: null
            );
        }

        private static ProductCardDto MapToProductCard(TrendplusProdavnica.Domain.Catalog.Product p)
        {
            // Note: this method will be used only inside EF projection via Select, so must be translatable.
            // We construct using client-evaluable helpers in projection where necessary.
            return new ProductCardDto(
                p.Id,
                p.Slug,
                "", // brand name filled in projection by caller when using DbContext
                p.Name,
                p.Media.Where(m => m.IsPrimary).Select(m => m.Url).FirstOrDefault() ?? p.Media.OrderBy(m => m.SortOrder).Select(m => m.Url).FirstOrDefault() ?? string.Empty,
                p.Media.Where(m => !m.IsPrimary).OrderBy(m => m.SortOrder).Select(m => m.Url).FirstOrDefault(),
                p.Variants.OrderBy(v => v.Price).Select(v => v.Price).FirstOrDefault(),
                p.Variants.OrderBy(v => v.Price).Select(v => v.OldPrice).FirstOrDefault(),
                p.Variants.Select(v => v.Currency).FirstOrDefault() ?? "RSD",
                new string[] { p.IsNew ? "Novi" : string.Empty }.Where(s => !string.IsNullOrEmpty(s)).ToArray(),
                p.Variants.Any(v => v.StockStatus != Domain.Enums.StockStatus.OutOfStock && v.IsActive),
                p.Variants.Count(v => v.IsActive),
                p.PrimaryColorName
            );
        }

        private static HomePageDto EmptyHomePage() => new HomePageDto(
            new SeoDto(string.Empty, string.Empty, null, null),
            null,
            new HeroSectionDto(string.Empty, string.Empty, string.Empty),
            Array.Empty<ProductCardDto>(),
            Array.Empty<ProductCardDto>(),
            Array.Empty<ProductCardDto>(),
            Array.Empty<ProductCardDto>(),
            Array.Empty<string>(),
            null,
            null,
            Array.Empty<TrustItemDto>(),
            null
        );
    }
}
