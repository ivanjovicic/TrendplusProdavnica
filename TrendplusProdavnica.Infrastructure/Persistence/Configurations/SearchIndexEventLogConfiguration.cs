#nullable enable
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrendplusProdavnica.Domain.Search;

namespace TrendplusProdavnica.Infrastructure.Persistence.Configurations
{
    public sealed class SearchIndexEventLogConfiguration : IEntityTypeConfiguration<SearchIndexEventLog>
    {
        public void Configure(EntityTypeBuilder<SearchIndexEventLog> builder)
        {
            builder.ToTable("search_index_events");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.EventId)
                .IsRequired()
                .HasMaxLength(36);

            builder.Property(e => e.Type)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(e => e.ProductId)
                .IsRequired();

            builder.Property(e => e.CreatedAtUtc)
                .IsRequired();

            builder.Property(e => e.RetryCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(e => e.LastErrorMessage)
                .HasMaxLength(500);

            builder.Property(e => e.LastRetryAtUtc);

            builder.Property(e => e.IsProcessed)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(e => e.IsDeadLettered)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(e => e.DeadLetteredAtUtc);

            builder.Property(e => e.DeadLetterReason)
                .HasMaxLength(500);

            builder.Property(e => e.ProcessedAtUtc);

            // Indexes
            builder.HasIndex(e => e.EventId)
                .IsUnique();

            builder.HasIndex(e => new { e.IsProcessed, e.CreatedAtUtc })
                .HasDatabaseName("ix_search_index_events_pending");

            builder.HasIndex(e => new { e.IsDeadLettered, e.DeadLetteredAtUtc })
                .HasDatabaseName("ix_search_index_events_dlq");

            builder.HasIndex(e => e.ProductId)
                .HasDatabaseName("ix_search_index_events_product_id");
        }
    }
}
