#nullable enable
using System;

namespace TrendplusProdavnica.Domain.Inventory
{
    /// <summary>
    /// Bazna klasa za stock evente
    /// </summary>
    public abstract class StockEvent
    {
        public long VariantId { get; protected set; }
        public long StoreId { get; protected set; }
        public int Quantity { get; protected set; }
        public DateTimeOffset OccurredAtUtc { get; protected set; } = DateTimeOffset.UtcNow;
        public string Reason { get; protected set; } = string.Empty;
        public long? OrderId { get; protected set; }
    }

    /// <summary>
    /// Događaj kada se količina na zalihi promijeni
    /// </summary>
    public class StockChangedEvent : StockEvent
    {
        public int PreviousQuantity { get; set; }
        public int NewQuantity { get; set; }

        public StockChangedEvent(long variantId, long storeId, int previousQuantity, int newQuantity, string reason)
        {
            VariantId = variantId;
            StoreId = storeId;
            PreviousQuantity = previousQuantity;
            NewQuantity = newQuantity;
            Quantity = newQuantity - previousQuantity;
            Reason = reason;
        }
    }

    /// <summary>
    /// Događaj kada se količina rezervira za narudžbu
    /// </summary>
    public class StockReservedEvent : StockEvent
    {
        public int ReservedQuantity { get; set; }

        public StockReservedEvent(long variantId, long storeId, int quantity, long orderId, string reason = "Order reserved")
        {
            VariantId = variantId;
            StoreId = storeId;
            Quantity = quantity;
            ReservedQuantity = quantity;
            OrderId = orderId;
            Reason = reason;
        }
    }

    /// <summary>
    /// Događaj kada se rezervirana količina oslobodi (otkazana narudžba)
    /// </summary>
    public class StockReleasedEvent : StockEvent
    {
        public int ReleasedQuantity { get; set; }

        public StockReleasedEvent(long variantId, long storeId, int quantity, long orderId, string reason = "Order cancelled")
        {
            VariantId = variantId;
            StoreId = storeId;
            Quantity = quantity;
            ReleasedQuantity = quantity;
            OrderId = orderId;
            Reason = reason;
        }
    }
}
