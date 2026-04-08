#nullable enable
using System.Collections.Generic;

namespace TrendplusProdavnica.Domain.ValueObjects
{
    public sealed class FaqItem
    {
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
    }

    public sealed class FeaturedLink
    {
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
    }

    public sealed class MerchBlock
    {
        public string Title { get; set; } = string.Empty;
        public string? Html { get; set; }
        public IEnumerable<string>? ProductSlugs { get; set; }
    }

    public sealed class HomeModule
    {
        public string Type { get; set; } = string.Empty;
        public object? Payload { get; set; }
    }

    public sealed class SocialLink
    {
        public string Platform { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    public sealed class ContactInfo
    {
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }
}
