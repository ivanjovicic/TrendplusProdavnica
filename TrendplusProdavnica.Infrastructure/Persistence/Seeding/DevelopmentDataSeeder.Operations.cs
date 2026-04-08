#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrendplusProdavnica.Domain.Catalog;
using TrendplusProdavnica.Domain.Content;
using TrendplusProdavnica.Domain.Enums;
using TrendplusProdavnica.Domain.Inventory;
using TrendplusProdavnica.Domain.ValueObjects;

namespace TrendplusProdavnica.Infrastructure.Persistence.Seeding
{
    public sealed partial class DevelopmentDataSeeder
    {
        private async Task<SizeGuide?> UpsertDefaultSizeGuideAsync(
            DateTimeOffset now,
            CancellationToken cancellationToken)
        {
            var guide = await _db.SizeGuides
                .FirstOrDefaultAsync(entity => entity.Slug == "zenska-standardna", cancellationToken);

            if (guide is null)
            {
                guide = new SizeGuide
                {
                    CreatedAtUtc = now
                };
                _db.SizeGuides.Add(guide);
            }

            guide.Name = "Zenska standardna tabela";
            guide.Slug = "zenska-standardna";
            guide.Description = "Orijentaciona tabela velicina za Trendplus modele.";
            guide.IsDefault = true;
            guide.IsActive = true;
            guide.UpdatedAtUtc = now;

            await _db.SaveChangesAsync(cancellationToken);

            var sizes = new[] { 36m, 37m, 38m, 39m, 40m, 41m };
            var existingRows = await _db.SizeGuideRows
                .Where(entity => entity.SizeGuideId == guide.Id)
                .ToDictionaryAsync(entity => entity.EuSize, cancellationToken);

            for (var i = 0; i < sizes.Length; i++)
            {
                if (!existingRows.TryGetValue(sizes[i], out var row))
                {
                    row = new SizeGuideRow();
                    _db.SizeGuideRows.Add(row);
                }

                row.SizeGuideId = guide.Id;
                row.EuSize = sizes[i];
                row.FootLengthMinMm = 230m + (i * 6m);
                row.FootLengthMaxMm = 235m + (i * 6m);
                row.Note = i < 2 ? "Ako ste izmedju dva broja, uzmite veci broj." : null;
                row.SortOrder = i + 1;
            }

            await _db.SaveChangesAsync(cancellationToken);
            return guide;
        }

        private async Task UpsertProductCategoryMappingsAsync(
            IReadOnlyList<ProductSeed> seeds,
            IReadOnlyDictionary<string, Product> products,
            IReadOnlyDictionary<string, Category> categories,
            DateTimeOffset now,
            CancellationToken cancellationToken)
        {
            var productIds = products.Values.Select(entity => entity.Id).ToArray();
            var existing = await _db.ProductCategoryMaps
                .Where(entity => productIds.Contains(entity.ProductId))
                .ToListAsync(cancellationToken);

            foreach (var seed in seeds)
            {
                var productId = products[seed.Slug].Id;

                for (var index = 0; index < seed.AdditionalCategorySlugs.Length; index++)
                {
                    var categorySlug = seed.AdditionalCategorySlugs[index];
                    if (!categories.TryGetValue(categorySlug, out var category))
                    {
                        continue;
                    }

                    var mapping = existing.FirstOrDefault(entity =>
                        entity.ProductId == productId &&
                        entity.CategoryId == category.Id);

                    if (mapping is null)
                    {
                        mapping = new ProductCategoryMap
                        {
                            ProductId = productId,
                            CategoryId = category.Id,
                            CreatedAtUtc = now
                        };
                        _db.ProductCategoryMaps.Add(mapping);
                        existing.Add(mapping);
                    }

                    mapping.SortOrder = index + 1;
                    mapping.UpdatedAtUtc = now;
                }
            }

            await _db.SaveChangesAsync(cancellationToken);
        }

        private async Task UpsertProductVariantsAsync(
            IReadOnlyList<ProductSeed> seeds,
            IReadOnlyDictionary<string, Product> products,
            DateTimeOffset now,
            CancellationToken cancellationToken)
        {
            var variantSeeds = BuildVariantSeeds(seeds);
            var skuList = variantSeeds.Select(seed => seed.Sku).ToArray();
            var existing = await _db.ProductVariants
                .Where(entity => skuList.Contains(entity.Sku))
                .ToDictionaryAsync(entity => entity.Sku, cancellationToken);

            foreach (var seed in variantSeeds)
            {
                if (!existing.TryGetValue(seed.Sku, out var variant))
                {
                    variant = new ProductVariant();
                    _db.ProductVariants.Add(variant);
                }

                variant.ProductId = products[seed.ProductSlug].Id;
                variant.Sku = seed.Sku;
                variant.SizeEu = seed.SizeEu;
                variant.ColorName = seed.ColorName;
                variant.Price = seed.Price;
                variant.OldPrice = seed.OldPrice;
                variant.Currency = "RSD";
                variant.StockStatus = seed.StockStatus;
                variant.TotalStock = seed.TotalStock;
                variant.LowStockThreshold = 2;
                variant.IsActive = true;
                variant.IsVisible = true;
                variant.SortOrder = seed.SortOrder;
                variant.CreatedAtUtc = variant.CreatedAtUtc == default ? now : variant.CreatedAtUtc;
                variant.UpdatedAtUtc = now;
            }

            await _db.SaveChangesAsync(cancellationToken);
        }

        private async Task UpsertProductMediaAsync(
            IReadOnlyList<ProductSeed> seeds,
            IReadOnlyDictionary<string, Product> products,
            CancellationToken cancellationToken)
        {
            var productIds = products.Values.Select(entity => entity.Id).ToArray();
            var existing = await _db.ProductMedia
                .Where(entity => productIds.Contains(entity.ProductId))
                .ToListAsync(cancellationToken);

            foreach (var seed in seeds)
            {
                var product = products[seed.Slug];
                var mediaSeeds = BuildMediaSeeds(seed);

                foreach (var mediaSeed in mediaSeeds)
                {
                    var media = existing.FirstOrDefault(entity =>
                        entity.ProductId == product.Id &&
                        entity.SortOrder == mediaSeed.SortOrder);

                    if (media is null)
                    {
                        media = new ProductMedia
                        {
                            ProductId = product.Id
                        };
                        _db.ProductMedia.Add(media);
                        existing.Add(media);
                    }

                    media.Url = mediaSeed.Url;
                    media.MobileUrl = mediaSeed.MobileUrl;
                    media.AltText = mediaSeed.AltText;
                    media.Title = mediaSeed.Title;
                    media.MediaType = MediaType.Image;
                    media.MediaRole = mediaSeed.MediaRole;
                    media.SortOrder = mediaSeed.SortOrder;
                    media.IsPrimary = mediaSeed.IsPrimary;
                    media.IsActive = true;
                }
            }

            await _db.SaveChangesAsync(cancellationToken);
        }

        private async Task UpsertRelatedProductsAsync(
            IReadOnlyList<ProductSeed> seeds,
            IReadOnlyDictionary<string, Product> products,
            DateTimeOffset now,
            CancellationToken cancellationToken)
        {
            var productIds = products.Values.Select(entity => entity.Id).ToArray();
            var existing = await _db.ProductRelatedProducts
                .Where(entity => productIds.Contains(entity.ProductId))
                .ToListAsync(cancellationToken);

            foreach (var seed in BuildRelatedSeeds(seeds, products))
            {
                var relation = existing.FirstOrDefault(entity =>
                    entity.ProductId == seed.ProductId &&
                    entity.RelatedProductId == seed.RelatedProductId &&
                    entity.RelationType == seed.RelationType);

                if (relation is null)
                {
                    relation = new ProductRelatedProduct
                    {
                        ProductId = seed.ProductId,
                        RelatedProductId = seed.RelatedProductId,
                        RelationType = seed.RelationType,
                        CreatedAtUtc = now
                    };
                    _db.ProductRelatedProducts.Add(relation);
                    existing.Add(relation);
                }

                relation.SortOrder = seed.SortOrder;
                relation.UpdatedAtUtc = now;
            }

            await _db.SaveChangesAsync(cancellationToken);
        }

        private async Task UpsertProductCollectionMappingsAsync(
            IReadOnlyList<ProductSeed> seeds,
            IReadOnlyDictionary<string, Product> products,
            DateTimeOffset now,
            CancellationToken cancellationToken)
        {
            var collectionSlugs = seeds
                .SelectMany(BuildCollectionAssignments)
                .Distinct()
                .ToArray();

            var collections = await _db.Collections.AsNoTracking()
                .Where(entity => collectionSlugs.Contains(entity.Slug))
                .ToDictionaryAsync(entity => entity.Slug, cancellationToken);
            var productIds = products.Values.Select(entity => entity.Id).ToArray();

            var existing = await _db.ProductCollectionMaps
                .Where(entity => productIds.Contains(entity.ProductId))
                .ToListAsync(cancellationToken);

            for (var index = 0; index < seeds.Count; index++)
            {
                var seed = seeds[index];
                var productId = products[seed.Slug].Id;

                foreach (var collectionSlug in BuildCollectionAssignments(seed))
                {
                    if (!collections.TryGetValue(collectionSlug, out var collection))
                    {
                        continue;
                    }

                    var mapping = existing.FirstOrDefault(entity =>
                        entity.ProductId == productId &&
                        entity.CollectionId == collection.Id);

                    if (mapping is null)
                    {
                        mapping = new ProductCollectionMap
                        {
                            ProductId = productId,
                            CollectionId = collection.Id,
                            CreatedAtUtc = now
                        };
                        _db.ProductCollectionMaps.Add(mapping);
                        existing.Add(mapping);
                    }

                    mapping.SortOrder = index + 1;
                    mapping.Pinned = seed.PinnedCollectionSlugs.Contains(collectionSlug);
                    mapping.MerchandisingScore = seed.IsBestseller ? 0.95m : seed.IsNew ? 0.85m : 0.70m;
                    mapping.UpdatedAtUtc = now;
                }
            }

            await _db.SaveChangesAsync(cancellationToken);
        }

        private async Task SeedStoreInventoryAsync(DateTimeOffset now, CancellationToken cancellationToken)
        {
            var stores = await _db.Stores.AsNoTracking()
                .Where(entity => GetStoreSeeds().Select(seed => seed.Slug).Contains(entity.Slug))
                .ToDictionaryAsync(entity => entity.Slug, cancellationToken);

            var productSeeds = GetProductSeeds();
            var productSlugs = productSeeds.Select(seed => seed.Slug).ToArray();
            var productIndexBySlug = productSeeds
                .Select((seed, index) => new { seed.Slug, Index = index })
                .ToDictionary(item => item.Slug, item => item.Index);

            var variantRows = await (
                    from variant in _db.ProductVariants.AsNoTracking()
                    join product in _db.Products.AsNoTracking() on variant.ProductId equals product.Id
                    where productSlugs.Contains(product.Slug)
                    select new VariantInventoryRow(
                        product.Slug,
                        variant.Id,
                        variant.TotalStock,
                        variant.SortOrder))
                .ToArrayAsync(cancellationToken);

            var variantIds = variantRows.Select(row => row.VariantId).Distinct().ToArray();
            if (variantIds.Length > 0)
            {
                await _db.StoreInventory
                    .Where(entity => variantIds.Contains(entity.VariantId))
                    .ExecuteDeleteAsync(cancellationToken);
            }

            foreach (var group in variantRows.GroupBy(row => row.ProductSlug))
            {
                if (!productIndexBySlug.TryGetValue(group.Key, out var productIndex))
                {
                    continue;
                }

                var coverage = ResolveStoreCoverage(productIndex)
                    .Where(stores.ContainsKey)
                    .ToArray();

                if (coverage.Length == 0)
                {
                    continue;
                }

                foreach (var variant in group)
                {
                    foreach (var allocation in AllocateStoreQuantities(
                                 variant.TotalStock,
                                 variant.SortOrder,
                                 coverage))
                    {
                        _db.StoreInventory.Add(new StoreInventory
                        {
                            StoreId = stores[allocation.StoreSlug].Id,
                            VariantId = variant.VariantId,
                            QuantityOnHand = allocation.QuantityOnHand,
                            ReservedQuantity = allocation.ReservedQuantity,
                            UpdatedAtUtc = now
                        });
                    }
                }
            }

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
