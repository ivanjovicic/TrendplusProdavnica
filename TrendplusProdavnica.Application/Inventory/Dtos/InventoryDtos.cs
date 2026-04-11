#nullable enable
namespace TrendplusProdavnica.Application.Inventory.Dtos
{
    /// <summary>
    /// DTO za čitanje StoreInventory informacija
    /// </summary>
    public class StoreInventoryDto
    {
        public long Id { get; set; }
        public long StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public long VariantId { get; set; }
        public int QuantityOnHand { get; set; }
        public int ReservedQuantity { get; set; }
        public int AvailableQuantity => QuantityOnHand - ReservedQuantity;
        public bool IsLowStock { get; set; }
    }

    /// <summary>
    /// Zahtjev za ažuriranje količine
    /// </summary>
    public class UpdateStockRequest
    {
        public long VariantId { get; set; }
        public long StoreId { get; set; }
        public int NewQuantity { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Zahtjev za brojanje pojedinačnog prodavnice-variante
    /// </summary>
    public class CountStockRequest
    {
        public long VariantId { get; set; }
        public long StoreId { get; set; }
        public int CountedQuantity { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// Zahtjev za reserviranje zalihe (za narudžbu)
    /// </summary>
    public class ReserveStockRequest
    {
        public long VariantId { get; set; }
        public long StoreId { get; set; }
        public int Quantity { get; set; }
        public long OrderId { get; set; }
    }

    /// <summary>
    /// Zahtjev za oslobađanjem rezervirane zalihe
    /// </summary>
    public class ReleaseStockRequest
    {
        public long VariantId { get; set; }
        public long StoreId { get; set; }
        public int Quantity { get; set; }
        public long OrderId { get; set; }
        public string Reason { get; set; } = "Order cancelled";
    }

    /// <summary>
    /// Rezultat stock operacije
    /// </summary>
    public class StockOperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public StoreInventoryDto? Inventory { get; set; }
        public string ErrorCode { get; set; } = string.Empty;
    }

    /// <summary>
    /// Sumarni inventar za varijantu (svi store-ovi)
    /// </summary>
    public class VariantStockSummaryDto
    {
        public long VariantId { get; set; }
        public int TotalQuantityOnHand { get; set; }
        public int TotalReservedQuantity { get; set; }
        public int TotalAvailableQuantity => TotalQuantityOnHand - TotalReservedQuantity;
        public List<StoreInventoryDto> StoreInventories { get; set; } = new();
    }
}
