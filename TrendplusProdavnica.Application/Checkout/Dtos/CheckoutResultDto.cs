namespace TrendplusProdavnica.Application.Checkout.Dtos;

/// <summary>
/// Resultado del checkout (orden creada)
/// </summary>
public class CheckoutResultDto
{
    public string OrderNumber { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = null!;
}
