#nullable enable
using System;
using System.Collections.Generic;
using TrendplusProdavnica.Domain.Common;
using TrendplusProdavnica.Domain.ValueObjects;
using TrendplusProdavnica.Domain.Enums;

namespace TrendplusProdavnica.Domain.Content
{
    public class EditorialArticle : AggregateRoot
    {
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Excerpt { get; set; } = string.Empty;
        public string? CoverImageUrl { get; set; }
        public string Body { get; set; } = string.Empty; // JSON or HTML depending on implementation
        public string? Topic { get; set; }
        public string? AuthorName { get; set; }
        public int? ReadingTimeMinutes { get; set; }
        public ContentStatus Status { get; set; } = ContentStatus.Draft;
        public DateTimeOffset? PublishedAtUtc { get; set; }
        public SeoMetadata? Seo { get; set; }

        public IList<EditorialArticleProduct> Products { get; } = new List<EditorialArticleProduct>();
        public IList<EditorialArticleCategory> Categories { get; } = new List<EditorialArticleCategory>();
        public IList<EditorialArticleBrand> Brands { get; } = new List<EditorialArticleBrand>();
        public IList<EditorialArticleCollection> Collections { get; } = new List<EditorialArticleCollection>();
    }
}
