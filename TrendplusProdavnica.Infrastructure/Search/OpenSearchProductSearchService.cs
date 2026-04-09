#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
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
        private const int FacetBucketSize = 20;
        private readonly IOpenSearchClient _client;
        private readonly OpenSearchSettings _openSearchSettings;
        private readonly SearchSettings _searchSettings;

        public OpenSearchProductSearchService(
            IOpenSearchClient client,
            IOptions<OpenSearchSettings> openSearchSettings,
            IOptions<SearchSettings> searchSettings)
        {
            _client = client;
            _openSearchSettings = openSearchSettings.Value;
            _searchSettings = searchSettings.Value;
        }

        public async Task<ProductSearchResultDto> SearchProductsAsync(ProductSearchQuery query, CancellationToken cancellationToken = default)
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
                    .Query(queryDescriptor => BuildQuery(queryDescriptor, normalizedQuery))
                    .Aggregations(aggregation => aggregation
                        .Terms("brands", terms => terms.Field("brandName.keyword").Size(FacetBucketSize))
                        .Terms("colors", terms => terms.Field("primaryColorName.keyword").Size(FacetBucketSize))
                        .Terms("sizes", terms => terms.Field("availableSizes").Size(FacetBucketSize))
                        .Filter("sale_true", filter => filter.Filter(filterQuery => filterQuery.Term(term => term
                            .Field(field => field.IsOnSale)
                            .Value(true))))
                        .Filter("new_true", filter => filter.Filter(filterQuery => filterQuery.Term(term => term
                            .Field(field => field.IsNew)
                            .Value(true))))
                        .Filter("stock_true", filter => filter.Filter(filterQuery => filterQuery.Term(term => term
                            .Field(field => field.InStock)
                            .Value(true)))));

                return ApplySort(searchDescriptor, normalizedQuery.Sort, hasSearchText);
            }, cancellationToken);

            if (!response.IsValid)
            {
                throw new InvalidOperationException(
                    $"Product search query failed: {response.ServerError?.ToString() ?? response.OriginalException?.Message}");
            }

            var items = response.Documents
                .Select(MapItem)
                .ToArray();

            var facets = MapFacets(response.Aggregations);
            var pagination = new ProductSearchPaginationDto(normalizedQuery.Page, normalizedQuery.PageSize, response.Total);

            return new ProductSearchResultDto(
                response.Total,
                items,
                pagination,
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
                .AllIndices(false)
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
                .Select(doc => new ProductAutocompleteItemDto(
                    doc.ProductId,
                    doc.Slug,
                    doc.Name,
                    doc.BrandName,
                    doc.PrimaryImageUrl))
                .Take(limit)
                .ToArray();

            return new ProductAutocompleteResultDto(items);
        }

        private class ProductSearchDocumentSlugComparer : IEqualityComparer<ProductSearchDocument>
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

            return query with
            {
                QueryText = text,
                Page = page,
                PageSize = pageSize,
                Sort = sort
            };
        }

        private static QueryContainer BuildQuery(
            QueryContainerDescriptor<ProductSearchDocument> queryDescriptor,
            ProductSearchQuery query)
        {
            var filters = new List<Func<QueryContainerDescriptor<ProductSearchDocument>, QueryContainer>>();

            if (!string.IsNullOrWhiteSpace(query.Brand))
            {
                var brand = query.Brand.Trim();
                filters.Add(filter => filter.Term(term => term
                    .Field("brandName.keyword")
                    .Value(brand)));
            }

            if (!string.IsNullOrWhiteSpace(query.Color))
            {
                var color = query.Color.Trim();
                filters.Add(filter => filter.Term(term => term
                    .Field("primaryColorName.keyword")
                    .Value(color)));
            }

            if (query.Size.HasValue)
            {
                filters.Add(filter => filter.Term(term => term
                    .Field(field => field.AvailableSizes)
                    .Value((double)query.Size.Value)));
            }

            if (query.IsOnSale.HasValue)
            {
                filters.Add(filter => filter.Term(term => term
                    .Field(field => field.IsOnSale)
                    .Value(query.IsOnSale.Value)));
            }

            if (query.IsNew.HasValue)
            {
                filters.Add(filter => filter.Term(term => term
                    .Field(field => field.IsNew)
                    .Value(query.IsNew.Value)));
            }

            if (query.InStockOnly == true)
            {
                filters.Add(filter => filter.Term(term => term
                    .Field(field => field.InStock)
                    .Value(true)));
            }

            if (!string.IsNullOrWhiteSpace(query.QueryText))
            {
                return queryDescriptor.Bool(boolean => boolean
                    .Must(must => must.MultiMatch(multiMatch => multiMatch
                        .Query(query.QueryText)
                        .Operator(Operator.And)
                        .Fields(fields => fields
                            .Field(field => field.Name, 4.0)
                            .Field(field => field.ShortDescription, 1.5)
                            .Field(field => field.SearchKeywords, 3.0)
                            .Field("brandName", 2.0)
                            .Field("primaryCategory", 1.5)
                            .Field("secondaryCategories", 1.2))))
                    .Filter(filters));
            }

            if (filters.Count == 0)
            {
                return queryDescriptor.MatchAll();
            }

            return queryDescriptor.Bool(boolean => boolean.Filter(filters));
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

        private static ProductSearchFacetsDto MapFacets(AggregateDictionary aggregations)
        {
            var brands = MapStringFacet(aggregations.Terms("brands"));
            var colors = MapStringFacet(aggregations.Terms("colors"));
            var sizes = MapDoubleFacet(aggregations.Terms<double>("sizes"));

            var saleCount = aggregations.Filter("sale_true")?.DocCount ?? 0;
            var newCount = aggregations.Filter("new_true")?.DocCount ?? 0;
            var stockCount = aggregations.Filter("stock_true")?.DocCount ?? 0;

            return new ProductSearchFacetsDto(
                brands,
                colors,
                sizes,
                new[] { new SearchFacetOptionDto("true", saleCount) },
                new[] { new SearchFacetOptionDto("true", newCount) },
                new[] { new SearchFacetOptionDto("true", stockCount) });
        }

        private static SearchFacetOptionDto[] MapStringFacet(TermsAggregate<string>? aggregate)
        {
            if (aggregate?.Buckets is null)
            {
                return Array.Empty<SearchFacetOptionDto>();
            }

            return aggregate.Buckets
                .Where(bucket => !string.IsNullOrWhiteSpace(bucket.Key))
                .Select(bucket => new SearchFacetOptionDto(
                    bucket.Key!,
                    bucket.DocCount ?? 0))
                .OrderByDescending(option => option.Count)
                .ThenBy(option => option.Value)
                .ToArray();
        }

        private static SearchFacetOptionDto[] MapDoubleFacet(TermsAggregate<double>? aggregate)
        {
            if (aggregate?.Buckets is null)
            {
                return Array.Empty<SearchFacetOptionDto>();
            }

            return aggregate.Buckets
                .Select(bucket => new SearchFacetOptionDto(
                    bucket.Key.ToString("0.#", CultureInfo.InvariantCulture),
                    bucket.DocCount ?? 0))
                .OrderBy(option =>
                {
                    return decimal.TryParse(option.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed)
                        ? parsed
                        : decimal.MaxValue;
                })
                .ToArray();
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
    }
}
