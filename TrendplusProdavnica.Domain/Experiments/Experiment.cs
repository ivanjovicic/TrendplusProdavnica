#nullable enable
using System;
using TrendplusProdavnica.Domain.Common;

namespace TrendplusProdavnica.Domain.Experiments
{
    /// <summary>
    /// A/B test eksperiment - sadrži dva varijantu i prati rezultate
    /// </summary>
    public class Experiment : EntityBase
    {
        /// <summary>Naziv eksperimenta</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Detaljni opis šta se testira</summary>
        public string? Description { get; set; }

        /// <summary>Tip eksperimenta (HomepageLayout, ProductGrid, CTA, itd.)</summary>
        public ExperimentType ExperimentType { get; set; }

        /// <summary>Status eksperimenta (Draft, Active, Paused, Completed, Cancelled)</summary>
        public ExperimentStatus Status { get; set; }

        /// <summary>Variant A - originalni/kontrolna grupa (npr. "Current Homepage")</summary>
        public string VariantA { get; set; } = string.Empty;

        /// <summary>Variant B - novi/test grupa (npr. "New Homepage")</summary>
        public string VariantB { get; set; } = string.Empty;

        /// <summary>Traffic split - % korisnika koji trebaju dobiti Variant A (npr. 50 = 50/50)</summary>
        public int TrafficSplit { get; set; } = 50;

        /// <summary>Minimum vrijeme eksperimenta u danima</summary>
        public int? MinimumDurationDays { get; set; }

        /// <summary>Početak eksperimenta</summary>
        public DateTimeOffset StartedAtUtc { get; set; }

        /// <summary>Završetak eksperimenta (ako je završen)</summary>
        public DateTimeOffset? EndedAtUtc { get; set; }

        /// <summary>Pobednicka varijanta (ako je eksperiment završen)</summary>
        public char? WinnerVariant { get; set; }

        /// <summary>Statistička značajnost (ako je dostignuta)</summary>
        public decimal? StatisticalSignificance { get; set; }

        // EF Core parameterless constructor
        private Experiment() { }

        public Experiment(
            string name,
            ExperimentType experimentType,
            string variantA,
            string variantB,
            int trafficSplit = 50)
        {
            Name = name;
            ExperimentType = experimentType;
            VariantA = variantA;
            VariantB = variantB;
            TrafficSplit = trafficSplit;
            Status = ExperimentStatus.Draft;
            StartedAtUtc = DateTimeOffset.UtcNow;
        }

        public void Activate()
        {
            if (Status != ExperimentStatus.Draft && Status != ExperimentStatus.Paused)
                throw new InvalidOperationException("Samo Draft ili Paused eksperimenti mogu biti aktivirani");

            Status = ExperimentStatus.Active;
        }

        public void Pause()
        {
            if (Status != ExperimentStatus.Active)
                throw new InvalidOperationException("Samo aktivni eksperimenti mogu biti pauzirati");

            Status = ExperimentStatus.Paused;
        }

        public void Complete(char winnerVariant, decimal? statisticalSignificance = null)
        {
            if (winnerVariant != 'A' && winnerVariant != 'B')
                throw new ArgumentException("Winner mora biti 'A' ili 'B'");

            Status = ExperimentStatus.Completed;
            EndedAtUtc = DateTimeOffset.UtcNow;
            WinnerVariant = winnerVariant;
            StatisticalSignificance = statisticalSignificance;
        }

        public void Cancel()
        {
            Status = ExperimentStatus.Cancelled;
            EndedAtUtc = DateTimeOffset.UtcNow;
        }
    }
}
