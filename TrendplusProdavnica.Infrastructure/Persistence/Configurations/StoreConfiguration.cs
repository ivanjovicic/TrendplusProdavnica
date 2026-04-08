#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrendplusProdavnica.Domain.Inventory;

namespace TrendplusProdavnica.Infrastructure.Persistence.Configurations
{
    public class StoreConfiguration : IEntityTypeConfiguration<Store>
    {
        public void Configure(EntityTypeBuilder<Store> builder)
        {
            builder.ToTable("stores", "inventory");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(160);
            builder.Property(x => x.Slug).IsRequired().HasMaxLength(180);
            builder.Property(x => x.City).IsRequired().HasMaxLength(100);
            builder.Property(x => x.AddressLine1).IsRequired().HasMaxLength(180);
            builder.Property(x => x.AddressLine2).HasMaxLength(180);
            builder.Property(x => x.PostalCode).HasMaxLength(20);
            builder.Property(x => x.MallName).HasMaxLength(120);
            builder.Property(x => x.Phone).HasMaxLength(40);
            builder.Property(x => x.Email).HasMaxLength(160);
            builder.Property(x => x.Latitude).HasPrecision(9,6);
            builder.Property(x => x.Longitude).HasPrecision(9,6);
            builder.Property(x => x.IsActive).HasDefaultValue(true);
            builder.Property(x => x.SortOrder).HasDefaultValue(0);

            builder.HasIndex(x => x.Slug).IsUnique().HasDatabaseName("ux_stores_slug");
        }
    }
}
