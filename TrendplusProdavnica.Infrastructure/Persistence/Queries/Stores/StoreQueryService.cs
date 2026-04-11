#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrendplusProdavnica.Application.Catalog.Dtos;
using TrendplusProdavnica.Application.Stores.Dtos;
using TrendplusProdavnica.Application.Stores.Queries;
using TrendplusProdavnica.Application.Stores.Services;
using TrendplusProdavnica.Domain.Enums;
using TrendplusProdavnica.Infrastructure.Persistence;
using TrendplusProdavnica.Infrastructure.Persistence.Queries.Catalog;

namespace TrendplusProdavnica.Infrastructure.Persistence.Queries.Stores
{
    public class StoreQueryService : IStoreQueryService
    {
        private readonly TrendplusDbContext _db;

        public StoreQueryService(TrendplusDbContext db)
        {
            _db = db;
        }

        public async Task<StoreCardDto[]> GetStoresAsync(GetStoresQuery query)
        {
            var page = Math.Max(1, query.Page);
            var pageSize = query.PageSize <= 0 ? 20 : Math.Min(query.PageSize, 100);

            var stores = _db.Stores.AsNoTracking()
                .Where(store => store.IsActive);

            if (!string.IsNullOrWhiteSpace(query.City))
            {
                var normalizedCity = query.City.Trim().ToLowerInvariant();
                stores = stores.Where(store => store.City.ToLower() == normalizedCity);
            }

            return await stores
                .OrderBy(store => store.City)
                .ThenBy(store => store.SortOrder)
                .ThenBy(store => store.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(store => new StoreCardDto(
                    store.Name,
                    store.Slug,
                    store.City,
                    store.AddressLine1,
                    store.WorkingHoursText ?? string.Empty,
                    store.Phone ?? string.Empty,
                    store.CoverImageUrl))
                .ToArrayAsync();
        }

        public async Task<StorePageDto> GetStorePageAsync(GetStorePageQuery query)
        {
            var store = await _db.Stores.AsNoTracking()
                .Where(entity => entity.Slug == query.Slug && entity.IsActive)
                .Select(entity => new
                {
                    entity.Id,
                    entity.Name,
                    entity.Slug,
                    entity.City,
                    entity.AddressLine1,
                    entity.AddressLine2,
                    entity.PostalCode,
                    entity.MallName,
                    entity.Phone,
                    entity.Email,
                    entity.Latitude,
                    entity.Longitude,
                    entity.WorkingHoursText,
                    entity.ShortDescription,
                    entity.CoverImageUrl,
                    entity.Seo
                })
                .FirstOrDefaultAsync();

            if (store is null)
            {
                throw new KeyNotFoundException($"Store '{query.Slug}' was not found.");
            }

            var content = await _db.StorePageContents.AsNoTracking()
                .Where(entity => entity.StoreId == store.Id && entity.IsPublished)
                .Select(entity => new
                {
                    entity.IntroText,
                    entity.HeroImageUrl,
                    entity.Seo
                })
                .FirstOrDefaultAsync();

            var availabilityRowsRaw = await (
                    from inventory in _db.StoreInventory.AsNoTracking()
                    join variant in _db.ProductVariants.AsNoTracking() on inventory.VariantId equals variant.Id
                    join product in _db.Products.AsNoTracking() on variant.ProductId equals product.Id
                    where inventory.StoreId == store.Id &&
                          (inventory.QuantityOnHand - inventory.ReservedQuantity) > 0 &&
                          variant.IsActive &&
                          variant.IsVisible &&
                          product.Status == ProductStatus.Published &&
                          product.IsVisible &&
                          product.IsPurchasable
                    select new
                    {
                        ProductId = product.Id,
                        product.BrandId,
                        product.PrimaryCategoryId
                    })
                .Distinct()
                .ToArrayAsync();
            var availabilityRows = availabilityRowsRaw
                .Select(row => new StoreAvailabilityProductRow(
                    row.ProductId,
                    row.BrandId,
                    row.PrimaryCategoryId))
                .ToArray();

            var featuredBrands = await BuildFeaturedBrandsAsync(availabilityRows);
            var featuredCategories = await BuildFeaturedCategoriesAsync(availabilityRows);

            var seo = ProductQueryMappingHelper.MapSeo(
                content?.Seo ?? store.Seo,
                store.Name,
                store.ShortDescription ?? string.Empty);

            return new StorePageDto(
                store.Name,
                store.Slug,
                store.City,
                store.AddressLine1,
                store.AddressLine2,
                store.PostalCode ?? string.Empty,
                store.MallName,
                store.Phone ?? string.Empty,
                store.Email ?? string.Empty,
                store.Latitude ?? 0m,
                store.Longitude ?? 0m,
                store.WorkingHoursText ?? string.Empty,
                content?.IntroText ?? store.ShortDescription ?? string.Empty,
                content?.HeroImageUrl ?? store.CoverImageUrl ?? string.Empty,
                seo,
                featuredCategories.Cast<object>().ToArray(),
                featuredBrands.Cast<object>().ToArray());
        }

        private async Task<BreadcrumbItemDto[]> BuildFeaturedBrandsAsync(
            IEnumerable<StoreAvailabilityProductRow> availabilityRows)
        {
            var rows = availabilityRows.ToArray();

            if (rows.Length == 0)
            {
                return Array.Empty<BreadcrumbItemDto>();
            }

            var brandCounts = rows
                .GroupBy(row => row.BrandId)
                .Select(group => new
                {
                    BrandId = group.Key,
                    Count = group.Count()
                })
                .OrderByDescending(item => item.Count)
                .ThenBy(item => item.BrandId)
                .Take(8)
                .ToArray();

            var brandIds = brandCounts.Select(item => item.BrandId).ToArray();
            var brands = await _db.Brands.AsNoTracking()
                .Where(brand => brandIds.Contains(brand.Id) && brand.IsActive)
                .Select(brand => new { brand.Id, brand.Name, brand.Slug })
                .ToArrayAsync();

            var brandById = brands.ToDictionary(brand => brand.Id);

            return brandCounts
                .Where(item => brandById.ContainsKey(item.BrandId))
                .Select(item =>
                {
                    var brand = brandById[item.BrandId];
                    return new BreadcrumbItemDto(brand.Name, $"/brendovi/{brand.Slug}");
                })
                .ToArray();
        }

        private async Task<BreadcrumbItemDto[]> BuildFeaturedCategoriesAsync(
            IEnumerable<StoreAvailabilityProductRow> availabilityRows)
        {
            var rows = availabilityRows.ToArray();

            if (rows.Length == 0)
            {
                return Array.Empty<BreadcrumbItemDto>();
            }

            var productIds = rows.Select(row => row.ProductId).Distinct().ToArray();
            var productCategoryPairs = new HashSet<(long ProductId, long CategoryId)>();

            foreach (var row in rows)
            {
                productCategoryPairs.Add((row.ProductId, row.PrimaryCategoryId));
            }

            var mappedCategories = await _db.ProductCategoryMaps.AsNoTracking()
                .Where(map => productIds.Contains(map.ProductId))
                .Select(map => new { map.ProductId, map.CategoryId })
                .ToArrayAsync();

            foreach (var item in mappedCategories)
            {
                productCategoryPairs.Add((item.ProductId, item.CategoryId));
            }

            var categoryCounts = productCategoryPairs
                .GroupBy(item => item.CategoryId)
                .Select(group => new
                {
                    CategoryId = group.Key,
                    Count = group.Count()
                })
                .OrderByDescending(item => item.Count)
                .ThenBy(item => item.CategoryId)
                .Take(8)
                .ToArray();

            var categoryIds = categoryCounts.Select(item => item.CategoryId).ToArray();
            var categories = await _db.Categories.AsNoTracking()
                .Where(category => categoryIds.Contains(category.Id) && category.IsActive)
                .Select(category => new { category.Id, category.Name, category.Slug })
                .ToArrayAsync();

            var categoryById = categories.ToDictionary(category => category.Id);

            return categoryCounts
                .Where(item => categoryById.ContainsKey(item.CategoryId))
                .Select(item =>
                {
                    var category = categoryById[item.CategoryId];
                    return new BreadcrumbItemDto(category.Name, $"/{category.Slug}");
                })
                .ToArray();
        }

        private sealed record StoreAvailabilityProductRow(
            long ProductId,
            long BrandId,
            long PrimaryCategoryId);
    }
}
