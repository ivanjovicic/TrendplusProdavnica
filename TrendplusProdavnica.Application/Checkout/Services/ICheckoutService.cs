using TrendplusProdavnica.Application.Checkout.Dtos;

namespace TrendplusProdavnica.Application.Checkout.Services;

/// <summary>
/// Interfaz para el servicio de checkout
/// Implementación: TrendplusProdavnica.Infrastructure.Services.CheckoutService
/// </summary>
public interface ICheckoutService
{
    Task<CheckoutSummaryDto?> GetCheckoutSummaryAsync(string cartToken, CancellationToken cancellationToken = default);
    Task<CheckoutResultDto> PlaceOrderAsync(CheckoutRequest request, CancellationToken cancellationToken = default);
    Task<OrderDto?> GetOrderByNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
}
