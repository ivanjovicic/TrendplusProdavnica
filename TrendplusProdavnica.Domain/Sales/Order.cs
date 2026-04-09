namespace TrendplusProdavnica.Domain.Sales;

/// <summary>
/// Representa una orden de compra
/// </summary>
public class Order
{
    // Identifiers
    public long Id { get; set; }
    public string OrderNumber { get; set; } = null!; // TP-2026-000001
    public long? CartId { get; set; }

    // Status & Configuration
    public OrderStatus Status { get; set; }
    public string Currency { get; set; } = "RSD";
    public DeliveryMethod DeliveryMethod { get; set; }
    public PaymentMethod PaymentMethod { get; set; }

    // Customer Information
    public string CustomerFirstName { get; set; } = null!;
    public string CustomerLastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;

    // Delivery Address
    public string DeliveryAddressLine1 { get; set; } = null!;
    public string? DeliveryAddressLine2 { get; set; }
    public string DeliveryCity { get; set; } = null!;
    public string DeliveryPostalCode { get; set; } = null!;

    // Additional Info
    public string? Note { get; set; }

    // Pricing Information
    public decimal SubtotalAmount { get; set; } // Сума товара без доставе
    public decimal DeliveryAmount { get; set; } // Цена доставе
    public decimal TotalAmount { get; set; } // Укупна цена (subtotal + delivery)

    // Audit Timestamps
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public DateTimeOffset? PlacedAtUtc { get; set; }

    // Relationship
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    /// <summary>
    /// Полное имя клиента
    /// </summary>
    public string GetCustomerFullName() => $"{CustomerFirstName} {CustomerLastName}".Trim();
}
