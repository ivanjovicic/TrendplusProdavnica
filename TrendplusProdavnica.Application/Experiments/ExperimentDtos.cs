#nullable enable
using System;
using TrendplusProdavnica.Domain.Experiments;

namespace TrendplusProdavnica.Application.Experiments
{
    /// <summary>Odgovore - detaljni prikaz eksperimenta</summary>
    public class ExperimentDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ExperimentType ExperimentType { get; set; }
        public ExperimentStatus Status { get; set; }
        public string VariantA { get; set; } = string.Empty;
        public string VariantB { get; set; } = string.Empty;
        public int TrafficSplit { get; set; }
        public int? MinimumDurationDays { get; set; }
        public DateTimeOffset StartedAtUtc { get; set; }
        public DateTimeOffset? EndedAtUtc { get; set; }
        public char? WinnerVariant { get; set; }
        public decimal? StatisticalSignificance { get; set; }
    }

    /// <summary>Zahtev za pravljenje novog eksperimenta</summary>
    public class CreateExperimentRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ExperimentType ExperimentType { get; set; }
        public string VariantA { get; set; } = string.Empty;
        public string VariantB { get; set; } = string.Empty;
        public int TrafficSplit { get; set; } = 50;
        public int? MinimumDurationDays { get; set; }
    }

    /// <summary>Zahtev za ažuriranje eksperimenta</summary>
    public class UpdateExperimentRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? TrafficSplit { get; set; }
        public int? MinimumDurationDays { get; set; }
    }

    /// <summary>Zahtev za završavanje eksperimenta</summary>
    public class CompleteExperimentRequest
    {
        public char WinnerVariant { get; set; }
        public decimal? StatisticalSignificance { get; set; }
    }

    /// <summary>Dodeljenost varijante za korisnika</summary>
    public class ExperimentAssignmentDto
    {
        public long ExperimentId { get; set; }
        public char AssignedVariant { get; set; }
        public DateTimeOffset AssignedAtUtc { get; set; }
    }

    /// <summary>Zahtev za dobijanje ili dodeljenost varijante</summary>
    public class GetOrAssignVariantRequest
    {
        public long ExperimentId { get; set; }
        public Guid? UserId { get; set; }
        public string? SessionId { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }

    /// <summary>Rezultati eksperimenta - metrike za admin</summary>
    public class ExperimentResultsDto
    {
        public long ExperimentId { get; set; }
        public string ExperimentName { get; set; } = string.Empty;
        public ExperimentStatus Status { get; set; }
        
        public int TotalAssignments { get; set; }
        public int VariantAAssignments { get; set; }
        public int VariantBAssignments { get; set; }
        public decimal VariantATrafficPercentage { get; set; }
        public decimal VariantBTrafficPercentage { get; set; }

        public int? VariantAConversions { get; set; }
        public int? VariantBConversions { get; set; }
        public decimal? VariantAConversionRate { get; set; }
        public decimal? VariantBConversionRate { get; set; }
        public decimal? ConversionDifference { get; set; }

        public char? WinnerVariant { get; set; }
        public decimal? StatisticalSignificance { get; set; }
        
        public DateTimeOffset StartedAtUtc { get; set; }
        public DateTimeOffset? EndedAtUtc { get; set; }
        public string? Duration { get; set; }
    }
}
