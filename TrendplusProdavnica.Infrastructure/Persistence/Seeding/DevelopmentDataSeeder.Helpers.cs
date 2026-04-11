#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using TrendplusProdavnica.Domain.Catalog;
using TrendplusProdavnica.Domain.Enums;

namespace TrendplusProdavnica.Infrastructure.Persistence.Seeding
{
    public sealed partial class DevelopmentDataSeeder
    {
        private sealed record VariantSeed(
            string ProductSlug,
            string Sku,
            decimal SizeEu,
            string ColorName,
            decimal Price,
            decimal? OldPrice,
            StockStatus StockStatus,
            int TotalStock,
            int SortOrder);

        private sealed record MediaSeed(
            string Url,
            string? MobileUrl,
            string AltText,
            string Title,
            MediaRole MediaRole,
            int SortOrder,
            bool IsPrimary);

        private sealed record RelatedSeed(
            long ProductId,
            long RelatedProductId,
            ProductRelationType RelationType,
            int SortOrder);

        private sealed record ProductRatingSummarySeed(
            long ProductId,
            decimal AverageRating,
            int ReviewCount,
            int RatingCount,
            int OneStarCount,
            int TwoStarCount,
            int ThreeStarCount,
            int FourStarCount,
            int FiveStarCount,
            DateTimeOffset? LastReviewAtUtc);

        private sealed record VariantInventoryRow(
            string ProductSlug,
            long VariantId,
            int TotalStock,
            int SortOrder);

        private sealed record StoreInventoryAllocation(
            string StoreSlug,
            int QuantityOnHand,
            int ReservedQuantity);

        private static IReadOnlyList<VariantSeed> BuildVariantSeeds(IReadOnlyList<ProductSeed> seeds)
        {
            var allSizes = new[] { 36m, 37m, 38m, 39m, 40m, 41m };
            var result = new List<VariantSeed>(seeds.Count * 5);

            for (var productIndex = 0; productIndex < seeds.Count; productIndex++)
            {
                var seed = seeds[productIndex];
                var variantCount = Math.Clamp(seed.VariantCount, 3, 6);
                var sizes = BuildVariantSizes(allSizes, variantCount, productIndex % allSizes.Length);
                var (minPrice, maxPrice) = ResolvePriceRange(seed.CategorySlug);
                var basePrice = BuildBasePrice(minPrice, maxPrice, productIndex);
                var hasSalePrice = productIndex % 3 == 0 || (seed.IsBestseller && productIndex % 5 == 0);

                for (var variantIndex = 0; variantIndex < sizes.Count; variantIndex++)
                {
                    var sortOrder = variantIndex + 1;
                    var size = sizes[variantIndex];
                    var rawPrice = basePrice + (variantIndex * 150m);
                    var price = Math.Clamp(rawPrice, minPrice, maxPrice);
                    var oldPrice = hasSalePrice
                        ? (decimal?)(price + 700m + (((productIndex + variantIndex) % 4) * 250m))
                        : null;

                    var (stockStatus, totalStock) = ResolveVariantStock(productIndex, variantIndex);
                    var sku = $"TP-{productIndex + 1:000}-{size:00}";

                    result.Add(new VariantSeed(
                        seed.Slug,
                        sku,
                        size,
                        seed.PrimaryColorName,
                        decimal.Round(price, 2),
                        oldPrice.HasValue ? decimal.Round(oldPrice.Value, 2) : null,
                        stockStatus,
                        totalStock,
                        sortOrder));
                }
            }

            return result;
        }

        private static IReadOnlyList<MediaSeed> BuildMediaSeeds(ProductSeed seed)
        {
            var mediaCount = Math.Clamp(seed.MediaCount, 3, 5);
            var result = new List<MediaSeed>(mediaCount);

            for (var index = 1; index <= mediaCount; index++)
            {
                var fileName = $"{seed.Slug}-{index}.jpg";
                var url = $"https://cdn.trendplus.demo/products/{seed.Slug}/{fileName}";
                var mobileUrl = index <= 2
                    ? $"https://cdn.trendplus.demo/products/{seed.Slug}/mobile/{fileName}"
                    : null;

                result.Add(new MediaSeed(
                    url,
                    mobileUrl,
                    $"{seed.Name} pogled {index}",
                    index == 1 ? $"{seed.Name} glavna slika" : $"{seed.Name} galerija {index}",
                    index == 1 ? MediaRole.Listing : MediaRole.Gallery,
                    index,
                    index == 1));
            }

            return result;
        }

        private static IReadOnlyList<ProductReviewSeed> BuildProductReviewSeeds(IReadOnlyList<ProductSeed> seeds)
        {
            var authors = new[]
            {
                "Ana",
                "Milica",
                "Jelena",
                "Ivana",
                "Tamara",
                "Marija",
                "Katarina",
                "Sofija"
            };

            var positiveTitles = new[]
            {
                "Odlican izbor za svaki dan",
                "Udoban model i lep izgled",
                "Bas ono sto sam trazila",
                "Preporuka za posao i grad"
            };

            var neutralTitles = new[]
            {
                "Lep model uz malu napomenu",
                "Dobro iskustvo kupovine"
            };

            var result = new List<ProductReviewSeed>(seeds.Count * 3);

            for (var productIndex = 0; productIndex < seeds.Count; productIndex++)
            {
                var seed = seeds[productIndex];
                var reviewCount = seed.IsBestseller ? 4 : seed.IsNew ? 3 : 2;

                for (var reviewIndex = 0; reviewIndex < reviewCount; reviewIndex++)
                {
                    var rating = ResolveReviewRating(seed, productIndex, reviewIndex);
                    var isPositive = rating >= 4m;
                    var authorName = authors[(productIndex + reviewIndex) % authors.Length];
                    var titlePool = isPositive ? positiveTitles : neutralTitles;
                    var title = titlePool[(productIndex + reviewIndex) % titlePool.Length];
                    var reviewBody = BuildReviewBody(seed, rating, reviewIndex);

                    result.Add(new ProductReviewSeed(
                        $"{seed.Slug}-review-{reviewIndex + 1}",
                        seed.Slug,
                        authorName,
                        title,
                        reviewBody,
                        rating,
                        (productIndex + reviewIndex) % 2 == 0,
                        seed.PublishedDaysAgo + (reviewIndex + 1) * 3,
                        ProductReviewStatus.Published));
                }
            }

            return result;
        }

        private static IReadOnlyList<RelatedSeed> BuildRelatedSeeds(
            IReadOnlyList<ProductSeed> seeds,
            IReadOnlyDictionary<string, Product> products)
        {
            var slugToSeed = seeds.ToDictionary(seed => seed.Slug, seed => seed);
            var slugToIndex = seeds
                .Select((seed, index) => new { seed.Slug, index })
                .ToDictionary(item => item.Slug, item => item.index);
            var byCategory = seeds
                .GroupBy(seed => seed.CategorySlug)
                .ToDictionary(group => group.Key, group => group.Select(item => item.Slug).ToArray());
            var byBrand = seeds
                .GroupBy(seed => seed.BrandSlug)
                .ToDictionary(group => group.Key, group => group.Select(item => item.Slug).ToArray());

            var dedupe = new HashSet<(long ProductId, long RelatedProductId, ProductRelationType Type)>();
            var result = new List<RelatedSeed>();

            foreach (var seed in seeds)
            {
                if (!products.TryGetValue(seed.Slug, out var product))
                {
                    continue;
                }

                var currentIndex = slugToIndex[seed.Slug];
                var similarSlug = NextSlugInGroup(byCategory[seed.CategorySlug], seed.Slug);
                var sameBrandSlug = NextSlugInGroup(byBrand[seed.BrandSlug], seed.Slug);
                var recommendedSlug = seeds[(currentIndex + 3) % seeds.Count].Slug;

                AddRelation(similarSlug, ProductRelationType.Similar, 1);
                AddRelation(sameBrandSlug, ProductRelationType.SameBrand, 2);
                AddRelation(recommendedSlug, ProductRelationType.Recommended, 3);

                void AddRelation(string? targetSlug, ProductRelationType relationType, int sortOrder)
                {
                    if (string.IsNullOrWhiteSpace(targetSlug) ||
                        targetSlug == seed.Slug ||
                        !slugToSeed.ContainsKey(targetSlug) ||
                        !products.TryGetValue(targetSlug, out var related))
                    {
                        return;
                    }

                    var key = (product.Id, related.Id, relationType);
                    if (!dedupe.Add(key))
                    {
                        return;
                    }

                    result.Add(new RelatedSeed(product.Id, related.Id, relationType, sortOrder));
                }
            }

            return result;
        }

        private static IEnumerable<string> BuildCollectionAssignments(ProductSeed seed)
        {
            var normalized = new List<string>();

            foreach (var manualSlug in seed.CollectionSlugs)
            {
                if (!string.IsNullOrWhiteSpace(manualSlug))
                {
                    normalized.Add(manualSlug.Trim().ToLowerInvariant());
                }
            }

            if (seed.IsNew)
            {
                normalized.Add("novo");
            }

            if (seed.IsBestseller)
            {
                normalized.Add("bestseleri");
            }

            return normalized.Distinct(StringComparer.Ordinal);
        }

        private static IEnumerable<string> ResolveStoreCoverage(int productIndex)
        {
            return (productIndex % 6) switch
            {
                0 => new[] { "beograd-usce", "beograd-knez", "novi-sad-promenada" },
                1 => new[] { "beograd-usce", "beograd-knez" },
                2 => new[] { "beograd-usce", "novi-sad-promenada" },
                3 => new[] { "beograd-knez" },
                4 => new[] { "novi-sad-promenada" },
                _ => new[] { "beograd-knez", "novi-sad-promenada" }
            };
        }

        private static IEnumerable<StoreInventoryAllocation> AllocateStoreQuantities(
            int totalStock,
            int variantSortOrder,
            string[] coverage)
        {
            if (totalStock <= 0 || coverage.Length == 0)
            {
                return Array.Empty<StoreInventoryAllocation>();
            }

            if (coverage.Length == 1)
            {
                var quantity = totalStock;
                var reserved = quantity > 2 && variantSortOrder % 3 == 0 ? 1 : 0;
                reserved = Math.Min(reserved, Math.Max(0, quantity - 1));

                return new[]
                {
                    new StoreInventoryAllocation(coverage[0], quantity, reserved)
                };
            }

            if (coverage.Length == 2)
            {
                var first = (int)Math.Ceiling(totalStock * 0.60m);
                var second = totalStock - first;

                return BuildAllocations(coverage, new[] { first, second }, variantSortOrder);
            }

            var firstShare = (int)Math.Ceiling(totalStock * 0.50m);
            var secondShare = (int)Math.Ceiling(totalStock * 0.30m);
            var thirdShare = Math.Max(0, totalStock - firstShare - secondShare);

            return BuildAllocations(coverage, new[] { firstShare, secondShare, thirdShare }, variantSortOrder);
        }

        private static string BuildEditorialBody(IEnumerable<string> paragraphs)
        {
            var clean = paragraphs
                .Where(paragraph => !string.IsNullOrWhiteSpace(paragraph))
                .Select(paragraph => paragraph.Trim())
                .ToArray();

            if (clean.Length == 0)
            {
                return "Editorial sadrzaj uskoro stize.";
            }

            return string.Join(Environment.NewLine + Environment.NewLine, clean);
        }

        private static string BuildTrustBody(string summary, IEnumerable<string> items)
        {
            var lines = new List<string>();

            if (!string.IsNullOrWhiteSpace(summary))
            {
                lines.Add(summary.Trim());
            }

            foreach (var item in items.Where(item => !string.IsNullOrWhiteSpace(item)))
            {
                lines.Add($"- {item.Trim()}");
            }

            return lines.Count == 0
                ? "Informacije ce biti dostupne uskoro."
                : string.Join(Environment.NewLine, lines);
        }

        private static IReadOnlyList<decimal> BuildVariantSizes(decimal[] allSizes, int count, int offset)
        {
            var result = new List<decimal>(count);

            for (var index = 0; index < count; index++)
            {
                var size = allSizes[(offset + index) % allSizes.Length];
                result.Add(size);
            }

            return result;
        }

        private static IReadOnlyList<ProductRatingSummarySeed> BuildProductRatingSummaries(
            IEnumerable<ProductReview> reviews,
            DateTimeOffset now)
        {
            return reviews
                .Where(review => review.Status == ProductReviewStatus.Published)
                .GroupBy(review => review.ProductId)
                .Select(group =>
                {
                    var publishedReviews = group
                        .Where(review => review.PublishedAtUtc.HasValue)
                        .OrderByDescending(review => review.PublishedAtUtc)
                        .ToArray();

                    var ratingCount = publishedReviews.Length;
                    var averageRating = ratingCount == 0
                        ? 0m
                        : decimal.Round(publishedReviews.Average(review => review.RatingValue), 2, MidpointRounding.AwayFromZero);

                    return new ProductRatingSummarySeed(
                        group.Key,
                        averageRating,
                        ratingCount,
                        ratingCount,
                        publishedReviews.Count(review => review.RatingValue >= 1m && review.RatingValue < 2m),
                        publishedReviews.Count(review => review.RatingValue >= 2m && review.RatingValue < 3m),
                        publishedReviews.Count(review => review.RatingValue >= 3m && review.RatingValue < 4m),
                        publishedReviews.Count(review => review.RatingValue >= 4m && review.RatingValue < 5m),
                        publishedReviews.Count(review => review.RatingValue >= 5m),
                        publishedReviews.FirstOrDefault()?.PublishedAtUtc ?? now);
                })
                .ToArray();
        }

        private static (decimal MinPrice, decimal MaxPrice) ResolvePriceRange(string categorySlug)
        {
            return categorySlug switch
            {
                "baletanke" => (5000m, 9000m),
                "salonke" => (7000m, 14000m),
                "lifestyle" => (8000m, 18000m),
                "gleznjace" => (10000m, 20000m),
                "sandale" => (5000m, 10000m),
                "papuce" => (4000m, 8500m),
                "mokasine" => (6500m, 12000m),
                _ => (6000m, 12000m)
            };
        }

        private static decimal BuildBasePrice(decimal minPrice, decimal maxPrice, int productIndex)
        {
            var buckets = 6m;
            var spread = Math.Max(0m, maxPrice - minPrice);
            var step = spread / buckets;
            var value = minPrice + ((productIndex % (int)buckets) * step);

            return decimal.Round(value / 10m, 0, MidpointRounding.AwayFromZero) * 10m;
        }

        private static (StockStatus StockStatus, int TotalStock) ResolveVariantStock(int productIndex, int variantIndex)
        {
            var pattern = (productIndex + variantIndex) % 7;

            return pattern switch
            {
                0 => (StockStatus.OutOfStock, 0),
                1 => (StockStatus.LowStock, 1),
                2 => (StockStatus.LowStock, 2),
                _ => (StockStatus.InStock, 4 + ((productIndex * 2 + variantIndex) % 9))
            };
        }

        private static decimal ResolveReviewRating(ProductSeed seed, int productIndex, int reviewIndex)
        {
            if (seed.IsBestseller)
            {
                return reviewIndex == 0 && productIndex % 4 == 0 ? 4m : 5m;
            }

            if (seed.IsNew)
            {
                return reviewIndex == 0 && productIndex % 5 == 0 ? 3m : 4m;
            }

            return (productIndex + reviewIndex) % 4 == 0 ? 3m : 4m;
        }

        private static string BuildReviewBody(ProductSeed seed, decimal rating, int reviewIndex)
        {
            var fitText = reviewIndex % 2 == 0
                ? "Velicina odgovara ocekivanjima"
                : "Model lepo lezi na nozi i ne zulja";

            if (rating >= 5m)
            {
                return $"{seed.Name} je ostavio odlican utisak. {fitText}, a utisak udobnosti je vrlo dobar i posle vise sati nosenja.";
            }

            if (rating >= 4m)
            {
                return $"{seed.Name} je veoma dobar izbor za svakodnevne kombinacije. {fitText}, a izgled uzivo je kao na slikama.";
            }

            return $"{seed.Name} izgleda lepo i kvalitetno je izradjen. {fitText}, ali mi je trebalo malo vremena da se naviknem na model.";
        }

        private static string? NextSlugInGroup(IReadOnlyList<string> group, string currentSlug)
        {
            if (group.Count < 2)
            {
                return null;
            }

            var currentIndex = -1;

            for (var index = 0; index < group.Count; index++)
            {
                if (group[index] == currentSlug)
                {
                    currentIndex = index;
                    break;
                }
            }

            if (currentIndex < 0)
            {
                return null;
            }

            return group[(currentIndex + 1) % group.Count];
        }

        private static IEnumerable<StoreInventoryAllocation> BuildAllocations(
            string[] coverage,
            int[] quantities,
            int variantSortOrder)
        {
            var allocations = new List<StoreInventoryAllocation>();

            for (var index = 0; index < coverage.Length && index < quantities.Length; index++)
            {
                var quantity = Math.Max(0, quantities[index]);
                if (quantity <= 0)
                {
                    continue;
                }

                var shouldReserve = (variantSortOrder + index) % 3 == 0;
                var reserved = shouldReserve ? 1 : 0;
                reserved = Math.Min(reserved, Math.Max(0, quantity - 1));

                allocations.Add(new StoreInventoryAllocation(coverage[index], quantity, reserved));
            }

            return allocations;
        }
    }
}
