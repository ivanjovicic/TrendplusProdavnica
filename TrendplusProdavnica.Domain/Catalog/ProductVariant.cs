#nullable enable
using TrendplusProdavnica.Domain.Common;
using TrendplusProdavnica.Domain.Enums;

namespace TrendplusProdavnica.Domain.Catalog
{
    public class ProductVariant : AggregateRoot
    {
        public long ProductId { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public decimal SizeEu { get; set; }
        public string? ColorName { get; set; }
        public string? ColorCode { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public string Currency { get; set; } = "RSD";
        public StockStatus StockStatus { get; set; } = StockStatus.OutOfStock;
        public int TotalStock { get; set; }
        public int LowStockThreshold { get; set; } = 2;
        public bool IsActive { get; set; } = true;
        public bool IsVisible { get; set; } = true;
        public int SortOrder { get; set; }
    }
}
