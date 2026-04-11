#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TrendplusProdavnica.Application.Common.Caching;
using TrendplusProdavnica.Application.Inventory.Dtos;
using TrendplusProdavnica.Application.Inventory.Services;
using TrendplusProdavnica.Application.Search.Services;
using TrendplusProdavnica.Domain.Enums;
using TrendplusProdavnica.Domain.Inventory;
using TrendplusProdavnica.Infrastructure.Caching;
using TrendplusProdavnica.Infrastructure.Persistence;
using TrendplusProdavnica.Infrastructure.Search.Services;

namespace TrendplusProdavnica.Infrastructure.Inventory
{
    /// <summary>
    /// Implementacija servisa za upravljanje zaliham
    /// </summary>
    #if false
    // InventoryService temporarily disabled due to missing StoreInventories DbSet
    // and entity property mismatches
    public class InventoryService : IInventoryService
    {
        private readonly TrendplusDbContext _db;
        private readonly IWebshopCache _cache;
        private readonly IWebshopCacheKeys _cacheKeys;
        private readonly IProductSearchService _searchService;
        private readonly ILogger<InventoryService> _logger;

        private const int LowStockThreshold = 10;

        public InventoryService(
            TrendplusDbContext db,
            IWebshopCache cache,
            IWebshopCacheKeys cacheKeys,
            IProductSearchService searchService,
            ILogger<InventoryService> logger)
        {
            _db = db;
            _cache = cache;
            _cacheKeys = cacheKeys;
            _searchService = searchService;
            _logger = logger;
        }

        public async Task<StoreInventoryDto?> GetStoreInventoryAsync(long storeId, long variantId, CancellationToken cancellationToken = default)
        {
            try
            {
                var inventory = await _db.StoreInventories
                    .AsNoTracking()
                    .Where(si => si.StoreId == storeId && si.VariantId == variantId)
                    .Include(si => si.Store)
                    .FirstOrDefaultAsync(cancellationToken);

                if (inventory == null)
                    return null;

                return new StoreInventoryDto
                {
                    Id = inventory.Id,
                    StoreId = inventory.StoreId,
                    StoreName = inventory.Store?.Name ?? string.Empty,
                    VariantId = inventory.VariantId,
                    QuantityOnHand = inventory.QuantityOnHand,
                    ReservedQuantity = inventory.ReservedQuantity,
                    IsLowStock = inventory.QuantityOnHand - inventory.ReservedQuantity <= LowStockThreshold
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting store inventory for store {StoreId}, variant {VariantId}", storeId, variantId);
                return null;
            }
        }

        public async Task<VariantStockSummaryDto> GetVariantStockSummaryAsync(long variantId, CancellationToken cancellationToken = default)
        {
            try
            {
                var inventories = await _db.StoreInventories
                    .AsNoTracking()
                    .Where(si => si.VariantId == variantId)
                    .Include(si => si.Store)
                    .ToListAsync(cancellationToken);

                var summary = new VariantStockSummaryDto
                {
                    VariantId = variantId,
                    TotalQuantityOnHand = inventories.Sum(i => i.QuantityOnHand),
                    TotalReservedQuantity = inventories.Sum(i => i.ReservedQuantity),
                    StoreInventories = inventories.Select(i => new StoreInventoryDto
                    {
                        Id = i.Id,
                        StoreId = i.StoreId,
                        StoreName = i.Store?.Name ?? string.Empty,
                        VariantId = i.VariantId,
                        QuantityOnHand = i.QuantityOnHand,
                        ReservedQuantity = i.ReservedQuantity,
                        IsLowStock = i.QuantityOnHand - i.ReservedQuantity <= LowStockThreshold
                    }).ToList()
                };

                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting variant stock summary for variant {VariantId}", variantId);
                return new VariantStockSummaryDto { VariantId = variantId };
            }
        }

        public async Task<List<StoreInventoryDto>> GetStoreInventoriesAsync(long storeId, CancellationToken cancellationToken = default)
        {
            try
            {
                var inventories = await _db.StoreInventories
                    .AsNoTracking()
                    .Where(si => si.StoreId == storeId)
                    .Include(si => si.Store)
                    .ToListAsync(cancellationToken);

                return inventories.Select(i => new StoreInventoryDto
                {
                    Id = i.Id,
                    StoreId = i.StoreId,
                    StoreName = i.Store?.Name ?? string.Empty,
                    VariantId = i.VariantId,
                    QuantityOnHand = i.QuantityOnHand,
                    ReservedQuantity = i.ReservedQuantity,
                    IsLowStock = i.QuantityOnHand - i.ReservedQuantity <= LowStockThreshold
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting store inventories for store {StoreId}", storeId);
                return new List<StoreInventoryDto>();
            }
        }

        public async Task<StockOperationResult> UpdateStockAsync(UpdateStockRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var inventory = await _db.StoreInventories
                    .Where(si => si.StoreId == request.StoreId && si.VariantId == request.VariantId)
                    .Include(si => si.Store)
                    .FirstOrDefaultAsync(cancellationToken);

                if (inventory == null)
                    return new StockOperationResult { Success = false, ErrorCode = "INVENTORY_NOT_FOUND" };

                if (request.NewQuantity < 0)
                    return new StockOperationResult { Success = false, ErrorCode = "INVALID_QUANTITY" };

                int previousQuantity = inventory.QuantityOnHand;
                inventory.QuantityOnHand = request.NewQuantity;
                inventory.UpdatedAtUtc = DateTimeOffset.UtcNow;

                // Emit event
                var @event = new StockChangedEvent(request.VariantId, request.StoreId, previousQuantity, request.NewQuantity, request.Reason);
                // Event će biti prikupljen iz Store entiteta - trebam preko Store-a

                _db.StoreInventories.Update(inventory);
                await _db.SaveChangesAsync(cancellationToken);

                await InvalidateCacheAsync(request.VariantId, cancellationToken);
                await SyncInventoryToSearchAsync(request.VariantId, cancellationToken);

                return new StockOperationResult
                {
                    Success = true,
                    Message = "Stock updated successfully",
                    Inventory = new StoreInventoryDto
                    {
                        Id = inventory.Id,
                        StoreId = inventory.StoreId,
                        StoreName = inventory.Store?.Name ?? string.Empty,
                        VariantId = inventory.VariantId,
                        QuantityOnHand = inventory.QuantityOnHand,
                        ReservedQuantity = inventory.ReservedQuantity
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stock for variant {VariantId}", request.VariantId);
                return new StockOperationResult { Success = false, ErrorCode = "UPDATE_FAILED" };
            }
        }

        public async Task<StockOperationResult> CountStockAsync(CountStockRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var inventory = await _db.StoreInventories
                    .Where(si => si.StoreId == request.StoreId && si.VariantId == request.VariantId)
                    .Include(si => si.Store)
                    .FirstOrDefaultAsync(cancellationToken);

                if (inventory == null)
                    return new StockOperationResult { Success = false, ErrorCode = "INVENTORY_NOT_FOUND" };

                int previousQuantity = inventory.QuantityOnHand;
                inventory.QuantityOnHand = request.CountedQuantity;
                inventory.UpdatedAtUtc = DateTimeOffset.UtcNow;

                _db.StoreInventories.Update(inventory);
                await _db.SaveChangesAsync(cancellationToken);

                await InvalidateCacheAsync(request.VariantId, cancellationToken);
                await SyncInventoryToSearchAsync(request.VariantId, cancellationToken);

                _logger.LogInformation("Stock counted for variant {VariantId}: {PreviousQuantity} -> {NewQuantity}. Notes: {Notes}",
                    request.VariantId, previousQuantity, request.CountedQuantity, request.Notes);

                return new StockOperationResult
                {
                    Success = true,
                    Message = "Stock counted and updated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting stock");
                return new StockOperationResult { Success = false, ErrorCode = "COUNT_FAILED" };
            }
        }

        public async Task<StockOperationResult> ReserveStockAsync(ReserveStockRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var inventory = await _db.StoreInventories
                    .Where(si => si.StoreId == request.StoreId && si.VariantId == request.VariantId)
                    .Include(si => si.Store)
                    .FirstOrDefaultAsync(cancellationToken);

                if (inventory == null)
                    return new StockOperationResult { Success = false, ErrorCode = "INVENTORY_NOT_FOUND" };

                int available = inventory.QuantityOnHand - inventory.ReservedQuantity;
                if (available < request.Quantity)
                    return new StockOperationResult 
                    { 
                        Success = false, 
                        ErrorCode = "INSUFFICIENT_STOCK",
                        Message = $"Only {available} items available"
                    };

                inventory.ReservedQuantity += request.Quantity;
                inventory.UpdatedAtUtc = DateTimeOffset.UtcNow;

                _db.StoreInventories.Update(inventory);
                await _db.SaveChangesAsync(cancellationToken);

                await InvalidateCacheAsync(request.VariantId, cancellationToken);
                await SyncInventoryToSearchAsync(request.VariantId, cancellationToken);

                _logger.LogInformation("Stock reserved for variant {VariantId}, order {OrderId}: {Quantity} units",
                    request.VariantId, request.OrderId, request.Quantity);

                return new StockOperationResult
                {
                    Success = true,
                    Message = "Stock reserved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reserving stock");
                return new StockOperationResult { Success = false, ErrorCode = "RESERVE_FAILED" };
            }
        }

        public async Task<StockOperationResult> ReleaseStockAsync(ReleaseStockRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var inventory = await _db.StoreInventories
                    .Where(si => si.StoreId == request.StoreId && si.VariantId == request.VariantId)
                    .Include(si => si.Store)
                    .FirstOrDefaultAsync(cancellationToken);

                if (inventory == null)
                    return new StockOperationResult { Success = false, ErrorCode = "INVENTORY_NOT_FOUND" };

                if (inventory.ReservedQuantity < request.Quantity)
                    return new StockOperationResult 
                    { 
                        Success = false, 
                        ErrorCode = "INVALID_RELEASE",
                        Message = $"Only {inventory.ReservedQuantity} units are reserved"
                    };

                inventory.ReservedQuantity -= request.Quantity;
                inventory.UpdatedAtUtc = DateTimeOffset.UtcNow;

                _db.StoreInventories.Update(inventory);
                await _db.SaveChangesAsync(cancellationToken);

                await InvalidateCacheAsync(request.VariantId, cancellationToken);
                await SyncInventoryToSearchAsync(request.VariantId, cancellationToken);

                _logger.LogInformation("Stock released for variant {VariantId}, order {OrderId}: {Quantity} units. Reason: {Reason}",
                    request.VariantId, request.OrderId, request.Quantity, request.Reason);

                return new StockOperationResult
                {
                    Success = true,
                    Message = "Stock released successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing stock");
                return new StockOperationResult { Success = false, ErrorCode = "RELEASE_FAILED" };
            }
        }

        public async Task<bool> SyncInventoryToSearchAsync(long variantId, CancellationToken cancellationToken = default)
        {
            try
            {
                var summary = await GetVariantStockSummaryAsync(variantId, cancellationToken);
                
                // Update OpenSearch index sa novim inventory podacima
                // Trebam znati kako se koristi IProductSearchService
                // Zasad samo logiramo
                
                _logger.LogInformation("Syncing inventory to search for variant {VariantId}. Available: {Available}",
                    variantId, summary.TotalAvailableQuantity);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing inventory to search");
                return false;
            }
        }

        public async Task InvalidateCacheAsync(long variantId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Invalidira sve cache ključeve vezane uz varijantu
                var cacheKey = $"variant:{variantId}:inventory";
                await _cache.RemoveAsync(cacheKey, cancellationToken);

                _logger.LogDebug("Cache invalidated for variant {VariantId}", variantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating cache");
            }
        }

        public async Task<bool> IsAvailableAsync(long variantId, long storeId, int quantity, CancellationToken cancellationToken = default)
        {
            try
            {
                var inventory = await GetStoreInventoryAsync(storeId, variantId, cancellationToken);
                if (inventory == null)
                    return false;

                return inventory.AvailableQuantity >= quantity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking availability");
                return false;
            }
        }

        public async Task<int> GetAvailableQuantityAsync(long variantId, long storeId, CancellationToken cancellationToken = default)
        {
            try
            {
                var inventory = await GetStoreInventoryAsync(storeId, variantId, cancellationToken);
                return inventory?.AvailableQuantity ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available quantity");
                return 0;
            }
        }
    }
    #endif  // InventoryService disabled due to missing StoreInventories DbSet
}
