#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrendplusProdavnica.Application.Catalog.Dtos;
using TrendplusProdavnica.Application.Catalog.Services;
using TrendplusProdavnica.Domain.Enums;
using TrendplusProdavnica.Domain.ValueObjects;
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
                    h.Modules
                })
                .FirstOrDefaultAsync();

            if (page is null)
            {
                return EmptyHomePage();
            }

            var seo = new SeoDto(
                page.Seo?.SeoTitle ?? page.Title,
                page.Seo?.SeoDescription ?? string.Empty,
                page.Seo?.CanonicalUrl,
                null);

            var liveProducts = _db.Products.AsNoTracking()
                .Where(p => p.Status == ProductStatus.Published && p.IsVisible && p.IsPurchasable);

            var newArrivals = await GetProductCardsAsync(
                liveProducts
                    .Where(p => p.IsNew)
                    .OrderByDescending(p => p.PublishedAtUtc),
                12);

            var bestsellers = await GetProductCardsAsync(
                liveProducts
                    .Where(p => p.IsBestseller)
                    .OrderByDescending(p => p.SortRank),
                12);

            var featuredCollectionIds = _db.Collections.AsNoTracking()
                .Where(c => c.IsFeatured && c.IsActive)
                .Select(c => c.Id);

            var featuredCollections = await GetProductCardsAsync(
                liveProducts
                    .Where(p => p.CollectionMaps.Any(cm => featuredCollectionIds.Contains(cm.CollectionId)))
                    .OrderByDescending(p => p.SortRank),
                12);

            var brandWall = await _db.Brands.AsNoTracking()
                .Where(b => b.IsActive && b.IsFeatured)
                .OrderBy(b => b.SortOrder)
                .Select(b => b.Slug)
                .ToArrayAsync();

            var storeTeaser = await _db.Stores.AsNoTracking()
                .Where(s => s.IsActive)
                .OrderBy(s => s.SortOrder)
                .Select(s => new StoreTeaserDto(s.Name, s.Slug, s.CoverImageUrl ?? string.Empty))
                .FirstOrDefaultAsync();

            ProductCardDto[] categoryCards = Array.Empty<ProductCardDto>();
            try
            {
                var productSlugs = ExtractModuleProductSlugs(page.Modules);
                if (productSlugs.Length > 0)
                {
                    categoryCards = await GetProductCardsAsync(
                        liveProducts
                            .Where(p => productSlugs.Contains(p.Slug))
                            .OrderBy(p => p.Name),
                        productSlugs.Length);
                }
            }
            catch
            {
                categoryCards = Array.Empty<ProductCardDto>();
            }

            return new HomePageDto(
                seo,
                AnnouncementBar: null,
                HeroSection: new HeroSectionDto(page.Title, string.Empty, string.Empty),
                CategoryCards: categoryCards,
                NewArrivals: newArrivals,
                FeaturedCollections: featuredCollections,
                Bestsellers: bestsellers,
                BrandWall: brandWall,
                EditorialStatement: null,
                StoreTeaser: storeTeaser,
                TrustItems: Array.Empty<TrustItemDto>(),
                Newsletter: null);
        }

        private async Task<ProductCardDto[]> GetProductCardsAsync(IQueryable<Domain.Catalog.Product> query, int take)
        {
            var projected = await query
                .Take(take)
                .Select(p => new ProductCardProjection(
                    p.Id,
                    p.Slug,
                    _db.Brands.Where(b => b.Id == p.BrandId).Select(b => b.Name).FirstOrDefault() ?? string.Empty,
                    p.Name,
                    p.Media.Where(m => m.IsPrimary).Select(m => m.Url).FirstOrDefault()
                        ?? p.Media.OrderBy(m => m.SortOrder).Select(m => m.Url).FirstOrDefault()
                        ?? string.Empty,
                    p.Media.Where(m => !m.IsPrimary).OrderBy(m => m.SortOrder).Select(m => m.Url).FirstOrDefault(),
                    p.Variants.OrderBy(v => v.Price).Select(v => v.Price).FirstOrDefault(),
                    p.Variants.OrderBy(v => v.Price).Select(v => v.OldPrice).FirstOrDefault(),
                    p.Variants.Select(v => v.Currency).FirstOrDefault() ?? "RSD",
                    p.IsNew,
                    p.Variants.Any(v => v.StockStatus != StockStatus.OutOfStock && v.IsActive),
                    p.Variants.Count(v => v.IsActive),
                    p.PrimaryColorName))
                .ToArrayAsync();

            return projected.Select(MapToProductCard).ToArray();
        }

        private static ProductCardDto MapToProductCard(ProductCardProjection projection)
        {
            var badges = projection.IsNew ? new[] { "Novi" } : Array.Empty<string>();

            return new ProductCardDto(
                projection.Id,
                projection.Slug,
                projection.BrandName,
                projection.Name,
                projection.PrimaryImageUrl,
                projection.SecondaryImageUrl,
                projection.Price,
                projection.OldPrice,
                projection.Currency,
                badges,
                projection.IsInStock,
                projection.AvailableSizesCount,
                projection.ColorLabel);
        }

        private static string[] ExtractModuleProductSlugs(IEnumerable<HomeModule> modules)
        {
            var slugs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var module in modules)
            {
                if (module.Payload is not JsonElement payload || payload.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                if (!payload.TryGetProperty("productSlugs", out var productSlugs) || productSlugs.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var item in productSlugs.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String)
                    {
                        var slug = item.GetString();
                        if (!string.IsNullOrWhiteSpace(slug))
                        {
                            slugs.Add(slug);
                        }
                    }
                }
            }

            return slugs.ToArray();
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
            null);

        private sealed record ProductCardProjection(
            long Id,
            string Slug,
            string BrandName,
            string Name,
            string PrimaryImageUrl,
            string? SecondaryImageUrl,
            decimal Price,
            decimal? OldPrice,
            string Currency,
            bool IsNew,
            bool IsInStock,
            int AvailableSizesCount,
            string? ColorLabel);
    }
}
