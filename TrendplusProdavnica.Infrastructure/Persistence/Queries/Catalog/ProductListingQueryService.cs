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
using TrendplusProdavnica.Infrastructure.Persistence;
using CatalogListing = TrendplusProdavnica.Application.Catalog.Listing;

namespace TrendplusProdavnica.Infrastructure.Persistence.Queries.Catalog
{
    public sealed class ProductListingQueryService : IProductListingQueryService
    {
        private static readonly ListingSortOptionDto[] SortOptions =
        {
            new("Preporuceno", "recommended"),
            new("Najnovije", "newest"),
            new("Cena: niza ka visoj", "price_asc"),
            new("Cena: visa ka nizoj", "price_desc"),
            new("Bestseller", "bestsellers")
        };

        private readonly TrendplusDbContext _db;
        private readonly CatalogListing.IProductListingReadService _listingReadService;

        public ProductListingQueryService(
            TrendplusDbContext db,
            CatalogListing.IProductListingReadService listingReadService)
        {
            _db = db;
            _listingReadService = listingReadService;
        }

        public Task<ProductListingPageDto> GetCategoryListingAsync(GetCategoryListingQuery query)
            => BuildListingAsync(ListingRequest.ForCategory(query));

        public Task<ProductListingPageDto> GetBrandListingAsync(GetBrandListingQuery query)
            => BuildListingAsync(ListingRequest.ForBrand(query));

        public Task<ProductListingPageDto> GetCollectionListingAsync(GetCollectionListingQuery query)
            => BuildListingAsync(ListingRequest.ForCollection(query));

        public Task<ProductListingPageDto> GetSaleListingAsync(GetSaleListingQuery query)
            => BuildListingAsync(ListingRequest.ForSale(query));

        private async Task<ProductListingPageDto> BuildListingAsync(ListingRequest request)
        {
            var context = await ResolveScopeContextAsync(request.Scope, request.Slug);
            var response = await _listingReadService.GetProductsAsync(ToReadQuery(request));

            return new ProductListingPageDto(
                context.Title,
                context.Slug,
                context.IntroText,
                context.Breadcrumbs,
                response.Products.Select(MapProductCard).ToArray(),
                response.TotalCount,
                response.Page,
                response.PageSize,
                SortOptions,
                MapAvailableFilters(response.Facets),
                context.MerchBlocks,
                context.Faq,
                context.Seo with { CanonicalUrl = response.CanonicalUrl });
        }

        private static CatalogListing.ProductListingQuery ToReadQuery(ListingRequest request)
        {
            return new CatalogListing.ProductListingQuery(
                request.Scope is ListingScope.Category or ListingScope.SaleCategory ? request.Slug : null,
                request.Scope == ListingScope.Brand ? request.Slug : null,
                request.Scope == ListingScope.Collection ? request.Slug : null,
                request.PriceFrom,
                request.PriceTo,
                request.Sizes?.Select(size => Convert.ToDecimal(size, CultureInfo.InvariantCulture)).ToArray(),
                request.Colors,
                request.Brands,
                request.IsOnSale,
                request.IsNew,
                request.InStockOnly,
                request.Page,
                request.PageSize,
                request.Sort);
        }

        private static ProductCardDto MapProductCard(CatalogListing.ProductCardDto product)
        {
            return new ProductCardDto(
                product.ProductId,
                product.Slug,
                product.BrandName,
                product.Name,
                product.PrimaryImageUrl,
                product.SecondaryImageUrl,
                product.Price,
                product.OldPrice,
                "RSD",
                ProductQueryMappingHelper.BuildBadges(product.IsNew, product.IsBestseller, product.IsOnSale),
                product.AvailableSizesCount > 0,
                product.AvailableSizesCount,
                product.Color,
                product.IsNew,
                product.IsBestseller,
                product.IsOnSale);
        }

        private static ListingAvailableFiltersDto MapAvailableFilters(CatalogListing.ProductListingFacets facets)
        {
            return new ListingAvailableFiltersDto(
                facets.Brands.Select(item => item.BrandName).ToArray(),
                facets.Colors.Select(item => item.Color).ToArray(),
                facets.Sizes.Select(item => item.Size).ToArray(),
                new ListingPriceRangeDto(facets.PriceRange.Min, facets.PriceRange.Max));
        }

        private async Task<ListingContext> ResolveScopeContextAsync(ListingScope scope, string? slug)
        {
            return scope switch
            {
                ListingScope.Category => await BuildCategoryContextAsync(slug ?? string.Empty),
                ListingScope.Brand => await BuildBrandContextAsync(slug ?? string.Empty),
                ListingScope.Collection => await BuildCollectionContextAsync(slug ?? string.Empty),
                ListingScope.Sale => await BuildSaleContextAsync(),
                ListingScope.SaleCategory => await BuildSaleCategoryContextAsync(slug ?? string.Empty),
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
                category.Slug,
                category.Name,
                introText,
                seo,
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
                brand.Slug,
                brand.Name,
                introText,
                seo,
                MapMerchBlocks(content?.MerchBlocks),
                MapFaq(content?.Faq),
                new[]
                {
                    new BreadcrumbItemDto("Pocetna", "/"),
                    new BreadcrumbItemDto("Brendovi", "/brendovi"),
                    new BreadcrumbItemDto(brand.Name, $"/brendovi/{brand.Slug}")
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
                collection.Slug,
                collection.Name,
                introText,
                seo,
                MapMerchBlocks(content?.MerchBlocks),
                MapFaq(content?.Faq),
                new[]
                {
                    new BreadcrumbItemDto("Pocetna", "/"),
                    new BreadcrumbItemDto("Kolekcije", "/kolekcije"),
                    new BreadcrumbItemDto(collection.Name, $"/kolekcije/{collection.Slug}")
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
            var introText = page?.IntroText ?? page?.Subtitle ?? "Aktuelni snizeni proizvodi.";
            var description = page?.Subtitle ?? "Aktuelni snizeni proizvodi.";
            var seo = ProductQueryMappingHelper.MapSeo(page?.Seo, title, description);

            return new ListingContext(
                page?.Slug ?? "akcija",
                title,
                introText,
                seo,
                Array.Empty<object>(),
                MapFaq(page?.Faq),
                new[]
                {
                    new BreadcrumbItemDto("Pocetna", "/"),
                    new BreadcrumbItemDto("Akcija", "/akcija")
                });
        }

        private async Task<ListingContext> BuildSaleCategoryContextAsync(string slug)
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

            var introText = content?.IntroText ?? category.ShortDescription ?? $"Snizeni proizvodi u kategoriji {category.Name}.";
            var description = content?.SeoText ?? category.ShortDescription ?? $"Snizeni proizvodi u kategoriji {category.Name}.";
            var seo = ProductQueryMappingHelper.MapSeo(content?.Seo ?? category.Seo, $"Akcija - {category.Name}", description);

            return new ListingContext(
                category.Slug,
                category.Name,
                introText,
                seo,
                MapMerchBlocks(content?.MerchBlocks),
                MapFaq(content?.Faq),
                new[]
                {
                    new BreadcrumbItemDto("Pocetna", "/"),
                    new BreadcrumbItemDto("Akcija", "/akcija"),
                    new BreadcrumbItemDto(category.Name, $"/akcija/{category.Slug}")
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

            breadcrumbs.AddRange(chain.Select(item => new BreadcrumbItemDto(item.Name, $"/kategorije/{item.Slug}")));
            return breadcrumbs.ToArray();
        }

        private static object[] MapMerchBlocks(IEnumerable<TrendplusProdavnica.Domain.ValueObjects.MerchBlock>? merchBlocks)
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

        private static object? MapFaq(IEnumerable<TrendplusProdavnica.Domain.ValueObjects.FaqItem>? faqItems)
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
            string Slug,
            string Title,
            string? IntroText,
            SeoDto Seo,
            object[] MerchBlocks,
            object? Faq,
            BreadcrumbItemDto[] Breadcrumbs);

        private sealed record ListingRequest(
            ListingScope Scope,
            string? Slug,
            int Page,
            int PageSize,
            string? Sort,
            long[]? Sizes,
            string[]? Colors,
            long[]? Brands,
            decimal? PriceFrom,
            decimal? PriceTo,
            bool? IsOnSale,
            bool? IsNew,
            bool? InStockOnly)
        {
            public static ListingRequest ForCategory(GetCategoryListingQuery query)
                => new(
                    ListingScope.Category,
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

            public static ListingRequest ForBrand(GetBrandListingQuery query)
                => new(
                    ListingScope.Brand,
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

            public static ListingRequest ForCollection(GetCollectionListingQuery query)
                => new(
                    ListingScope.Collection,
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

            public static ListingRequest ForSale(GetSaleListingQuery query)
                => new(
                    string.IsNullOrWhiteSpace(query.CategorySlug) ? ListingScope.Sale : ListingScope.SaleCategory,
                    query.CategorySlug,
                    query.Page,
                    query.PageSize,
                    query.Sort,
                    query.Sizes,
                    query.Colors,
                    query.Brands,
                    query.PriceFrom,
                    query.PriceTo,
                    true,
                    query.IsNew,
                    query.InStockOnly);
        }

        public async Task<List<CategorySeoDto>> GetAllCategoriesForSeoAsync()
        {
            var categories = await _db.Categories
                .AsNoTracking()
                .Select(c => new CategorySeoDto(c.Slug, c.UpdatedAtUtc))
                .ToListAsync();
            return categories;
        }

        private enum ListingScope
        {
            Category,
            Brand,
            Collection,
            Sale,
            SaleCategory
        }
    }
}
