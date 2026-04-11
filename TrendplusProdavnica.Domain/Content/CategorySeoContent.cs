#nullable enable
using System;
using System.Collections.Generic;
using TrendplusProdavnica.Domain.Common;
using TrendplusProdavnica.Domain.ValueObjects;

namespace TrendplusProdavnica.Domain.Content
{
    /// <summary>
    /// SEO landing stranica za kategoriju sa FAQ
    /// </summary>
    public class CategorySeoContent : EntityBase
    {
        public long CategoryId { get; set; }

        // SEO Meta
        public string MetaTitle { get; set; } = string.Empty;
        public string MetaDescription { get; set; } = string.Empty;

        // Landing Content
        public string? IntroTitle { get; set; }
        public string? IntroText { get; set; }
        public string? MainContent { get; set; }

        // FAQ
        public IEnumerable<FaqItem>? Faq { get; set; }

        // Publikovanje
        public bool IsPublished { get; set; }
        public DateTimeOffset PublishedAtUtc { get; set; }

        public CategorySeoContent()
        {
        }

        public CategorySeoContent(long categoryId, string metaTitle, string metaDescription)
        {
            CategoryId = categoryId;
            MetaTitle = metaTitle;
            MetaDescription = metaDescription;
            IsPublished = false;
            PublishedAtUtc = DateTimeOffset.UtcNow;
        }

        public void Publish()
        {
            IsPublished = true;
            PublishedAtUtc = DateTimeOffset.UtcNow;
        }

        public void Unpublish()
        {
            IsPublished = false;
        }
    }
}
