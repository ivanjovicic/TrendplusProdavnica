using TrendplusProdavnica.Domain.Sales;

namespace TrendplusProdavnica.Application.Checkout.Dtos;

/// <summary>
/// Solicitud para crear una orden
/// </summary>
public class CheckoutRequest
{
    // Cart
    public string CartToken { get; set; } = null!;

    // Customer
    public string CustomerFirstName { get; set; } = null!;
    public string CustomerLastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;

    // Delivery
    public string DeliveryAddressLine1 { get; set; } = null!;
    public string? DeliveryAddressLine2 { get; set; }
    public string DeliveryCity { get; set; } = null!;
    public string DeliveryPostalCode { get; set; } = null!;
    public DeliveryMethod DeliveryMethod { get; set; }

    // Payment
    public PaymentMethod PaymentMethod { get; set; }

    // Optional
    public string? Note { get; set; }
}
