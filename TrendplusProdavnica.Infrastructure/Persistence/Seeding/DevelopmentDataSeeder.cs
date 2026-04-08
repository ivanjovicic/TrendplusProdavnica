#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TrendplusProdavnica.Domain.Catalog;
using TrendplusProdavnica.Domain.Content;
using TrendplusProdavnica.Domain.Enums;
using TrendplusProdavnica.Domain.Inventory;
using TrendplusProdavnica.Domain.ValueObjects;

namespace TrendplusProdavnica.Infrastructure.Persistence.Seeding
{
    public sealed partial class DevelopmentDataSeeder
    {
        private readonly TrendplusDbContext _db;
        private readonly ILogger<DevelopmentDataSeeder> _logger;

        public DevelopmentDataSeeder(TrendplusDbContext db, ILogger<DevelopmentDataSeeder> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task SeedAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Development seed started.");

            await SeedBrandsAsync(cancellationToken);
            await SeedCollectionsAsync(cancellationToken);
            await SeedProductsAsync(cancellationToken);
            await SeedStoresAsync(cancellationToken);
            await SeedContentAsync(cancellationToken);
            await SeedEditorialAsync(cancellationToken);
            await SeedTrustPagesAsync(cancellationToken);

            _logger.LogInformation("Development seed finished.");
        }

        public async Task SeedBrandsAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;
            var seeds = GetBrandSeeds();
            var slugs = seeds.Select(seed => seed.Slug).ToArray();
            var existing = await _db.Brands
                .Where(entity => slugs.Contains(entity.Slug))
                .ToDictionaryAsync(entity => entity.Slug, cancellationToken);

            foreach (var seed in seeds)
            {
                if (!existing.TryGetValue(seed.Slug, out var brand))
                {
                    brand = new Brand
                    {
                        CreatedAtUtc = now
                    };
                    _db.Brands.Add(brand);
                }

                brand.Name = seed.Name;
                brand.Slug = seed.Slug;
                brand.ShortDescription = seed.ShortDescription;
                brand.LongDescription = seed.LongDescription;
                brand.LogoUrl = seed.LogoUrl;
                brand.CoverImageUrl = seed.CoverImageUrl;
                brand.WebsiteUrl = seed.WebsiteUrl;
                brand.IsFeatured = seed.IsFeatured;
                brand.IsActive = true;
                brand.SortOrder = seed.SortOrder;
                brand.Seo = new SeoMetadata
                {
                    SeoTitle = $"{seed.Name} zenska obuca - Trendplus",
                    SeoDescription = seed.ShortDescription,
                    CanonicalUrl = $"/brend/{seed.Slug}"
                };
                brand.UpdatedAtUtc = now;
            }

            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Brands seeded.");
        }

        public async Task SeedCollectionsAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;
            var seeds = GetCollectionSeeds();
            var slugs = seeds.Select(seed => seed.Slug).ToArray();
            var existing = await _db.Collections
                .Where(entity => slugs.Contains(entity.Slug))
                .ToDictionaryAsync(entity => entity.Slug, cancellationToken);

            foreach (var seed in seeds)
            {
                if (!existing.TryGetValue(seed.Slug, out var collection))
                {
                    collection = new Collection
                    {
                        CreatedAtUtc = now
                    };
                    _db.Collections.Add(collection);
                }

                collection.Name = seed.Name;
                collection.Slug = seed.Slug;
                collection.CollectionType = seed.CollectionType;
                collection.ShortDescription = seed.ShortDescription;
                collection.LongDescription = seed.LongDescription;
                collection.CoverImageUrl = seed.CoverImageUrl;
                collection.ThumbnailImageUrl = seed.ThumbnailImageUrl;
                collection.BadgeText = seed.BadgeText;
                collection.IsActive = true;
                collection.IsFeatured = seed.IsFeatured;
                collection.SortOrder = seed.SortOrder;
                collection.Seo = new SeoMetadata
                {
                    SeoTitle = $"{seed.Name} - Trendplus",
                    SeoDescription = seed.ShortDescription,
                    CanonicalUrl = $"/kolekcija/{seed.Slug}"
                };
                collection.UpdatedAtUtc = now;
            }

            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Collections seeded.");
        }

        public async Task SeedProductsAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;
            var productSeeds = GetProductSeeds();
            var brandSlugs = productSeeds.Select(seed => seed.BrandSlug).Distinct().ToArray();
            var categorySlugs = productSeeds
                .Select(seed => seed.CategorySlug)
                .Concat(productSeeds.SelectMany(seed => seed.AdditionalCategorySlugs))
                .Distinct()
                .ToArray();

            var brands = await _db.Brands.AsNoTracking()
                .Where(entity => brandSlugs.Contains(entity.Slug))
                .ToDictionaryAsync(entity => entity.Slug, cancellationToken);
            var categories = await _db.Categories.AsNoTracking()
                .Where(entity => categorySlugs.Contains(entity.Slug))
                .ToDictionaryAsync(entity => entity.Slug, cancellationToken);

            EnsureSeedDependencies(brands, categories, productSeeds);
            var sizeGuide = await UpsertDefaultSizeGuideAsync(now, cancellationToken);

            var productSlugs = productSeeds.Select(seed => seed.Slug).ToArray();
            var existingProducts = await _db.Products
                .Where(entity => productSlugs.Contains(entity.Slug))
                .ToDictionaryAsync(entity => entity.Slug, cancellationToken);

            for (var index = 0; index < productSeeds.Count; index++)
            {
                var seed = productSeeds[index];

                if (!existingProducts.TryGetValue(seed.Slug, out var product))
                {
                    product = new Product
                    {
                        CreatedAtUtc = now
                    };
                    _db.Products.Add(product);
                }

                product.BrandId = brands[seed.BrandSlug].Id;
                product.PrimaryCategoryId = categories[seed.CategorySlug].Id;
                product.SizeGuideId = sizeGuide?.Id;
                product.Name = seed.Name;
                product.Slug = seed.Slug;
                product.Subtitle = seed.Subtitle;
                product.ShortDescription = seed.ShortDescription;
                product.LongDescription = seed.LongDescription;
                product.PrimaryColorName = seed.PrimaryColorName;
                product.StyleTag = seed.StyleTag;
                product.OccasionTag = seed.OccasionTag;
                product.SeasonTag = seed.SeasonTag;
                product.Status = ProductStatus.Published;
                product.IsVisible = true;
                product.IsPurchasable = true;
                product.IsNew = seed.IsNew;
                product.IsBestseller = seed.IsBestseller;
                product.SortRank = seed.SortRank;
                product.PublishedAtUtc = now.AddDays(-seed.PublishedDaysAgo).AddMinutes(-(index * 9));
                product.SearchKeywords = seed.SearchKeywords;
                product.Seo = new SeoMetadata
                {
                    SeoTitle = $"{seed.Name} - Trendplus",
                    SeoDescription = seed.ShortDescription,
                    CanonicalUrl = $"/proizvod/{seed.Slug}"
                };
                product.UpdatedAtUtc = now;
            }

            await _db.SaveChangesAsync(cancellationToken);

            var products = await _db.Products
                .Where(entity => productSlugs.Contains(entity.Slug))
                .ToDictionaryAsync(entity => entity.Slug, cancellationToken);

            await UpsertProductCategoryMappingsAsync(productSeeds, products, categories, now, cancellationToken);
            await UpsertProductVariantsAsync(productSeeds, products, now, cancellationToken);
            await UpsertProductMediaAsync(productSeeds, products, cancellationToken);
            await UpsertRelatedProductsAsync(productSeeds, products, now, cancellationToken);
            await UpsertProductCollectionMappingsAsync(productSeeds, products, now, cancellationToken);

            _logger.LogInformation("Products, variants and catalog mappings seeded.");
        }

        public async Task SeedStoresAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;
            var seeds = GetStoreSeeds();
            var slugs = seeds.Select(seed => seed.Slug).ToArray();
            var existing = await _db.Stores
                .Where(entity => slugs.Contains(entity.Slug))
                .ToDictionaryAsync(entity => entity.Slug, cancellationToken);

            foreach (var seed in seeds)
            {
                if (!existing.TryGetValue(seed.Slug, out var store))
                {
                    store = new Store
                    {
                        CreatedAtUtc = now
                    };
                    _db.Stores.Add(store);
                }

                store.Name = seed.Name;
                store.Slug = seed.Slug;
                store.City = seed.City;
                store.AddressLine1 = seed.AddressLine1;
                store.AddressLine2 = seed.AddressLine2;
                store.PostalCode = seed.PostalCode;
                store.MallName = seed.MallName;
                store.Phone = seed.Phone;
                store.Email = seed.Email;
                store.Latitude = seed.Latitude;
                store.Longitude = seed.Longitude;
                store.WorkingHoursText = seed.WorkingHoursText;
                store.ShortDescription = seed.ShortDescription;
                store.CoverImageUrl = seed.CoverImageUrl;
                store.DirectionsUrl = seed.DirectionsUrl;
                store.IsActive = true;
                store.SortOrder = seed.SortOrder;
                store.Seo = new SeoMetadata
                {
                    SeoTitle = $"{seed.Name} - Trendplus prodavnica",
                    SeoDescription = seed.ShortDescription,
                    CanonicalUrl = $"/prodavnica/{seed.Slug}"
                };
                store.UpdatedAtUtc = now;
            }

            await _db.SaveChangesAsync(cancellationToken);
            await SeedStoreInventoryAsync(now, cancellationToken);

            _logger.LogInformation("Stores and inventory seeded.");
        }

        public async Task SeedContentAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;
            await UpsertHomePageAsync(now, cancellationToken);
            await UpsertBrandPageContentAsync(now, cancellationToken);
            await UpsertCollectionPageContentAsync(now, cancellationToken);
            await UpsertStorePageContentAsync(now, cancellationToken);
            await UpsertSalePageAsync(now, cancellationToken);
            _logger.LogInformation("Home and page content seeded.");
        }

        public async Task SeedEditorialAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;
            var seeds = GetEditorialSeeds();
            var slugs = seeds.Select(seed => seed.Slug).ToArray();
            var existing = await _db.EditorialArticles
                .Where(entity => slugs.Contains(entity.Slug))
                .ToDictionaryAsync(entity => entity.Slug, cancellationToken);

            foreach (var seed in seeds)
            {
                if (!existing.TryGetValue(seed.Slug, out var article))
                {
                    article = new EditorialArticle
                    {
                        CreatedAtUtc = now
                    };
                    _db.EditorialArticles.Add(article);
                }

                article.Title = seed.Title;
                article.Slug = seed.Slug;
                article.Excerpt = seed.Excerpt;
                article.CoverImageUrl = seed.CoverImageUrl;
                article.Body = BuildEditorialBody(seed.BodyParagraphs);
                article.Topic = seed.Topic;
                article.AuthorName = seed.AuthorName;
                article.ReadingTimeMinutes = seed.ReadingTimeMinutes;
                article.Status = ContentStatus.Published;
                article.PublishedAtUtc = now.AddDays(-seed.PublishedDaysAgo);
                article.Seo = new SeoMetadata
                {
                    SeoTitle = seed.SeoTitle,
                    SeoDescription = seed.SeoDescription,
                    CanonicalUrl = $"/editorial/{seed.Slug}"
                };
                article.UpdatedAtUtc = now;
            }

            await _db.SaveChangesAsync(cancellationToken);
            await UpsertEditorialLinksAsync(now, cancellationToken);
            _logger.LogInformation("Editorial seeded.");
        }

        public async Task SeedTrustPagesAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;
            var seeds = GetTrustPageSeeds();
            var slugs = seeds.Select(seed => seed.Slug).ToArray();
            var kinds = seeds.Select(seed => seed.Kind).ToArray();

            var existing = await _db.TrustPages
                .Where(entity => slugs.Contains(entity.Slug) || kinds.Contains(entity.PageKind))
                .ToListAsync(cancellationToken);

            foreach (var seed in seeds)
            {
                var page = existing.FirstOrDefault(entity => entity.Slug == seed.Slug)
                           ?? existing.FirstOrDefault(entity => entity.PageKind == seed.Kind);

                if (page is null)
                {
                    page = new TrustPage
                    {
                        CreatedAtUtc = now
                    };
                    _db.TrustPages.Add(page);
                    existing.Add(page);
                }

                page.PageKind = seed.Kind;
                page.Title = seed.Title;
                page.Slug = seed.Slug;
                page.Body = BuildTrustBody(seed.Summary, seed.Items);
                page.IsPublished = true;
                page.Seo = new SeoMetadata
                {
                    SeoTitle = seed.SeoTitle,
                    SeoDescription = seed.SeoDescription,
                    CanonicalUrl = $"/{seed.Slug}"
                };
                page.UpdatedAtUtc = now;
            }

            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Trust pages seeded.");
        }

        private static void EnsureSeedDependencies(
            IReadOnlyDictionary<string, Brand> brands,
            IReadOnlyDictionary<string, Category> categories,
            IReadOnlyList<ProductSeed> seeds)
        {
            foreach (var seed in seeds)
            {
                if (!brands.ContainsKey(seed.BrandSlug))
                {
                    throw new InvalidOperationException($"Brand '{seed.BrandSlug}' not found.");
                }

                if (!categories.ContainsKey(seed.CategorySlug))
                {
                    throw new InvalidOperationException($"Category '{seed.CategorySlug}' not found.");
                }

                foreach (var additionalCategory in seed.AdditionalCategorySlugs)
                {
                    if (!categories.ContainsKey(additionalCategory))
                    {
                        throw new InvalidOperationException($"Additional category '{additionalCategory}' not found.");
                    }
                }
            }
        }
    }
}
