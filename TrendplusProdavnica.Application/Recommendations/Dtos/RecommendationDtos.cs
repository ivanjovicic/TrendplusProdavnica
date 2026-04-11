#nullable enable
namespace TrendplusProdavnica.Application.Recommendations.Dtos
{
    /// <summary>
    /// DTOza preporučeni proizvod
    /// </summary>
    public class RecommendedProductDto
    {
        public long ProductId { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string? MobileImageUrl { get; set; }
        
        // Dodatni podaci za reranking na frontend-u
        public decimal? AverageRating { get; set; }
        public int RatingCount { get; set; }
        public bool IsBestseller { get; set; }
        public bool IsNew { get; set; }
        
        // Unutarnja logika
        public decimal RecommendationScore { get; set; }
    }

    /// <summary>
    /// Zahtjev za preporuke
    /// </summary>
    public class RecommendationRequest
    {
        public long ProductId { get; set; }
        public int Limit { get; set; } = 8;
        public RecommendationType Type { get; set; } = RecommendationType.RelatedProducts;
    }

    /// <summary>
    /// Odgovor sa preporukama
    /// </summary>
    public class RecommendationResponse
    {
        public long SourceProductId { get; set; }
        public string Title { get; set; } = string.Empty;
        public List<RecommendedProductDto> Items { get; set; } = new();
    }

    /// <summary>
    /// Tip preporuke
    /// </summary>
    public enum RecommendationType
    {
        RelatedProducts = 1,   // "Možda će vam se svideti"
        CrossSell = 2,         // "Uz ovaj proizvod kupci često uzimaju"
        Trending = 3,
        NewArrivals = 4
    }

    /// <summary>
    /// Detaljni scoring za debug
    /// </summary>
    public class ProductScoringDetails
    {
        public long ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal CategoryScore { get; set; }
        public decimal BrandScore { get; set; }
        public decimal PriceScore { get; set; }
        public decimal PopularityScore { get; set; }
        public decimal TotalScore { get; set; }
    }
}
