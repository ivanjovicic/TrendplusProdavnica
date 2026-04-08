#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrendplusProdavnica.Application.Catalog.Dtos;
using TrendplusProdavnica.Application.Catalog.Queries;
using TrendplusProdavnica.Application.Catalog.Services;
using TrendplusProdavnica.Application.Content.Dtos;
using TrendplusProdavnica.Domain.Catalog;
using TrendplusProdavnica.Infrastructure.Persistence;

namespace TrendplusProdavnica.Infrastructure.Persistence.Queries.Catalog
{
    public class ProductListingQueryService : IProductListingQueryService
    {
        private readonly TrendplusDbContext _db;

        public ProductListingQueryService(TrendplusDbContext db)
        {
            _db = db;
        }

        public Task<ProductListingPageDto> GetCategoryListingAsync(GetCategoryListingQuery query)
            => BuildListingAsync(query, ListingScope.Category);

        public Task<ProductListingPageDto> GetBrandListingAsync(GetBrandListingQuery query)
            => BuildListingAsync(ToListingQuery(query), ListingScope.Brand);

        public Task<ProductListingPageDto> GetCollectionListingAsync(GetCollectionListingQuery query)
            => BuildListingAsync(ToListingQuery(query), ListingScope.Collection);

        public Task<ProductListingPageDto> GetSaleListingAsync(GetSaleListingQuery query)
            => BuildListingAsync(ToListingQuery(query), ListingScope.Sale);

        private static GetCategoryListingQuery ToListingQuery(GetBrandListingQuery query)
            => new(
                query.Slug,
                query.Page,
                query.PageSize,
                query.Sort,
                query.Sizes,
                query.Colors,
                query.Brands,
                query.PriceFrom,
                query.PriceTo,
                query.IsOnSale,
                query.IsNew,
                query.InStockOnly);

        private static GetCategoryListingQuery ToListingQuery(GetCollectionListingQuery query)
            => new(
                query.Slug,
                query.Page,
                query.PageSize,
                query.Sort,
                query.Sizes,
                query.Colors,
                query.Brands,
                query.PriceFrom,
                query.PriceTo,
                query.IsOnSale,
                query.IsNew,
                query.InStockOnly);

        private static GetCategoryListingQuery ToListingQuery(GetSaleListingQuery query)
            => new(
                "sale",
                query.Page,
                query.PageSize,
                query.Sort,
                query.Sizes,
                query.Colors,
                query.Brands,
                query.PriceFrom,
                query.PriceTo,
                query.IsOnSale,
                query.IsNew,
                query.InStockOnly);

        private async Task<ProductListingPageDto> BuildListingAsync(GetCategoryListingQuery query, ListingScope scope)
        {
            var page = Math.Max(1, query.Page);
            var pageSize = query.PageSize <= 0 ? 24 : Math.Min(query.PageSize, 100);

            var context = await ResolveScopeContextAsync(scope, query.Slug);
            var baseProducts = BuildScopedProductsQuery(scope, context);
            var filteredProducts = ApplyListingFilters(baseProducts, query, scope);
            var sortedProducts = ApplySort(filteredProducts, query.Sort);

            var totalItems = await filteredProducts.LongCountAsync();
            var projections = ProductQueryMappingHelper.ToProductCardProjection(
                sortedProducts,
                _db.Brands.AsNoTracking());

            var productCards = await projections
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToArrayAsync();

            var facets = await BuildFacetsAsync(filteredProducts, query);
            var appliedFilters = await BuildAppliedFiltersAsync(query, scope, context);

            return new ProductListingPageDto(
                context.Title,
                context.Description,
                context.Seo,
                context.Breadcrumbs,
                context.IntroTitle,
                context.IntroText,
                ProductQueryMappingHelper.ToProductCardDtos(productCards),
                facets,
                appliedFilters,
                new PaginationDto(page, pageSize, totalItems),
                context.MerchBlocks,
                context.Faq);
        }

        private IQueryable<Product> BuildScopedProductsQuery(ListingScope scope, ListingContext context)
        {
            var products = ProductQueryMappingHelper
                .ApplyBaseProductVisibility(_db.Products.AsNoTracking());

            return scope switch
            {
                ListingScope.Category => products.Where(product =>
                    product.PrimaryCategoryId == context.ScopeId ||
                    product.CategoryMaps.Any(map => map.CategoryId == context.ScopeId)),
                ListingScope.Brand => products.Where(product => product.BrandId == context.ScopeId),
                ListingScope.Collection => products.Where(product =>
                    product.CollectionMaps.Any(map => map.CollectionId == context.ScopeId)),
                ListingScope.Sale => products.Where(product =>
                    product.Variants.Any(variant =>
                        variant.IsActive &&
                        variant.IsVisible &&
                        variant.OldPrice.HasValue &&
                        variant.OldPrice.Value > variant.Price)),
                _ => products
            };
        }

        private static IQueryable<Product> ApplyListingFilters(
            IQueryable<Product> products,
            GetCategoryListingQuery query,
            ListingScope scope)
        {
            if (query.Brands is { Length: > 0 })
            {
                products = products.Where(product => query.Brands!.Contains(product.BrandId));
            }

            if (query.Sizes is { Length: > 0 })
            {
                var requestedSizes = query.Sizes
                    .Select(size => Convert.ToDecimal(size, CultureInfo.InvariantCulture))
                    .ToArray();

                products = products.Where(product => product.Variants.Any(variant =>
                    variant.IsActive &&
                    variant.IsVisible &&
                    requestedSizes.Contains(variant.SizeEu)));
            }

            if (query.Colors is { Length: > 0 })
            {
                var colors = query.Colors
                    .Where(color => !string.IsNullOrWhiteSpace(color))
                    .Select(color => color.Trim().ToLowerInvariant())
                    .Distinct()
                    .ToArray();

                if (colors.Length > 0)
                {
                    products = products.Where(product =>
                        (product.PrimaryColorName != null && colors.Contains(product.PrimaryColorName.ToLower())) ||
                        product.Variants.Any(variant =>
                            variant.IsActive &&
                            variant.IsVisible &&
                            variant.ColorName != null &&
                            colors.Contains(variant.ColorName.ToLower())));
                }
            }

            if (query.PriceFrom.HasValue)
            {
                products = products.Where(product => product.Variants.Any(variant =>
                    variant.IsActive &&
                    variant.IsVisible &&
                    variant.Price >= query.PriceFrom.Value));
            }

            if (query.PriceTo.HasValue)
            {
                products = products.Where(product => product.Variants.Any(variant =>
                    variant.IsActive &&
                    variant.IsVisible &&
                    variant.Price <= query.PriceTo.Value));
            }

            if (query.IsOnSale == true || scope == ListingScope.Sale)
            {
                products = products.Where(product => product.Variants.Any(variant =>
                    variant.IsActive &&
                    variant.IsVisible &&
                    variant.OldPrice.HasValue &&
                    variant.OldPrice.Value > variant.Price));
            }

            if (query.IsNew == true)
            {
                products = products.Where(product => product.IsNew);
            }

            if (query.InStockOnly == true)
            {
                products = products.Where(product => product.Variants.Any(variant =>
                    variant.IsActive &&
                    variant.IsVisible &&
                    variant.TotalStock > 0));
            }

            return products;
        }

        private static IQueryable<Product> ApplySort(IQueryable<Product> products, string? sort)
        {
            var normalizedSort = sort?.Trim().ToLowerInvariant();

            return normalizedSort switch
            {
                "newest" => products
                    .OrderByDescending(product => product.PublishedAtUtc)
                    .ThenByDescending(product => product.Id),
                "price_asc" => products
                    .OrderBy(product => product.Variants
                        .Where(variant => variant.IsActive && variant.IsVisible)
                        .Select(variant => variant.Price)
                        .DefaultIfEmpty(decimal.MaxValue)
                        .Min())
                    .ThenByDescending(product => product.SortRank),
                "price_desc" => products
                    .OrderByDescending(product => product.Variants
                        .Where(variant => variant.IsActive && variant.IsVisible)
                        .Select(variant => variant.Price)
                        .DefaultIfEmpty(0m)
                        .Min())
                    .ThenByDescending(product => product.SortRank),
                "bestsellers" => products
                    .OrderByDescending(product => product.IsBestseller)
                    .ThenByDescending(product => product.SortRank)
                    .ThenByDescending(product => product.PublishedAtUtc),
                _ => products
                    .OrderByDescending(product => product.SortRank)
                    .ThenByDescending(product => product.IsBestseller)
                    .ThenByDescending(product => product.PublishedAtUtc)
                    .ThenByDescending(product => product.Id)
            };
        }

        private async Task<FilterFacetDto[]> BuildFacetsAsync(
            IQueryable<Product> filteredProducts,
            GetCategoryListingQuery query)
        {
            var selectedSizes = new HashSet<long>(query.Sizes ?? Array.Empty<long>());
            var selectedColors = new HashSet<string>(
                (query.Colors ?? Array.Empty<string>())
                    .Where(color => !string.IsNullOrWhiteSpace(color))
                    .Select(color => color.Trim().ToLowerInvariant()));
            var selectedBrands = new HashSet<long>(query.Brands ?? Array.Empty<long>());

            var sizeRows = await filteredProducts
                .SelectMany(product => product.Variants
                    .Where(variant => variant.IsActive && variant.IsVisible)
                    .Select(variant => new { product.Id, variant.SizeEu }))
                .Distinct()
                .GroupBy(row => row.SizeEu)
                .Select(group => new
                {
                    Size = group.Key,
                    Count = group.Count()
                })
                .OrderBy(row => row.Size)
                .ToArrayAsync();

            var colorRows = await filteredProducts
                .SelectMany(product => product.Variants
                    .Where(variant => variant.IsActive && variant.IsVisible)
                    .Select(variant => new
                    {
                        product.Id,
                        Color = (variant.ColorName ?? product.PrimaryColorName) ?? string.Empty
                    }))
                .Where(row => row.Color != string.Empty)
                .Distinct()
                .GroupBy(row => row.Color)
                .Select(group => new
                {
                    Color = group.Key,
                    Count = group.Count()
                })
                .OrderBy(row => row.Color)
                .ToArrayAsync();

            var brandRows = await (
                from product in filteredProducts
                join brand in _db.Brands.AsNoTracking() on product.BrandId equals brand.Id
                group product by new { brand.Id, brand.Name } into grouped
                orderby grouped.Key.Name
                select new
                {
                    BrandId = grouped.Key.Id,
                    BrandName = grouped.Key.Name,
                    Count = grouped.Count()
                })
                .ToArrayAsync();

            var saleCount = await filteredProducts.CountAsync(product => product.Variants.Any(variant =>
                variant.IsActive &&
                variant.IsVisible &&
                variant.OldPrice.HasValue &&
                variant.OldPrice.Value > variant.Price));

            var newCount = await filteredProducts.CountAsync(product => product.IsNew);

            var stockCount = await filteredProducts.CountAsync(product => product.Variants.Any(variant =>
                variant.IsActive &&
                variant.IsVisible &&
                variant.TotalStock > 0));

            return new FilterFacetDto[]
            {
                new(
                    "sizes",
                    "Velicine",
                    "multi",
                    sizeRows
                        .Select(row => new FilterOptionDto(
                            row.Size.ToString("0.#", CultureInfo.InvariantCulture),
                            row.Size.ToString("0.#", CultureInfo.InvariantCulture),
                            row.Count,
                            decimal.Truncate(row.Size) == row.Size &&
                            selectedSizes.Contains((long)row.Size),
                            row.Count == 0))
                        .ToArray()),
                new(
                    "colors",
                    "Boje",
                    "multi",
                    colorRows
                        .Select(row => new FilterOptionDto(
                            row.Color,
                            row.Color,
                            row.Count,
                            selectedColors.Contains(row.Color.ToLowerInvariant()),
                            row.Count == 0))
                        .ToArray()),
                new(
                    "brands",
                    "Brendovi",
                    "multi",
                    brandRows
                        .Select(row => new FilterOptionDto(
                            row.BrandId.ToString(CultureInfo.InvariantCulture),
                            row.BrandName,
                            row.Count,
                            selectedBrands.Contains(row.BrandId),
                            row.Count == 0))
                        .ToArray()),
                new(
                    "sale",
                    "Akcija",
                    "boolean",
                    new[]
                    {
                        new FilterOptionDto(
                            "true",
                            "Na akciji",
                            saleCount,
                            query.IsOnSale == true,
                            saleCount == 0)
                    }),
                new(
                    "new",
                    "Novo",
                    "boolean",
                    new[]
                    {
                        new FilterOptionDto(
                            "true",
                            "Novo",
                            newCount,
                            query.IsNew == true,
                            newCount == 0)
                    }),
                new(
                    "stock",
                    "Na stanju",
                    "boolean",
                    new[]
                    {
                        new FilterOptionDto(
                            "true",
                            "Na stanju",
                            stockCount,
                            query.InStockOnly == true,
                            stockCount == 0)
                    })
            };
        }

        private async Task<AppliedFilterDto[]> BuildAppliedFiltersAsync(
            GetCategoryListingQuery query,
            ListingScope scope,
            ListingContext context)
        {
            var filters = new List<AppliedFilterDto>();

            if (scope == ListingScope.Category)
            {
                filters.Add(new AppliedFilterDto("category", "Kategorija", context.Slug, context.Title));
            }
            else if (scope == ListingScope.Brand)
            {
                filters.Add(new AppliedFilterDto("brandScope", "Brend", context.Slug, context.Title));
            }
            else if (scope == ListingScope.Collection)
            {
                filters.Add(new AppliedFilterDto("collection", "Kolekcija", context.Slug, context.Title));
            }
            else if (scope == ListingScope.Sale)
            {
                filters.Add(new AppliedFilterDto("saleScope", "Akcija", "true", "Akcija"));
            }

            if (query.Sizes is { Length: > 0 })
            {
                filters.AddRange(query.Sizes.Select(size => new AppliedFilterDto(
                    "size",
                    "Velicina",
                    size.ToString(CultureInfo.InvariantCulture),
                    size.ToString(CultureInfo.InvariantCulture))));
            }

            if (query.Colors is { Length: > 0 })
            {
                filters.AddRange(query.Colors
                    .Where(color => !string.IsNullOrWhiteSpace(color))
                    .Select(color => new AppliedFilterDto("color", "Boja", color, color)));
            }

            if (query.Brands is { Length: > 0 })
            {
                var selectedBrandIds = query.Brands.Distinct().ToArray();
                var selectedBrands = await _db.Brands.AsNoTracking()
                    .Where(brand => selectedBrandIds.Contains(brand.Id))
                    .Select(brand => new { brand.Id, brand.Name })
                    .ToDictionaryAsync(brand => brand.Id, brand => brand.Name);

                filters.AddRange(selectedBrandIds.Select(brandId => new AppliedFilterDto(
                    "brand",
                    "Brend",
                    brandId.ToString(CultureInfo.InvariantCulture),
                    selectedBrands.TryGetValue(brandId, out var brandName)
                        ? brandName
                        : brandId.ToString(CultureInfo.InvariantCulture))));
            }

            if (query.PriceFrom.HasValue || query.PriceTo.HasValue)
            {
                var fromText = query.PriceFrom?.ToString("0.##", CultureInfo.InvariantCulture) ?? "0";
                var toText = query.PriceTo?.ToString("0.##", CultureInfo.InvariantCulture) ?? "max";
                var display = $"{fromText} - {toText}";
                filters.Add(new AppliedFilterDto("price", "Cena", display, display));
            }

            if (query.IsOnSale == true)
            {
                filters.Add(new AppliedFilterDto("isOnSale", "Akcija", "true", "Na akciji"));
            }

            if (query.IsNew == true)
            {
                filters.Add(new AppliedFilterDto("isNew", "Novo", "true", "Novo"));
            }

            if (query.InStockOnly == true)
            {
                filters.Add(new AppliedFilterDto("inStockOnly", "Na stanju", "true", "Na stanju"));
            }

            return filters.ToArray();
        }

        private async Task<ListingContext> ResolveScopeContextAsync(ListingScope scope, string slug)
        {
            return scope switch
            {
                ListingScope.Category => await BuildCategoryContextAsync(slug),
                ListingScope.Brand => await BuildBrandContextAsync(slug),
                ListingScope.Collection => await BuildCollectionContextAsync(slug),
                ListingScope.Sale => await BuildSaleContextAsync(),
                _ => throw new KeyNotFoundException("Listing scope is not supported.")
            };
        }

        private async Task<ListingContext> BuildCategoryContextAsync(string slug)
        {
            var category = await _db.Categories.AsNoTracking()
                .Where(entity => entity.Slug == slug && entity.IsActive)
                .Select(entity => new
                {
                    entity.Id,
                    entity.Name,
                    entity.Slug,
                    entity.ShortDescription,
                    entity.Seo
                })
                .FirstOrDefaultAsync();

            if (category is null)
            {
                throw new KeyNotFoundException($"Category '{slug}' was not found.");
            }

            var content = await _db.CategoryPageContents.AsNoTracking()
                .Where(entity => entity.CategoryId == category.Id && entity.IsPublished)
                .Select(entity => new
                {
                    entity.IntroTitle,
                    entity.IntroText,
                    entity.SeoText,
                    entity.Seo,
                    entity.MerchBlocks,
                    entity.Faq
                })
                .FirstOrDefaultAsync();

            var introText = content?.IntroText ?? category.ShortDescription ?? string.Empty;
            var description = content?.SeoText ?? category.ShortDescription ?? string.Empty;
            var seo = ProductQueryMappingHelper.MapSeo(content?.Seo ?? category.Seo, category.Name, description);
            var breadcrumbs = await BuildCategoryBreadcrumbsAsync(category.Id);

            return new ListingContext(
                category.Id,
                category.Slug,
                category.Name,
                description,
                seo,
                content?.IntroTitle,
                introText,
                MapMerchBlocks(content?.MerchBlocks),
                MapFaq(content?.Faq),
                breadcrumbs);
        }

        private async Task<ListingContext> BuildBrandContextAsync(string slug)
        {
            var brand = await _db.Brands.AsNoTracking()
                .Where(entity => entity.Slug == slug && entity.IsActive)
                .Select(entity => new
                {
                    entity.Id,
                    entity.Name,
                    entity.Slug,
                    entity.ShortDescription,
                    entity.LongDescription,
                    entity.Seo
                })
                .FirstOrDefaultAsync();

            if (brand is null)
            {
                throw new KeyNotFoundException($"Brand '{slug}' was not found.");
            }

            var content = await _db.BrandPageContents.AsNoTracking()
                .Where(entity => entity.BrandId == brand.Id && entity.IsPublished)
                .Select(entity => new
                {
                    entity.IntroTitle,
                    entity.IntroText,
                    entity.SeoText,
                    entity.Seo,
                    entity.MerchBlocks,
                    entity.Faq
                })
                .FirstOrDefaultAsync();

            var introText = content?.IntroText ?? brand.ShortDescription ?? brand.LongDescription ?? string.Empty;
            var description = content?.SeoText ?? brand.ShortDescription ?? string.Empty;
            var seo = ProductQueryMappingHelper.MapSeo(content?.Seo ?? brand.Seo, brand.Name, description);

            return new ListingContext(
                brand.Id,
                brand.Slug,
                brand.Name,
                description,
                seo,
                content?.IntroTitle,
                introText,
                MapMerchBlocks(content?.MerchBlocks),
                MapFaq(content?.Faq),
                new[]
                {
                    new BreadcrumbItemDto("Pocetna", "/"),
                    new BreadcrumbItemDto("Brendovi", "/brendovi"),
                    new BreadcrumbItemDto(brand.Name, $"/brend/{brand.Slug}")
                });
        }

        private async Task<ListingContext> BuildCollectionContextAsync(string slug)
        {
            var collection = await _db.Collections.AsNoTracking()
                .Where(entity => entity.Slug == slug && entity.IsActive)
                .Select(entity => new
                {
                    entity.Id,
                    entity.Name,
                    entity.Slug,
                    entity.ShortDescription,
                    entity.LongDescription,
                    entity.Seo
                })
                .FirstOrDefaultAsync();

            if (collection is null)
            {
                throw new KeyNotFoundException($"Collection '{slug}' was not found.");
            }

            var content = await _db.CollectionPageContents.AsNoTracking()
                .Where(entity => entity.CollectionId == collection.Id && entity.IsPublished)
                .Select(entity => new
                {
                    entity.IntroTitle,
                    entity.IntroText,
                    entity.SeoText,
                    entity.Seo,
                    entity.MerchBlocks,
                    entity.Faq
                })
                .FirstOrDefaultAsync();

            var introText = content?.IntroText ?? collection.ShortDescription ?? collection.LongDescription ?? string.Empty;
            var description = content?.SeoText ?? collection.ShortDescription ?? string.Empty;
            var seo = ProductQueryMappingHelper.MapSeo(content?.Seo ?? collection.Seo, collection.Name, description);

            return new ListingContext(
                collection.Id,
                collection.Slug,
                collection.Name,
                description,
                seo,
                content?.IntroTitle,
                introText,
                MapMerchBlocks(content?.MerchBlocks),
                MapFaq(content?.Faq),
                new[]
                {
                    new BreadcrumbItemDto("Pocetna", "/"),
                    new BreadcrumbItemDto("Kolekcije", "/kolekcije"),
                    new BreadcrumbItemDto(collection.Name, $"/kolekcija/{collection.Slug}")
                });
        }

        private async Task<ListingContext> BuildSaleContextAsync()
        {
            var page = await _db.SalePages.AsNoTracking()
                .Where(entity => entity.IsPublished)
                .OrderByDescending(entity => entity.UpdatedAtUtc)
                .Select(entity => new
                {
                    entity.Slug,
                    entity.Title,
                    entity.Subtitle,
                    entity.IntroText,
                    entity.Seo,
                    entity.Faq
                })
                .FirstOrDefaultAsync();

            var title = page?.Title ?? "Akcija";
            var description = page?.Subtitle ?? string.Empty;
            var seo = ProductQueryMappingHelper.MapSeo(page?.Seo, title, description);

            return new ListingContext(
                0L,
                page?.Slug ?? "sale",
                title,
                description,
                seo,
                title,
                page?.IntroText,
                Array.Empty<object>(),
                MapFaq(page?.Faq),
                new[]
                {
                    new BreadcrumbItemDto("Pocetna", "/"),
                    new BreadcrumbItemDto("Akcija", "/akcija")
                });
        }

        private async Task<BreadcrumbItemDto[]> BuildCategoryBreadcrumbsAsync(long categoryId)
        {
            var categories = await _db.Categories.AsNoTracking()
                .Select(entity => new
                {
                    entity.Id,
                    entity.ParentId,
                    entity.Name,
                    entity.Slug
                })
                .ToDictionaryAsync(entity => entity.Id);

            if (!categories.ContainsKey(categoryId))
            {
                return new[] { new BreadcrumbItemDto("Pocetna", "/") };
            }

            var chain = new List<(string Name, string Slug)>();
            var visited = new HashSet<long>();
            var currentId = categoryId;

            while (categories.TryGetValue(currentId, out var current) && visited.Add(currentId))
            {
                chain.Add((current.Name, current.Slug));

                if (!current.ParentId.HasValue)
                {
                    break;
                }

                currentId = current.ParentId.Value;
            }

            chain.Reverse();

            var breadcrumbs = new List<BreadcrumbItemDto>(chain.Count + 1)
            {
                new("Pocetna", "/")
            };

            breadcrumbs.AddRange(chain.Select(item => new BreadcrumbItemDto(item.Name, $"/kategorija/{item.Slug}")));
            return breadcrumbs.ToArray();
        }

        private static object[] MapMerchBlocks(IEnumerable<Domain.ValueObjects.MerchBlock>? merchBlocks)
        {
            if (merchBlocks is null)
            {
                return Array.Empty<object>();
            }

            return merchBlocks
                .Select(block => (object)new MerchBlockDto(
                    block.Title,
                    block.Html ?? string.Empty,
                    (block.ProductSlugs ?? Array.Empty<string>()).ToArray()))
                .ToArray();
        }

        private static object? MapFaq(IEnumerable<Domain.ValueObjects.FaqItem>? faqItems)
        {
            if (faqItems is null)
            {
                return null;
            }

            var mapped = faqItems
                .Select(item => new FaqItemDto(item.Question, item.Answer))
                .ToArray();

            return mapped.Length == 0 ? null : mapped;
        }

        private sealed record ListingContext(
            long ScopeId,
            string Slug,
            string Title,
            string Description,
            SeoDto Seo,
            string? IntroTitle,
            string? IntroText,
            object[] MerchBlocks,
            object? Faq,
            BreadcrumbItemDto[] Breadcrumbs);

        private enum ListingScope
        {
            Category,
            Brand,
            Collection,
            Sale
        }
    }
}
