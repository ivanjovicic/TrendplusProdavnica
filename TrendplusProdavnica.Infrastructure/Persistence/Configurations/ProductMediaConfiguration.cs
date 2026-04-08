#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrendplusProdavnica.Domain.Catalog;
using TrendplusProdavnica.Domain.Enums;

namespace TrendplusProdavnica.Infrastructure.Persistence.Configurations
{
    public class ProductMediaConfiguration : IEntityTypeConfiguration<ProductMedia>
    {
        public void Configure(EntityTypeBuilder<ProductMedia> builder)
        {
            builder.ToTable("product_media", "catalog");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Url).IsRequired();
            builder.Property(x => x.MobileUrl).HasMaxLength(500);
            builder.Property(x => x.AltText).HasMaxLength(200);
            builder.Property(x => x.Title).HasMaxLength(160);

            builder.Property(x => x.MediaType).HasConversion<short>();
            builder.Property(x => x.MediaRole).HasConversion<short>();

            builder.Property(x => x.SortOrder).HasDefaultValue(0);
            builder.Property(x => x.IsPrimary).HasDefaultValue(false);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasIndex(e => new { e.ProductId, e.SortOrder, e.Id }).HasDatabaseName("ix_product_media_product_sort_id");
            builder.HasIndex(e => new { e.ProductId, e.MediaRole, e.IsActive, e.SortOrder }).HasDatabaseName("ix_product_media_role_active_sort");

            // partial unique index for primary image: created in migration to include filter "is_primary = true AND is_active = true"
        }
    }
}
