#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrendplusProdavnica.Domain.Sales;

namespace TrendplusProdavnica.Infrastructure.Persistence.Configurations
{
    public class CartConfiguration : IEntityTypeConfiguration<Cart>
    {
        public void Configure(EntityTypeBuilder<Cart> builder)
        {
            builder.ToTable("carts", "sales");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.CartToken)
                .IsRequired()
                .HasMaxLength(36);

            builder.Property(x => x.Status)
                .HasConversion<short>();

            builder.Property(x => x.Currency)
                .HasMaxLength(3)
                .HasDefaultValue("RSD");

            // Indexes
            builder.HasIndex(x => x.CartToken)
                .IsUnique()
                .HasDatabaseName("ux_carts_cart_token");

            builder.HasIndex(x => x.Status)
                .HasDatabaseName("ix_carts_status");

            builder.HasIndex(x => x.ExpiresAtUtc)
                .HasDatabaseName("ix_carts_expires_at_utc");

            // Navigation
            builder.HasMany(x => x.Items)
                .WithOne()
                .HasForeignKey(x => x.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // Concurrency on aggregate root
            builder.Property<uint>("Version").HasColumnName("xmin").IsRowVersion();
        }
    }
}
