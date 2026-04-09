namespace TrendplusProdavnica.Application.Checkout.Dtos;

/// <summary>
/// Resumen del carrito para el checkout
/// </summary>
public class CheckoutSummaryDto
{
    public string CartToken { get; set; } = null!;
    public List<CartItemSummaryDto> Items { get; set; } = new();
    public decimal SubtotalAmount { get; set; }
    public decimal DeliveryAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
}

public class CartItemSummaryDto
{
    public string ProductName { get; set; } = null!;
    public string BrandName { get; set; } = null!;
    public decimal SizeEu { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}
