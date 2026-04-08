#nullable enable
using System;
using TrendplusProdavnica.Domain.Common;

namespace TrendplusProdavnica.Domain.Inventory
{
    public class StoreInventory : EntityBase
    {
        public long StoreId { get; set; }
        public long VariantId { get; set; }
        public int QuantityOnHand { get; set; }
        public int ReservedQuantity { get; set; }
    }
}
