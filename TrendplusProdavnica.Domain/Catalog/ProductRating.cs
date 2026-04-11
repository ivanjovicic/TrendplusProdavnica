#nullable enable
using System;
using TrendplusProdavnica.Domain.Common;

namespace TrendplusProdavnica.Domain.Catalog
{
    public class ProductRating : EntityBase
    {
        public long ProductId { get; set; }
        public decimal AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public int RatingCount { get; set; }
        public int OneStarCount { get; set; }
        public int TwoStarCount { get; set; }
        public int ThreeStarCount { get; set; }
        public int FourStarCount { get; set; }
        public int FiveStarCount { get; set; }
        public DateTimeOffset? LastReviewAtUtc { get; set; }

        public Product? Product { get; set; }
    }
}
