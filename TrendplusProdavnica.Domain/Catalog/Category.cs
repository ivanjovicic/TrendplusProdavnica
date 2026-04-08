#nullable enable
using System.Collections.Generic;
using TrendplusProdavnica.Domain.Common;
using TrendplusProdavnica.Domain.ValueObjects;
using TrendplusProdavnica.Domain.Enums;

namespace TrendplusProdavnica.Domain.Catalog
{
    public class Category : AggregateRoot
    {
        public long? ParentId { get; set; }
        public Category? Parent { get; set; }
        public IList<Category> Children { get; } = new List<Category>();

        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? MenuLabel { get; set; }
        public string? ShortDescription { get; set; }
        public string? ImageUrl { get; set; }
        public int SortOrder { get; set; }
        public short Depth { get; set; }
        public bool IsActive { get; set; } = true;
        public SeoMetadata? Seo { get; set; }
        public CategoryType Type { get; set; } = CategoryType.Root;
    }
}
