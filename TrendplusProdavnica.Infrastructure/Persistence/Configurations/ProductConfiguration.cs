#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrendplusProdavnica.Domain.Catalog;
using TrendplusProdavnica.Domain.Enums;

namespace TrendplusProdavnica.Infrastructure.Persistence.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("products", "catalog");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(180);
            builder.Property(x => x.Slug).IsRequired().HasMaxLength(220);
            builder.Property(x => x.Subtitle).HasMaxLength(180);
            builder.Property(x => x.ShortDescription).IsRequired();
            builder.Property(x => x.PrimaryColorName).HasMaxLength(80);
            builder.Property(x => x.StyleTag).HasMaxLength(80);
            builder.Property(x => x.OccasionTag).HasMaxLength(80);
            builder.Property(x => x.SeasonTag).HasMaxLength(80);

            builder.Property(x => x.Status).HasConversion<short>();

            builder.Property(x => x.IsVisible).HasDefaultValue(true);
            builder.Property(x => x.IsPurchasable).HasDefaultValue(true);
            builder.Property(x => x.IsNew).HasDefaultValue(false);
            builder.Property(x => x.IsBestseller).HasDefaultValue(false);
            builder.Property(x => x.SortRank).HasDefaultValue(0);

            // Search vector column - provider-specific; create as tsvector via migration
            // Note: This is handled at migration time for PostgreSQL full-text search
            // builder.Property<string?>("search_vector").HasColumnType("tsvector");

            // Unique slug
            builder.HasIndex(x => x.Slug).IsUnique().HasDatabaseName("ux_products_slug");
            builder.HasIndex(x => x.BrandId).HasDatabaseName("ix_products_brand_id");
            builder.HasIndex(x => x.PrimaryCategoryId).HasDatabaseName("ix_products_primary_category_id");

            // Partial/live indexes: defined via migration for precise filters and includes

            // Concurrency token -> xmin
            builder.Property<uint>("Version").HasColumnName("xmin").IsRowVersion();
        }
    }
}
