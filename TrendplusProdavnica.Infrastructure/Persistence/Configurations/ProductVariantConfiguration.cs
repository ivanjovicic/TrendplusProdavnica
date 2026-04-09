#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrendplusProdavnica.Domain.Catalog;

namespace TrendplusProdavnica.Infrastructure.Persistence.Configurations
{
    public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
    {
        public void Configure(EntityTypeBuilder<ProductVariant> builder)
        {
            builder.ToTable("product_variants", "catalog");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Sku).IsRequired().HasMaxLength(80);
            builder.Property(x => x.Barcode).HasMaxLength(64);
            builder.Property(x => x.SizeEu).HasPrecision(4, 1);
            builder.Property(x => x.Price).HasPrecision(12, 2).IsRequired();
            builder.Property(x => x.OldPrice).HasPrecision(12, 2);
            builder.Property(x => x.Currency).HasMaxLength(3).HasDefaultValue("RSD");
            builder.Property(x => x.TotalStock).HasDefaultValue(0);
            builder.Property(x => x.LowStockThreshold).HasDefaultValue(2);
            builder.Property(x => x.IsActive).HasDefaultValue(true);
            builder.Property(x => x.IsVisible).HasDefaultValue(true);

            builder.HasIndex(x => x.Sku).IsUnique().HasDatabaseName("ux_product_variants_sku");
            builder.HasIndex(x => x.ProductId).HasDatabaseName("ix_product_variant_product_id");
            builder.HasIndex(x => x.Price).HasDatabaseName("ix_product_variants_price");

            // Partial in-stock index created in migration for WHERE total_stock > 0 AND is_active = true AND is_visible = true

            // Concurrency on aggregate root
            builder.Property<uint>("Version").HasColumnName("xmin").IsRowVersion();
        }
    }
}
