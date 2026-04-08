#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrendplusProdavnica.Application.Catalog.Dtos;
using TrendplusProdavnica.Application.Catalog.Queries;
using TrendplusProdavnica.Application.Catalog.Services;
using TrendplusProdavnica.Domain.Catalog;
using TrendplusProdavnica.Domain.Enums;
using TrendplusProdavnica.Infrastructure.Persistence;

namespace TrendplusProdavnica.Infrastructure.Persistence.Queries.Catalog
{
    public class ProductListingQueryService : IProductListingQueryService
    {
        private readonly TrendplusDbContext _db;
        public ProductListingQueryService(TrendplusDbContext db) => _db = db;

        public Task<ProductListingPageDto> GetCategoryListingAsync(GetCategoryListingQuery query)
            => BuildListingAsync(query, ListingScope.Category);

        public Task<ProductListingPageDto> GetBrandListingAsync(GetBrandListingQuery query)
            => BuildListingAsync(ToSearchQuery(query), ListingScope.Brand);

        public Task<ProductListingPageDto> GetCollectionListingAsync(GetCollectionListingQuery query)
            => BuildListingAsync(ToSearchQuery(query), ListingScope.Collection);

        public Task<ProductListingPageDto> GetSaleListingAsync(GetSaleListingQuery query)
            => BuildListingAsync(ToSearchQuery(query), ListingScope.Sale);

        private static GetCategoryListingQuery ToSearchQuery(GetBrandListingQuery query)
            => new(query.Slug, query.Page, query.PageSize, query.Sort, query.Sizes, query.Colors, query.Brands, query.PriceFrom, query.PriceTo, query.IsOnSale, query.IsNew, query.InStockOnly);

        private static GetCategoryListingQuery ToSearchQuery(GetCollectionListingQuery query)
            => new(query.Slug, query.Page, query.PageSize, query.Sort, query.Sizes, query.Colors, query.Brands, query.PriceFrom, query.PriceTo, query.IsOnSale, query.IsNew, query.InStockOnly);

        private static GetCategoryListingQuery ToSearchQuery(GetSaleListingQuery query)
            => new("sale", query.Page, query.PageSize, query.Sort, query.Sizes, query.Colors, query.Brands, query.PriceFrom, query.PriceTo, query.IsOnSale, query.IsNew, query.InStockOnly);

        private async Task<ProductListingPageDto> BuildListingAsync(GetCategoryListingQuery query, ListingScope scope)
        {
            var page = Math.Max(query.Page, 1);
            var pageSize = query.PageSize > 0 ? query.PageSize : 24;

            var products = _db.Products.AsNoTracking()
                .Where(p => p.Status == ProductStatus.Published && p.IsVisible && p.IsPurchasable);

            if (scope == ListingScope.Category && !string.IsNullOrWhiteSpace(query.Slug) && !string.Equals(query.Slug, "sale", StringComparison.OrdinalIgnoreCase))
            {
                var categoryId = await _db.Categories.AsNoTracking().Where(c => c.Slug == query.Slug).Select(c => c.Id).FirstOrDefaultAsync();
                if (categoryId <= 0)
                    throw new KeyNotFoundException($"Category '{query.Slug}' not found.");

                products = products.Where(p => p.PrimaryCategoryId == categoryId || p.CategoryMaps.Any(cm => cm.CategoryId == categoryId));
            }

            if (scope == ListingScope.Brand)
            {
                var brandId = await _db.Brands.AsNoTracking().Where(b => b.Slug == query.Slug).Select(b => b.Id).FirstOrDefaultAsync();
                if (brandId <= 0)
                    throw new KeyNotFoundException($"Brand '{query.Slug}' not found.");

                products = products.Where(p => p.BrandId == brandId);
            }

            if (scope == ListingScope.Collection)
            {
                var collectionId = await _db.Collections.AsNoTracking().Where(c => c.Slug == query.Slug).Select(c => c.Id).FirstOrDefaultAsync();
                if (collectionId <= 0)
                    throw new KeyNotFoundException($"Collection '{query.Slug}' not found.");

                products = products.Where(p => p.CollectionMaps.Any(cm => cm.CollectionId == collectionId));
            }

            if (scope == ListingScope.Sale || query.IsOnSale == true)
            {
                products = products.Where(p => p.Variants.Any(v => v.IsActive && v.IsVisible && v.OldPrice != null && v.OldPrice > v.Price));
            }

            products = ApplyFilters(products, query);
            products = ApplySort(products, query.Sort);

            var total = await products.LongCountAsync();

            var productCards = await products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductCardProjection(
                    p.Id,
                    p.Slug,
                    _db.Brands.Where(b => b.Id == p.BrandId).Select(b => b.Name).FirstOrDefault() ?? string.Empty,
                    p.Name,
                    p.PrimaryColorName,
                    p.Media.Where(m => m.IsActive && m.IsPrimary).Select(m => m.Url).FirstOrDefault(),
                    p.Media.Where(m => m.IsActive && !m.IsPrimary).OrderBy(m => m.SortOrder).Select(m => m.Url).FirstOrDefault(),
                    p.Media.Where(m => m.IsActive).OrderBy(m => m.SortOrder).Select(m => m.Url).FirstOrDefault(),
                    p.Variants.Where(v => v.IsActive && v.IsVisible).OrderBy(v => v.Price).Select(v => v.Price).FirstOrDefault(),
                    p.Variants.Where(v => v.IsActive && v.IsVisible).OrderBy(v => v.Price).Select(v => v.OldPrice).FirstOrDefault(),
                    p.Variants.Where(v => v.IsActive && v.IsVisible).Select(v => v.Currency).FirstOrDefault() ?? "RSD",
                    p.IsNew,
                    p.IsBestseller,
                    p.Variants.Any(v => v.IsActive && v.IsVisible && v.OldPrice != null && v.OldPrice > v.Price),
                    p.Variants.Any(v => v.IsActive && v.IsVisible && v.StockStatus != StockStatus.OutOfStock),
                    p.Variants.Count(v => v.IsActive && v.IsVisible)
                ))
                .ToArrayAsync();

            var cards = productCards.Select(MapProjectionToDto).ToArray();
            var facets = await BuildFacetsAsync(products, query);
            var appliedFilters = BuildAppliedFilters(query, scope);
            var (title, description, seo, breadcrumbs, introTitle, introText) = await BuildPageMetadata(scope, query);

            return new ProductListingPageDto(
                title,
                description,
                seo,
                breadcrumbs,
                introTitle,
                introText,
                cards,
                facets,
                appliedFilters,
                new PaginationDto(page, pageSize, total),
                Array.Empty<object>(),
                null
            );
        }

        private async Task<(string Title, string Description, SeoDto Seo, BreadcrumbItemDto[] Breadcrumbs, string? IntroTitle, string? IntroText)> BuildPageMetadata(ListingScope scope, GetCategoryListingQuery query)
        {
            var title = "Proizvodi";
            var description = string.Empty;
            var breadcrumbs = new List<BreadcrumbItemDto>();
            var introTitle = (string?)null;
            var introText = (string?)null;
            var seoTitle = "Proizvodi";
            var seoDescription = string.Empty;

            if (scope == ListingScope.Category && !string.IsNullOrWhiteSpace(query.Slug) && !string.Equals(query.Slug, "sale", StringComparison.OrdinalIgnoreCase))
            {
                var category = await _db.Categories.AsNoTracking().Where(c => c.Slug == query.Slug).Select(c => new { c.Name, c.Slug }).FirstOrDefaultAsync();
                if (category != null)
                {
                    title = category.Name;
                    seoTitle = category.Name;
                    breadcrumbs.Add(new BreadcrumbItemDto(category.Name, $"/kategorija/{category.Slug}"));
                }
            }
            else if (scope == ListingScope.Brand)
            {
                var brand = await _db.Brands.AsNoTracking().Where(b => b.Slug == query.Slug).Select(b => new { b.Name, b.Slug }).FirstOrDefaultAsync();
                if (brand != null)
                {
                    title = brand.Name;
                    seoTitle = brand.Name;
                    breadcrumbs.Add(new BreadcrumbItemDto(brand.Name, $"/brand/{brand.Slug}"));
                }
            }
            else if (scope == ListingScope.Collection)
            {
                var collection = await _db.Collections.AsNoTracking().Where(c => c.Slug == query.Slug).Select(c => new { c.Name, c.Slug }).FirstOrDefaultAsync();
                if (collection != null)
                {
                    title = collection.Name;
                    seoTitle = collection.Name;
                    breadcrumbs.Add(new BreadcrumbItemDto(collection.Name, $"/kolekcija/{collection.Slug}"));
                }
            }
            else if (scope == ListingScope.Sale)
            {
                title = "Akcijski proizvodi";
                seoTitle = "Akcijski proizvodi";
                breadcrumbs.Add(new BreadcrumbItemDto("Akcija", "/akcija"));
            }

            return (title, description, new SeoDto(seoTitle, seoDescription, null, null), breadcrumbs.ToArray(), introTitle, introText);
        }

        private IQueryable<Product> ApplyFilters(IQueryable<Product> products, GetCategoryListingQuery query)
        {
            if (query.Brands?.Length > 0)
            {
                products = products.Where(p => query.Brands.Contains(p.BrandId));
            }

            if (query.Sizes?.Length > 0)
            {
                var requestedSizes = query.Sizes.Select(size => (decimal)size).ToArray();
                products = products.Where(p => p.Variants.Any(v => v.IsActive && v.IsVisible && requestedSizes.Contains(v.SizeEu)));
            }

            if (query.Colors?.Length > 0)
            {
                products = products.Where(p => query.Colors.Any(color => p.PrimaryColorName == color || p.Variants.Any(v => v.IsActive && v.IsVisible && v.ColorName == color)));
            }

            if (query.PriceFrom.HasValue)
            {
                products = products.Where(p => p.Variants.Any(v => v.IsActive && v.IsVisible && v.Price >= query.PriceFrom.Value));
            }

            if (query.PriceTo.HasValue)
            {
                products = products.Where(p => p.Variants.Any(v => v.IsActive && v.IsVisible && v.Price <= query.PriceTo.Value));
            }

            if (query.IsNew == true)
            {
                products = products.Where(p => p.IsNew);
            }

            if (query.InStockOnly == true)
            {
                products = products.Where(p => p.Variants.Any(v => v.IsActive && v.IsVisible && v.StockStatus != StockStatus.OutOfStock));
            }

            return products;
        }

        private IQueryable<Product> ApplySort(IQueryable<Product> products, string? sort)
        {
            return sort?.ToLowerInvariant() switch
            {
                "newest" => products.OrderByDescending(p => p.PublishedAtUtc),
                "price_asc" => products.OrderBy(p => p.Variants.Where(v => v.IsActive && v.IsVisible).Min(v => v.Price)),
                "price_desc" => products.OrderByDescending(p => p.Variants.Where(v => v.IsActive && v.IsVisible).Min(v => v.Price)),
                "bestsellers" => products.OrderByDescending(p => p.IsBestseller).ThenByDescending(p => p.SortRank),
                _ => products.OrderByDescending(p => p.IsNew).ThenByDescending(p => p.SortRank)
            };
        }

        private async Task<FilterFacetDto[]> BuildFacetsAsync(IQueryable<Product> products, GetCategoryListingQuery query)
        {
            var sizeOptions = await products
                .SelectMany(p => p.Variants.Where(v => v.IsActive && v.IsVisible).Select(v => v.SizeEu))
                .Distinct()
                .OrderBy(size => size)
                .Select(size => new FilterOptionDto(size.ToString(), size.ToString(), products.Count(p => p.Variants.Any(v => v.IsActive && v.IsVisible && v.SizeEu == size)), false, false))
                .ToArrayAsync();

            var colorOptions = await products
                .SelectMany(p => p.Variants.Where(v => v.IsActive && v.IsVisible).Select(v => v.ColorName ?? p.PrimaryColorName ?? string.Empty))
                .Where(color => !string.IsNullOrEmpty(color))
                .Distinct()
                .OrderBy(color => color)
                .Select(color => new FilterOptionDto(color, color, products.Count(p => p.PrimaryColorName == color || p.Variants.Any(v => v.IsActive && v.IsVisible && v.ColorName == color)), false, false))
                .ToArrayAsync();

            var brandOptions = await products
                .GroupBy(p => p.BrandId)
                .Select(g => new
                {
                    BrandId = g.Key,
                    Count = g.LongCount(),
                    Name = _db.Brands.Where(b => b.Id == g.Key).Select(b => b.Name).FirstOrDefault() ?? string.Empty
                })
                .OrderBy(b => b.Name)
                .Select(b => new FilterOptionDto(b.BrandId.ToString(), b.Name, (int)b.Count, false, false))
                .ToArrayAsync();

            var priceOptions = new[]
            {
                new FilterOptionDto("0-1999", "do 1.999 RSD", await products.CountAsync(p => p.Variants.Any(v => v.IsActive && v.IsVisible && v.Price <= 1999)), false, false),
                new FilterOptionDto("2000-4999", "2.000-4.999 RSD", await products.CountAsync(p => p.Variants.Any(v => v.IsActive && v.IsVisible && v.Price >= 2000 && v.Price <= 4999)), false, false),
                new FilterOptionDto("5000-9999", "5.000-9.999 RSD", await products.CountAsync(p => p.Variants.Any(v => v.IsActive && v.IsVisible && v.Price >= 5000 && v.Price <= 9999)), false, false),
                new FilterOptionDto("10000+", "10.000+ RSD", await products.CountAsync(p => p.Variants.Any(v => v.IsActive && v.IsVisible && v.Price >= 10000)), false, false)
            };

            var boolOptions = new[]
            {
                new FilterFacetDto("isOnSale", "Na akciji", "boolean", new[] { new FilterOptionDto("true", "Da", await products.CountAsync(p => p.Variants.Any(v => v.IsActive && v.IsVisible && v.OldPrice != null && v.OldPrice > v.Price)), false, false) }),
                new FilterFacetDto("isNew", "Novo", "boolean", new[] { new FilterOptionDto("true", "Da", await products.CountAsync(p => p.IsNew), false, false) }),
                new FilterFacetDto("inStockOnly", "Na stanju", "boolean", new[] { new FilterOptionDto("true", "Da", await products.CountAsync(p => p.Variants.Any(v => v.IsActive && v.IsVisible && v.StockStatus != StockStatus.OutOfStock)), false, false) })
            };

            var facets = new List<FilterFacetDto>
            {
                new FilterFacetDto("sizes", "Velicine", "multi", sizeOptions),
                new FilterFacetDto("colors", "Boje", "multi", colorOptions),
                new FilterFacetDto("brands", "Brendovi", "multi", brandOptions),
                new FilterFacetDto("price", "Cena", "range", priceOptions),
            };

            facets.AddRange(boolOptions);
            return facets.ToArray();
        }

        private static AppliedFilterDto[] BuildAppliedFilters(GetCategoryListingQuery query, ListingScope scope)
        {
            var applied = new List<AppliedFilterDto>();

            if (scope == ListingScope.Category && !string.IsNullOrWhiteSpace(query.Slug) && !string.Equals(query.Slug, "sale", StringComparison.OrdinalIgnoreCase))
            {
                applied.Add(new AppliedFilterDto("category", "Kategorija", query.Slug, query.Slug));
            }

            if (scope == ListingScope.Brand && !string.IsNullOrWhiteSpace(query.Slug))
            {
                applied.Add(new AppliedFilterDto("brand", "Brend", query.Slug, query.Slug));
            }

            if (scope == ListingScope.Collection && !string.IsNullOrWhiteSpace(query.Slug))
            {
                applied.Add(new AppliedFilterDto("collection", "Kolekcija", query.Slug, query.Slug));
            }

            if (scope == ListingScope.Sale)
            {
                applied.Add(new AppliedFilterDto("sale", "Akcija", "true", "Akcija"));
            }

            if (query.Sizes?.Length > 0)
            {
                applied.AddRange(query.Sizes.Select(size => new AppliedFilterDto("size", "Velicina", size.ToString(), size.ToString())));
            }

            if (query.Colors?.Length > 0)
            {
                applied.AddRange(query.Colors.Select(color => new AppliedFilterDto("color", "Boja", color, color)));
            }

            if (query.Brands?.Length > 0)
            {
                applied.AddRange(query.Brands.Select(brandId => new AppliedFilterDto("brand", "Brend", brandId.ToString(), brandId.ToString())));
            }

            if (query.PriceFrom.HasValue || query.PriceTo.HasValue)
            {
                var display = $"{query.PriceFrom?.ToString() ?? "0"} - {query.PriceTo?.ToString() ?? "max"}";
                applied.Add(new AppliedFilterDto("price", "Cena", display, display));
            }

            if (query.IsNew == true)
            {
                applied.Add(new AppliedFilterDto("isNew", "Novo", "true", "Novo"));
            }

            if (query.InStockOnly == true)
            {
                applied.Add(new AppliedFilterDto("inStockOnly", "Na stanju", "true", "Na stanju"));
            }

            return applied.ToArray();
        }

        private static ProductCardDto MapProjectionToDto(ProductCardProjection projection)
        {
            var badges = new List<string>();
            if (projection.IsNew) badges.Add("Novi");
            if (projection.IsBestseller) badges.Add("Bestseller");
            if (projection.IsOnSale) badges.Add("Akcija");

            var primaryImage = projection.PrimaryImageUrl ?? projection.FallbackImageUrl ?? string.Empty;
            var secondaryImage = projection.SecondaryImageUrl;

            return new ProductCardDto(
                projection.Id,
                projection.Slug,
                projection.BrandName,
                projection.Name,
                primaryImage,
                secondaryImage,
                projection.Price,
                projection.OldPrice,
                projection.Currency,
                badges.ToArray(),
                projection.InStock,
                projection.AvailableSizesCount,
                projection.ColorLabel
            );
        }

        private sealed record ProductCardProjection(
            long Id,
            string Slug,
            string BrandName,
            string Name,
            string? ColorLabel,
            string? PrimaryImageUrl,
            string? SecondaryImageUrl,
            string? FallbackImageUrl,
            decimal Price,
            decimal? OldPrice,
            string Currency,
            bool IsNew,
            bool IsBestseller,
            bool IsOnSale,
            bool InStock,
            int AvailableSizesCount
        );

        private enum ListingScope
        {
            Category,
            Brand,
            Collection,
            Sale
        }
    }
}

