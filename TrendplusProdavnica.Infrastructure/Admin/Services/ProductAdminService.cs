#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TrendplusProdavnica.Application.Admin.Common;
using TrendplusProdavnica.Application.Admin.Dtos;
using TrendplusProdavnica.Application.Admin.Services;
using TrendplusProdavnica.Application.Common.Caching;
using TrendplusProdavnica.Application.Search.Services;
using TrendplusProdavnica.Domain.Catalog;
using TrendplusProdavnica.Domain.Enums;
using TrendplusProdavnica.Infrastructure.Admin.Common;
using TrendplusProdavnica.Infrastructure.Persistence;

namespace TrendplusProdavnica.Infrastructure.Admin.Services
{
    public class ProductAdminService : IProductAdminService
    {
        private readonly TrendplusDbContext _db;
        private readonly IWebshopCacheInvalidationService _cacheInvalidationService;
        private readonly IProductSearchIndexService _searchIndexService;
        private readonly ILogger<ProductAdminService> _logger;

        public ProductAdminService(
            TrendplusDbContext db,
            IWebshopCacheInvalidationService cacheInvalidationService,
            IProductSearchIndexService searchIndexService,
            ILogger<ProductAdminService> logger)
        {
            _db = db;
            _cacheInvalidationService = cacheInvalidationService;
            _searchIndexService = searchIndexService;
            _logger = logger;
        }

        public async Task<IReadOnlyList<ProductAdminListDto>> GetListAsync(
            long? brandId = null,
            long? categoryId = null,
            ProductStatus? status = null,
            bool? isNew = null,
            bool? isBestseller = null,
            CancellationToken cancellationToken = default)
        {
            var query = _db.Products.AsNoTracking().AsQueryable();

            if (brandId.HasValue)
            {
                query = query.Where(entity => entity.BrandId == brandId.Value);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(entity =>
                    entity.PrimaryCategoryId == categoryId.Value ||
                    entity.CategoryMaps.Any(map => map.CategoryId == categoryId.Value));
            }

            if (status.HasValue)
            {
                query = query.Where(entity => entity.Status == status.Value);
            }

            if (isNew.HasValue)
            {
                query = query.Where(entity => entity.IsNew == isNew.Value);
            }

            if (isBestseller.HasValue)
            {
                query = query.Where(entity => entity.IsBestseller == isBestseller.Value);
            }

            return await (
                from product in query
                join brand in _db.Brands.AsNoTracking() on product.BrandId equals brand.Id
                orderby product.UpdatedAtUtc descending, product.Id descending
                select new ProductAdminListDto(
                    product.Id,
                    product.BrandId,
                    brand.Name,
                    product.PrimaryCategoryId,
                    product.Name,
                    product.Slug,
                    product.Status,
                    product.IsVisible,
                    product.IsPurchasable,
                    product.IsNew,
                    product.IsBestseller,
                    product.SortRank,
                    product.PublishedAtUtc,
                    product.UpdatedAtUtc,
                    product.Version))
                .ToArrayAsync(cancellationToken);
        }

        public async Task<ProductAdminDetailDto> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        {
            var entity = await _db.Products.AsNoTracking()
                .Include(item => item.CategoryMaps)
                .Include(item => item.CollectionMaps)
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

            if (entity is null)
            {
                throw new AdminNotFoundException($"Product with id '{id}' was not found.");
            }

            return MapDetail(entity);
        }

        public async Task<ProductAdminDetailDto> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            var normalizedSlug = AdminValidationHelper.NormalizeSlug(slug);
            var entity = await _db.Products.AsNoTracking()
                .Include(item => item.CategoryMaps)
                .Include(item => item.CollectionMaps)
                .FirstOrDefaultAsync(item => item.Slug == normalizedSlug, cancellationToken);

            if (entity is null)
            {
                throw new AdminNotFoundException($"Product with slug '{slug}' was not found.");
            }

            return MapDetail(entity);
        }

        public async Task<ProductAdminDetailDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
        {
            await ValidateRequestAsync(request, null, cancellationToken);

            var now = DateTimeOffset.UtcNow;
            var normalizedSlug = AdminValidationHelper.NormalizeSlug(request.Slug);
            var secondaryCategoryIds = NormalizeIds(request.SecondaryCategoryIds)
                .Where(id => id != request.PrimaryCategoryId)
                .ToArray();
            var collectionIds = NormalizeIds(request.CollectionIds);

            var entity = new Product
            {
                BrandId = request.BrandId,
                PrimaryCategoryId = request.PrimaryCategoryId,
                SizeGuideId = request.SizeGuideId,
                Name = request.Name.Trim(),
                Slug = normalizedSlug,
                Subtitle = request.Subtitle,
                ShortDescription = request.ShortDescription,
                LongDescription = request.LongDescription,
                PrimaryColorName = request.PrimaryColorName,
                StyleTag = request.StyleTag,
                OccasionTag = request.OccasionTag,
                SeasonTag = request.SeasonTag,
                Status = request.Status,
                IsVisible = request.IsVisible,
                IsPurchasable = request.IsPurchasable,
                IsNew = request.IsNew,
                IsBestseller = request.IsBestseller,
                SortRank = request.SortRank,
                Seo = AdminMappingHelper.ToSeoModel(request.Seo),
                PublishedAtUtc = request.Status == ProductStatus.Published ? now : null,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            _db.Products.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);

            await ReplaceCategoryMapsAsync(entity.Id, secondaryCategoryIds, now, cancellationToken);
            await ReplaceCollectionMapsAsync(entity.Id, collectionIds, now, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            await OnProductChangedAsync(entity.Id, null, entity.Slug, cancellationToken);

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<ProductAdminDetailDto> UpdateAsync(long id, UpdateProductRequest request, CancellationToken cancellationToken = default)
        {
            var entity = await _db.Products
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

            if (entity is null)
            {
                throw new AdminNotFoundException($"Product with id '{id}' was not found.");
            }

            await ValidateRequestAsync(request, id, cancellationToken);

            var now = DateTimeOffset.UtcNow;
            var previousSlug = entity.Slug;
            var normalizedSlug = AdminValidationHelper.NormalizeSlug(request.Slug);
            var secondaryCategoryIds = NormalizeIds(request.SecondaryCategoryIds)
                .Where(categoryId => categoryId != request.PrimaryCategoryId)
                .ToArray();
            var collectionIds = NormalizeIds(request.CollectionIds);

            _db.Entry(entity).Property(item => item.Version).OriginalValue = request.Version;

            entity.BrandId = request.BrandId;
            entity.PrimaryCategoryId = request.PrimaryCategoryId;
            entity.SizeGuideId = request.SizeGuideId;
            entity.Name = request.Name.Trim();
            entity.Slug = normalizedSlug;
            entity.Subtitle = request.Subtitle;
            entity.ShortDescription = request.ShortDescription;
            entity.LongDescription = request.LongDescription;
            entity.PrimaryColorName = request.PrimaryColorName;
            entity.StyleTag = request.StyleTag;
            entity.OccasionTag = request.OccasionTag;
            entity.SeasonTag = request.SeasonTag;
            entity.Status = request.Status;
            entity.IsVisible = request.IsVisible;
            entity.IsPurchasable = request.IsPurchasable;
            entity.IsNew = request.IsNew;
            entity.IsBestseller = request.IsBestseller;
            entity.SortRank = request.SortRank;
            entity.Seo = AdminMappingHelper.ToSeoModel(request.Seo);
            entity.PublishedAtUtc = request.Status switch
            {
                ProductStatus.Published when entity.PublishedAtUtc is null => now,
                ProductStatus.Draft => null,
                _ => entity.PublishedAtUtc
            };
            entity.UpdatedAtUtc = now;

            await ReplaceCategoryMapsAsync(entity.Id, secondaryCategoryIds, now, cancellationToken);
            await ReplaceCollectionMapsAsync(entity.Id, collectionIds, now, cancellationToken);

            try
            {
                await _db.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new AdminConflictException($"Product '{id}' was modified by another user. Refresh and retry.");
            }

            await OnProductChangedAsync(entity.Id, previousSlug, entity.Slug, cancellationToken);
            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<ProductAdminDetailDto> PublishAsync(long id, CancellationToken cancellationToken = default)
        {
            var entity = await _db.Products
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

            if (entity is null)
            {
                throw new AdminNotFoundException($"Product with id '{id}' was not found.");
            }

            entity.Status = ProductStatus.Published;
            entity.PublishedAtUtc ??= DateTimeOffset.UtcNow;
            entity.UpdatedAtUtc = DateTimeOffset.UtcNow;

            try
            {
                await _db.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new AdminConflictException($"Product '{id}' was modified by another user. Refresh and retry.");
            }

            await OnProductChangedAsync(entity.Id, entity.Slug, entity.Slug, cancellationToken);
            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<ProductAdminDetailDto> ArchiveAsync(long id, CancellationToken cancellationToken = default)
        {
            var entity = await _db.Products
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

            if (entity is null)
            {
                throw new AdminNotFoundException($"Product with id '{id}' was not found.");
            }

            entity.Status = ProductStatus.Archived;
            entity.UpdatedAtUtc = DateTimeOffset.UtcNow;

            try
            {
                await _db.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new AdminConflictException($"Product '{id}' was modified by another user. Refresh and retry.");
            }

            await OnProductChangedAsync(entity.Id, entity.Slug, entity.Slug, cancellationToken);
            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<ProductAdminDetailDto> UnarchiveToDraftAsync(long id, CancellationToken cancellationToken = default)
        {
            var entity = await _db.Products
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

            if (entity is null)
            {
                throw new AdminNotFoundException($"Product with id '{id}' was not found.");
            }

            entity.Status = ProductStatus.Draft;
            entity.PublishedAtUtc = null;
            entity.UpdatedAtUtc = DateTimeOffset.UtcNow;

            try
            {
                await _db.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new AdminConflictException($"Product '{id}' was modified by another user. Refresh and retry.");
            }

            await OnProductChangedAsync(entity.Id, entity.Slug, entity.Slug, cancellationToken);
            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        private async Task OnProductChangedAsync(
            long productId,
            string? previousSlug,
            string currentSlug,
            CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(previousSlug) &&
                !string.Equals(previousSlug, currentSlug, StringComparison.OrdinalIgnoreCase))
            {
                await _cacheInvalidationService.InvalidateProductBySlugAsync(previousSlug, cancellationToken);
            }

            await _cacheInvalidationService.InvalidateProductBySlugAsync(currentSlug, cancellationToken);
            await TryReindexProductAsync(productId, cancellationToken);
        }

        private async Task TryReindexProductAsync(long productId, CancellationToken cancellationToken)
        {
            try
            {
                await _searchIndexService.ReindexProductAsync(productId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Product reindex failed for product {ProductId}", productId);
            }
        }

        private static ProductAdminDetailDto MapDetail(Product entity)
        {
            return new ProductAdminDetailDto(
                entity.Id,
                entity.BrandId,
                entity.PrimaryCategoryId,
                entity.SizeGuideId,
                entity.Name,
                entity.Slug,
                entity.Subtitle,
                entity.ShortDescription,
                entity.LongDescription,
                entity.PrimaryColorName,
                entity.StyleTag,
                entity.OccasionTag,
                entity.SeasonTag,
                entity.Status,
                entity.IsVisible,
                entity.IsPurchasable,
                entity.IsNew,
                entity.IsBestseller,
                entity.SortRank,
                AdminMappingHelper.ToSeoDto(entity.Seo),
                entity.CategoryMaps
                    .Select(map => map.CategoryId)
                    .Distinct()
                    .ToArray(),
                entity.CollectionMaps
                    .Select(map => map.CollectionId)
                    .Distinct()
                    .ToArray(),
                entity.PublishedAtUtc,
                entity.CreatedAtUtc,
                entity.UpdatedAtUtc,
                entity.Version);
        }

        private async Task ValidateRequestAsync(CreateProductRequest request, long? excludeId, CancellationToken cancellationToken)
        {
            var errors = new Dictionary<string, string[]>();
            ValidateSharedRequest(
                request.BrandId,
                request.PrimaryCategoryId,
                request.SizeGuideId,
                request.Name,
                request.Slug,
                request.ShortDescription,
                request.Status,
                request.Seo,
                request.SecondaryCategoryIds,
                request.CollectionIds,
                errors);

            await ValidateRequestReferencesAsync(
                request.BrandId,
                request.PrimaryCategoryId,
                request.SizeGuideId,
                request.SecondaryCategoryIds,
                request.CollectionIds,
                errors,
                cancellationToken);

            AdminValidationHelper.ThrowIfAny(errors, "Product request validation failed.");

            var normalizedSlug = AdminValidationHelper.NormalizeSlug(request.Slug);
            var slugExists = await _db.Products.AsNoTracking()
                .AnyAsync(item => item.Slug == normalizedSlug && (!excludeId.HasValue || item.Id != excludeId.Value), cancellationToken);

            if (slugExists)
            {
                throw new AdminConflictException($"Product slug '{normalizedSlug}' already exists.");
            }
        }

        private async Task ValidateRequestAsync(UpdateProductRequest request, long? excludeId, CancellationToken cancellationToken)
        {
            var errors = new Dictionary<string, string[]>();
            ValidateSharedRequest(
                request.BrandId,
                request.PrimaryCategoryId,
                request.SizeGuideId,
                request.Name,
                request.Slug,
                request.ShortDescription,
                request.Status,
                request.Seo,
                request.SecondaryCategoryIds,
                request.CollectionIds,
                errors);

            if (request.Version == 0)
            {
                AdminValidationHelper.AddError(errors, nameof(request.Version), "Version is required for optimistic concurrency.");
            }

            await ValidateRequestReferencesAsync(
                request.BrandId,
                request.PrimaryCategoryId,
                request.SizeGuideId,
                request.SecondaryCategoryIds,
                request.CollectionIds,
                errors,
                cancellationToken);

            AdminValidationHelper.ThrowIfAny(errors, "Product request validation failed.");

            var normalizedSlug = AdminValidationHelper.NormalizeSlug(request.Slug);
            var slugExists = await _db.Products.AsNoTracking()
                .AnyAsync(item => item.Slug == normalizedSlug && (!excludeId.HasValue || item.Id != excludeId.Value), cancellationToken);

            if (slugExists)
            {
                throw new AdminConflictException($"Product slug '{normalizedSlug}' already exists.");
            }
        }

        private static void ValidateSharedRequest(
            long brandId,
            long primaryCategoryId,
            long? sizeGuideId,
            string name,
            string slug,
            string shortDescription,
            ProductStatus status,
            SeoAdminDto? seo,
            long[] secondaryCategoryIds,
            long[] collectionIds,
            IDictionary<string, string[]> errors)
        {
            if (brandId <= 0)
            {
                AdminValidationHelper.AddError(errors, nameof(brandId), "BrandId must be greater than 0.");
            }

            if (primaryCategoryId <= 0)
            {
                AdminValidationHelper.AddError(errors, nameof(primaryCategoryId), "PrimaryCategoryId must be greater than 0.");
            }

            if (sizeGuideId.HasValue && sizeGuideId.Value <= 0)
            {
                AdminValidationHelper.AddError(errors, nameof(sizeGuideId), "SizeGuideId must be greater than 0.");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                AdminValidationHelper.AddError(errors, nameof(name), "Name is required.");
            }

            if (string.IsNullOrWhiteSpace(slug))
            {
                AdminValidationHelper.AddError(errors, nameof(slug), "Slug is required.");
            }
            else if (!AdminValidationHelper.IsValidSlug(AdminValidationHelper.NormalizeSlug(slug)))
            {
                AdminValidationHelper.AddError(errors, nameof(slug), "Slug must contain only lowercase letters, numbers and hyphens.");
            }

            if (string.IsNullOrWhiteSpace(shortDescription))
            {
                AdminValidationHelper.AddError(errors, nameof(shortDescription), "ShortDescription is required.");
            }

            if (!Enum.IsDefined(status))
            {
                AdminValidationHelper.AddError(errors, nameof(status), "Status is not valid.");
            }

            if (!string.IsNullOrWhiteSpace(seo?.OgImageUrl) && !AdminValidationHelper.IsValidAbsoluteUrl(seo.OgImageUrl))
            {
                AdminValidationHelper.AddError(errors, "seo.ogImageUrl", "Seo ogImageUrl must be a valid absolute URL.");
            }

            if (secondaryCategoryIds.Any(id => id <= 0))
            {
                AdminValidationHelper.AddError(errors, nameof(secondaryCategoryIds), "SecondaryCategoryIds must contain only positive ids.");
            }

            if (collectionIds.Any(id => id <= 0))
            {
                AdminValidationHelper.AddError(errors, nameof(collectionIds), "CollectionIds must contain only positive ids.");
            }
        }

        private async Task ValidateRequestReferencesAsync(
            long brandId,
            long primaryCategoryId,
            long? sizeGuideId,
            long[] secondaryCategoryIds,
            long[] collectionIds,
            IDictionary<string, string[]> errors,
            CancellationToken cancellationToken)
        {
            if (!await _db.Brands.AsNoTracking().AnyAsync(entity => entity.Id == brandId, cancellationToken))
            {
                AdminValidationHelper.AddError(errors, nameof(brandId), $"Brand '{brandId}' does not exist.");
            }

            if (!await _db.Categories.AsNoTracking().AnyAsync(entity => entity.Id == primaryCategoryId, cancellationToken))
            {
                AdminValidationHelper.AddError(errors, nameof(primaryCategoryId), $"Primary category '{primaryCategoryId}' does not exist.");
            }

            if (sizeGuideId.HasValue &&
                !await _db.SizeGuides.AsNoTracking().AnyAsync(entity => entity.Id == sizeGuideId.Value, cancellationToken))
            {
                AdminValidationHelper.AddError(errors, nameof(sizeGuideId), $"SizeGuide '{sizeGuideId.Value}' does not exist.");
            }

            var normalizedSecondaryIds = NormalizeIds(secondaryCategoryIds)
                .Where(id => id != primaryCategoryId)
                .ToArray();
            if (normalizedSecondaryIds.Length > 0)
            {
                var existingCount = await _db.Categories.AsNoTracking()
                    .CountAsync(entity => normalizedSecondaryIds.Contains(entity.Id), cancellationToken);

                if (existingCount != normalizedSecondaryIds.Length)
                {
                    AdminValidationHelper.AddError(errors, nameof(secondaryCategoryIds), "One or more secondary categories do not exist.");
                }
            }

            var normalizedCollectionIds = NormalizeIds(collectionIds);
            if (normalizedCollectionIds.Length > 0)
            {
                var existingCount = await _db.Collections.AsNoTracking()
                    .CountAsync(entity => normalizedCollectionIds.Contains(entity.Id), cancellationToken);

                if (existingCount != normalizedCollectionIds.Length)
                {
                    AdminValidationHelper.AddError(errors, nameof(collectionIds), "One or more collections do not exist.");
                }
            }
        }

        private async Task ReplaceCategoryMapsAsync(long productId, long[] secondaryCategoryIds, DateTimeOffset now, CancellationToken cancellationToken)
        {
            var existing = await _db.ProductCategoryMaps
                .Where(map => map.ProductId == productId)
                .ToListAsync(cancellationToken);

            _db.ProductCategoryMaps.RemoveRange(existing);

            for (var index = 0; index < secondaryCategoryIds.Length; index++)
            {
                _db.ProductCategoryMaps.Add(new ProductCategoryMap
                {
                    ProductId = productId,
                    CategoryId = secondaryCategoryIds[index],
                    SortOrder = index + 1,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                });
            }
        }

        private async Task ReplaceCollectionMapsAsync(long productId, long[] collectionIds, DateTimeOffset now, CancellationToken cancellationToken)
        {
            var existing = await _db.ProductCollectionMaps
                .Where(map => map.ProductId == productId)
                .ToListAsync(cancellationToken);

            _db.ProductCollectionMaps.RemoveRange(existing);

            for (var index = 0; index < collectionIds.Length; index++)
            {
                _db.ProductCollectionMaps.Add(new ProductCollectionMap
                {
                    ProductId = productId,
                    CollectionId = collectionIds[index],
                    SortOrder = index + 1,
                    Pinned = false,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                });
            }
        }

        private static long[] NormalizeIds(long[]? ids)
        {
            return ids?
                .Where(id => id > 0)
                .Distinct()
                .ToArray()
                ?? Array.Empty<long>();
        }
    }
}
