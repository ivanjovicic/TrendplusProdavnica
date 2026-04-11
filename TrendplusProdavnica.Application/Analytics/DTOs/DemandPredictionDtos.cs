#nullable enable
using System;
using System.Collections.Generic;

namespace TrendplusProdavnica.Application.Analytics.DTOs
{
    /// <summary>
    /// Zahtev za demand prediction za proizvod
    /// </summary>
    public class DemandPredictionRequest
    {
        /// <summary>
        /// ID proizvoda za koji se predviđa potražnja
        /// </summary>
        public long ProductId { get; set; }

        /// <summary>
        /// Broj meseci unazad za analizu (default: 12)
        /// </summary>
        public int HistorymonthsCount { get; set; } = 12;

        /// <summary>
        /// Da li je proizvod obuća (za specifičnu logiku veličina)
        /// </summary>
        public bool IsFootwear { get; set; } = true;
    }

    /// <summary>
    /// Predviđanje potražnje za proizvod
    /// </summary>
    public class DemandPredictionDto
    {
        /// <summary>
        /// ID proizvoda
        /// </summary>
        public long ProductId { get; set; }

        /// <summary>
        /// Naziv proizvoda
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Očekivana mesečna prodaja (komada)
        /// </summary>
        public decimal ExpectedMonthlySales { get; set; }

        /// <summary>
        /// Grafikon trendova - poslednji 12 meseci
        /// </summary>
        public List<MonthlySalesData> MonthlySalesHistory { get; set; } = new();

        /// <summary>
        /// Distribucija veličina - koliko se cui svake veličine
        /// </summary>
        public List<SizeDistributionData> SizeDistribution { get; set; } = new();

        /// <summary>
        /// Sezonalnost - kako se potražnja menja po sezoni
        /// </summary>
        public List<SeasonalityData> SeasonalityIndex { get; set; } = new();

        /// <summary>
        /// Predviđanje za sledeći mesec
        /// </summary>
        public decimal ForecastNextMonth { get; set; }

        /// <summary>
        /// Sigurnost predviđanja (0-100%)
        /// </summary>
        public decimal ConfidenceScore { get; set; }

        /// <summary>
        /// Datum poslednje analize
        /// </summary>
        public DateTimeOffset AnalyzedAtUtc { get; set; }

        /// <summary>
        /// Status analitike
        /// </summary>
        public string Status { get; set; } = "COMPLETED";
    }

    /// <summary>
    /// Podatak o mesečnoj prodaji
    /// </summary>
    public class MonthlySalesData
    {
        /// <summary>
        /// Mesec i godina (YYYY-MM)
        /// </summary>
        public string Month { get; set; } = string.Empty;

        /// <summary>
        /// Broj prodatih komada
        /// </summary>
        public decimal UnitsSOld { get; set; }

        /// <summary>
        /// Ukupna vrednost u originalnoj valuti
        /// </summary>
        public decimal Revenue { get; set; }
    }

    /// <summary>
    /// Distribucija po veličinama - za obuću (EU size)
    /// </summary>
    public class SizeDistributionData
    {
        /// <summary>
        /// Veličina (EU size)
        /// </summary>
        public decimal Size { get; set; }

        /// <summary>
        /// Broj prodatih parova/komada
        /// </summary>
        public int UnitsSold { get; set; }

        /// <summary>
        /// Procenat od ukupne prodaje
        /// </summary>
        public decimal PercentageOfTotal { get; set; }

        /// <summary>
        /// Predložena količina za nabavku sledeći mesec
        /// </summary>
        public int RecommendedStockQuantity { get; set; }
    }

    /// <summary>
    /// Sezonalni indeks - kako se potražnja menja po sezoni
    /// </summary>
    public class SeasonalityData
    {
        /// <summary>
        /// Naziv sezone (SPRING, SUMMER, FALL, WINTER)
        /// </summary>
        public string Season { get; set; } = string.Empty;

        /// <summary>
        /// Индекс sezonalnosti (1.0 = average, >1.0 peak season, <1.0 low season)
        /// </summary>
        public decimal SeasonalIndex { get; set; }

        /// <summary>
        /// Prosečna prodaja u ovoj sezoni
        /// </summary>
        public decimal AverageMonthlyUnits { get; set; }
    }

    /// <summary>
    /// Zahtev za bulk prediction više proizvoda
    /// </summary>
    public class BulkDemandPredictionRequest
    {
        public List<long> ProductIds { get; set; } = new();
        public int HistoryMonthsCount { get; set; } = 12;
    }

    /// <summary>
    /// Odgovor za bulk prediction
    /// </summary>
    public class BulkDemandPredictionResponse
    {
        public List<DemandPredictionDto> Predictions { get; set; } = new();
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
