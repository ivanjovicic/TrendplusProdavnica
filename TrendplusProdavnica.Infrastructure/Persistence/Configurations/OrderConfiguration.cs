#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrendplusProdavnica.Domain.Sales;

namespace TrendplusProdavnica.Infrastructure.Persistence.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("orders", "sales");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.OrderNumber)
                .IsRequired()
                .HasMaxLength(32);

            builder.Property(x => x.CheckoutIdempotencyKey)
                .HasMaxLength(128);

            builder.Property(x => x.Status)
                .HasConversion<short>();

            builder.Property(x => x.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValue("RSD");

            builder.Property(x => x.CustomerFirstName)
                .IsRequired()
                .HasMaxLength(120);

            builder.Property(x => x.CustomerLastName)
                .IsRequired()
                .HasMaxLength(120);

            builder.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(320);

            builder.Property(x => x.Phone)
                .IsRequired()
                .HasMaxLength(40);

            builder.Property(x => x.DeliveryAddressLine1)
                .IsRequired()
                .HasMaxLength(240);

            builder.Property(x => x.DeliveryAddressLine2)
                .HasMaxLength(240);

            builder.Property(x => x.DeliveryCity)
                .IsRequired()
                .HasMaxLength(120);

            builder.Property(x => x.DeliveryPostalCode)
                .IsRequired()
                .HasMaxLength(32);

            builder.Property(x => x.Note)
                .HasMaxLength(2000);

            builder.Property(x => x.SubtotalAmount).HasPrecision(12, 2);
            builder.Property(x => x.DeliveryAmount).HasPrecision(12, 2);
            builder.Property(x => x.TotalAmount).HasPrecision(12, 2);

            builder.HasIndex(x => x.OrderNumber)
                .IsUnique()
                .HasDatabaseName("ux_orders_order_number");

            builder.HasIndex(x => x.CartId)
                .IsUnique()
                .HasFilter("\"cart_id\" IS NOT NULL")
                .HasDatabaseName("ux_orders_cart_id");

            builder.HasIndex(x => x.CheckoutIdempotencyKey)
                .IsUnique()
                .HasFilter("\"checkout_idempotency_key\" IS NOT NULL")
                .HasDatabaseName("ux_orders_checkout_idempotency_key");

            builder.HasMany(x => x.Items)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
