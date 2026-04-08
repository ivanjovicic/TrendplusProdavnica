#nullable enable
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrendplusProdavnica.Domain.Content;
using TrendplusProdavnica.Domain.ValueObjects;

namespace TrendplusProdavnica.Infrastructure.Persistence.Configurations
{
    public class StorePageContentConfiguration : IEntityTypeConfiguration<StorePageContent>
    {
        public void Configure(EntityTypeBuilder<StorePageContent> builder)
        {
            builder.ToTable("store_page_contents", "content");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Faq)
                .HasConversion(new JsonValueConverter<IEnumerable<FaqItem>>())
                .HasColumnType("jsonb");
            builder.Property(x => x.FeaturedLinks)
                .HasConversion(new JsonValueConverter<IEnumerable<FeaturedLink>>())
                .HasColumnType("jsonb");
            builder.Property(x => x.MerchBlocks)
                .HasConversion(new JsonValueConverter<IEnumerable<MerchBlock>>())
                .HasColumnType("jsonb");
        }
    }
}
