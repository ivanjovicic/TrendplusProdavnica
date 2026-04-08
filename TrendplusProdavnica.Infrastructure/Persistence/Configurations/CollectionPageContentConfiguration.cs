#nullable enable
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrendplusProdavnica.Domain.Content;
using TrendplusProdavnica.Domain.ValueObjects;

namespace TrendplusProdavnica.Infrastructure.Persistence.Configurations
{
    public class CollectionPageContentConfiguration : IEntityTypeConfiguration<CollectionPageContent>
    {
        public void Configure(EntityTypeBuilder<CollectionPageContent> builder)
        {
            builder.ToTable("collection_page_contents", "content");
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
