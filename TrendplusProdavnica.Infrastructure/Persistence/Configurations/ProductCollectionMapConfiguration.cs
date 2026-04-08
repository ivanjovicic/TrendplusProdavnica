#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrendplusProdavnica.Domain.Catalog;

namespace TrendplusProdavnica.Infrastructure.Persistence.Configurations
{
    public class ProductCollectionMapConfiguration : IEntityTypeConfiguration<ProductCollectionMap>
    {
        public void Configure(EntityTypeBuilder<ProductCollectionMap> builder)
        {
            builder.ToTable("product_collection_map", "catalog");
            builder.HasKey(x => new { x.ProductId, x.CollectionId });

            builder.Property(x => x.SortOrder).HasDefaultValue(0);
            builder.Property(x => x.Pinned).HasDefaultValue(false);
            builder.Property(x => x.MerchandisingScore).HasPrecision(9,4);

            builder.HasOne<Product>()
                .WithMany(p => p.CollectionMaps)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Collection>()
                .WithMany(c => c.ProductMaps)
                .HasForeignKey(x => x.CollectionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
