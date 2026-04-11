#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using OpenSearch.Client;
using TrendplusProdavnica.Application.Search.Queries;

namespace TrendplusProdavnica.Infrastructure.Search
{
    public sealed class ProductSearchQueryBuilder
    {
        public Func<QueryContainerDescriptor<ProductSearchDocument>, QueryContainer> Build(ProductSearchQuery query)
        {
            return Build(query, ProductSearchFacetScope.None);
        }

        public Func<QueryContainerDescriptor<ProductSearchDocument>, QueryContainer> Build(
            ProductSearchQuery query,
            ProductSearchFacetScope excludedFacet)
        {
            return descriptor =>
            {
                var must = BuildMustClauses(query);
                var filters = BuildFilterClauses(query, excludedFacet);

                if (must.Count == 0 && filters.Count == 0)
                {
                    return descriptor.MatchAll();
                }

                return descriptor.Bool(boolean =>
                {
                    if (must.Count > 0)
                    {
                        boolean = boolean.Must(must);
                    }

                    if (filters.Count > 0)
                    {
                        boolean = boolean.Filter(filters);
                    }

                    return boolean;
                });
            };
        }

        private static List<Func<QueryContainerDescriptor<ProductSearchDocument>, QueryContainer>> BuildMustClauses(ProductSearchQuery query)
        {
            var must = new List<Func<QueryContainerDescriptor<ProductSearchDocument>, QueryContainer>>();

            if (!string.IsNullOrWhiteSpace(query.QueryText))
            {
                must.Add(descriptor => descriptor.MultiMatch(multiMatch => multiMatch
                    .Query(query.QueryText)
                    .Operator(Operator.And)
                    .Fields(fields => fields
                        .Field(field => field.Name, 4.0)
                        .Field(field => field.ShortDescription, 1.5)
                        .Field(field => field.SearchKeywords, 3.0)
                        .Field("brandName", 2.0)
                        .Field("primaryCategory", 1.5)
                        .Field("secondaryCategories", 1.2))));
            }

            return must;
        }

        private static List<Func<QueryContainerDescriptor<ProductSearchDocument>, QueryContainer>> BuildFilterClauses(
            ProductSearchQuery query,
            ProductSearchFacetScope excludedFacet)
        {
            var filters = new List<Func<QueryContainerDescriptor<ProductSearchDocument>, QueryContainer>>();

            if (excludedFacet != ProductSearchFacetScope.Brand)
            {
                var brands = NormalizeStrings(query.Brands);
                if (brands.Length > 0)
                {
                    filters.Add(BuildStringAnyOfFilter("brandName.keyword", brands));
                }
            }

            if (excludedFacet != ProductSearchFacetScope.Color)
            {
                var colors = NormalizeStrings(query.Colors);
                if (colors.Length > 0)
                {
                    filters.Add(BuildStringAnyOfFilter("primaryColorName.keyword", colors));
                }
            }

            if (excludedFacet != ProductSearchFacetScope.Size)
            {
                var sizes = query.Sizes?
                    .Where(value => value > 0)
                    .Distinct()
                    .ToArray() ?? Array.Empty<decimal>();

                if (sizes.Length > 0)
                {
                    filters.Add(BuildDecimalAnyOfFilter("availableSizes", sizes));
                }
            }

            if (excludedFacet != ProductSearchFacetScope.Price)
            {
                if (query.MinPrice.HasValue)
                {
                    var selectedMin = (double)query.MinPrice.Value;
                    filters.Add(descriptor => descriptor.Range(range => range
                        .Field(field => field.MaxPrice)
                        .GreaterThanOrEquals(selectedMin)));
                }

                if (query.MaxPrice.HasValue)
                {
                    var selectedMax = (double)query.MaxPrice.Value;
                    filters.Add(descriptor => descriptor.Range(range => range
                        .Field(field => field.MinPrice)
                        .LessThanOrEquals(selectedMax)));
                }
            }

            if (excludedFacet != ProductSearchFacetScope.Availability)
            {
                var availability = NormalizeAvailability(query.Availability, query.InStockOnly);
                if (availability.Count == 1)
                {
                    var isInStock = availability.Contains(AvailabilityInStockValue);
                    filters.Add(descriptor => descriptor.Term(term => term
                        .Field(field => field.InStock)
                        .Value(isInStock)));
                }
            }

            if (excludedFacet != ProductSearchFacetScope.Sale && query.IsOnSale.HasValue)
            {
                filters.Add(descriptor => descriptor.Term(term => term
                    .Field(field => field.IsOnSale)
                    .Value(query.IsOnSale.Value)));
            }

            if (excludedFacet != ProductSearchFacetScope.New && query.IsNew.HasValue)
            {
                filters.Add(descriptor => descriptor.Term(term => term
                    .Field(field => field.IsNew)
                    .Value(query.IsNew.Value)));
            }

            return filters;
        }

        private static Func<QueryContainerDescriptor<ProductSearchDocument>, QueryContainer> BuildStringAnyOfFilter(
            string fieldName,
            string[] values)
        {
            return descriptor => descriptor.Bool(boolean => boolean.Should(values.Select(value =>
                (Func<QueryContainerDescriptor<ProductSearchDocument>, QueryContainer>)(query => query.Term(term => term
                    .Field(fieldName)
                    .Value(value))))));
        }

        private static Func<QueryContainerDescriptor<ProductSearchDocument>, QueryContainer> BuildDecimalAnyOfFilter(
            string fieldName,
            decimal[] values)
        {
            return descriptor => descriptor.Bool(boolean => boolean.Should(values.Select(value =>
                (Func<QueryContainerDescriptor<ProductSearchDocument>, QueryContainer>)(query => query.Term(term => term
                    .Field(fieldName)
                    .Value((double)value))))));
        }

        private static string[] NormalizeStrings(string[]? values)
        {
            return values?
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray() ?? Array.Empty<string>();
        }

        private static HashSet<string> NormalizeAvailability(string[]? values, bool? inStockOnly)
        {
            var normalized = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (inStockOnly == true)
            {
                normalized.Add(AvailabilityInStockValue);
            }

            if (values is null || values.Length == 0)
            {
                return normalized;
            }

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
                        normalized.Add(AvailabilityInStockValue);
                        break;
                    case "out_of_stock":
                    case "outofstock":
                    case "unavailable":
                    case "out-of-stock":
                        normalized.Add(AvailabilityOutOfStockValue);
                        break;
                }
            }

            return normalized;
        }

        public const string AvailabilityInStockValue = "in_stock";
        public const string AvailabilityOutOfStockValue = "out_of_stock";
    }
}
