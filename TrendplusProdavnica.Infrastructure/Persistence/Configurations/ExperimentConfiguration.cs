#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrendplusProdavnica.Domain.Experiments;

namespace TrendplusProdavnica.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// EF Core konfiguracija za Experiment entitet
    /// </summary>
    public class ExperimentConfiguration : IEntityTypeConfiguration<Experiment>
    {
        public void Configure(EntityTypeBuilder<Experiment> builder)
        {
            builder.ToTable("experiments", "experiments");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.Property(x => x.ExperimentType)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<int>()
                .HasDefaultValue(ExperimentStatus.Draft);

            builder.Property(x => x.VariantA)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.VariantB)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.TrafficSplit)
                .IsRequired()
                .HasDefaultValue(50);

            builder.Property(x => x.MinimumDurationDays)
                .IsRequired(false);

            builder.Property(x => x.StartedAtUtc)
                .IsRequired();

            builder.Property(x => x.EndedAtUtc)
                .IsRequired(false);

            builder.Property(x => x.WinnerVariant)
                .IsRequired(false);

            builder.Property(x => x.StatisticalSignificance)
                .IsRequired(false)
                .HasPrecision(5, 2);

            // Indexes za performance
            builder.HasIndex(x => x.Status)
                .HasDatabaseName("IX_experiments_status");

            builder.HasIndex(x => x.ExperimentType)
                .HasDatabaseName("IX_experiments_experimenttype");

            builder.HasIndex(x => new { x.Status, x.StartedAtUtc })
                .HasDatabaseName("IX_experiments_status_startedat");

            builder.HasIndex(x => x.StartedAtUtc)
                .HasDatabaseName("IX_experiments_startedat");

            // Relationships
            builder.HasMany<ExperimentAssignment>()
                .WithOne()
                .HasForeignKey(x => x.ExperimentId)
                .HasConstraintName("FK_experiment_assignments_experimentid")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
