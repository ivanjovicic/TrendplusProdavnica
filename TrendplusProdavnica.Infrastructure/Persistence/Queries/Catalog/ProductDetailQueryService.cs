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
using TrendplusProdavnica.Domain.Enums;
using TrendplusProdavnica.Infrastructure.Persistence;

namespace TrendplusProdavnica.Infrastructure.Persistence.Queries.Catalog
{
    public class ProductDetailQueryService : IProductDetailQueryService
    {
        private readonly TrendplusDbContext _db;

        public ProductDetailQueryService(TrendplusDbContext db)
        {
            _db = db;
        }

        public async Task<ProductDetailDto> GetProductDetailAsync(GetProductDetailQuery query)
        {
            var product = await (
                from entity in _db.Products.AsNoTracking()
                join brand in _db.Brands.AsNoTracking() on entity.BrandId equals brand.Id
                join category in _db.Categories.AsNoTracking() on entity.PrimaryCategoryId equals category.Id
                where entity.Slug == query.Slug &&
                      entity.Status == ProductStatus.Published &&
                      entity.IsVisible
                select new ProductHeaderProjection(
                    entity.Id,
                    entity.Slug,
                    entity.BrandId,
                    brand.Slug,
                    brand.Name,
                    entity.PrimaryCategoryId,
                    category.Slug,
                    category.Name,
                    entity.Name,
                    entity.Subtitle,
                    entity.ShortDescription,
                    entity.LongDescription,
                    entity.PrimaryColorName,
                    entity.IsNew,
                    entity.IsBestseller,
                    entity.SizeGuideId,
                    entity.Seo))
                .FirstOrDefaultAsync();

            if (product is null)
            {
                throw new KeyNotFoundException($"Product '{query.Slug}' was not found.");
            }

            var variants = await _db.ProductVariants.AsNoTracking()
                .Where(variant =>
                    variant.ProductId == product.Id &&
                    variant.IsActive &&
                    variant.IsVisible)
                .OrderBy(variant => variant.SortOrder)
                .ThenBy(variant => variant.SizeEu)
                .Select(variant => new VariantProjection(
                    variant.Id,
                    variant.Sku,
                    variant.Barcode,
                    variant.SizeEu,
                    variant.SortOrder,
                    variant.Price,
                    variant.OldPrice,
                    variant.Currency,
                    variant.StockStatus,
                    variant.TotalStock,
                    variant.LowStockThreshold,
                    variant.IsActive,
                    variant.IsVisible))
                .ToArrayAsync();

            if (variants.Length == 0)
            {
                throw new KeyNotFoundException($"Product '{query.Slug}' is not available.");
            }

            var media = await _db.ProductMedia.AsNoTracking()
                .Where(item => item.ProductId == product.Id && item.IsActive)
                .OrderBy(item => item.SortOrder)
                .ThenByDescending(item => item.IsPrimary)
                .ThenBy(item => item.Id)
                .Select(item => new ProductMediaProjection(
                    item.Id,
                    item.ProductId,
                    item.VariantId,
                    item.Url,
                    item.MobileUrl,
                    item.AltText,
                    item.Title,
                    item.MediaType,
                    item.MediaRole,
                    item.SortOrder,
                    item.IsPrimary,
                    item.IsActive))
                .ToArrayAsync();

            var summaryPrice = variants.Min(variant => variant.Price);
            var summaryOldPrice = ResolveSummaryOldPrice(variants);
            var primaryVariant = variants
                .OrderByDescending(variant => variant.TotalStock > 0)
                .ThenBy(variant => variant.SortOrder)
                .ThenBy(variant => variant.SizeEu)
                .First();
            var currency = variants
                .Select(variant => variant.Currency)
                .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))
                ?? "RSD";
            var hasSale = variants.Any(variant => variant.OldPrice.HasValue && variant.OldPrice.Value > variant.Price);

            var relatedProducts = await LoadRelatedProductCardsAsync(
                product.Id,
                new[] { ProductRelationType.Recommended, ProductRelationType.SameBrand },
                8);
            var similarProducts = await LoadRelatedProductCardsAsync(
                product.Id,
                new[] { ProductRelationType.Similar },
                8);

            var breadcrumbs = await BuildProductBreadcrumbsAsync(
                product.PrimaryCategoryId,
                product.Name,
                product.Slug);
            var storeAvailability = await BuildStoreAvailabilitySummaryAsync(
                variants.Select(variant => variant.Id).ToArray());
            var sizeGuide = await BuildSizeGuideAsync(product.SizeGuideId);
            var trustInfo = await BuildTrustInfoAsync();
            var seo = ProductQueryMappingHelper.MapSeo(product.Seo, product.Name, product.ShortDescription);
            var rating = await _db.ProductRatings.AsNoTracking()
                .Where(item => item.ProductId == product.Id)
                .Select(item => new ProductRatingProjection(
                    item.AverageRating,
                    item.ReviewCount,
                    item.RatingCount))
                .FirstOrDefaultAsync();
            var reviews = await _db.ProductReviews.AsNoTracking()
                .Where(item =>
                    item.ProductId == product.Id &&
                    item.Status == ProductReviewStatus.Published &&
                    item.PublishedAtUtc.HasValue)
                .OrderByDescending(item => item.PublishedAtUtc)
                .ThenByDescending(item => item.UpdatedAtUtc)
                .ThenByDescending(item => item.Id)
                .Take(6)
                .Select(item => new ProductReviewProjection(
                    item.AuthorName,
                    item.Title,
                    item.ReviewBody,
                    item.RatingValue,
                    item.PublishedAtUtc))
                .ToArrayAsync();
            var aggregateRating = rating is null
                ? null
                : new ProductAggregateRatingDto(
                    rating.AverageRating,
                    rating.ReviewCount,
                    rating.RatingCount,
                    5m,
                    1m);

            return new ProductDetailDto(
                product.Id,
                product.Slug,
                product.BrandId,
                product.BrandSlug,
                product.BrandName,
                product.PrimaryCategoryId,
                product.CategorySlug,
                product.CategoryName,
                product.Name,
                product.Subtitle,
                product.ShortDescription,
                product.LongDescription,
                product.PrimaryColorName,
                null,
                product.SizeGuideId,
                primaryVariant.Sku,
                null,
                null,
                primaryVariant.Barcode,
                summaryPrice,
                summaryOldPrice,
                currency,
                product.IsNew,
                product.IsBestseller,
                hasSale,
                ProductQueryMappingHelper.BuildBadges(product.IsNew, product.IsBestseller, hasSale),
                breadcrumbs,
                media.Select(MapMedia).ToArray(),
                variants.Select(MapSizeOption).ToArray(),
                storeAvailability,
                relatedProducts,
                similarProducts,
                relatedProducts,
                similarProducts,
                seo,
                trustInfo.DeliveryInfo,
                trustInfo.ReturnInfo,
                null,
                sizeGuide,
                aggregateRating,
                rating?.AverageRating,
                rating?.ReviewCount,
                rating?.RatingCount,
                reviews
                    .Select(review => new ProductReviewDto(
                        review.AuthorName,
                        review.Title,
                        review.ReviewBody,
                        review.RatingValue,
                        review.PublishedAtUtc))
                    .ToArray());
        }

        private async Task<ProductCardDto[]> LoadRelatedProductCardsAsync(
            long productId,
            ProductRelationType[] relationTypes,
            int take)
        {
            var relatedIds = await _db.ProductRelatedProducts.AsNoTracking()
                .Where(item =>
                    item.ProductId == productId &&
                    relationTypes.Contains(item.RelationType))
                .OrderBy(item => item.SortOrder)
                .Select(item => item.RelatedProductId)
                .Distinct()
                .Take(take * 2)
                .ToArrayAsync();

            if (relatedIds.Length == 0)
            {
                return Array.Empty<ProductCardDto>();
            }

            var products = ProductQueryMappingHelper.ApplyBaseProductVisibility(_db.Products.AsNoTracking())
                .Where(product => relatedIds.Contains(product.Id));
            var projections = await ProductQueryMappingHelper
                .ToProductCardProjection(products, _db.Brands.AsNoTracking())
                .ToArrayAsync();

            var indexById = relatedIds
                .Select((id, index) => new { id, index })
                .ToDictionary(item => item.id, item => item.index);

            var ordered = projections
                .OrderBy(item => indexById.TryGetValue(item.Id, out var index) ? index : int.MaxValue)
                .Take(take)
                .ToArray();

            return ProductQueryMappingHelper.ToProductCardDtos(ordered);
        }

        private async Task<object?> BuildStoreAvailabilitySummaryAsync(long[] variantIds)
        {
            if (variantIds.Length == 0)
            {
                return null;
            }

            var rows = await (
                from inventory in _db.StoreInventory.AsNoTracking()
                join store in _db.Stores.AsNoTracking() on inventory.StoreId equals store.Id
                where variantIds.Contains(inventory.VariantId) &&
                      store.IsActive &&
                      (inventory.QuantityOnHand - inventory.ReservedQuantity) > 0
                group inventory by new
                {
                    store.Id,
                    store.Name,
                    store.Slug,
                    store.City,
                    store.AddressLine1,
                    store.WorkingHoursText
                }
                into grouped
                orderby grouped.Key.City, grouped.Key.Name
                select new StoreAvailabilityItemDto(
                    grouped.Key.Name,
                    grouped.Key.Slug,
                    grouped.Key.City,
                    grouped.Key.AddressLine1,
                    grouped.Key.WorkingHoursText ?? string.Empty,
                    grouped.Sum(item => item.QuantityOnHand - item.ReservedQuantity)))
                .ToArrayAsync();

            if (rows.Length == 0)
            {
                return new StoreAvailabilitySummaryDto(0, Array.Empty<StoreAvailabilityItemDto>());
            }

            return new StoreAvailabilitySummaryDto(
                rows.Length,
                rows.Take(5).ToArray());
        }

        private async Task<object?> BuildSizeGuideAsync(long? sizeGuideId)
        {
            if (!sizeGuideId.HasValue)
            {
                return null;
            }

            var guide = await _db.SizeGuides.AsNoTracking()
                .Where(item => item.Id == sizeGuideId.Value && item.IsActive)
                .Select(item => new
                {
                    item.Id,
                    item.Name,
                    item.Slug,
                    item.Description
                })
                .FirstOrDefaultAsync();

            if (guide is null)
            {
                return null;
            }

            var rows = await _db.SizeGuideRows.AsNoTracking()
                .Where(item => item.SizeGuideId == guide.Id)
                .OrderBy(item => item.SortOrder)
                .Select(item => new SizeGuideRowDto(
                    item.EuSize,
                    item.FootLengthMinMm,
                    item.FootLengthMaxMm,
                    item.Note))
                .ToArrayAsync();

            return new SizeGuideDto(
                guide.Name,
                guide.Slug,
                guide.Description,
                rows);
        }

        private async Task<TrustInfo> BuildTrustInfoAsync()
        {
            var pages = await _db.TrustPages.AsNoTracking()
                .Where(page =>
                    page.IsPublished &&
                    (page.PageKind == TrustPageKind.Delivery || page.PageKind == TrustPageKind.Returns))
                .Select(page => new { page.PageKind, page.Body })
                .ToArrayAsync();

            var delivery = pages
                .Where(page => page.PageKind == TrustPageKind.Delivery)
                .Select(page => page.Body)
                .FirstOrDefault(body => !string.IsNullOrWhiteSpace(body))
                ?? "Dostava u roku od 2 do 5 radnih dana.";
            var returns = pages
                .Where(page => page.PageKind == TrustPageKind.Returns)
                .Select(page => page.Body)
                .FirstOrDefault(body => !string.IsNullOrWhiteSpace(body))
                ?? "Povrat u roku od 14 dana.";

            return new TrustInfo(delivery, returns);
        }

        private async Task<BreadcrumbItemDto[]> BuildProductBreadcrumbsAsync(
            long primaryCategoryId,
            string productName,
            string productSlug)
        {
            var categories = await _db.Categories.AsNoTracking()
                .Select(category => new
                {
                    category.Id,
                    category.ParentId,
                    category.Name,
                    category.Slug
                })
                .ToDictionaryAsync(category => category.Id);

            var breadcrumbs = new List<BreadcrumbItemDto>
            {
                new("Pocetna", "/")
            };

            if (categories.TryGetValue(primaryCategoryId, out _))
            {
                var categoryChain = new List<(string Name, string Slug)>();
                var visited = new HashSet<long>();
                var currentId = primaryCategoryId;

                while (categories.TryGetValue(currentId, out var current) && visited.Add(currentId))
                {
                    categoryChain.Add((current.Name, current.Slug));

                    if (!current.ParentId.HasValue)
                    {
                        break;
                    }

                    currentId = current.ParentId.Value;
                }

                categoryChain.Reverse();
                breadcrumbs.AddRange(categoryChain.Select(item =>
                    new BreadcrumbItemDto(item.Name, $"/{item.Slug}")));
            }

            breadcrumbs.Add(new BreadcrumbItemDto(productName, $"/proizvod/{productSlug}"));
            return breadcrumbs.ToArray();
        }

        private static ProductMediaDto MapMedia(ProductMediaProjection media)
        {
            return new ProductMediaDto(
                media.Id,
                media.ProductId,
                media.VariantId,
                media.Url,
                media.MobileUrl,
                media.AltText,
                media.Title,
                (int)media.MediaType,
                (int)media.MediaRole,
                media.SortOrder,
                media.IsPrimary,
                media.IsActive);
        }

        private static ProductSizeOptionDto MapSizeOption(VariantProjection variant)
        {
            return new ProductSizeOptionDto(
                variant.Id,
                variant.SizeEu,
                variant.SizeEu.ToString("0.#", CultureInfo.InvariantCulture),
                variant.Sku,
                variant.Barcode,
                variant.IsActive,
                variant.IsVisible,
                variant.TotalStock > 0,
                variant.StockStatus == StockStatus.LowStock || variant.TotalStock <= variant.LowStockThreshold,
                variant.TotalStock,
                (int)variant.StockStatus,
                variant.LowStockThreshold,
                variant.Price,
                variant.OldPrice,
                variant.Currency);
        }

        private static decimal? ResolveSummaryOldPrice(IEnumerable<VariantProjection> variants)
        {
            var saleOldPrices = variants
                .Where(variant => variant.OldPrice.HasValue && variant.OldPrice.Value > variant.Price)
                .Select(variant => variant.OldPrice!.Value)
                .ToArray();

            if (saleOldPrices.Length > 0)
            {
                return saleOldPrices.Min();
            }

            var allOldPrices = variants
                .Where(variant => variant.OldPrice.HasValue)
                .Select(variant => variant.OldPrice!.Value)
                .ToArray();

            return allOldPrices.Length > 0 ? allOldPrices.Min() : null;
        }

        private sealed record ProductHeaderProjection(
            long Id,
            string Slug,
            long BrandId,
            string BrandSlug,
            string BrandName,
            long PrimaryCategoryId,
            string CategorySlug,
            string CategoryName,
            string Name,
            string? Subtitle,
            string ShortDescription,
            string? LongDescription,
            string? PrimaryColorName,
            bool IsNew,
            bool IsBestseller,
            long? SizeGuideId,
            Domain.ValueObjects.SeoMetadata? Seo);

        private sealed record VariantProjection(
            long Id,
            string Sku,
            string? Barcode,
            decimal SizeEu,
            int SortOrder,
            decimal Price,
            decimal? OldPrice,
            string Currency,
            StockStatus StockStatus,
            int TotalStock,
            int LowStockThreshold,
            bool IsActive,
            bool IsVisible);

        private sealed record ProductMediaProjection(
            long Id,
            long ProductId,
            long? VariantId,
            string Url,
            string? MobileUrl,
            string? AltText,
            string? Title,
            MediaType MediaType,
            MediaRole MediaRole,
            int SortOrder,
            bool IsPrimary,
            bool IsActive);

        private sealed record ProductRatingProjection(
            decimal AverageRating,
            int ReviewCount,
            int RatingCount);

        private sealed record ProductReviewProjection(
            string AuthorName,
            string? Title,
            string? ReviewBody,
            decimal RatingValue,
            DateTimeOffset? PublishedAtUtc);

        private sealed record StoreAvailabilityItemDto(
            string StoreName,
            string StoreSlug,
            string City,
            string AddressLine1,
            string WorkingHoursText,
            int AvailableQuantity);

        private sealed record StoreAvailabilitySummaryDto(
            int AvailableStoresCount,
            StoreAvailabilityItemDto[] Stores);

        private sealed record SizeGuideRowDto(
            decimal EuSize,
            decimal? FootLengthMinMm,
            decimal? FootLengthMaxMm,
            string? Note);

        private sealed record SizeGuideDto(
            string Name,
            string Slug,
            string? Description,
            SizeGuideRowDto[] Rows);

        private sealed record TrustInfo(string DeliveryInfo, string ReturnInfo);
    }
}
