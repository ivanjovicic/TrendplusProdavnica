using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrendplusProdavnica.Domain.Sales;

namespace TrendplusProdavnica.Infrastructure.Persistence.EntityConfigurations
{
    public class WishlistConfiguration : IEntityTypeConfiguration<Wishlist>
    {
        public void Configure(EntityTypeBuilder<Wishlist> builder)
        {
            builder.ToTable("wishlists", schema: "sales");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.WishlistToken)
                .IsRequired()
                .HasMaxLength(64);

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.Property(x => x.UpdatedAtUtc)
                .IsRequired();

            // Indices
            builder.HasIndex(x => x.WishlistToken)
                .IsUnique();

            // Relationships
            builder.HasMany(x => x.Items)
                .WithOne(x => x.Wishlist)
                .HasForeignKey(x => x.WishlistId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
