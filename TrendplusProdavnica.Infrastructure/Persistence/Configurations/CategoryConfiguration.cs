#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrendplusProdavnica.Domain.Catalog;
using TrendplusProdavnica.Domain.Enums;

namespace TrendplusProdavnica.Infrastructure.Persistence.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("categories", "catalog");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(120);
            builder.Property(x => x.Slug).IsRequired().HasMaxLength(140);
            builder.Property(x => x.MenuLabel).HasMaxLength(80);
            builder.Property(x => x.ShortDescription).HasMaxLength(1000);
            builder.Property(x => x.ImageUrl).HasMaxLength(500);
            builder.Property(x => x.SortOrder).HasDefaultValue(0);
            builder.Property(x => x.Depth).HasDefaultValue((short)0);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            // enum -> smallint
            builder.Property(x => x.Type).HasConversion<short>();

            builder.HasIndex(x => x.Slug).IsUnique().HasDatabaseName("ux_categories_slug");

            builder.HasIndex(e => new { e.ParentId, e.SortOrder, e.Id })
                .HasDatabaseName("ix_categories_parent_sort_id");

            builder.HasIndex(e => new { e.IsActive, e.SortOrder, e.Id })
                .HasDatabaseName("ix_categories_active_sort_id")
                .IncludeProperties(x => new { x.Name, x.Slug, x.MenuLabel });

            builder.HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Concurrency token mapping to xmin (configured individually on aggregate roots by convention)
            builder.Property<uint>("Version").HasColumnName("xmin").IsRowVersion();
        }
    }
}
