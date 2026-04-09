namespace TrendplusProdavnica.Domain.Sales;

/// <summary>
/// Metodos de envío disponibles
/// </summary>
public enum DeliveryMethod : short
{
    /// <summary>
    /// Envío por courier
    /// </summary>
    Courier = 0,

    /// <summary>
    /// Recogida en tienda
    /// </summary>
    StorePickup = 1,
}
