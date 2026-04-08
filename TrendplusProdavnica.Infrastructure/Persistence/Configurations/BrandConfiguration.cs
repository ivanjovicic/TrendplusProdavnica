#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrendplusProdavnica.Domain.Catalog;

namespace TrendplusProdavnica.Infrastructure.Persistence.Configurations
{
    public class BrandConfiguration : IEntityTypeConfiguration<Brand>
    {
        public void Configure(EntityTypeBuilder<Brand> builder)
        {
            builder.ToTable("brands", "catalog");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(120);
            builder.Property(x => x.Slug).IsRequired().HasMaxLength(140);
            builder.Property(x => x.ShortDescription).HasMaxLength(1000);
            builder.Property(x => x.LongDescription).HasMaxLength(4000);
            builder.Property(x => x.LogoUrl).HasMaxLength(500);
            builder.Property(x => x.CoverImageUrl).HasMaxLength(500);
            builder.Property(x => x.WebsiteUrl).HasMaxLength(500);
            builder.Property(x => x.IsFeatured).HasDefaultValue(false);
            builder.Property(x => x.IsActive).HasDefaultValue(true);
            builder.Property(x => x.SortOrder).HasDefaultValue(0);

            builder.HasIndex(x => x.Slug).IsUnique().HasDatabaseName("ux_brands_slug");
        }
    }
}
