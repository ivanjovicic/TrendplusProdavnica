#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrendplusProdavnica.Domain.Pricing;

namespace TrendplusProdavnica.Infrastructure.Persistence.Configurations
{
    public class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
    {
        public void Configure(EntityTypeBuilder<Promotion> builder)
        {
            builder.ToTable("promotions", "pricing");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(160);
            builder.Property(x => x.Code).HasMaxLength(60);
            builder.Property(x => x.DiscountValue).HasPrecision(12, 2);
            builder.Property<short>("DiscountType").HasConversion<short>();
            builder.Property(x => x.BadgeText).HasMaxLength(40);
            builder.Property(x => x.Priority).HasDefaultValue((short)0);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            // unique partial on code where code is not null -> create in migration
        }
    }
}
