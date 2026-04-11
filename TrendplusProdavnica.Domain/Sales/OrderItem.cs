using System.ComponentModel.DataAnnotations.Schema;

namespace TrendplusProdavnica.Domain.Sales;

/// <summary>
/// Single order line.
/// </summary>
public class OrderItem
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public long ProductId { get; set; }
    public long ProductVariantId { get; set; }

    public string ProductNameSnapshot { get; set; } = null!;
    public string BrandNameSnapshot { get; set; } = null!;
    public decimal SizeEuSnapshot { get; set; }

    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }

    public Order? Order { get; set; }

    [NotMapped]
    public decimal Price
    {
        get => UnitPrice;
        set => UnitPrice = value;
    }

    [NotMapped]
    public long VariantId
    {
        get => ProductVariantId;
        set => ProductVariantId = value;
    }
}
