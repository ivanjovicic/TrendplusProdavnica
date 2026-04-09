#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using TrendplusProdavnica.Domain.Enums;

namespace TrendplusProdavnica.Application.Admin.Dtos
{
    public class CreateProductVariantRequest
    {
        [Required]
        public long ProductId { get; set; }

        [Required]
        [MaxLength(80)]
        public string Sku { get; set; } = string.Empty;

        [MaxLength(64)]
        public string? Barcode { get; set; }

        [Required]
        public decimal SizeEu { get; set; }

        public string? ColorName { get; set; }
        public string? ColorCode { get; set; }

        [Required]
        public decimal Price { get; set; }

        public decimal? OldPrice { get; set; }

        [Required]
        [MaxLength(3)]
        public string Currency { get; set; } = "RSD";

        public StockStatus StockStatus { get; set; } = StockStatus.OutOfStock;
        public int TotalStock { get; set; }
        public int LowStockThreshold { get; set; } = 2;
        public bool IsActive { get; set; } = true;
        public bool IsVisible { get; set; } = true;
        public int SortOrder { get; set; }
    }

    public class UpdateProductVariantRequest
    {
        [Required]
        public uint Version { get; set; }

        [Required]
        public long ProductId { get; set; }

        [Required]
        [MaxLength(80)]
        public string Sku { get; set; } = string.Empty;

        [MaxLength(64)]
        public string? Barcode { get; set; }

        [Required]
        public decimal SizeEu { get; set; }

        public string? ColorName { get; set; }
        public string? ColorCode { get; set; }

        [Required]
        public decimal Price { get; set; }

        public decimal? OldPrice { get; set; }

        [Required]
        [MaxLength(3)]
        public string Currency { get; set; } = "RSD";

        public StockStatus StockStatus { get; set; } = StockStatus.OutOfStock;
        public int TotalStock { get; set; }
        public int LowStockThreshold { get; set; } = 2;
        public bool IsActive { get; set; } = true;
        public bool IsVisible { get; set; } = true;
        public int SortOrder { get; set; }
    }

    public record ProductVariantAdminDto(
        long Id,
        long ProductId,
        string Sku,
        string? Barcode,
        decimal SizeEu,
        string? ColorName,
        string? ColorCode,
        decimal Price,
        decimal? OldPrice,
        string Currency,
        StockStatus StockStatus,
        int TotalStock,
        int LowStockThreshold,
        bool IsActive,
        bool IsVisible,
        int SortOrder,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset UpdatedAtUtc,
        uint Version);
}
