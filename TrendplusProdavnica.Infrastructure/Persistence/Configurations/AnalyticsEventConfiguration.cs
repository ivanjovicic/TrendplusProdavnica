#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrendplusProdavnica.Domain.Analytics;

namespace TrendplusProdavnica.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// EF Core konfiguracija za AnalyticsEvent entitet
    /// </summary>
    public class AnalyticsEventConfiguration : IEntityTypeConfiguration<AnalyticsEvent>
    {
        public void Configure(EntityTypeBuilder<AnalyticsEvent> builder)
        {
            builder.ToTable("analytics_events", "analytics");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.EventType)
                .IsRequired()
                .HasConversion<short>();

            builder.Property(x => x.ProductId);

            builder.Property(x => x.UserId);

            builder.Property(x => x.SessionId)
                .HasMaxLength(256);

            builder.Property(x => x.EventTimestamp)
                .IsRequired();

            builder.Property(x => x.IpAddress)
                .HasMaxLength(45); // IPv6 je do 45 karaktera

            builder.Property(x => x.UserAgent)
                .HasMaxLength(500);

            builder.Property(x => x.PageUrl)
                .HasMaxLength(2048);

            builder.Property(x => x.ReferrerUrl)
                .HasMaxLength(2048);

            builder.Property(x => x.EventData)
                .HasColumnType("jsonb");

            // Indexes za performanse
            builder.HasIndex(x => x.EventTimestamp)
                .HasDatabaseName("IX_analytics_events_timestamp");

            builder.HasIndex(x => x.EventType)
                .HasDatabaseName("IX_analytics_events_eventtype");

            builder.HasIndex(x => x.ProductId)
                .HasDatabaseName("IX_analytics_events_productid");

            builder.HasIndex(x => x.UserId)
                .HasDatabaseName("IX_analytics_events_userid");

            builder.HasIndex(x => x.SessionId)
                .HasDatabaseName("IX_analytics_events_sessionid");

            builder.HasIndex(x => new { x.EventType, x.EventTimestamp })
                .HasDatabaseName("IX_analytics_events_type_timestamp");

            builder.HasIndex(x => new { x.ProductId, x.EventType })
                .HasDatabaseName("IX_analytics_events_product_eventtype");
        }
    }
}
