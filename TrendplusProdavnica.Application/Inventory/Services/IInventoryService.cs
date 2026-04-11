#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Inventory.Dtos;

namespace TrendplusProdavnica.Application.Inventory.Services
{
    /// <summary>
    /// Servis za upravljanje zaliham i inventarom
    /// </summary>
    public interface IInventoryService
    {
        /// <summary>
        /// Dohvata inventar za specifičnu prodavnicu i varijantu
        /// </summary>
        Task<StoreInventoryDto?> GetStoreInventoryAsync(long storeId, long variantId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Dohvata sve inventare za specifičnu varijantu (svi store-ovi)
        /// </summary>
        Task<VariantStockSummaryDto> GetVariantStockSummaryAsync(long variantId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Dohvata sve inventare za specifičnu prodavnicu
        /// </summary>
        Task<List<StoreInventoryDto>> GetStoreInventoriesAsync(long storeId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Ažurira količinu na zalihi
        /// </summary>
        Task<StockOperationResult> UpdateStockAsync(UpdateStockRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Broji zalihe (fyzički pregled)
        /// </summary>
        Task<StockOperationResult> CountStockAsync(CountStockRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Rezervira zalihu za narudžbu
        /// </summary>
        Task<StockOperationResult> ReserveStockAsync(ReserveStockRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Oslobađa rezerviranu zalihu
        /// </summary>
        Task<StockOperationResult> ReleaseStockAsync(ReleaseStockRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sinhronizira OpenSearch index za zalihe određene varijante
        /// </summary>
        Task<bool> SyncInventoryToSearchAsync(long variantId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Invalidira cache za određenu varijantu
        /// </summary>
        Task InvalidateCacheAsync(long variantId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Provjerava dostupnost zalihe
        /// </summary>
        Task<bool> IsAvailableAsync(long variantId, long storeId, int quantity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Dohvata dostupnu količinu za varijantu
        /// </summary>
        Task<int> GetAvailableQuantityAsync(long variantId, long storeId, CancellationToken cancellationToken = default);
    }
}
