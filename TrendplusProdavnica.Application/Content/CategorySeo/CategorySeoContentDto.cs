#nullable enable
using System;
using System.Collections.Generic;
using TrendplusProdavnica.Domain.ValueObjects;

namespace TrendplusProdavnica.Application.Content.CategorySeo
{
    /// <summary>
    /// DTO za SEO landing stranicu kategorije
    /// </summary>
    public class CategorySeoContentDto
    {
        public long Id { get; set; }
        public long CategoryId { get; set; }
        public string MetaTitle { get; set; } = string.Empty;
        public string MetaDescription { get; set; } = string.Empty;
        public string? IntroTitle { get; set; }
        public string? IntroText { get; set; }
        public string? MainContent { get; set; }
        public IEnumerable<FaqItem>? Faq { get; set; }
        public bool IsPublished { get; set; }
        public DateTime PublishedAtUtc { get; set; }
    }

    /// <summary>
    /// Request za kreiranje SEO stranice
    /// </summary>
    public class CreateCategorySeoContentRequest
    {
        public long CategoryId { get; set; }
        public string MetaTitle { get; set; } = string.Empty;
        public string MetaDescription { get; set; } = string.Empty;
        public string? IntroTitle { get; set; }
        public string? IntroText { get; set; }
        public string? MainContent { get; set; }
        public IEnumerable<FaqItem>? Faq { get; set; }
    }

    /// <summary>
    /// Request za ažuriranje SEO stranice
    /// </summary>
    public class UpdateCategorySeoContentRequest
    {
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? IntroTitle { get; set; }
        public string? IntroText { get; set; }
        public string? MainContent { get; set; }
        public IEnumerable<FaqItem>? Faq { get; set; }
    }

    /// <summary>
    /// Request za publikovanje
    /// </summary>
    public class PublishCategorySeoContentRequest
    {
        public bool IsPublished { get; set; }
    }
}
