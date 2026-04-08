#nullable enable
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrendplusProdavnica.Domain.Content;
using TrendplusProdavnica.Domain.ValueObjects;

namespace TrendplusProdavnica.Infrastructure.Persistence.Configurations
{
    public class SalePageConfiguration : IEntityTypeConfiguration<SalePage>
    {
        public void Configure(EntityTypeBuilder<SalePage> builder)
        {
            builder.ToTable("sale_pages", "content");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Faq)
                .HasConversion(new JsonValueConverter<IEnumerable<FaqItem>>())
                .HasColumnType("jsonb");
        }
    }
}
