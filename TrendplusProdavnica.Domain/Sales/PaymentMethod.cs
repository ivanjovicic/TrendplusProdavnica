namespace TrendplusProdavnica.Domain.Sales;

/// <summary>
/// Métodos de pago disponibles
/// </summary>
public enum PaymentMethod : short
{
    /// <summary>
    /// Pago contra entega (dinero en mano)
    /// </summary>
    CashOnDelivery = 0,

    /// <summary>
    /// Pago por tarjeta (placeholder para integración futura)
    /// </summary>
    CardPlaceholder = 1,
}
