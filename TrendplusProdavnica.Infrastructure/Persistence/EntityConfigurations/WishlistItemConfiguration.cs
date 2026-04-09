using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrendplusProdavnica.Domain.Sales;

namespace TrendplusProdavnica.Infrastructure.Persistence.EntityConfigurations
{
    public class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
    {
        public void Configure(EntityTypeBuilder<WishlistItem> builder)
        {
            builder.ToTable("wishlist_items", schema: "sales");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.WishlistId)
                .IsRequired();

            builder.Property(x => x.ProductId)
                .IsRequired();

            builder.Property(x => x.AddedAtUtc)
                .IsRequired();

            // Indices
            builder.HasIndex(x => new { x.WishlistId, x.ProductId })
                .IsUnique();

            // Relationships
            builder.HasOne(x => x.Wishlist)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.WishlistId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
