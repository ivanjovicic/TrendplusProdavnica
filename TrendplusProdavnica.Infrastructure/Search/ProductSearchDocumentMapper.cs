#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace TrendplusProdavnica.Infrastructure.Search
{
    internal static class ProductSearchDocumentMapper
    {
        internal static ProductSearchDocument Map(ProductSearchSource source)
        {
            var searchKeywords = BuildSearchKeywords(source);

            return new ProductSearchDocument
            {
                ProductId = source.ProductId,
                Slug = source.Slug,
                BrandName = source.BrandName,
                Name = source.Name,
                ShortDescription = source.ShortDescription,
                PrimaryCategory = source.PrimaryCategory,
                SecondaryCategories = source.SecondaryCategories,
                PrimaryColorName = source.PrimaryColorName,
                IsNew = source.IsNew,
                IsBestseller = source.IsBestseller,
                IsOnSale = source.IsOnSale,
                MinPrice = source.MinPrice,
                MaxPrice = source.MaxPrice,
                AvailableSizes = source.AvailableSizes,
                InStock = source.InStock,
                PrimaryImageUrl = source.PrimaryImageUrl,
                SearchKeywords = searchKeywords,
                SortRank = source.SortRank,
                PublishedAtUtc = source.PublishedAtUtc
            };
        }

        private static string[] BuildSearchKeywords(ProductSearchSource source)
        {
            var keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            AddKeyword(keywords, source.Name);
            AddKeyword(keywords, source.BrandName);
            AddKeyword(keywords, source.PrimaryCategory);
            AddKeyword(keywords, source.PrimaryColorName);
            AddKeyword(keywords, source.ShortDescription);
            AddKeyword(keywords, source.RawSearchKeywords);

            foreach (var synonym in source.SearchSynonyms ?? Array.Empty<string>())
            {
                AddKeyword(keywords, synonym);
            }

            foreach (var category in source.SecondaryCategories)
            {
                AddKeyword(keywords, category);
            }

            return keywords.ToArray();
        }

        private static void AddKeyword(ISet<string> target, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            target.Add(value.Trim());
        }
    }

    internal sealed record ProductSearchSource(
        long ProductId,
        string Slug,
        string BrandName,
        string Name,
        string? ShortDescription,
        string? PrimaryCategory,
        string[] SecondaryCategories,
        string? PrimaryColorName,
        bool IsNew,
        bool IsBestseller,
        bool IsOnSale,
        double? MinPrice,
        double? MaxPrice,
        double[] AvailableSizes,
        bool InStock,
        string? PrimaryImageUrl,
        string? RawSearchKeywords,
        string[]? SearchSynonyms,
        int SortRank,
        DateTimeOffset? PublishedAtUtc);
}
