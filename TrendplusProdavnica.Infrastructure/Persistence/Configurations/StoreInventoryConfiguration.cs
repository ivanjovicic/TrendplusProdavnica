#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrendplusProdavnica.Domain.Inventory;

namespace TrendplusProdavnica.Infrastructure.Persistence.Configurations
{
    public class StoreInventoryConfiguration : IEntityTypeConfiguration<StoreInventory>
    {
        public void Configure(EntityTypeBuilder<StoreInventory> builder)
        {
            builder.ToTable("store_inventory", "inventory");
            builder.HasKey(x => new { x.StoreId, x.VariantId });

            builder.Property(x => x.QuantityOnHand).HasDefaultValue(0);
            builder.Property(x => x.ReservedQuantity).HasDefaultValue(0);
            builder.Property(x => x.UpdatedAtUtc).IsRequired();

            builder.HasOne<Store>()
                .WithMany(s => s.Inventory)
                .HasForeignKey(x => x.StoreId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<TrendplusProdavnica.Domain.Catalog.ProductVariant>()
                .WithMany()
                .HasForeignKey(x => x.VariantId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
