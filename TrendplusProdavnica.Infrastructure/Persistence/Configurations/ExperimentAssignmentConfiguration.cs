#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrendplusProdavnica.Domain.Experiments;

namespace TrendplusProdavnica.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// EF Core konfiguracija za ExperimentAssignment entitet - praćenje dodeljenosti varijanti
    /// </summary>
    public class ExperimentAssignmentConfiguration : IEntityTypeConfiguration<ExperimentAssignment>
    {
        public void Configure(EntityTypeBuilder<ExperimentAssignment> builder)
        {
            builder.ToTable("experiment_assignments", "experiments");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ExperimentId)
                .IsRequired();

            builder.Property(x => x.UserId)
                .IsRequired(false);

            builder.Property(x => x.SessionId)
                .HasMaxLength(500);

            builder.Property(x => x.AssignedVariant)
                .IsRequired();

            builder.Property(x => x.AssignedAtUtc)
                .IsRequired();

            builder.Property(x => x.IpAddress)
                .HasMaxLength(45);

            builder.Property(x => x.UserAgent)
                .HasMaxLength(500);

            // Indexes za performance i jedinstvenu dodeljenost
            builder.HasIndex(x => new { x.ExperimentId, x.UserId })
                .HasDatabaseName("IX_experiment_assignments_experimentid_userid")
                .IsUnique(false);

            builder.HasIndex(x => new { x.ExperimentId, x.SessionId })
                .HasDatabaseName("IX_experiment_assignments_experimentid_sessionid")
                .IsUnique(false);

            builder.HasIndex(x => x.AssignedVariant)
                .HasDatabaseName("IX_experiment_assignments_assignedvariant");

            builder.HasIndex(x => x.AssignedAtUtc)
                .HasDatabaseName("IX_experiment_assignments_assignedat");

            // Composite index za brzo pronalaženje dodeljenosti
            builder.HasIndex(x => new { x.ExperimentId, x.AssignedAtUtc })
                .HasDatabaseName("IX_experiment_assignments_experimentid_assignedat");
        }
    }
}
