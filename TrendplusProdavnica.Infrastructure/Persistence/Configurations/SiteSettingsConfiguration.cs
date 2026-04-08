#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrendplusProdavnica.Domain.Content;
using TrendplusProdavnica.Domain.ValueObjects;

namespace TrendplusProdavnica.Infrastructure.Persistence.Configurations
{
    public class SiteSettingsConfiguration : IEntityTypeConfiguration<SiteSettings>
    {
        public void Configure(EntityTypeBuilder<SiteSettings> builder)
        {
            builder.ToTable("site_settings", "content");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.SiteName).IsRequired().HasMaxLength(120);
            builder.Property(x => x.DefaultSeoTitleSuffix).HasMaxLength(120);
            builder.Property(x => x.DefaultOgImageUrl).HasMaxLength(500);
            builder.Property(x => x.SupportEmail).HasMaxLength(160);
            builder.Property(x => x.SupportPhone).HasMaxLength(40);

            // JSONB fields
            builder.Property(x => x.SocialLinks)
                .HasConversion(new JsonValueConverter<IEnumerable<TrendplusProdavnica.Domain.ValueObjects.SocialLink>>())
                .HasColumnType("jsonb");
            builder.Property(x => x.ContactInfo)
                .HasConversion(new JsonValueConverter<TrendplusProdavnica.Domain.ValueObjects.ContactInfo>())
                .HasColumnType("jsonb");
            builder.Property(x => x.AnalyticsSettings).HasColumnType("jsonb");
        }
    }
}
