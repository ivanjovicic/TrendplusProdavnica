#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrendplusProdavnica.Domain.Sales;
using TrendplusProdavnica.Domain.Catalog;

namespace TrendplusProdavnica.Infrastructure.Persistence.Configurations
{
    public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
    {
        public void Configure(EntityTypeBuilder<CartItem> builder)
        {
            builder.ToTable("cart_items", "sales", t =>
            {
                t.HasCheckConstraint("ck_cart_items_quantity", "quantity > 0");
                t.HasCheckConstraint("ck_cart_items_unit_price", "unit_price > 0");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.CartId)
                .IsRequired();

            builder.Property(x => x.ProductVariantId)
                .IsRequired();

            builder.Property(x => x.Quantity)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(x => x.UnitPrice)
                .HasPrecision(12, 2)
                .IsRequired();

            // Indexes
            builder.HasIndex(x => x.CartId)
                .HasDatabaseName("ix_cart_items_cart_id");

            builder.HasIndex(x => x.ProductVariantId)
                .HasDatabaseName("ix_cart_items_product_variant_id");

            // Unique constraint: one variant per cart
            builder.HasIndex(x => new { x.CartId, x.ProductVariantId })
                .IsUnique()
                .HasDatabaseName("ux_cart_items_cart_id_product_variant_id");

            // Foreign key to ProductVariant with restrict
            // Note: ProductVariant navigation not mapped as it's a catalog domain entity
            builder.HasOne<TrendplusProdavnica.Domain.Catalog.ProductVariant>()
                .WithMany()
                .HasForeignKey(x => x.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
