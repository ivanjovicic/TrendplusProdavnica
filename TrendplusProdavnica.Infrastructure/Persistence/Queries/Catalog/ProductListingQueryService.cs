#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrendplusProdavnica.Application.Catalog.Dtos;
using TrendplusProdavnica.Application.Catalog.Queries;
using TrendplusProdavnica.Application.Catalog.Services;
using TrendplusProdavnica.Infrastructure.Persistence;

namespace TrendplusProdavnica.Infrastructure.Persistence.Queries.Catalog
{
    public class ProductListingQueryService : IProductListingQueryService
    {
        private readonly TrendplusDbContext _db;
        public ProductListingQueryService(TrendplusDbContext db) => _db = db;

        public async Task<ProductListingPageDto> GetCategoryListingAsync(GetCategoryListingQuery query)
        {
            var page = query.Page < 1 ? 1 : query.Page;
            var pageSize = query.PageSize <= 0 ? 24 : query.PageSize;
            var isSaleSlug = string.Equals(query.Slug, "sale", StringComparison.OrdinalIgnoreCase);

            var q = _db.Products.AsNoTracking()
                .Where(p => p.IsVisible && p.IsPurchasable && p.Status == Domain.Enums.ProductStatus.Published);

            // filter by category slug if provided
            if (!string.IsNullOrWhiteSpace(query.Slug) && !isSaleSlug)
            {
                var categoryId = await _db.Categories.AsNoTracking().Where(c => c.Slug == query.Slug).Select(c => c.Id).FirstOrDefaultAsync();
                if (categoryId <= 0)
                {
                    throw new System.Collections.Generic.KeyNotFoundException($"Category '{query.Slug}' not found.");
                }

                q = q.Where(p => p.PrimaryCategoryId == categoryId || p.CategoryMaps.Any(cm => cm.CategoryId == categoryId));
            }

            // sale special: products with variants where OldPrice > Price
            if (isSaleSlug)
            {
                q = q.Where(p => p.Variants.Any(v => v.OldPrice != null && v.OldPrice > v.Price));
            }

            // Apply inStock filter
            if (query.InStockOnly == true)
            {
                q = q.Where(p => p.Variants.Any(v => v.StockStatus != Domain.Enums.StockStatus.OutOfStock && v.IsActive));
            }

            var total = await q.LongCountAsync();

            var items = await q
                .OrderByDescending(p => p.IsNew)
                .ThenByDescending(p => p.SortRank)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductCardDto(
                    p.Id,
                    p.Slug,
                    // Brand name via subquery
                    _db.Brands.Where(b => b.Id == p.BrandId).Select(b => b.Name).FirstOrDefault() ?? string.Empty,
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
                ))
                .ToArrayAsync();

            var pagination = new PaginationDto(page, pageSize, total);

            return new ProductListingPageDto(
                Title: "Proizvodi",
                Description: string.Empty,
                Seo: new SeoDto("Proizvodi", string.Empty, null, null),
                Breadcrumbs: new BreadcrumbItemDto[0],
                IntroTitle: null,
                IntroText: null,
                Products: items,
                Facets: new FilterFacetDto[0],
                AppliedFilters: new AppliedFilterDto[0],
                Pagination: pagination,
                MerchBlocks: new object[0],
                Faq: null
            );
        }

        public async Task<ProductListingPageDto> GetBrandListingAsync(GetBrandListingQuery query)
        {
            var page = query.Page < 1 ? 1 : query.Page;
            var pageSize = query.PageSize <= 0 ? 24 : query.PageSize;
            var brandId = await _db.Brands.AsNoTracking().Where(b => b.Slug == query.Slug).Select(b => b.Id).FirstOrDefaultAsync();
            if (brandId <= 0)
            {
                throw new System.Collections.Generic.KeyNotFoundException($"Brand '{query.Slug}' not found.");
            }

            // reuse main method but apply brand filter
            var q = _db.Products.AsNoTracking().Where(p => p.BrandId == brandId && p.IsVisible && p.IsPurchasable && p.Status == Domain.Enums.ProductStatus.Published);

            var total = await q.LongCountAsync();
            var items = await q.OrderByDescending(p => p.IsNew).ThenByDescending(p => p.SortRank)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(p => new ProductCardDto(
                    p.Id,
                    p.Slug,
                    _db.Brands.Where(b => b.Id == p.BrandId).Select(b => b.Name).FirstOrDefault() ?? string.Empty,
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
                ))
                .ToArrayAsync();

            var pagination = new PaginationDto(page, pageSize, total);
            return new ProductListingPageDto("Proizvodi", string.Empty, new SeoDto("Proizvodi", string.Empty, null, null), new BreadcrumbItemDto[0], null, null, items, new FilterFacetDto[0], new AppliedFilterDto[0], pagination, new object[0], null);
        }

        public async Task<ProductListingPageDto> GetCollectionListingAsync(GetCollectionListingQuery query)
        {
            var page = query.Page < 1 ? 1 : query.Page;
            var pageSize = query.PageSize <= 0 ? 24 : query.PageSize;
            var collectionId = await _db.Collections.AsNoTracking().Where(c => c.Slug == query.Slug).Select(c => c.Id).FirstOrDefaultAsync();
            if (collectionId <= 0)
            {
                throw new System.Collections.Generic.KeyNotFoundException($"Collection '{query.Slug}' not found.");
            }

            var q = _db.Products.AsNoTracking().Where(p => p.CollectionMaps.Any(cm => cm.CollectionId == collectionId) && p.IsVisible && p.IsPurchasable && p.Status == Domain.Enums.ProductStatus.Published);

            var total = await q.LongCountAsync();
            var items = await q.OrderByDescending(p => p.IsNew).ThenByDescending(p => p.SortRank)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(p => new ProductCardDto(
                    p.Id,
                    p.Slug,
                    _db.Brands.Where(b => b.Id == p.BrandId).Select(b => b.Name).FirstOrDefault() ?? string.Empty,
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
                ))
                .ToArrayAsync();

            var pagination = new PaginationDto(page, pageSize, total);
            return new ProductListingPageDto("Proizvodi", string.Empty, new SeoDto("Proizvodi", string.Empty, null, null), new BreadcrumbItemDto[0], null, null, items, new FilterFacetDto[0], new AppliedFilterDto[0], pagination, new object[0], null);
        }

        public Task<ProductListingPageDto> GetSaleListingAsync(GetSaleListingQuery query) => GetCategoryListingAsync(new GetCategoryListingQuery("sale", query.Page, query.PageSize));
    }
}
