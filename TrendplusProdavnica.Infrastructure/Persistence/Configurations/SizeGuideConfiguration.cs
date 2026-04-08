#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrendplusProdavnica.Domain.Catalog;

namespace TrendplusProdavnica.Infrastructure.Persistence.Configurations
{
    public class SizeGuideConfiguration : IEntityTypeConfiguration<SizeGuide>
    {
        public void Configure(EntityTypeBuilder<SizeGuide> builder)
        {
            builder.ToTable("size_guides", "catalog");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.BrandId).IsRequired(false);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(120);
            builder.Property(x => x.Slug).IsRequired().HasMaxLength(140);
            builder.Property(x => x.Description).HasMaxLength(1000);
            builder.Property(x => x.IsDefault).HasDefaultValue(false);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasIndex(x => x.Slug).IsUnique().HasDatabaseName("ux_size_guides_slug");
        }
    }
}
