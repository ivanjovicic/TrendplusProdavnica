#nullable enable
namespace TrendplusProdavnica.Domain.Enums
{
    /// <summary>
    /// Tipovi merchandising pravila
    /// </summary>
    public enum MerchandisingRuleType : short
    {
        /// <summary>
        /// Pin proizvod na vrh (highest priority)
        /// </summary>
        Pin = 1,

        /// <summary>
        /// Povećaj vidljivost proizvoda (boost)
        /// </summary>
        Boost = 2,

        /// <summary>
        /// Smanji vidljivost proizvoda (demote)
        /// </summary>
        Demote = 3
    }
}
