#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrendplusProdavnica.Application.Catalog.Listing;
using TrendplusProdavnica.Domain.Enums;
using TrendplusProdavnica.Infrastructure.Persistence;
using ZiggyCreatures.Caching.Fusion;

namespace TrendplusProdavnica.Infrastructure.Persistence.Queries.Catalog
{
    public sealed class ProductListingReadService : IProductListingReadService
    {
        private static readonly string[] CacheTags = { "plp" };
        private static readonly FusionCacheEntryOptions CacheOptions = new()
        {
            Duration = TimeSpan.FromSeconds(60),
            EagerRefreshThreshold = 0.80f,
            IsFailSafeEnabled = true,
            FailSafeMaxDuration = TimeSpan.FromMinutes(5),
            FailSafeThrottleDuration = TimeSpan.FromSeconds(15),
            FactorySoftTimeout = TimeSpan.FromMilliseconds(150),
            FactoryHardTimeout = TimeSpan.FromSeconds(2),
            DistributedCacheDuration = TimeSpan.FromSeconds(60)
        };

        private readonly TrendplusDbContext _db;
        private readonly IFusionCache _cache;

        public ProductListingReadService(TrendplusDbContext db, IFusionCache cache)
        {
            _db = db;
            _cache = cache;
        }

        public Task<ProductListingResponse> GetProductsAsync(ProductListingQuery query, CancellationToken cancellationToken = default)
        {
            var normalized = Normalize(query);
            var cacheKey = $"plp:{ComputeQueryHash(normalized)}";

            return _cache.GetOrSetAsync<ProductListingResponse>(
                cacheKey,
                (_, token) => QueryDatabaseAsync(normalized, token),
                MaybeValue<ProductListingResponse>.None,
                CacheOptions,
                CacheTags,
                cancellationToken).AsTask();
        }

        private async Task<ProductListingResponse> QueryDatabaseAsync(NormalizedListingQuery query, CancellationToken cancellationToken)
        {
            var products = _db.Products.AsNoTracking()
                .Where(product =>
                    product.Status == ProductStatus.Published &&
                    product.IsVisible &&
                    product.IsPurchasable &&
                    product.Variants.Any(variant => variant.IsActive && variant.IsVisible));

            products = await ApplyScopeFiltersAsync(products, query, cancellationToken);
            products = ApplyValueFilters(products, query);

            var totalCount = await products.LongCountAsync(cancellationToken);
            var sorted = ApplySort(products, query.Sort);

            var productRows = await (
                    from product in sorted
                        .Skip((query.Page - 1) * query.PageSize)
                        .Take(query.PageSize)
                    join brand in _db.Brands.AsNoTracking() on product.BrandId equals brand.Id
                    select new
                    {
                        product.Id,
                        product.Slug,
                        product.Name,
                        BrandName = brand.Name,
                        product.IsNew,
                        product.PrimaryColorName,
                        Price = product.Variants
                            .Where(variant => variant.IsActive && variant.IsVisible)
                            .Select(variant => (decimal?)variant.Price)
                            .Min(),
                        OldPrice = product.Variants
                            .Where(variant =>
                                variant.IsActive &&
                                variant.IsVisible &&
                                variant.OldPrice.HasValue &&
                                variant.OldPrice.Value > variant.Price)
                            .Select(variant => variant.OldPrice)
                            .Min(),
                        IsOnSale = product.Variants.Any(variant =>
                            variant.IsActive &&
                            variant.IsVisible &&
                            variant.OldPrice.HasValue &&
                            variant.OldPrice.Value > variant.Price),
                        PrimaryImageUrl = product.Media
                            .Where(media => media.IsActive && media.IsPrimary)
                            .OrderBy(media => media.SortOrder)
                            .Select(media => media.Url)
                            .FirstOrDefault(),
                        SecondaryImageUrl = product.Media
                            .Where(media => media.IsActive && !media.IsPrimary)
                            .OrderBy(media => media.SortOrder)
                            .Select(media => media.Url)
                            .FirstOrDefault(),
                        FallbackImageUrl = product.Media
                            .Where(media => media.IsActive)
                            .OrderBy(media => media.SortOrder)
                            .Select(media => media.Url)
                            .FirstOrDefault(),
                        AvailableSizesCount = product.Variants
                            .Where(variant => variant.IsActive && variant.IsVisible && variant.TotalStock > 0)
                            .Select(variant => variant.SizeEu)
                            .Distinct()
                            .Count()
                    })
                .ToArrayAsync(cancellationToken);

            var productCards = productRows
                .Select(row =>
                {
                    var displayPrice = row.Price ?? 0m;
                    var oldPrice = row.OldPrice;
                    var discountPercent = oldPrice.HasValue && oldPrice.Value > displayPrice && displayPrice > 0
                        ? (int?)Math.Round((oldPrice.Value - displayPrice) / oldPrice.Value * 100m, MidpointRounding.AwayFromZero)
                        : null;

                    return new ProductCardDto(
                        row.Id,
                        row.Slug,
                        row.Name,
                        row.BrandName,
                        displayPrice,
                        oldPrice,
                        discountPercent,
                        row.PrimaryImageUrl ?? row.FallbackImageUrl ?? string.Empty,
                        row.SecondaryImageUrl,
                        row.AvailableSizesCount,
                        row.IsNew,
                        row.IsOnSale,
                        row.PrimaryColorName);
                })
                .ToArray();

            var facets = await BuildFacetsAsync(products, cancellationToken);

            return new ProductListingResponse(
                productCards,
                totalCount,
                query.Page,
                query.PageSize,
                facets,
                BuildCanonicalUrl(query));
        }

        private async Task<IQueryable<Domain.Catalog.Product>> ApplyScopeFiltersAsync(
            IQueryable<Domain.Catalog.Product> products,
            NormalizedListingQuery query,
            CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(query.CategorySlug))
            {
                var categoryId = await _db.Categories.AsNoTracking()
                    .Where(category => category.Slug == query.CategorySlug && category.IsActive)
                    .Select(category => (long?)category.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (!categoryId.HasValue)
                {
                    return products.Where(_ => false);
                }

                products = products.Where(product =>
                    product.PrimaryCategoryId == categoryId.Value ||
                    product.CategoryMaps.Any(map => map.CategoryId == categoryId.Value));
            }

            if (!string.IsNullOrWhiteSpace(query.BrandSlug))
            {
                var brandId = await _db.Brands.AsNoTracking()
                    .Where(brand => brand.Slug == query.BrandSlug && brand.IsActive)
                    .Select(brand => (long?)brand.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (!brandId.HasValue)
                {
                    return products.Where(_ => false);
                }

                products = products.Where(product => product.BrandId == brandId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.CollectionSlug))
            {
                var collectionId = await _db.Collections.AsNoTracking()
                    .Where(collection => collection.Slug == query.CollectionSlug && collection.IsActive)
                    .Select(collection => (long?)collection.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (!collectionId.HasValue)
                {
                    return products.Where(_ => false);
                }

                products = products.Where(product => product.CollectionMaps.Any(map => map.CollectionId == collectionId.Value));
            }

            return products;
        }

        private static IQueryable<Domain.Catalog.Product> ApplyValueFilters(
            IQueryable<Domain.Catalog.Product> products,
            NormalizedListingQuery query)
        {
            if (query.Brands.Length > 0)
            {
                products = products.Where(product => query.Brands.Contains(product.BrandId));
            }

            if (query.Sizes.Length > 0)
            {
                products = products.Where(product => product.Variants.Any(variant =>
                    variant.IsActive &&
                    variant.IsVisible &&
                    query.Sizes.Contains(variant.SizeEu)));
            }

            if (query.Colors.Length > 0)
            {
                products = products.Where(product =>
                    (product.PrimaryColorName != null && query.Colors.Contains(product.PrimaryColorName.ToLower())) ||
                    product.Variants.Any(variant =>
                        variant.IsActive &&
                        variant.IsVisible &&
                        variant.ColorName != null &&
                        query.Colors.Contains(variant.ColorName.ToLower())));
            }

            if (query.MinPrice.HasValue)
            {
                products = products.Where(product => product.Variants.Any(variant =>
                    variant.IsActive &&
                    variant.IsVisible &&
                    variant.Price >= query.MinPrice.Value));
            }

            if (query.MaxPrice.HasValue)
            {
                products = products.Where(product => product.Variants.Any(variant =>
                    variant.IsActive &&
                    variant.IsVisible &&
                    variant.Price <= query.MaxPrice.Value));
            }

            if (query.IsOnSale == true)
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

            return products;
        }

        private static IQueryable<Domain.Catalog.Product> ApplySort(
            IQueryable<Domain.Catalog.Product> products,
            string sort)
        {
            return sort switch
            {
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
                "newest" => products
                    .OrderByDescending(product => product.PublishedAtUtc)
                    .ThenByDescending(product => product.Id),
                _ => products
                    .OrderByDescending(product => product.SortRank)
                    .ThenByDescending(product => product.IsBestseller)
                    .ThenByDescending(product => product.PublishedAtUtc)
                    .ThenByDescending(product => product.Id)
            };
        }

        private async Task<ProductListingFacets> BuildFacetsAsync(
            IQueryable<Domain.Catalog.Product> products,
            CancellationToken cancellationToken)
        {
            var brandFacets = await (
                    from product in products
                    join brand in _db.Brands.AsNoTracking() on product.BrandId equals brand.Id
                    group product by new { brand.Id, brand.Name }
                    into grouped
                    orderby grouped.Count() descending, grouped.Key.Name
                    select new BrandFacetItem(grouped.Key.Id, grouped.Key.Name, grouped.Count()))
                .Take(30)
                .ToArrayAsync(cancellationToken);

            var sizeFacets = await products
                .SelectMany(product => product.Variants
                    .Where(variant => variant.IsActive && variant.IsVisible)
                    .Select(variant => new { product.Id, variant.SizeEu }))
                .Distinct()
                .GroupBy(row => row.SizeEu)
                .Select(group => new SizeFacetItem(group.Key, group.Count()))
                .OrderBy(item => item.Size)
                .ToArrayAsync(cancellationToken);

            var colorFacets = await products
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
                .Select(group => new ColorFacetItem(group.Key, group.Count()))
                .OrderBy(item => item.Color)
                .ToArrayAsync(cancellationToken);

            var priceRange = await products
                .SelectMany(product => product.Variants
                    .Where(variant => variant.IsActive && variant.IsVisible)
                    .Select(variant => (decimal?)variant.Price))
                .GroupBy(_ => 1)
                .Select(group => new PriceRangeFacet(group.Min(), group.Max()))
                .FirstOrDefaultAsync(cancellationToken)
                ?? new PriceRangeFacet(null, null);

            return new ProductListingFacets(
                brandFacets,
                sizeFacets,
                colorFacets,
                priceRange);
        }

        private static NormalizedListingQuery Normalize(ProductListingQuery query)
        {
            var page = query.Page < 1 ? 1 : query.Page;
            var pageSize = query.PageSize <= 0 ? 24 : Math.Min(query.PageSize, 100);

            return new NormalizedListingQuery(
                NormalizeSlug(query.CategorySlug),
                NormalizeSlug(query.BrandSlug),
                NormalizeSlug(query.CollectionSlug),
                query.MinPrice,
                query.MaxPrice,
                (query.Sizes ?? Array.Empty<decimal>())
                    .Where(size => size > 0)
                    .Distinct()
                    .OrderBy(size => size)
                    .ToArray(),
                (query.Colors ?? Array.Empty<string>())
                    .Where(color => !string.IsNullOrWhiteSpace(color))
                    .Select(color => color.Trim().ToLowerInvariant())
                    .Distinct()
                    .OrderBy(color => color)
                    .ToArray(),
                (query.Brands ?? Array.Empty<long>())
                    .Where(brandId => brandId > 0)
                    .Distinct()
                    .OrderBy(brandId => brandId)
                    .ToArray(),
                query.IsOnSale,
                query.IsNew,
                page,
                pageSize,
                NormalizeSort(query.Sort));
        }

        private static string ComputeQueryHash(NormalizedListingQuery query)
        {
            var canonical = string.Join("|", new[]
            {
                $"category={query.CategorySlug}",
                $"brand={query.BrandSlug}",
                $"collection={query.CollectionSlug}",
                $"min={FormatDecimal(query.MinPrice)}",
                $"max={FormatDecimal(query.MaxPrice)}",
                $"sizes={string.Join(",", query.Sizes.Select(size => size.ToString("0.##", CultureInfo.InvariantCulture)))}",
                $"colors={string.Join(",", query.Colors)}",
                $"brands={string.Join(",", query.Brands)}",
                $"sale={BoolToken(query.IsOnSale)}",
                $"new={BoolToken(query.IsNew)}",
                $"page={query.Page}",
                $"pageSize={query.PageSize}",
                $"sort={query.Sort}"
            });

            var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));
            return Convert.ToHexString(hashBytes).Substring(0, 16).ToLowerInvariant();
        }

        private static string BuildCanonicalUrl(NormalizedListingQuery query)
        {
            string path;
            if (!string.IsNullOrWhiteSpace(query.CategorySlug))
            {
                path = $"/{query.CategorySlug}";
            }
            else if (!string.IsNullOrWhiteSpace(query.BrandSlug))
            {
                path = $"/brendovi/{query.BrandSlug}";
            }
            else if (!string.IsNullOrWhiteSpace(query.CollectionSlug))
            {
                path = $"/kolekcije/{query.CollectionSlug}";
            }
            else
            {
                path = "/katalog";
            }

            var parameters = new List<string>();

            if (query.MinPrice.HasValue)
            {
                parameters.Add($"minPrice={Uri.EscapeDataString(FormatDecimal(query.MinPrice))}");
            }

            if (query.MaxPrice.HasValue)
            {
                parameters.Add($"maxPrice={Uri.EscapeDataString(FormatDecimal(query.MaxPrice))}");
            }

            if (query.Sizes.Length > 0)
            {
                parameters.Add($"sizes={Uri.EscapeDataString(string.Join(",", query.Sizes.Select(value => value.ToString("0.##", CultureInfo.InvariantCulture))))}");
            }

            if (query.Colors.Length > 0)
            {
                parameters.Add($"colors={Uri.EscapeDataString(string.Join(",", query.Colors))}");
            }

            if (query.Brands.Length > 0)
            {
                parameters.Add($"brands={Uri.EscapeDataString(string.Join(",", query.Brands))}");
            }

            if (query.IsOnSale == true)
            {
                parameters.Add("isOnSale=true");
            }

            if (query.IsNew == true)
            {
                parameters.Add("isNew=true");
            }

            if (!string.Equals(query.Sort, "popular", StringComparison.Ordinal))
            {
                parameters.Add($"sort={Uri.EscapeDataString(query.Sort)}");
            }

            if (query.Page > 1)
            {
                parameters.Add($"page={query.Page.ToString(CultureInfo.InvariantCulture)}");
            }

            if (query.PageSize != 24)
            {
                parameters.Add($"pageSize={query.PageSize.ToString(CultureInfo.InvariantCulture)}");
            }

            if (parameters.Count == 0)
            {
                return path;
            }

            return $"{path}?{string.Join("&", parameters)}";
        }

        private static string NormalizeSort(string? sort)
        {
            var normalized = string.IsNullOrWhiteSpace(sort)
                ? "popular"
                : sort.Trim().ToLowerInvariant();

            return normalized switch
            {
                "price_asc" => "price_asc",
                "price_desc" => "price_desc",
                "newest" => "newest",
                _ => "popular"
            };
        }

        private static string? NormalizeSlug(string? slug)
        {
            return string.IsNullOrWhiteSpace(slug)
                ? null
                : slug.Trim().ToLowerInvariant();
        }

        private static string FormatDecimal(decimal? value)
        {
            return value.HasValue
                ? value.Value.ToString("0.##", CultureInfo.InvariantCulture)
                : string.Empty;
        }

        private static string BoolToken(bool? value)
        {
            return value.HasValue ? (value.Value ? "1" : "0") : string.Empty;
        }

        private sealed record NormalizedListingQuery(
            string? CategorySlug,
            string? BrandSlug,
            string? CollectionSlug,
            decimal? MinPrice,
            decimal? MaxPrice,
            decimal[] Sizes,
            string[] Colors,
            long[] Brands,
            bool? IsOnSale,
            bool? IsNew,
            int Page,
            int PageSize,
            string Sort);
    }
}
