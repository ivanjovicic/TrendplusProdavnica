#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrendplusProdavnica.Domain.Catalog;

namespace TrendplusProdavnica.Infrastructure.Persistence.Configurations
{
    public class ProductRelatedProductConfiguration : IEntityTypeConfiguration<ProductRelatedProduct>
    {
        public void Configure(EntityTypeBuilder<ProductRelatedProduct> builder)
        {
            builder.ToTable("product_related_products", "catalog");
            builder.HasKey(x => new { x.ProductId, x.RelatedProductId, x.RelationType });

            builder.Property(x => x.SortOrder).HasDefaultValue(0);

            builder.HasOne<Product>()
                .WithMany(p => p.RelatedProducts)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Product>()
                .WithMany()
                .HasForeignKey(x => x.RelatedProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
