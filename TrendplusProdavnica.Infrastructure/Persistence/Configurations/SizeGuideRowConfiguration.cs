#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrendplusProdavnica.Domain.Catalog;

namespace TrendplusProdavnica.Infrastructure.Persistence.Configurations
{
    public class SizeGuideRowConfiguration : IEntityTypeConfiguration<SizeGuideRow>
    {
        public void Configure(EntityTypeBuilder<SizeGuideRow> builder)
        {
            builder.ToTable("size_guide_rows", "catalog");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.SizeGuideId).IsRequired();
            builder.Property(x => x.EuSize).HasPrecision(4,1);
            builder.Property(x => x.FootLengthMinMm).HasPrecision(10,2);
            builder.Property(x => x.FootLengthMaxMm).HasPrecision(10,2);
            builder.Property(x => x.Note).HasMaxLength(120);
            builder.Property(x => x.SortOrder).HasDefaultValue(0);

            builder.HasIndex(x => new { x.SizeGuideId, x.EuSize }).IsUnique().HasDatabaseName("ux_size_guide_rows_eu_size");
        }
    }
}
