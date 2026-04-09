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
    public class ProductMediaAdminService : IProductMediaAdminService
    {
        private readonly TrendplusDbContext _db;
        private readonly IWebshopCacheInvalidationService _cacheInvalidationService;
        private readonly IProductSearchIndexService _searchIndexService;
        private readonly ILogger<ProductMediaAdminService> _logger;

        public ProductMediaAdminService(
            TrendplusDbContext db,
            IWebshopCacheInvalidationService cacheInvalidationService,
            IProductSearchIndexService searchIndexService,
            ILogger<ProductMediaAdminService> logger)
        {
            _db = db;
            _cacheInvalidationService = cacheInvalidationService;
            _searchIndexService = searchIndexService;
            _logger = logger;
        }

        public async Task<IReadOnlyList<ProductMediaAdminDto>> GetListAsync(long? productId = null, CancellationToken cancellationToken = default)
        {
            var query = _db.ProductMedia.AsNoTracking().AsQueryable();

            if (productId.HasValue)
            {
                query = query.Where(entity => entity.ProductId == productId.Value);
            }

            var entities = await query
                .OrderBy(entity => entity.ProductId)
                .ThenBy(entity => entity.SortOrder)
                .ThenBy(entity => entity.Id)
                .ToArrayAsync(cancellationToken);

            return entities.Select(Map).ToArray();
        }

        public async Task<ProductMediaAdminDto> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        {
            var entity = await _db.ProductMedia.AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

            if (entity is null)
            {
                throw new AdminNotFoundException($"Product media with id '{id}' was not found.");
            }

            return Map(entity);
        }

        public async Task<ProductMediaAdminDto> CreateAsync(CreateProductMediaRequest request, CancellationToken cancellationToken = default)
        {
            await ValidateAsync(
                request.ProductId,
                request.VariantId,
                request.Url,
                request.MobileUrl,
                request.MediaType,
                request.MediaRole,
                cancellationToken);

            var now = DateTimeOffset.UtcNow;
            var entity = new ProductMedia
            {
                ProductId = request.ProductId,
                VariantId = request.VariantId,
                Url = request.Url.Trim(),
                MobileUrl = request.MobileUrl,
                AltText = request.AltText,
                Title = request.Title,
                MediaType = request.MediaType,
                MediaRole = request.MediaRole,
                SortOrder = request.SortOrder,
                IsPrimary = request.IsPrimary,
                IsActive = request.IsActive,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            _db.ProductMedia.Add(entity);

            if (request.IsPrimary)
            {
                await ClearOtherPrimaryMediaAsync(request.ProductId, null, cancellationToken);
            }

            await _db.SaveChangesAsync(cancellationToken);
            await OnProductMediaChangedAsync(entity.ProductId, cancellationToken);
            return Map(entity);
        }

        public async Task<ProductMediaAdminDto> UpdateAsync(long id, UpdateProductMediaRequest request, CancellationToken cancellationToken = default)
        {
            var entity = await _db.ProductMedia
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

            if (entity is null)
            {
                throw new AdminNotFoundException($"Product media with id '{id}' was not found.");
            }

            await ValidateAsync(
                request.ProductId,
                request.VariantId,
                request.Url,
                request.MobileUrl,
                request.MediaType,
                request.MediaRole,
                cancellationToken);

            entity.ProductId = request.ProductId;
            entity.VariantId = request.VariantId;
            entity.Url = request.Url.Trim();
            entity.MobileUrl = request.MobileUrl;
            entity.AltText = request.AltText;
            entity.Title = request.Title;
            entity.MediaType = request.MediaType;
            entity.MediaRole = request.MediaRole;
            entity.SortOrder = request.SortOrder;
            entity.IsPrimary = request.IsPrimary;
            entity.IsActive = request.IsActive;
            entity.UpdatedAtUtc = DateTimeOffset.UtcNow;

            if (request.IsPrimary)
            {
                await ClearOtherPrimaryMediaAsync(request.ProductId, id, cancellationToken);
            }

            await _db.SaveChangesAsync(cancellationToken);
            await OnProductMediaChangedAsync(entity.ProductId, cancellationToken);
            return Map(entity);
        }

        public async Task<ProductMediaAdminDto> DeactivateAsync(long id, CancellationToken cancellationToken = default)
        {
            var entity = await _db.ProductMedia
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

            if (entity is null)
            {
                throw new AdminNotFoundException($"Product media with id '{id}' was not found.");
            }

            entity.IsActive = false;
            entity.UpdatedAtUtc = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            await OnProductMediaChangedAsync(entity.ProductId, cancellationToken);

            return Map(entity);
        }

        public async Task<ProductMediaAdminDto> ActivateAsync(long id, CancellationToken cancellationToken = default)
        {
            var entity = await _db.ProductMedia
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

            if (entity is null)
            {
                throw new AdminNotFoundException($"Product media with id '{id}' was not found.");
            }

            entity.IsActive = true;
            entity.UpdatedAtUtc = DateTimeOffset.UtcNow;

            if (entity.IsPrimary)
            {
                await ClearOtherPrimaryMediaAsync(entity.ProductId, entity.Id, cancellationToken);
            }

            await _db.SaveChangesAsync(cancellationToken);
            await OnProductMediaChangedAsync(entity.ProductId, cancellationToken);
            return Map(entity);
        }

        private async Task OnProductMediaChangedAsync(long productId, CancellationToken cancellationToken)
        {
            var productSlug = await _db.Products.AsNoTracking()
                .Where(product => product.Id == productId)
                .Select(product => product.Slug)
                .FirstOrDefaultAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(productSlug))
            {
                await _cacheInvalidationService.InvalidateProductBySlugAsync(productSlug, cancellationToken);
            }

            try
            {
                await _searchIndexService.ReindexProductAsync(productId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Product reindex failed after media change for product {ProductId}", productId);
            }
        }

        private async Task ValidateAsync(
            long productId,
            long? variantId,
            string url,
            string? mobileUrl,
            MediaType mediaType,
            MediaRole mediaRole,
            CancellationToken cancellationToken)
        {
            var errors = new Dictionary<string, string[]>();

            if (productId <= 0)
            {
                AdminValidationHelper.AddError(errors, nameof(productId), "ProductId must be greater than 0.");
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                AdminValidationHelper.AddError(errors, nameof(url), "Url is required.");
            }
            else if (!AdminValidationHelper.IsValidAbsoluteUrl(url))
            {
                AdminValidationHelper.AddError(errors, nameof(url), "Url must be a valid absolute URL.");
            }

            if (!string.IsNullOrWhiteSpace(mobileUrl) && !AdminValidationHelper.IsValidAbsoluteUrl(mobileUrl))
            {
                AdminValidationHelper.AddError(errors, nameof(mobileUrl), "MobileUrl must be a valid absolute URL.");
            }

            if (!Enum.IsDefined(mediaType))
            {
                AdminValidationHelper.AddError(errors, nameof(mediaType), "MediaType is not valid.");
            }

            if (!Enum.IsDefined(mediaRole))
            {
                AdminValidationHelper.AddError(errors, nameof(mediaRole), "MediaRole is not valid.");
            }

            if (!await _db.Products.AsNoTracking().AnyAsync(entity => entity.Id == productId, cancellationToken))
            {
                AdminValidationHelper.AddError(errors, nameof(productId), $"Product '{productId}' does not exist.");
            }

            if (variantId.HasValue)
            {
                var belongsToProduct = await _db.ProductVariants.AsNoTracking()
                    .AnyAsync(entity => entity.Id == variantId.Value && entity.ProductId == productId, cancellationToken);

                if (!belongsToProduct)
                {
                    AdminValidationHelper.AddError(errors, nameof(variantId), "VariantId must belong to the specified product.");
                }
            }

            AdminValidationHelper.ThrowIfAny(errors, "Product media request validation failed.");
        }

        private async Task ClearOtherPrimaryMediaAsync(long productId, long? excludeId, CancellationToken cancellationToken)
        {
            var others = await _db.ProductMedia
                .Where(item =>
                    item.ProductId == productId &&
                    item.IsPrimary &&
                    (!excludeId.HasValue || item.Id != excludeId.Value))
                .ToListAsync(cancellationToken);

            foreach (var media in others)
            {
                media.IsPrimary = false;
                media.UpdatedAtUtc = DateTimeOffset.UtcNow;
            }
        }

        private static ProductMediaAdminDto Map(ProductMedia entity)
        {
            return new ProductMediaAdminDto(
                entity.Id,
                entity.ProductId,
                entity.VariantId,
                entity.Url,
                entity.MobileUrl,
                entity.AltText,
                entity.Title,
                entity.MediaType,
                entity.MediaRole,
                entity.SortOrder,
                entity.IsPrimary,
                entity.IsActive,
                entity.CreatedAtUtc,
                entity.UpdatedAtUtc);
        }
    }
}
