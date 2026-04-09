namespace TrendplusProdavnica.Application.Checkout.Dtos;

public class OrderDto
{
    public string OrderNumber { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string CustomerFullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    
    public string DeliveryAddressLine1 { get; set; } = null!;
    public string? DeliveryAddressLine2 { get; set; }
    public string DeliveryCity { get; set; } = null!;
    public string DeliveryPostalCode { get; set; } = null!;
    
    public string DeliveryMethod { get; set; } = null!;
    public string PaymentMethod { get; set; } = null!;
    
    public decimal SubtotalAmount { get; set; }
    public decimal DeliveryAmount { get; set; }
    public decimal TotalAmount { get; set; }
    
    public List<OrderItemDto> Items { get; set; } = new();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? PlacedAt { get; set; }
}

public class OrderItemDto
{
    public string ProductName { get; set; } = null!;
    public string BrandName { get; set; } = null!;
    public decimal SizeEu { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}
