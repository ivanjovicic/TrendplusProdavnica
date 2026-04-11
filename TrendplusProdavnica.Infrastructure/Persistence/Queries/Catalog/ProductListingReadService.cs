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
using Microsoft.Extensions.Logging;
using TrendplusProdavnica.Application.Catalog.Listing;
using TrendplusProdavnica.Application.Merchandising.Services;
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
        private readonly IMerchandisingService _merchandisingService;
        private readonly ILogger<ProductListingReadService> _logger;

        public ProductListingReadService(
            TrendplusDbContext db,
            IFusionCache cache,
            IMerchandisingService merchandisingService,
            ILogger<ProductListingReadService> logger)
        {
            _db = db;
            _cache = cache;
            _merchandisingService = merchandisingService;
            _logger = logger;
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
            var candidateRows = totalCount == 0
                ? Array.Empty<ListingCandidateRow>()
                : await BuildCandidateRowsQuery(products).ToArrayAsync(cancellationToken);

            var pageProductIds = await SortAndPageCandidatesAsync(candidateRows, query, cancellationToken);

            var productRows = pageProductIds.Length == 0
                ? Array.Empty<ListingProductRow>()
                : await BuildProductRowsQuery(pageProductIds).ToArrayAsync(cancellationToken);

            var orderMap = pageProductIds
                .Select((productId, index) => new { productId, index })
                .ToDictionary(item => item.productId, item => item.index);

            var productCards = productRows
                .OrderBy(row => orderMap[row.ProductId])
                .Select(MapProductCard)
                .ToArray();

            var facets = totalCount == 0
                ? new ProductListingFacets(
                    Array.Empty<BrandFacetItem>(),
                    Array.Empty<SizeFacetItem>(),
                    Array.Empty<ColorFacetItem>(),
                    new PriceRangeFacet(null, null))
                : await BuildFacetsAsync(products, cancellationToken);

            return new ProductListingResponse(
                productCards,
                totalCount,
                query.Page,
                query.PageSize,
                facets,
                BuildCanonicalUrl(query));
        }

        private IQueryable<ListingCandidateRow> BuildCandidateRowsQuery(IQueryable<Domain.Catalog.Product> products)
        {
            return products.Select(product => new ListingCandidateRow(
                product.Id,
                product.PrimaryCategoryId,
                product.BrandId,
                product.Variants
                    .Where(variant => variant.IsActive && variant.IsVisible)
                    .Select(variant => (decimal?)variant.Price)
                    .Min(),
                product.IsBestseller,
                product.SortRank,
                product.PublishedAtUtc));
        }

        private IQueryable<ListingProductRow> BuildProductRowsQuery(long[] pageProductIds)
        {
            return from product in _db.Products.AsNoTracking()
                   where pageProductIds.Contains(product.Id)
                   join brand in _db.Brands.AsNoTracking() on product.BrandId equals brand.Id
                   select new ListingProductRow(
                       product.Id,
                       product.Slug,
                       product.Name,
                       brand.Name,
                       product.PrimaryColorName,
                       product.IsNew,
                       product.IsBestseller,
                       product.Variants.Any(variant =>
                           variant.IsActive &&
                           variant.IsVisible &&
                           variant.OldPrice.HasValue &&
                           variant.OldPrice.Value > variant.Price),
                       product.Variants
                           .Where(variant => variant.IsActive && variant.IsVisible)
                           .Select(variant => (decimal?)variant.Price)
                           .Min(),
                       product.Variants
                           .Where(variant =>
                               variant.IsActive &&
                               variant.IsVisible &&
                               variant.OldPrice.HasValue &&
                               variant.OldPrice.Value > variant.Price)
                           .Select(variant => variant.OldPrice)
                           .Min(),
                       product.Media
                           .Where(media => media.IsActive && media.IsPrimary)
                           .OrderBy(media => media.SortOrder)
                           .Select(media => media.Url)
                           .FirstOrDefault(),
                       product.Media
                           .Where(media => media.IsActive && !media.IsPrimary)
                           .OrderBy(media => media.SortOrder)
                           .Select(media => media.Url)
                           .FirstOrDefault(),
                       product.Media
                           .Where(media => media.IsActive)
                           .OrderBy(media => media.SortOrder)
                           .Select(media => media.Url)
                           .FirstOrDefault(),
                       product.Variants
                           .Where(variant => variant.IsActive && variant.IsVisible && variant.TotalStock > 0)
                           .Select(variant => variant.SizeEu)
                           .Distinct()
                           .Count());
        }

        private async Task<long[]> SortAndPageCandidatesAsync(
            IReadOnlyList<ListingCandidateRow> candidates,
            NormalizedListingQuery query,
            CancellationToken cancellationToken)
        {
            if (candidates.Count == 0)
            {
                return Array.Empty<long>();
            }

            var adjustments = await EvaluateMerchandisingAdjustmentsAsync(candidates, cancellationToken);
            var scoredCandidates = candidates
                .Select(candidate => new ScoredListingCandidate(
                    candidate,
                    adjustments.TryGetValue(candidate.ProductId, out var adjustedScore)
                        ? adjustedScore
                        : CalculateBaseScore(candidate)))
                .ToArray();

            var sortedCandidates = query.Sort switch
            {
                "newest" => scoredCandidates
                    .OrderByDescending(item => item.AdjustedScore)
                    .ThenByDescending(item => item.Candidate.PublishedAtUtc)
                    .ThenByDescending(item => item.Candidate.ProductId),
                "price_asc" => scoredCandidates
                    .OrderBy(item => item.Candidate.MinPrice ?? decimal.MaxValue)
                    .ThenByDescending(item => item.AdjustedScore)
                    .ThenByDescending(item => item.Candidate.ProductId),
                "price_desc" => scoredCandidates
                    .OrderByDescending(item => item.Candidate.MinPrice ?? 0m)
                    .ThenByDescending(item => item.AdjustedScore)
                    .ThenByDescending(item => item.Candidate.ProductId),
                "bestsellers" => scoredCandidates
                    .OrderByDescending(item => item.AdjustedScore)
                    .ThenByDescending(item => item.Candidate.IsBestseller)
                    .ThenByDescending(item => item.Candidate.SortRank)
                    .ThenByDescending(item => item.Candidate.PublishedAtUtc)
                    .ThenByDescending(item => item.Candidate.ProductId),
                _ => scoredCandidates
                    .OrderByDescending(item => item.AdjustedScore)
                    .ThenByDescending(item => item.Candidate.SortRank)
                    .ThenByDescending(item => item.Candidate.IsBestseller)
                    .ThenByDescending(item => item.Candidate.PublishedAtUtc)
                    .ThenByDescending(item => item.Candidate.ProductId)
            };

            return sortedCandidates
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(item => item.Candidate.ProductId)
                .ToArray();
        }

        private async Task<Dictionary<long, decimal>> EvaluateMerchandisingAdjustmentsAsync(
            IReadOnlyList<ListingCandidateRow> candidates,
            CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var inputs = candidates.Select(candidate => new RuleEvaluationInput
                {
                    ProductId = candidate.ProductId,
                    CategoryId = candidate.PrimaryCategoryId,
                    BrandId = candidate.BrandId,
                    CurrentScore = CalculateBaseScore(candidate)
                });

                return await _merchandisingService.EvaluateRulesAsync(inputs, DateTimeOffset.UtcNow);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to evaluate merchandising rules for PLP query. Falling back to base sorting.");
                return new Dictionary<long, decimal>();
            }
        }

        private static decimal CalculateBaseScore(ListingCandidateRow candidate)
        {
            var bestsellerBoost = candidate.IsBestseller ? 1000m : 0m;
            var freshnessAdjustment = 0m;

            if (candidate.PublishedAtUtc.HasValue)
            {
                var ageDays = (DateTimeOffset.UtcNow - candidate.PublishedAtUtc.Value).Days;
                freshnessAdjustment = ageDays > 0 ? -(ageDays * 0.1m) : 0m;
            }

            return candidate.SortRank + bestsellerBoost + freshnessAdjustment;
        }

        private static ProductCardDto MapProductCard(ListingProductRow row)
        {
            var displayPrice = row.Price ?? 0m;
            var oldPrice = row.OldPrice;
            var discountPercent = oldPrice.HasValue && oldPrice.Value > displayPrice && displayPrice > 0
                ? (int?)Math.Round((oldPrice.Value - displayPrice) / oldPrice.Value * 100m, MidpointRounding.AwayFromZero)
                : null;

            return new ProductCardDto(
                row.ProductId,
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
                row.IsBestseller,
                row.IsOnSale,
                row.Color);
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

            if (query.InStockOnly == true)
            {
                products = products.Where(product => product.Variants.Any(variant =>
                    variant.IsActive &&
                    variant.IsVisible &&
                    variant.TotalStock > 0));
            }

            return products;
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

            var sizeFacetRows = await products
                .SelectMany(product => product.Variants
                    .Where(variant => variant.IsActive && variant.IsVisible)
                    .Select(variant => new { product.Id, variant.SizeEu }))
                .Distinct()
                .ToArrayAsync(cancellationToken);

            var sizeFacets = sizeFacetRows
                .GroupBy(row => row.SizeEu)
                .Select(group => new SizeFacetItem(group.Key, group.Count()))
                .OrderBy(item => item.Size)
                .ToArray();

            var colorFacetRows = await products
                .SelectMany(product => product.Variants
                    .Where(variant => variant.IsActive && variant.IsVisible)
                    .Select(variant => new
                    {
                        product.Id,
                        Color = (variant.ColorName ?? product.PrimaryColorName) ?? string.Empty
                    }))
                .Where(row => row.Color != string.Empty)
                .Distinct()
                .ToArrayAsync(cancellationToken);

            var colorFacets = colorFacetRows
                .GroupBy(row => row.Color)
                .Select(group => new ColorFacetItem(group.Key, group.Count()))
                .OrderBy(item => item.Color)
                .ToArray();

            var priceRows = await products
                .SelectMany(product => product.Variants
                    .Where(variant => variant.IsActive && variant.IsVisible)
                    .Select(variant => (decimal?)variant.Price))
                .ToArrayAsync(cancellationToken);

            var priceRange = priceRows.Length == 0
                ? new PriceRangeFacet(null, null)
                : new PriceRangeFacet(priceRows.Min(), priceRows.Max());

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
                query.InStockOnly,
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
                $"stock={BoolToken(query.InStockOnly)}",
                $"page={query.Page}",
                $"pageSize={query.PageSize}",
                $"sort={query.Sort}"
            });

            var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));
            return Convert.ToHexString(hashBytes).Substring(0, 16).ToLowerInvariant();
        }

        private static string BuildCanonicalUrl(NormalizedListingQuery query)
        {
            if (!string.IsNullOrWhiteSpace(query.BrandSlug))
            {
                return $"/brendovi/{query.BrandSlug}";
            }

            if (!string.IsNullOrWhiteSpace(query.CollectionSlug))
            {
                return $"/kolekcije/{query.CollectionSlug}";
            }

            if (query.IsOnSale == true && !string.IsNullOrWhiteSpace(query.CategorySlug))
            {
                return $"/akcija/{query.CategorySlug}";
            }

            if (query.IsOnSale == true)
            {
                return "/akcija";
            }

            if (!string.IsNullOrWhiteSpace(query.CategorySlug))
            {
                return $"/{query.CategorySlug}";
            }

            return "/katalog";
        }

        private static string NormalizeSort(string? sort)
        {
            var normalized = string.IsNullOrWhiteSpace(sort)
                ? "popular"
                : sort.Trim().ToLowerInvariant();

            return normalized switch
            {
                "popular" => "popular",
                "recommended" => "popular",
                "price_asc" => "price_asc",
                "price-asc" => "price_asc",
                "price_desc" => "price_desc",
                "price-desc" => "price_desc",
                "newest" => "newest",
                "bestsellers" => "bestsellers",
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

        private sealed record ListingCandidateRow(
            long ProductId,
            long PrimaryCategoryId,
            long BrandId,
            decimal? MinPrice,
            bool IsBestseller,
            int SortRank,
            DateTimeOffset? PublishedAtUtc);

        private sealed record ScoredListingCandidate(ListingCandidateRow Candidate, decimal AdjustedScore);

        private sealed record ListingProductRow(
            long ProductId,
            string Slug,
            string Name,
            string BrandName,
            string? Color,
            bool IsNew,
            bool IsBestseller,
            bool IsOnSale,
            decimal? Price,
            decimal? OldPrice,
            string? PrimaryImageUrl,
            string? SecondaryImageUrl,
            string? FallbackImageUrl,
            int AvailableSizesCount);

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
            bool? InStockOnly,
            int Page,
            int PageSize,
            string Sort);
    }
}
