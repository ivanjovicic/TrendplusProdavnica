namespace TrendplusProdavnica.Domain.Sales;

/// <summary>
/// Representa un artículo dentro de una orden
/// </summary>
public class OrderItem
{
    // Identifiers
    public long Id { get; set; }
    public long OrderId { get; set; }
    public long ProductId { get; set; }
    public long ProductVariantId { get; set; }

    // Product Snapshot (for history/audit)
    public string ProductNameSnapshot { get; set; } = null!;
    public string BrandNameSnapshot { get; set; } = null!;
    public decimal SizeEuSnapshot { get; set; }

    // Pricing & Quantity
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; } // UnitPrice * Quantity

    // Relationship
    public Order? Order { get; set; }
}
