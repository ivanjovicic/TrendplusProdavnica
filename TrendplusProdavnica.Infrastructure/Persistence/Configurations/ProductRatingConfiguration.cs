#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrendplusProdavnica.Domain.Catalog;

namespace TrendplusProdavnica.Infrastructure.Persistence.Configurations
{
    public sealed class ProductRatingConfiguration : IEntityTypeConfiguration<ProductRating>
    {
        public void Configure(EntityTypeBuilder<ProductRating> builder)
        {
            builder.ToTable("product_ratings", "catalog");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.AverageRating).HasPrecision(3, 2).HasDefaultValue(0m);
            builder.Property(x => x.ReviewCount).HasDefaultValue(0);
            builder.Property(x => x.RatingCount).HasDefaultValue(0);
            builder.Property(x => x.OneStarCount).HasDefaultValue(0);
            builder.Property(x => x.TwoStarCount).HasDefaultValue(0);
            builder.Property(x => x.ThreeStarCount).HasDefaultValue(0);
            builder.Property(x => x.FourStarCount).HasDefaultValue(0);
            builder.Property(x => x.FiveStarCount).HasDefaultValue(0);

            builder.HasIndex(x => x.ProductId)
                .IsUnique()
                .HasDatabaseName("ux_product_ratings_product_id");

            builder.HasOne(x => x.Product)
                .WithOne(x => x.Rating)
                .HasForeignKey<ProductRating>(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
