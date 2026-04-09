using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrendplusProdavnica.Domain.Sales;

namespace TrendplusProdavnica.Infrastructure.Persistence.EntityConfigurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders", "sales");

        builder.HasKey(o => o.Id);

        // Natural key
        builder.HasIndex(o => o.OrderNumber)
            .IsUnique()
            .HasDatabaseName("ux_orders_order_number");

        // Properties
        builder.Property(o => o.OrderNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(o => o.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("RSD");

        builder.Property(o => o.CustomerFirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.CustomerLastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.Email)
            .IsRequired()
            .HasMaxLength(160);

        builder.Property(o => o.Phone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(o => o.DeliveryAddressLine1)
            .IsRequired()
            .HasMaxLength(180);

        builder.Property(o => o.DeliveryAddressLine2)
            .HasMaxLength(180);

        builder.Property(o => o.DeliveryCity)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.DeliveryPostalCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(o => o.Note)
            .HasMaxLength(1000);

        // Decimal precision for money
        builder.Property(o => o.SubtotalAmount)
            .HasPrecision(12, 2);

        builder.Property(o => o.DeliveryAmount)
            .HasPrecision(12, 2);

        builder.Property(o => o.TotalAmount)
            .HasPrecision(12, 2);

        // Enums
        builder.Property(o => o.Status)
            .HasConversion<short>();

        builder.Property(o => o.DeliveryMethod)
            .HasConversion<short>();

        builder.Property(o => o.PaymentMethod)
            .HasConversion<short>();

        // Relationships
        builder.HasMany(o => o.Items)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Audit
        builder.Property(o => o.CreatedAtUtc)
            .ValueGeneratedOnAdd();

        builder.Property(o => o.UpdatedAtUtc)
            .ValueGeneratedOnAddOrUpdate();
    }
}
