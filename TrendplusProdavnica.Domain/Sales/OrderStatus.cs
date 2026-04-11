namespace TrendplusProdavnica.Domain.Sales;

/// <summary>
/// Possible lifecycle statuses for an order.
/// </summary>
public enum OrderStatus : short
{
    /// <summary>
    /// Order created and awaiting payment.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Payment captured.
    /// </summary>
    Paid = 2,

    /// <summary>
    /// Order left warehouse and is in delivery.
    /// </summary>
    Shipped = 4,

    /// <summary>
    /// Order delivered and completed.
    /// </summary>
    Completed = 5,

    /// <summary>
    /// Order cancelled.
    /// </summary>
    Cancelled = 3,
}
