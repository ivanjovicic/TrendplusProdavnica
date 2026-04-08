#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrendplusProdavnica.Domain.Catalog;

namespace TrendplusProdavnica.Infrastructure.Persistence.Configurations
{
    public class ProductCategoryMapConfiguration : IEntityTypeConfiguration<ProductCategoryMap>
    {
        public void Configure(EntityTypeBuilder<ProductCategoryMap> builder)
        {
            builder.ToTable("product_category_map", "catalog");
            builder.HasKey(x => new { x.ProductId, x.CategoryId });

            builder.Property(x => x.SortOrder).HasDefaultValue(0);

            builder.HasOne<Product>()
                .WithMany(p => p.CategoryMaps)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Category>()
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
