#nullable enable
using TrendplusProdavnica.Domain.Common;
using TrendplusProdavnica.Domain.Enums;

namespace TrendplusProdavnica.Domain.Catalog
{
    public class ProductMedia : EntityBase
    {
        public long ProductId { get; set; }
        public long? VariantId { get; set; }
        public string Url { get; set; } = string.Empty;
        public string? MobileUrl { get; set; }
        public string? AltText { get; set; }
        public string? Title { get; set; }
        public MediaType MediaType { get; set; } = MediaType.Image;
        public MediaRole MediaRole { get; set; } = MediaRole.Gallery;
        public int SortOrder { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
