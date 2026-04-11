#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Options;
using OpenSearch.Client;
using TrendplusProdavnica.Application.Search.Dtos;
using TrendplusProdavnica.Application.Search.Queries;

namespace TrendplusProdavnica.Infrastructure.Search
{
    public sealed class ProductSearchFacetBuilder
    {
        private readonly ProductSearchQueryBuilder _queryBuilder;
        private readonly SearchSettings _searchSettings;

        public ProductSearchFacetBuilder(
            ProductSearchQueryBuilder queryBuilder,
            IOptions<SearchSettings> searchSettings)
        {
            _queryBuilder = queryBuilder;
            _searchSettings = searchSettings.Value;
        }

        public AggregationContainerDescriptor<ProductSearchDocument> BuildAggregations(
            AggregationContainerDescriptor<ProductSearchDocument> aggregations,
            ProductSearchQuery query)
        {
            var facetBucketSize = _searchSettings.FacetBucketSize <= 0 ? 20 : _searchSettings.FacetBucketSize;

            return aggregations
                .Filter(BrandsScopeName, filter => filter
                    .Filter(_queryBuilder.Build(query, ProductSearchFacetScope.Brand))
                    .Aggregations(children => children
                        .Terms(BrandsValuesName, terms => terms
                            .Field("brandName.keyword")
                            .Size(facetBucketSize))))
                .Filter(SizesScopeName, filter => filter
                    .Filter(_queryBuilder.Build(query, ProductSearchFacetScope.Size))
                    .Aggregations(children => children
                        .Terms(SizesValuesName, terms => terms
                            .Field(field => field.AvailableSizes)
                            .Size(facetBucketSize))))
                .Filter(ColorsScopeName, filter => filter
                    .Filter(_queryBuilder.Build(query, ProductSearchFacetScope.Color))
                    .Aggregations(children => children
                        .Terms(ColorsValuesName, terms => terms
                            .Field("primaryColorName.keyword")
                            .Size(facetBucketSize))))
                .Filter(PriceScopeName, filter => filter
                    .Filter(_queryBuilder.Build(query, ProductSearchFacetScope.Price))
                    .Aggregations(children => children
                        .Min(PriceMinName, minimum => minimum.Field(field => field.MinPrice))
                        .Max(PriceMaxName, maximum => maximum.Field(field => field.MaxPrice))))
                .Filter(AvailabilityScopeName, filter => filter
                    .Filter(_queryBuilder.Build(query, ProductSearchFacetScope.Availability))
                    .Aggregations(children => children
                        .Filter(AvailabilityInStockName, scoped => scoped.Filter(filterQuery => filterQuery.Term(term => term
                            .Field(field => field.InStock)
                            .Value(true))))
                        .Filter(AvailabilityOutOfStockName, scoped => scoped.Filter(filterQuery => filterQuery.Term(term => term
                            .Field(field => field.InStock)
                            .Value(false))))))
                .Filter(SaleScopeName, filter => filter
                    .Filter(_queryBuilder.Build(query, ProductSearchFacetScope.Sale))
                    .Aggregations(children => children
                        .Filter(SaleTrueName, scoped => scoped.Filter(filterQuery => filterQuery.Term(term => term
                            .Field(field => field.IsOnSale)
                            .Value(true))))
                        .Filter(SaleFalseName, scoped => scoped.Filter(filterQuery => filterQuery.Term(term => term
                            .Field(field => field.IsOnSale)
                            .Value(false))))))
                .Filter(NewScopeName, filter => filter
                    .Filter(_queryBuilder.Build(query, ProductSearchFacetScope.New))
                    .Aggregations(children => children
                        .Filter(NewTrueName, scoped => scoped.Filter(filterQuery => filterQuery.Term(term => term
                            .Field(field => field.IsNew)
                            .Value(true))))
                        .Filter(NewFalseName, scoped => scoped.Filter(filterQuery => filterQuery.Term(term => term
                            .Field(field => field.IsNew)
                            .Value(false))))));
        }

        public SearchFacetsDto MapFacets(AggregateDictionary aggregations, ProductSearchQuery query)
        {
            var selectedBrands = query.Brands ?? Array.Empty<string>();
            var selectedColors = query.Colors ?? Array.Empty<string>();
            var selectedSizes = query.Sizes?
                .Select(FormatDecimal)
                .ToArray() ?? Array.Empty<string>();

            var brandTerms = GetAggregate<TermsAggregate<string>>(aggregations.Filter(BrandsScopeName), BrandsValuesName);
            var sizeTerms = GetAggregate<TermsAggregate<double>>(aggregations.Filter(SizesScopeName), SizesValuesName);
            var colorTerms = GetAggregate<TermsAggregate<string>>(aggregations.Filter(ColorsScopeName), ColorsValuesName);
            var priceScope = aggregations.Filter(PriceScopeName);
            var availabilityScope = aggregations.Filter(AvailabilityScopeName);
            var saleScope = aggregations.Filter(SaleScopeName);
            var newScope = aggregations.Filter(NewScopeName);

            return new SearchFacetsDto(
                MapStringFacet(brandTerms, selectedBrands),
                MapDoubleFacet(sizeTerms, selectedSizes),
                MapStringFacet(colorTerms, selectedColors),
                new SearchPriceRangeFacetDto(
                    ToDecimal(GetAggregate<ValueAggregate>(priceScope, PriceMinName)?.Value),
                    ToDecimal(GetAggregate<ValueAggregate>(priceScope, PriceMaxName)?.Value),
                    query.MinPrice,
                    query.MaxPrice),
                new[]
                {
                    new SearchFacetValueDto(
                        ProductSearchQueryBuilder.AvailabilityInStockValue,
                        "In stock",
                        GetBucketDocCount(availabilityScope, AvailabilityInStockName),
                        IsAvailabilitySelected(query, ProductSearchQueryBuilder.AvailabilityInStockValue)),
                    new SearchFacetValueDto(
                        ProductSearchQueryBuilder.AvailabilityOutOfStockValue,
                        "Out of stock",
                        GetBucketDocCount(availabilityScope, AvailabilityOutOfStockName),
                        IsAvailabilitySelected(query, ProductSearchQueryBuilder.AvailabilityOutOfStockValue))
                },
                new[]
                {
                    new SearchFacetValueDto("true", "On sale", GetBucketDocCount(saleScope, SaleTrueName), query.IsOnSale == true),
                    new SearchFacetValueDto("false", "Regular price", GetBucketDocCount(saleScope, SaleFalseName), query.IsOnSale == false)
                },
                new[]
                {
                    new SearchFacetValueDto("true", "New arrivals", GetBucketDocCount(newScope, NewTrueName), query.IsNew == true),
                    new SearchFacetValueDto("false", "Existing", GetBucketDocCount(newScope, NewFalseName), query.IsNew == false)
                });
        }

        private static SearchFacetValueDto[] MapStringFacet(TermsAggregate<string>? aggregate, string[] selectedValues)
        {
            if (aggregate?.Buckets is null)
            {
                return Array.Empty<SearchFacetValueDto>();
            }

            var selected = selectedValues
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return aggregate.Buckets
                .Where(bucket => !string.IsNullOrWhiteSpace(bucket.Key))
                .Select(bucket => new SearchFacetValueDto(
                    bucket.Key!,
                    bucket.Key!,
                    bucket.DocCount ?? 0,
                    selected.Contains(bucket.Key!)))
                .OrderByDescending(option => option.Count)
                .ThenBy(option => option.Label)
                .ToArray();
        }

        private static SearchFacetValueDto[] MapDoubleFacet(TermsAggregate<double>? aggregate, string[] selectedValues)
        {
            if (aggregate?.Buckets is null)
            {
                return Array.Empty<SearchFacetValueDto>();
            }

            var selected = selectedValues
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return aggregate.Buckets
                .Select(bucket =>
                {
                    var value = FormatDouble(bucket.Key);
                    return new SearchFacetValueDto(
                        value,
                        value,
                        bucket.DocCount ?? 0,
                        selected.Contains(value));
                })
                .OrderBy(option => decimal.TryParse(option.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed)
                    ? parsed
                    : decimal.MaxValue)
                .ToArray();
        }

        private static bool IsAvailabilitySelected(ProductSearchQuery query, string expectedValue)
        {
            if (query.InStockOnly == true && string.Equals(expectedValue, ProductSearchQueryBuilder.AvailabilityInStockValue, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return query.Availability?.Any(value => string.Equals(value, expectedValue, StringComparison.OrdinalIgnoreCase)) == true;
        }

        private static string FormatDecimal(decimal value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }

        private static string FormatDouble(double value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }

        private static decimal? ToDecimal(double? value)
        {
            return value.HasValue ? (decimal)value.Value : null;
        }

        private static TAggregate? GetAggregate<TAggregate>(
            IReadOnlyDictionary<string, IAggregate>? aggregations,
            string aggregationName)
            where TAggregate : class, IAggregate
        {
            if (aggregations is null || !aggregations.TryGetValue(aggregationName, out var aggregate))
            {
                return null;
            }

            return aggregate as TAggregate;
        }

        private static long GetBucketDocCount(
            IReadOnlyDictionary<string, IAggregate>? aggregations,
            string aggregationName)
        {
            return GetAggregate<SingleBucketAggregate>(aggregations, aggregationName)?.DocCount ?? 0;
        }

        public const string BrandsScopeName = "brands_scope";
        public const string BrandsValuesName = "brands";
        public const string SizesScopeName = "sizes_scope";
        public const string SizesValuesName = "sizes";
        public const string ColorsScopeName = "colors_scope";
        public const string ColorsValuesName = "colors";
        public const string PriceScopeName = "price_scope";
        public const string PriceMinName = "price_min";
        public const string PriceMaxName = "price_max";
        public const string AvailabilityScopeName = "availability_scope";
        public const string AvailabilityInStockName = "availability_in_stock";
        public const string AvailabilityOutOfStockName = "availability_out_of_stock";
        public const string SaleScopeName = "sale_scope";
        public const string SaleTrueName = "sale_true";
        public const string SaleFalseName = "sale_false";
        public const string NewScopeName = "new_scope";
        public const string NewTrueName = "new_true";
        public const string NewFalseName = "new_false";
    }
}
