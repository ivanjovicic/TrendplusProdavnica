#nullable enable
using System;
using TrendplusProdavnica.Domain.Common;
using TrendplusProdavnica.Domain.Enums;

namespace TrendplusProdavnica.Domain.Catalog
{
    public class ProductReview : EntityBase
    {
        public long ProductId { get; set; }
        public string? ExternalKey { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? ReviewBody { get; set; }
        public decimal RatingValue { get; set; }
        public bool IsVerifiedPurchase { get; set; }
        public ProductReviewStatus Status { get; set; } = ProductReviewStatus.Pending;
        public DateTimeOffset? PublishedAtUtc { get; set; }

        public Product? Product { get; set; }
    }
}
