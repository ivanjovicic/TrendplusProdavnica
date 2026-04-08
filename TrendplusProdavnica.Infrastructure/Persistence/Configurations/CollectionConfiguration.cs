#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrendplusProdavnica.Domain.Catalog;

namespace TrendplusProdavnica.Infrastructure.Persistence.Configurations
{
    public class CollectionConfiguration : IEntityTypeConfiguration<Collection>
    {
        public void Configure(EntityTypeBuilder<Collection> builder)
        {
            builder.ToTable("collections", "catalog");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(140);
            builder.Property(x => x.Slug).IsRequired().HasMaxLength(160);
            builder.Property(x => x.CollectionType).HasConversion<short>();
            builder.Property(x => x.ShortDescription).HasMaxLength(1000);
            builder.Property(x => x.LongDescription).HasMaxLength(4000);
            builder.Property(x => x.CoverImageUrl).HasMaxLength(500);
            builder.Property(x => x.ThumbnailImageUrl).HasMaxLength(500);
            builder.Property(x => x.BadgeText).HasMaxLength(40);
            builder.Property(x => x.IsFeatured).HasDefaultValue(false);
            builder.Property(x => x.IsActive).HasDefaultValue(true);
            builder.Property(x => x.SortOrder).HasDefaultValue(0);

            builder.HasIndex(x => x.Slug).IsUnique().HasDatabaseName("ux_collections_slug");
        }
    }
}
