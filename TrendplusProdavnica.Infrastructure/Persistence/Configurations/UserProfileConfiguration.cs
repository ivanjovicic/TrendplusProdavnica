#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrendplusProdavnica.Domain.Personalization;

namespace TrendplusProdavnica.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// EF Core konfiguracija za UserProfile entitet
    /// </summary>
    public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
    {
        public void Configure(EntityTypeBuilder<UserProfile> builder)
        {
            builder.ToTable("user_profiles", "personalization");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.UserId)
                .IsRequired();

            builder.Property(x => x.FavoriteBrandIds)
                .HasConversion(
                    v => string.Join(",", v),
                    v => v.Split(",", System.StringSplitOptions.RemoveEmptyEntries)
                        .Select(long.Parse)
                        .ToList())
                .HasMaxLength(1000);

            builder.Property(x => x.PreferredCategoryIds)
                .HasConversion(
                    v => string.Join(",", v),
                    v => v.Split(",", System.StringSplitOptions.RemoveEmptyEntries)
                        .Select(long.Parse)
                        .ToList())
                .HasMaxLength(1000);

            builder.Property(x => x.PreferredPriceMin)
                .HasPrecision(18, 2);

            builder.Property(x => x.PreferredPriceMax)
                .HasPrecision(18, 2);

            builder.Property(x => x.RecentlyViewed)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<long, DateTimeOffset>>(v, (System.Text.Json.JsonSerializerOptions?)null) 
                        ?? new Dictionary<long, DateTimeOffset>())
                .HasColumnType("jsonb");

            builder.Property(x => x.LastUpdatedAtUtc)
                .IsRequired();

            builder.Property(x => x.LastPersonalizationAtUtc)
                .IsRequired(false);

            // Indexes za performance
            builder.HasIndex(x => x.UserId)
                .IsUnique()
                .HasDatabaseName("IX_user_profiles_userid");

            builder.HasIndex(x => x.LastUpdatedAtUtc)
                .HasDatabaseName("IX_user_profiles_lastupdatedat");

            builder.HasIndex(x => x.LastPersonalizationAtUtc)
                .HasDatabaseName("IX_user_profiles_lastpersonalizationat");
        }
    }
}
