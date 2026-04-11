#nullable enable
namespace TrendplusProdavnica.Domain.Experiments
{
    /// <summary>
    /// Status eksperimenta
    /// </summary>
    public enum ExperimentStatus
    {
        /// <summary>Eksperiment je kreiran ali nije pokrenutan</summary>
        Draft = 1,

        /// <summary>Eksperiment je aktivan i prikuplja podatke</summary>
        Active = 2,

        /// <summary>Eksperiment je pauziran</summary>
        Paused = 3,

        /// <summary>Eksperiment je završen</summary>
        Completed = 4,

        /// <summary>Eksperiment je otkazan</summary>
        Cancelled = 5
    }

    /// <summary>
    /// Tip eksperimenta (šta se testira)
    /// </summary>
    public enum ExperimentType
    {
        /// <summary>Testiranje homepage layout-a</summary>
        HomepageLayout = 1,

        /// <summary>Testiranje product grid prikaza</summary>
        ProductGrid = 2,

        /// <summary>Testiranje CTA dugmadi</summary>
        CallToAction = 3,

        /// <summary>Testiranje cijene prikaza</summary>
        PricingDisplay = 4,

        /// <summary>Testiranje checkout flow-a</summary>
        CheckoutFlow = 5
    }
}
