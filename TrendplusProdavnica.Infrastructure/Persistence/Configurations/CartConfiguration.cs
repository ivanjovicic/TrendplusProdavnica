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

            builder.Property(x => x.UserId)
                .HasMaxLength(128);

            builder.Property(x => x.SessionId)
                .HasMaxLength(128);

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

            builder.HasIndex(x => x.SessionId)
                .HasDatabaseName("ix_carts_session_id");

            builder.HasIndex(x => x.UserId)
                .HasDatabaseName("ix_carts_user_id");

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
