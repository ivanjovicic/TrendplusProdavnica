#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OpenSearch.Client;
using TrendplusProdavnica.Application.Search.Dtos;
using TrendplusProdavnica.Application.Search.Queries;
using TrendplusProdavnica.Application.Search.Services;

namespace TrendplusProdavnica.Infrastructure.Search
{
    public sealed class OpenSearchProductSearchService : IProductSearchService
    {
        private readonly IOpenSearchClient _client;
        private readonly OpenSearchSettings _openSearchSettings;
        private readonly SearchSettings _searchSettings;
        private readonly ProductSearchQueryBuilder _queryBuilder;
        private readonly ProductSearchFacetBuilder _facetBuilder;

        public OpenSearchProductSearchService(
            IOpenSearchClient client,
            IOptions<OpenSearchSettings> openSearchSettings,
            IOptions<SearchSettings> searchSettings,
            ProductSearchQueryBuilder queryBuilder,
            ProductSearchFacetBuilder facetBuilder)
        {
            _client = client;
            _openSearchSettings = openSearchSettings.Value;
            _searchSettings = searchSettings.Value;
            _queryBuilder = queryBuilder;
            _facetBuilder = facetBuilder;
        }

        public async Task<SearchResponseDto> SearchProductsAsync(ProductSearchQuery query, CancellationToken cancellationToken = default)
        {
            var normalizedQuery = Normalize(query);
            var from = (normalizedQuery.Page - 1) * normalizedQuery.PageSize;
            var hasSearchText = !string.IsNullOrWhiteSpace(normalizedQuery.QueryText);

            var response = await _client.SearchAsync<ProductSearchDocument>(descriptor =>
            {
                var searchDescriptor = descriptor
                    .Index(_openSearchSettings.IndexName)
                    .From(from)
                    .Size(normalizedQuery.PageSize)
                    .TrackTotalHits()
                    .Query(_queryBuilder.Build(normalizedQuery))
                    .Aggregations(aggregation => _facetBuilder.BuildAggregations(aggregation, normalizedQuery));

                return ApplySort(searchDescriptor, normalizedQuery.Sort, hasSearchText);
            }, cancellationToken);

            if (!response.IsValid)
            {
                throw new InvalidOperationException(
                    $"Product search query failed: {response.ServerError?.ToString() ?? response.OriginalException?.Message}");
            }

            var products = response.Documents
                .Select(MapItem)
                .ToArray();

            var facets = _facetBuilder.MapFacets(response.Aggregations, normalizedQuery);

            return new SearchResponseDto(
                products,
                response.Total,
                normalizedQuery.Page,
                normalizedQuery.PageSize,
                facets);
        }

        public async Task<ProductAutocompleteResultDto> AutocompleteAsync(ProductAutocompleteQuery query, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query.QueryText))
            {
                return new ProductAutocompleteResultDto(Array.Empty<ProductAutocompleteItemDto>());
            }

            var normalizedText = query.QueryText.Trim();
            var maxQueryLength = _searchSettings.MaxQueryLength <= 0 ? 120 : _searchSettings.MaxQueryLength;
            normalizedText = normalizedText[..Math.Min(normalizedText.Length, maxQueryLength)];
            var limit = Math.Max(1, Math.Min(query.Limit, 50));

            var response = await _client.SearchAsync<ProductSearchDocument>(descriptor => descriptor
                .Index(_openSearchSettings.IndexName)
                .Size(limit)
                .Query(queryDescriptor => queryDescriptor.MultiMatch(multiMatch => multiMatch
                    .Query(normalizedText)
                    .Operator(Operator.Or)
                    .Fields(fields => fields
                        .Field(field => field.Name, 3.0)
                        .Field(field => field.ShortDescription, 1.0)
                        .Field(field => field.SearchKeywords, 2.5)
                        .Field("brandName", 2.0))))
                .Sort(sort => sort
                    .Descending(field => field.SortRank)
                    .Descending(field => field.PublishedAtUtc)), cancellationToken);

            if (!response.IsValid)
            {
                throw new InvalidOperationException(
                    $"Product autocomplete query failed: {response.ServerError?.ToString() ?? response.OriginalException?.Message}");
            }

            var items = response.Documents
                .Distinct(new ProductSearchDocumentSlugComparer())
                .Select(document => new ProductAutocompleteItemDto(
                    document.ProductId,
                    document.Slug,
                    document.Name,
                    document.BrandName,
                    document.PrimaryImageUrl))
                .Take(limit)
                .ToArray();

            return new ProductAutocompleteResultDto(items);
        }

        private ProductSearchQuery Normalize(ProductSearchQuery query)
        {
            var page = query.Page < 1 ? 1 : query.Page;
            var defaultPageSize = _searchSettings.DefaultPageSize <= 0 ? 24 : _searchSettings.DefaultPageSize;
            var maxPageSize = _searchSettings.MaxPageSize <= 0 ? 60 : _searchSettings.MaxPageSize;
            var pageSize = query.PageSize <= 0
                ? defaultPageSize
                : Math.Min(query.PageSize, maxPageSize);
            var maxQueryLength = _searchSettings.MaxQueryLength <= 0 ? 120 : _searchSettings.MaxQueryLength;
            var text = string.IsNullOrWhiteSpace(query.QueryText)
                ? null
                : query.QueryText.Trim()[..Math.Min(query.QueryText.Trim().Length, maxQueryLength)];
            var sort = string.IsNullOrWhiteSpace(query.Sort)
                ? "relevance"
                : query.Sort.Trim().ToLowerInvariant();
            var brands = NormalizeStrings(query.Brands);
            var colors = NormalizeStrings(query.Colors);
            var sizes = query.Sizes?
                .Where(value => value > 0)
                .Distinct()
                .OrderBy(value => value)
                .ToArray();
            var availability = NormalizeAvailability(query.Availability, query.InStockOnly);

            var minPrice = query.MinPrice.HasValue && query.MinPrice.Value >= 0
                ? query.MinPrice
                : null;
            var maxPrice = query.MaxPrice.HasValue && query.MaxPrice.Value >= 0
                ? query.MaxPrice
                : null;

            return query with
            {
                QueryText = text,
                Page = page,
                PageSize = pageSize,
                Brands = brands.Length == 0 ? null : brands,
                Colors = colors.Length == 0 ? null : colors,
                Sizes = sizes is { Length: > 0 } ? sizes : null,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                Availability = availability.Length == 0 ? null : availability,
                InStockOnly = null,
                Sort = sort
            };
        }

        private static string[] NormalizeStrings(string[]? values)
        {
            return values?
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray() ?? Array.Empty<string>();
        }

        private static string[] NormalizeAvailability(string[]? values, bool? inStockOnly)
        {
            var normalized = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (inStockOnly == true)
            {
                normalized.Add(ProductSearchQueryBuilder.AvailabilityInStockValue);
            }

            if (values is not null)
            {
                foreach (var value in values)
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        continue;
                    }

                    switch (value.Trim().ToLowerInvariant())
                    {
                        case "in_stock":
                        case "instock":
                        case "available":
                        case "in-stock":
                            normalized.Add(ProductSearchQueryBuilder.AvailabilityInStockValue);
                            break;
                        case "out_of_stock":
                        case "outofstock":
                        case "unavailable":
                        case "out-of-stock":
                            normalized.Add(ProductSearchQueryBuilder.AvailabilityOutOfStockValue);
                            break;
                    }
                }
            }

            return normalized.ToArray();
        }

        private static SearchDescriptor<ProductSearchDocument> ApplySort(
            SearchDescriptor<ProductSearchDocument> descriptor,
            string? sort,
            bool hasSearchText)
        {
            var normalizedSort = string.IsNullOrWhiteSpace(sort)
                ? "relevance"
                : sort.Trim().ToLowerInvariant();

            return normalizedSort switch
            {
                "popular" => descriptor.Sort(sortDescriptor => sortDescriptor
                    .Descending(field => field.SortRank)
                    .Descending(field => field.PublishedAtUtc)),
                "newest" => descriptor.Sort(sortDescriptor => sortDescriptor
                    .Descending(field => field.PublishedAtUtc)
                    .Descending(field => field.SortRank)),
                "price_asc" => descriptor.Sort(sortDescriptor => sortDescriptor
                    .Ascending(field => field.MinPrice)
                    .Descending(field => field.SortRank)),
                "price_desc" => descriptor.Sort(sortDescriptor => sortDescriptor
                    .Descending(field => field.MaxPrice)
                    .Descending(field => field.SortRank)),
                "bestsellers" => descriptor.Sort(sortDescriptor => sortDescriptor
                    .Descending(field => field.IsBestseller)
                    .Descending(field => field.SortRank)
                    .Descending(field => field.PublishedAtUtc)),
                _ when hasSearchText => descriptor,
                _ => descriptor.Sort(sortDescriptor => sortDescriptor
                    .Descending(field => field.SortRank)
                    .Descending(field => field.PublishedAtUtc))
            };
        }

        private static ProductSearchItemDto MapItem(ProductSearchDocument document)
        {
            return new ProductSearchItemDto(
                document.ProductId,
                document.Slug,
                document.BrandName,
                document.Name,
                document.ShortDescription,
                document.PrimaryCategory,
                document.SecondaryCategories,
                document.PrimaryColorName,
                document.IsNew,
                document.IsBestseller,
                document.IsOnSale,
                ToDecimal(document.MinPrice),
                ToDecimal(document.MaxPrice),
                document.AvailableSizes
                    .Select(value => (decimal)value)
                    .ToArray(),
                document.InStock,
                document.PrimaryImageUrl,
                document.PublishedAtUtc,
                document.SortRank);
        }

        private static decimal? ToDecimal(double? value)
        {
            return value.HasValue ? (decimal)value.Value : null;
        }

        private sealed class ProductSearchDocumentSlugComparer : IEqualityComparer<ProductSearchDocument>
        {
            public bool Equals(ProductSearchDocument? x, ProductSearchDocument? y)
            {
                return x?.Slug == y?.Slug;
            }

            public int GetHashCode(ProductSearchDocument obj)
            {
                return obj.Slug.GetHashCode();
            }
        }
    }
}
