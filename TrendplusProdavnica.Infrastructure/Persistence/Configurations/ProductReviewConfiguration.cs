#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrendplusProdavnica.Domain.Catalog;

namespace TrendplusProdavnica.Infrastructure.Persistence.Configurations
{
    public sealed class ProductReviewConfiguration : IEntityTypeConfiguration<ProductReview>
    {
        public void Configure(EntityTypeBuilder<ProductReview> builder)
        {
            builder.ToTable("product_reviews", "catalog");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ExternalKey).HasMaxLength(120);
            builder.Property(x => x.AuthorName).IsRequired().HasMaxLength(120);
            builder.Property(x => x.Title).HasMaxLength(180);
            builder.Property(x => x.ReviewBody).HasMaxLength(4000);
            builder.Property(x => x.RatingValue).HasPrecision(2, 1).IsRequired();
            builder.Property(x => x.Status).HasConversion<short>();
            builder.Property(x => x.IsVerifiedPurchase).HasDefaultValue(false);

            builder.HasIndex(x => x.ExternalKey)
                .IsUnique()
                .HasDatabaseName("ux_product_reviews_external_key");

            builder.HasIndex(x => new { x.ProductId, x.Status, x.PublishedAtUtc })
                .HasDatabaseName("ix_product_reviews_product_status_published_at");

            builder.HasOne(x => x.Product)
                .WithMany(x => x.Reviews)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
