#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrendplusProdavnica.Domain.Content;
using TrendplusProdavnica.Domain.ValueObjects;

namespace TrendplusProdavnica.Infrastructure.Persistence.Configurations
{
    public class HomePageConfiguration : IEntityTypeConfiguration<HomePage>
    {
        public void Configure(EntityTypeBuilder<HomePage> builder)
        {
            builder.ToTable("home_pages", "content");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title).IsRequired().HasMaxLength(160);
            builder.Property(x => x.Slug).IsRequired().HasMaxLength(40).HasDefaultValue("/");
            builder.Property(x => x.IsPublished).HasDefaultValue(false);

            // Modules is a JSON payload
            builder.Property(x => x.Modules)
                .HasConversion(JsonValueConverter<IEnumerable<HomeModule>>.NonNullable())
                .HasColumnType("jsonb");

            // Partial unique index for single published homepage - created in migration
        }
    }
}
