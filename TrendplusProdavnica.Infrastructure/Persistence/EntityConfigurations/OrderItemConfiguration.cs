using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrendplusProdavnica.Domain.Sales;

namespace TrendplusProdavnica.Infrastructure.Persistence.EntityConfigurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items", "sales");

        builder.HasKey(oi => oi.Id);

        // Foreign key
        builder.HasIndex(oi => oi.OrderId)
            .HasDatabaseName("ix_order_items_order_id");

        // Properties
        builder.Property(oi => oi.ProductNameSnapshot)
            .IsRequired()
            .HasMaxLength(180);

        builder.Property(oi => oi.BrandNameSnapshot)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(oi => oi.SizeEuSnapshot)
            .HasPrecision(4, 1);

        // Decimal precision for money
        builder.Property(oi => oi.UnitPrice)
            .HasPrecision(12, 2);

        builder.Property(oi => oi.LineTotal)
            .HasPrecision(12, 2);
    }
}
