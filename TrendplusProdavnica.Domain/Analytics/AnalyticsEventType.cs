#nullable enable
namespace TrendplusProdavnica.Domain.Analytics
{
    /// <summary>
    /// Tipovi analitičkih događaja u ecommerce sistemu
    /// </summary>
    public enum AnalyticsEventType
    {
        /// <summary>Korisnik je pogledao proizvod</summary>
        ProductView = 1,

        /// <summary>Korisnik je kliknuo na proizvod</summary>
        ProductClick = 2,

        /// <summary>Korisnik je dodao proizvod u korpu</summary>
        AddToCart = 3,

        /// <summary>Korisnik je započeo checkout proces</summary>
        CheckoutStarted = 4,

        /// <summary>Korisnik je kompletan porudžbinu</summary>
        OrderCompleted = 5
    }
}
