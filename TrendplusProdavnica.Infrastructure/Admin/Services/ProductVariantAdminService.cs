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
    public class ProductVariantAdminService : IProductVariantAdminService
    {
        private readonly TrendplusDbContext _db;
        private readonly IWebshopCacheInvalidationService _cacheInvalidationService;
        private readonly IProductSearchIndexService _searchIndexService;
        private readonly ILogger<ProductVariantAdminService> _logger;

        public ProductVariantAdminService(
            TrendplusDbContext db,
            IWebshopCacheInvalidationService cacheInvalidationService,
            IProductSearchIndexService searchIndexService,
            ILogger<ProductVariantAdminService> logger)
        {
            _db = db;
            _cacheInvalidationService = cacheInvalidationService;
            _searchIndexService = searchIndexService;
            _logger = logger;
        }

        public async Task<IReadOnlyList<ProductVariantAdminDto>> GetByProductAsync(long productId, CancellationToken cancellationToken = default)
        {
            var entities = await _db.ProductVariants.AsNoTracking()
                .Where(entity => entity.ProductId == productId)
                .OrderBy(entity => entity.SortOrder)
                .ThenBy(entity => entity.SizeEu)
                .ToArrayAsync(cancellationToken);

            return entities.Select(Map).ToArray();
        }

        public async Task<ProductVariantAdminDto> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        {
            var entity = await _db.ProductVariants.AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

            if (entity is null)
            {
                throw new AdminNotFoundException($"Product variant with id '{id}' was not found.");
            }

            return Map(entity);
        }

        public async Task<ProductVariantAdminDto> CreateAsync(CreateProductVariantRequest request, CancellationToken cancellationToken = default)
        {
            await ValidateAsync(
                request.ProductId,
                request.Sku,
                request.SizeEu,
                request.Price,
                request.OldPrice,
                request.Currency,
                request.StockStatus,
                request.TotalStock,
                request.LowStockThreshold,
                null,
                cancellationToken);

            var now = DateTimeOffset.UtcNow;
            var entity = new ProductVariant
            {
                ProductId = request.ProductId,
                Sku = request.Sku.Trim().ToUpperInvariant(),
                Barcode = request.Barcode,
                SizeEu = request.SizeEu,
                ColorName = request.ColorName,
                ColorCode = request.ColorCode,
                Price = request.Price,
                OldPrice = request.OldPrice,
                Currency = request.Currency.Trim().ToUpperInvariant(),
                StockStatus = request.StockStatus,
                TotalStock = request.TotalStock,
                LowStockThreshold = request.LowStockThreshold,
                IsActive = request.IsActive,
                IsVisible = request.IsVisible,
                SortOrder = request.SortOrder,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            _db.ProductVariants.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);
            await OnProductVariantChangedAsync(entity.ProductId, cancellationToken);

            return Map(entity);
        }

        public async Task<ProductVariantAdminDto> UpdateAsync(long id, UpdateProductVariantRequest request, CancellationToken cancellationToken = default)
        {
            var entity = await _db.ProductVariants
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

            if (entity is null)
            {
                throw new AdminNotFoundException($"Product variant with id '{id}' was not found.");
            }

            await ValidateAsync(
                request.ProductId,
                request.Sku,
                request.SizeEu,
                request.Price,
                request.OldPrice,
                request.Currency,
                request.StockStatus,
                request.TotalStock,
                request.LowStockThreshold,
                id,
                cancellationToken);

            if (request.Version == 0)
            {
                throw new AdminValidationException("Version is required for optimistic concurrency.", new Dictionary<string, string[]>
                {
                    [nameof(request.Version)] = new[] { "Version is required for optimistic concurrency." }
                });
            }

            _db.Entry(entity).Property(item => item.Version).OriginalValue = request.Version;

            entity.ProductId = request.ProductId;
            entity.Sku = request.Sku.Trim().ToUpperInvariant();
            entity.Barcode = request.Barcode;
            entity.SizeEu = request.SizeEu;
            entity.ColorName = request.ColorName;
            entity.ColorCode = request.ColorCode;
            entity.Price = request.Price;
            entity.OldPrice = request.OldPrice;
            entity.Currency = request.Currency.Trim().ToUpperInvariant();
            entity.StockStatus = request.StockStatus;
            entity.TotalStock = request.TotalStock;
            entity.LowStockThreshold = request.LowStockThreshold;
            entity.IsActive = request.IsActive;
            entity.IsVisible = request.IsVisible;
            entity.SortOrder = request.SortOrder;
            entity.UpdatedAtUtc = DateTimeOffset.UtcNow;

            try
            {
                await _db.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new AdminConflictException($"Product variant '{id}' was modified by another user. Refresh and retry.");
            }

            await OnProductVariantChangedAsync(entity.ProductId, cancellationToken);
            return Map(entity);
        }

        public async Task<ProductVariantAdminDto> DeactivateAsync(long id, CancellationToken cancellationToken = default)
        {
            var entity = await _db.ProductVariants
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

            if (entity is null)
            {
                throw new AdminNotFoundException($"Product variant with id '{id}' was not found.");
            }

            entity.IsActive = false;
            entity.UpdatedAtUtc = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            await OnProductVariantChangedAsync(entity.ProductId, cancellationToken);

            return Map(entity);
        }

        public async Task<ProductVariantAdminDto> ReactivateAsync(long id, CancellationToken cancellationToken = default)
        {
            var entity = await _db.ProductVariants
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

            if (entity is null)
            {
                throw new AdminNotFoundException($"Product variant with id '{id}' was not found.");
            }

            entity.IsActive = true;
            entity.UpdatedAtUtc = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            await OnProductVariantChangedAsync(entity.ProductId, cancellationToken);

            return Map(entity);
        }

        private async Task OnProductVariantChangedAsync(long productId, CancellationToken cancellationToken)
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
                _logger.LogWarning(ex, "Product reindex failed after variant change for product {ProductId}", productId);
            }
        }

        private async Task ValidateAsync(
            long productId,
            string sku,
            decimal sizeEu,
            decimal price,
            decimal? oldPrice,
            string currency,
            StockStatus stockStatus,
            int totalStock,
            int lowStockThreshold,
            long? excludeId,
            CancellationToken cancellationToken)
        {
            var errors = new Dictionary<string, string[]>();

            if (productId <= 0)
            {
                AdminValidationHelper.AddError(errors, nameof(productId), "ProductId must be greater than 0.");
            }

            if (string.IsNullOrWhiteSpace(sku))
            {
                AdminValidationHelper.AddError(errors, nameof(sku), "Sku is required.");
            }

            if (sizeEu <= 0)
            {
                AdminValidationHelper.AddError(errors, nameof(sizeEu), "SizeEu must be greater than 0.");
            }

            if (price <= 0)
            {
                AdminValidationHelper.AddError(errors, nameof(price), "Price must be greater than 0.");
            }

            if (oldPrice.HasValue && oldPrice.Value <= price)
            {
                AdminValidationHelper.AddError(errors, nameof(oldPrice), "OldPrice must be greater than Price when provided.");
            }

            if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3)
            {
                AdminValidationHelper.AddError(errors, nameof(currency), "Currency must be a 3-letter ISO code.");
            }

            if (!Enum.IsDefined(stockStatus))
            {
                AdminValidationHelper.AddError(errors, nameof(stockStatus), "StockStatus is not valid.");
            }

            if (totalStock < 0)
            {
                AdminValidationHelper.AddError(errors, nameof(totalStock), "TotalStock must be zero or positive.");
            }

            if (lowStockThreshold < 0)
            {
                AdminValidationHelper.AddError(errors, nameof(lowStockThreshold), "LowStockThreshold must be zero or positive.");
            }

            if (!await _db.Products.AsNoTracking().AnyAsync(entity => entity.Id == productId, cancellationToken))
            {
                AdminValidationHelper.AddError(errors, nameof(productId), $"Product '{productId}' does not exist.");
            }

            var normalizedSku = sku.Trim().ToUpperInvariant();
            var skuExists = await _db.ProductVariants.AsNoTracking()
                .AnyAsync(item => item.Sku == normalizedSku && (!excludeId.HasValue || item.Id != excludeId.Value), cancellationToken);
            if (skuExists)
            {
                throw new AdminConflictException($"SKU '{normalizedSku}' already exists.");
            }

            AdminValidationHelper.ThrowIfAny(errors, "Product variant request validation failed.");
        }

        private static ProductVariantAdminDto Map(ProductVariant entity)
        {
            return new ProductVariantAdminDto(
                entity.Id,
                entity.ProductId,
                entity.Sku,
                entity.Barcode,
                entity.SizeEu,
                entity.ColorName,
                entity.ColorCode,
                entity.Price,
                entity.OldPrice,
                entity.Currency,
                entity.StockStatus,
                entity.TotalStock,
                entity.LowStockThreshold,
                entity.IsActive,
                entity.IsVisible,
                entity.SortOrder,
                entity.CreatedAtUtc,
                entity.UpdatedAtUtc,
                entity.Version);
        }
    }
}
