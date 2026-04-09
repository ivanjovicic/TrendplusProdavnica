namespace TrendplusProdavnica.Domain.Sales;

/// <summary>
/// Estados posibles de una orden
/// </summary>
public enum OrderStatus : short
{
    /// <summary>
    /// Orden en borrador, no confirmada
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Orden creada, esperando pago
    /// </summary>
    PendingPayment = 1,

    /// <summary>
    /// Orden confirmada y pagada
    /// </summary>
    Placed = 2,

    /// <summary>
    /// Orden cancelada
    /// </summary>
    Cancelled = 3,
}
