using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrendplusProdavnica.Domain.Sales;

/// <summary>
/// Purchase order aggregate.
/// </summary>
public class Order
{
    public long Id { get; set; }
    public string OrderNumber { get; set; } = null!;
    public long? CartId { get; set; }
    public string? CheckoutIdempotencyKey { get; set; }

    public OrderStatus Status { get; set; }
    public string Currency { get; set; } = "RSD";
    public DeliveryMethod DeliveryMethod { get; set; }
    public PaymentMethod PaymentMethod { get; set; }

    public string CustomerFirstName { get; set; } = null!;
    public string CustomerLastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;

    public string DeliveryAddressLine1 { get; set; } = null!;
    public string? DeliveryAddressLine2 { get; set; }
    public string DeliveryCity { get; set; } = null!;
    public string DeliveryPostalCode { get; set; } = null!;

    public string? Note { get; set; }

    public decimal SubtotalAmount { get; set; }
    public decimal DeliveryAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public DateTimeOffset? PlacedAtUtc { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    [NotMapped]
    public string CustomerEmail
    {
        get => Email;
        set => Email = value;
    }

    [NotMapped]
    public decimal TotalPrice
    {
        get => TotalAmount;
        set => TotalAmount = value;
    }

    [NotMapped]
    public DateTimeOffset CreatedAt => CreatedAtUtc;

    public string GetCustomerFullName() => $"{CustomerFirstName} {CustomerLastName}".Trim();
}
