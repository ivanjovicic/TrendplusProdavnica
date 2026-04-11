#nullable enable
using System;
using System.Collections.Generic;
using TrendplusProdavnica.Domain.Common;
using TrendplusProdavnica.Domain.Enums;

namespace TrendplusProdavnica.Domain.Sales
{
    /// <summary>
    /// Shopping cart aggregate for guest/anonymous users.
    /// Cart lifecycle: Active -> Abandoned or Converted
    /// </summary>
    public class Cart : AggregateRoot
    {
        /// <summary>
        /// Unique token for identifying/sharing the cart (client-side reference)
        /// </summary>
        public string CartToken { get; set; } = string.Empty;

        /// <summary>
        /// Optional user identifier for authenticated carts.
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Session identifier used to fetch cart in stateless API requests.
        /// </summary>
        public string? SessionId { get; set; }

        /// <summary>
        /// Current status of the cart (Active, Abandoned, Converted)
        /// </summary>
        public CartStatus Status { get; set; } = CartStatus.Active;

        /// <summary>
        /// Currency code (e.g., RSD)
        /// </summary>
        public string Currency { get; set; } = "RSD";

        /// <summary>
        /// When the cart expires and should be deleted
        /// </summary>
        public DateTimeOffset? ExpiresAtUtc { get; set; }

        /// <summary>
        /// Items in the cart
        /// </summary>
        public IList<CartItem> Items { get; } = new List<CartItem>();
    }
}
