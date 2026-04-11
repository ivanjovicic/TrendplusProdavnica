#nullable enable
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrendplusProdavnica.Domain.Content;
using TrendplusProdavnica.Domain.ValueObjects;

namespace TrendplusProdavnica.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// EF Core konfiguracija za CategorySeoContent entitet
    /// </summary>
    public class CategorySeoContentConfiguration : IEntityTypeConfiguration<CategorySeoContent>
    {
        public void Configure(EntityTypeBuilder<CategorySeoContent> builder)
        {
            builder.ToTable("category_seo_content", "content");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CategoryId)
                .IsRequired();

            builder.Property(x => x.MetaTitle)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(x => x.MetaDescription)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.IntroTitle)
                .HasMaxLength(256);

            builder.Property(x => x.IntroText)
                .HasColumnType("text");

            builder.Property(x => x.MainContent)
                .HasColumnType("text");

            // FAQ stored as JSONB
            builder.Property(x => x.Faq)
                .HasConversion(new JsonValueConverter<IEnumerable<FaqItem>>())
                .HasColumnType("jsonb");

            builder.Property(x => x.IsPublished)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.PublishedAtUtc)
                .IsRequired();

            // Indexes for performance
            builder.HasIndex(x => x.CategoryId)
                .IsUnique()
                .HasDatabaseName("IX_category_seo_content_categoryid");

            builder.HasIndex(x => new { x.IsPublished, x.PublishedAtUtc })
                .HasDatabaseName("IX_category_seo_content_ispublished_publisheda");
        }
    }
}
