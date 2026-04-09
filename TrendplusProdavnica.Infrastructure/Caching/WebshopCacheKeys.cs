#nullable enable
using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using TrendplusProdavnica.Application.Catalog.Queries;
using TrendplusProdavnica.Application.Common.Caching;

namespace TrendplusProdavnica.Infrastructure.Caching
{
    public sealed class WebshopCacheKeys : IWebshopCacheKeys
    {
        private const string VersionToken = "v1";
        private readonly CacheSettings _settings;

        public WebshopCacheKeys(IOptions<CacheSettings> settings)
        {
            _settings = settings.Value;
        }

        public string HomePage() => BuildKey("home", VersionToken);

        public string ProductDetail(string slug) => BuildKey("pdp", VersionToken, NormalizeSlug(slug));

        public string BrandPage(string slug) => BuildKey("brand", VersionToken, NormalizeSlug(slug));

        public string CollectionPage(string slug) => BuildKey("collection", VersionToken, NormalizeSlug(slug));

        public string StorePage(string slug) => BuildKey("store", VersionToken, NormalizeSlug(slug));

        public string EditorialDetail(string slug) => BuildKey("editorial", VersionToken, NormalizeSlug(slug));

        public string EditorialList() => BuildKey("editorial-list", VersionToken);

        public string? CategoryListing(GetCategoryListingQuery query)
        {
            return BuildListingKey("category", query.Slug, query);
        }

        public string? BrandListing(GetBrandListingQuery query)
        {
            return BuildListingKey("brand", query.Slug, query);
        }

        public string? CollectionListing(GetCollectionListingQuery query)
        {
            return BuildListingKey("collection", query.Slug, query);
        }

        public string? SaleListing(GetSaleListingQuery query)
        {
            return BuildListingKey("sale", query.CategorySlug ?? "all", query);
        }

        private string? BuildListingKey(string scope, string scopeSlug, GetCategoryListingQuery query)
        {
            var listingSettings = _settings.Listing;

            if (!listingSettings.Enabled)
            {
                return null;
            }

            var page = query.Page <= 0 ? 1 : query.Page;
            if (listingSettings.FirstPageOnly && page != 1)
            {
                return null;
            }

            var pageSize = query.PageSize <= 0 ? listingSettings.MaxPageSize : query.PageSize;
            if (pageSize > listingSettings.MaxPageSize)
            {
                return null;
            }

            if (listingSettings.CacheOnlyWithoutFilters && HasCustomFilters(query))
            {
                return null;
            }

            var hash = ComputeListingHash(
                page,
                pageSize,
                query.Sort,
                query.Sizes,
                query.Colors,
                query.Brands,
                query.PriceFrom,
                query.PriceTo,
                query.IsOnSale,
                query.IsNew,
                query.InStockOnly);

            return BuildKey("listing", scope, VersionToken, NormalizeSlug(scopeSlug), hash);
        }

        private string? BuildListingKey(string scope, string scopeSlug, GetBrandListingQuery query)
        {
            return BuildListingKey(scope, scopeSlug, new GetCategoryListingQuery(
                scopeSlug,
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
                query.InStockOnly));
        }

        private string? BuildListingKey(string scope, string scopeSlug, GetCollectionListingQuery query)
        {
            return BuildListingKey(scope, scopeSlug, new GetCategoryListingQuery(
                scopeSlug,
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
                query.InStockOnly));
        }

        private string? BuildListingKey(string scope, string scopeSlug, GetSaleListingQuery query)
        {
            return BuildListingKey(scope, scopeSlug, new GetCategoryListingQuery(
                scopeSlug,
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
                query.InStockOnly));
        }

        private static bool HasCustomFilters(GetCategoryListingQuery query)
        {
            var hasSizes = query.Sizes is { Length: > 0 };
            var hasColors = query.Colors is { Length: > 0 };
            var hasBrands = query.Brands is { Length: > 0 };

            return hasSizes ||
                   hasColors ||
                   hasBrands ||
                   query.PriceFrom.HasValue ||
                   query.PriceTo.HasValue ||
                   query.IsOnSale == true ||
                   query.IsNew == true ||
                   query.InStockOnly == true ||
                   (!string.IsNullOrWhiteSpace(query.Sort) && !string.Equals(query.Sort, "recommended", StringComparison.OrdinalIgnoreCase));
        }

        private static string ComputeListingHash(
            int page,
            int pageSize,
            string? sort,
            long[]? sizes,
            string[]? colors,
            long[]? brands,
            decimal? priceFrom,
            decimal? priceTo,
            bool? isOnSale,
            bool? isNew,
            bool? inStockOnly)
        {
            var canonical = string.Join("|", new[]
            {
                $"p={page}",
                $"ps={pageSize}",
                $"s={NormalizeSort(sort)}",
                $"sz={JoinLongs(sizes)}",
                $"c={JoinStrings(colors)}",
                $"b={JoinLongs(brands)}",
                $"pf={FormatDecimal(priceFrom)}",
                $"pt={FormatDecimal(priceTo)}",
                $"sale={BoolToken(isOnSale)}",
                $"new={BoolToken(isNew)}",
                $"stock={BoolToken(inStockOnly)}"
            });

            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));
            return Convert.ToHexString(bytes).Substring(0, 12).ToLowerInvariant();
        }

        private string BuildKey(params string[] segments)
        {
            var normalizedPrefix = string.IsNullOrWhiteSpace(_settings.KeyPrefix)
                ? "tp"
                : _settings.KeyPrefix.Trim().ToLowerInvariant();

            return $"{normalizedPrefix}:{string.Join(':', segments)}";
        }

        private static string NormalizeSort(string? sort)
        {
            return string.IsNullOrWhiteSpace(sort)
                ? "recommended"
                : sort.Trim().ToLowerInvariant();
        }

        private static string NormalizeSlug(string slug)
        {
            return slug.Trim().ToLowerInvariant();
        }

        private static string JoinLongs(long[]? values)
        {
            if (values is null || values.Length == 0)
            {
                return string.Empty;
            }

            return string.Join(",", values.OrderBy(value => value));
        }

        private static string JoinStrings(string[]? values)
        {
            if (values is null || values.Length == 0)
            {
                return string.Empty;
            }

            return string.Join(",", values
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim().ToLowerInvariant())
                .OrderBy(value => value));
        }

        private static string FormatDecimal(decimal? value)
        {
            return value.HasValue
                ? value.Value.ToString("0.####", CultureInfo.InvariantCulture)
                : string.Empty;
        }

        private static string BoolToken(bool? value)
        {
            return value.HasValue ? (value.Value ? "1" : "0") : string.Empty;
        }
    }
}
