#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using TrendplusProdavnica.Application.Catalog.Dtos;
using TrendplusProdavnica.Domain.Catalog;
using TrendplusProdavnica.Domain.Enums;
using TrendplusProdavnica.Domain.ValueObjects;

namespace TrendplusProdavnica.Infrastructure.Persistence.Queries.Catalog
{
    internal static class ProductQueryMappingHelper
    {
        private const string DefaultCurrency = "RSD";

        internal static IQueryable<ProductCardProjection> ToProductCardProjection(
            IQueryable<Product> products,
            IQueryable<Brand> brands)
        {
            return from product in products
                   join brand in brands on product.BrandId equals brand.Id
                   select new ProductCardProjection(
                       product.Id,
                       product.Slug,
                       brand.Name,
                       product.Name,
                       product.PrimaryColorName,
                       product.Media.Where(media => media.IsActive && media.IsPrimary)
                           .OrderBy(media => media.SortOrder)
                           .Select(media => media.Url)
                           .FirstOrDefault(),
                       product.Media.Where(media =>
                               media.IsActive &&
                               !media.IsPrimary &&
                               (media.MediaRole == MediaRole.Listing || media.MediaRole == MediaRole.Gallery))
                           .OrderBy(media => media.SortOrder)
                           .Select(media => media.Url)
                           .FirstOrDefault(),
                       product.Media.Where(media => media.IsActive)
                           .OrderBy(media => media.SortOrder)
                           .Select(media => media.Url)
                           .FirstOrDefault(),
                       product.Variants
                           .Where(variant => variant.IsActive && variant.IsVisible)
                           .Select(variant => (decimal?)variant.Price)
                           .Min(),
                       product.Variants
                           .Where(variant => variant.IsActive && variant.IsVisible && variant.OldPrice.HasValue)
                           .Select(variant => variant.OldPrice)
                           .Min(),
                       product.Variants
                           .Where(variant => variant.IsActive && variant.IsVisible)
                           .Select(variant => variant.Currency)
                           .FirstOrDefault(),
                       product.IsNew,
                       product.IsBestseller,
                       product.Variants.Any(variant =>
                           variant.IsActive &&
                           variant.IsVisible &&
                           variant.OldPrice.HasValue &&
                           variant.OldPrice.Value > variant.Price),
                       product.Variants.Any(variant =>
                           variant.IsActive &&
                           variant.IsVisible &&
                           variant.TotalStock > 0),
                       product.Variants.Count(variant => variant.IsActive && variant.IsVisible),
                       product.SortRank,
                       product.PublishedAtUtc);
        }

        internal static ProductCardDto ToProductCardDto(ProductCardProjection projection)
        {
            return new ProductCardDto(
                projection.Id,
                projection.Slug,
                projection.BrandName,
                projection.Name,
                ResolvePrimaryImage(projection.PrimaryImageUrl, projection.FallbackImageUrl),
                projection.SecondaryImageUrl,
                projection.DisplayPrice ?? 0m,
                projection.OldPrice,
                string.IsNullOrWhiteSpace(projection.Currency) ? DefaultCurrency : projection.Currency!,
                BuildBadges(projection.IsNew, projection.IsBestseller, projection.IsOnSale),
                projection.IsInStock,
                projection.AvailableSizesCount,
                projection.ColorLabel);
        }

        internal static ProductCardDto[] ToProductCardDtos(IEnumerable<ProductCardProjection> projections)
            => projections.Select(ToProductCardDto).ToArray();

        internal static string[] BuildBadges(bool isNew, bool isBestseller, bool isOnSale)
        {
            var badges = new List<string>(3);

            if (isNew)
            {
                badges.Add("Novo");
            }

            if (isBestseller)
            {
                badges.Add("Bestseller");
            }

            if (isOnSale)
            {
                badges.Add("Akcija");
            }

            return badges.ToArray();
        }

        internal static string ResolvePrimaryImage(string? primaryImageUrl, string? fallbackImageUrl)
            => !string.IsNullOrWhiteSpace(primaryImageUrl)
                ? primaryImageUrl
                : fallbackImageUrl ?? string.Empty;

        internal static SeoDto MapSeo(SeoMetadata? seo, string fallbackTitle, string fallbackDescription)
        {
            return new SeoDto(
                !string.IsNullOrWhiteSpace(seo?.SeoTitle) ? seo!.SeoTitle! : fallbackTitle,
                !string.IsNullOrWhiteSpace(seo?.SeoDescription) ? seo!.SeoDescription! : fallbackDescription,
                seo?.CanonicalUrl,
                null);
        }

        internal static IQueryable<Product> ApplyBaseProductVisibility(IQueryable<Product> products)
        {
            return products.Where(product =>
                product.Status == ProductStatus.Published &&
                product.IsVisible &&
                product.IsPurchasable &&
                product.Variants.Any(variant => variant.IsActive && variant.IsVisible));
        }
    }

    internal sealed record ProductCardProjection(
        long Id,
        string Slug,
        string BrandName,
        string Name,
        string? ColorLabel,
        string? PrimaryImageUrl,
        string? SecondaryImageUrl,
        string? FallbackImageUrl,
        decimal? DisplayPrice,
        decimal? OldPrice,
        string? Currency,
        bool IsNew,
        bool IsBestseller,
        bool IsOnSale,
        bool IsInStock,
        int AvailableSizesCount,
        int SortRank,
        DateTimeOffset? PublishedAtUtc);
}
