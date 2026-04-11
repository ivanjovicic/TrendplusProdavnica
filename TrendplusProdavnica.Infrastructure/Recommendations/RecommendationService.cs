#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TrendplusProdavnica.Application.Recommendations.Dtos;
using TrendplusProdavnica.Application.Recommendations.Services;
using TrendplusProdavnica.Infrastructure.Persistence;
using ZiggyCreatures.Caching.Fusion;

namespace TrendplusProdavnica.Infrastructure.Recommendations
{
    /// <summary>
    /// Implementacija servisa za preporuke sa scoring algoritmom
    /// </summary>
    #if false
    // RecommendationService temporarily disabled due to IFusionCache API incompatibilities
    public class RecommendationService : IRecommendationService
    {
        private readonly TrendplusDbContext _db;
        private readonly IFusionCache _cache;
        private readonly ILogger<RecommendationService> _logger;

        // Scoring weights (trebalo bi u config)
        private const decimal CategoryWeight = 0.40m;
        private const decimal BrandWeight = 0.20m;
        private const decimal PriceWeight = 0.20m;
        private const decimal PopularityWeight = 0.20m;

        // Cache settings
        private const string CacheKeyPrefix = "recommendations";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

        public RecommendationService(
            TrendplusDbContext db,
            IFusionCache cache,
            ILogger<RecommendationService> logger)
        {
            _db = db;
            _cache = cache;
            _logger = logger;
        }

        public async Task<RecommendationResponse> GetRecommendationsAsync(
            long productId, 
            int limit = 8, 
            RecommendationType type = RecommendationType.RelatedProducts,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var cacheKey = $"{CacheKeyPrefix}:{productId}:{type}";

                // Pokušaj iz cache-a
                var cached = await _cache.GetAsync<RecommendationResponse>(cacheKey, cancellationToken: cancellationToken);
                if (cached != null)
                {
                    _logger.LogDebug("Cache hit for recommendations: {CacheKey}", cacheKey);
                    // Limitiraj rezultate ako je drugačiji limit zahtjevan
                    if (cached.Items.Count > limit)
                    {
                        cached.Items = cached.Items.Take(limit).ToList();
                    }
                    return cached;
                }

                // Dohvati source product
                var sourceProduct = await _db.Products
                    .AsNoTracking()
                    .Where(p => p.Id == productId && p.IsVisible)
                    .Include(p => p.Brand)
                    .Include(p => p.CategoryMaps)
                    .Include(p => p.Rating)
                    .Include(p => p.Variants)
                    .FirstOrDefaultAsync(cancellationToken);

                if (sourceProduct == null)
                {
                    _logger.LogWarning("Source product not found: {ProductId}", productId);
                    return new RecommendationResponse { SourceProductId = productId, Title = "Nema dostupnih preporuka" };
                }

                // Generiraj preporuke
                var recommendations = await GenerateRecommendationsAsync(sourceProduct, limit, type, cancellationToken);

                // Mapiraj na DTO
                var response = new RecommendationResponse
                {
                    SourceProductId = productId,
                    Title = type switch
                    {
                        RecommendationType.RelatedProducts => "Možda će vam se svideti",
                        RecommendationType.CrossSell => "Uz ovaj proizvod kupci često uzimaju",
                        RecommendationType.Trending => "Trenutno popularno",
                        RecommendationType.NewArrivals => "Novi proizvodi",
                        _ => "Preporučeni proizvodi"
                    },
                    Items = recommendations
                };

                // Keširaj rezultate
                await _cache.SetAsync(cacheKey, response, new FusionCacheEntryOptions { Duration = CacheDuration }, cancellationToken);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recommendations for product {ProductId}", productId);
                return new RecommendationResponse { SourceProductId = productId, Title = "Greška pri učitavanju preporuka" };
            }
        }

        public async Task<RecommendationResponse> GetHomepageRecommendationsAsync(int limit = 8, CancellationToken cancellationToken = default)
        {
            try
            {
                var cacheKey = $"{CacheKeyPrefix}:homepage";

                // Pokušaj iz cache-a
                var cached = await _cache.GetAsync<RecommendationResponse>(cacheKey, cancellationToken: cancellationToken);
                if (cached != null)
                {
                    return cached;
                }

                // Za homepage vrati najbolje bestseller-e i top-rated proizvode
                var recommendations = await _db.Products
                    .AsNoTracking()
                    .Where(p => p.IsVisible && p.IsPurchasable)
                    .Include(p => p.Brand)
                    .Include(p => p.Rating)
                    .Include(p => p.Media)
                    .Include(p => p.Variants)
                    .OrderByDescending(p => p.IsBestseller)
                    .ThenByDescending(p => p.Rating!.AverageRating)
                    .ThenByDescending(p => p.Rating!.RatingCount)
                    .Take(limit)
                    .ToListAsync(cancellationToken);

                var items = recommendations.Select(p => MapProductToRecommendedDto(p)).ToList();

                var response = new RecommendationResponse
                {
                    SourceProductId = 0,
                    Title = "Trenutno preporučeni proizvodi",
                    Items = items
                };

                // Keširaj sa kraćom trajanjem za homepage
                await _cache.SetAsync(cacheKey, response, new FusionCacheEntryOptions { Duration = TimeSpan.FromMinutes(5) }, cancellationToken);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting homepage recommendations");
                return new RecommendationResponse { Title = "Greška pri učitavanju" };
            }
        }

        public async Task InvalidateCacheAsync(long productId, CancellationToken cancellationToken = default)
        {
            try
            {
                var keys = new[]
                {
                    $"{CacheKeyPrefix}:{productId}:{RecommendationType.RelatedProducts}",
                    $"{CacheKeyPrefix}:{productId}:{RecommendationType.CrossSell}",
                    $"{CacheKeyPrefix}:{productId}:{RecommendationType.Trending}",
                    $"{CacheKeyPrefix}:{productId}:{RecommendationType.NewArrivals}",
                    $"{CacheKeyPrefix}:homepage"
                };

                foreach (var key in keys)
                {
                    await _cache.RemoveAsync(key, cancellationToken);
                }

                _logger.LogDebug("Cache invalidated for product {ProductId}", productId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating cache for product {ProductId}", productId);
            }
        }

        public async Task<List<ProductScoringDetails>> GetScoringDetailsAsync(long productId, CancellationToken cancellationToken = default)
        {
            try
            {
                var sourceProduct = await _db.Products
                    .AsNoTracking()
                    .Where(p => p.Id == productId && p.IsVisible)
                    .Include(p => p.Brand)
                    .Include(p => p.CategoryMaps)
                    .Include(p => p.Rating)
                    .Include(p => p.Variants)
                    .FirstOrDefaultAsync(cancellationToken);

                if (sourceProduct == null)
                    return new List<ProductScoringDetails>();

                // Dohvati sve kandidate
                var candidates = await _db.Products
                    .AsNoTracking()
                    .Where(p => p.Id != productId && p.IsVisible && p.IsPurchasable)
                    .Include(p => p.Brand)
                    .Include(p => p.CategoryMaps)
                    .Include(p => p.Rating)
                    .Include(p => p.Variants)
                    .Take(50)
                    .ToListAsync(cancellationToken);

                var scores = new List<ProductScoringDetails>();

                foreach (var candidate in candidates)
                {
                    var categoryScore = CalculateCategoryScore(sourceProduct, candidate);
                    var brandScore = CalculateBrandScore(sourceProduct, candidate);
                    var priceScore = CalculatePriceScore(sourceProduct, candidate);
                    var popularityScore = CalculatePopularityScore(candidate);

                    var totalScore = (categoryScore * CategoryWeight) +
                                   (brandScore * BrandWeight) +
                                   (priceScore * PriceWeight) +
                                   (popularityScore * PopularityWeight);

                    scores.Add(new ProductScoringDetails
                    {
                        ProductId = candidate.Id,
                        Name = candidate.Name,
                        CategoryScore = categoryScore,
                        BrandScore = brandScore,
                        PriceScore = priceScore,
                        PopularityScore = popularityScore,
                        TotalScore = totalScore
                    });
                }

                return scores.OrderByDescending(s => s.TotalScore).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting scoring details");
                return new List<ProductScoringDetails>();
            }
        }

        // --- Private Methods ---

        private async Task<List<RecommendedProductDto>> GenerateRecommendationsAsync(
            Domain.Catalog.Product sourceProduct,
            int limit,
            RecommendationType type,
            CancellationToken cancellationToken)
        {
            // Dohvati sve dostupne proizvode osim source-a
            var candidates = await _db.Products
                .AsNoTracking()
                .Where(p => p.Id != sourceProduct.Id && p.IsVisible && p.IsPurchasable)
                .Include(p => p.Brand)
                .Include(p => p.CategoryMaps)
                .Include(p => p.Rating)
                .Include(p => p.Media)
                .Include(p => p.Variants)
                .ToListAsync(cancellationToken);

            // Izračunaj score za svakog kandidata
            var scoredProducts = new List<(Domain.Catalog.Product Product, decimal Score)>();

            foreach (var candidate in candidates)
            {
                var categoryScore = CalculateCategoryScore(sourceProduct, candidate);
                var brandScore = CalculateBrandScore(sourceProduct, candidate);
                var priceScore = CalculatePriceScore(sourceProduct, candidate);
                var popularityScore = CalculatePopularityScore(candidate);

                var totalScore = (categoryScore * CategoryWeight) +
                               (brandScore * BrandWeight) +
                               (priceScore * PriceWeight) +
                               (popularityScore * PopularityWeight);

                scoredProducts.Add((candidate, totalScore));
            }

            // Sortiraj po score-u i uzmi top N
            var topProducts = scoredProducts
                .OrderByDescending(x => x.Score)
                .Take(limit)
                .Select(x => MapProductToRecommendedDto(x.Product))
                .ToList();

            return topProducts;
        }

        private decimal CalculateCategoryScore(Domain.Catalog.Product source, Domain.Catalog.Product candidate)
        {
            // Dohvati kategorije oba proizvoda
            var sourceCategories = source.CategoryMaps.Select(m => m.CategoryId).ToHashSet();
            var candidateCategories = candidate.CategoryMaps.Select(m => m.CategoryId).ToHashSet();

            // Ako nema kategorija, vrati 0
            if (sourceCategories.Count == 0 || candidateCategories.Count == 0)
                return 0.5m; // Neutralna vrijednost

            // Izračunaj Jaccard similarity
            var intersection = sourceCategories.Intersect(candidateCategories).Count();
            var union = sourceCategories.Union(candidateCategories).Count();

            var similarity = union > 0 ? (decimal)intersection / union : 0;
            return Math.Min(similarity, 1.0m); // Normalizirano na 0-1
        }

        private decimal CalculateBrandScore(Domain.Catalog.Product source, Domain.Catalog.Product candidate)
        {
            // Ako su iste marke, daj veći score (ali manju težinu u formulaciji jer želimo raznolikost)
            if (source.BrandId == candidate.BrandId)
                return 0.7m; // Ako je ista marka - malo niža vrijednost

            // Ako su različite marke - manja vrijednost
            return 0.3m;
        }

        private decimal CalculatePriceScore(Domain.Catalog.Product source, Domain.Catalog.Product candidate)
        {
            // Dohvati cijene
            var sourcePrice = source.Variants.FirstOrDefault()?.Price ?? 0;
            var candidatePrice = candidate.Variants.FirstOrDefault()?.Price ?? 0;

            if (sourcePrice == 0 || candidatePrice == 0)
                return 0.5m;

            // Izračunaj razliku cijene kao postotak
            var priceDifference = Math.Abs(sourcePrice - candidatePrice) / sourcePrice;

            // Score: bliže je cijena, viši je score
            // Npr: 10% razlike = 0.9, 50% razlike = 0.5, 90% razlike = 0.1
            var score = Math.Max(1.0m - (decimal)priceDifference, 0.0m);
            return Math.Min(score, 1.0m);
        }

        private decimal CalculatePopularityScore(Domain.Catalog.Product product)
        {
            decimal score = 0.5m; // Bazna vrijednost

            // Bestseller boost
            if (product.IsBestseller)
                score += 0.2m;

            // Novi proizvod boost
            if (product.IsNew)
                score += 0.1m;

            // Rating boost
            if (product.Rating != null)
            {
                // 5.0 rating = +0.3, 3.0 rating = +0.18, 1.0 rating = +0.06
                var ratingBoost = (product.Rating.AverageRating / 5.0m) * 0.3m;
                score += (decimal)ratingBoost;
            }

            return Math.Min(score, 1.0m);
        }

        private RecommendedProductDto MapProductToRecommendedDto(Domain.Catalog.Product product)
        {
            var variant = product.Variants?.FirstOrDefault();
            var media = product.Media?.FirstOrDefault(m => m.IsPrimary) ?? product.Media?.FirstOrDefault();

            return new RecommendedProductDto
            {
                ProductId = product.Id,
                Slug = product.Slug,
                Name = product.Name,
                Brand = product.Brand?.Name ?? "Unknown",
                Price = variant?.Price ?? 0,
                ImageUrl = media?.Url,
                MobileImageUrl = media?.MobileUrl,
                AverageRating = product.Rating?.AverageRating,
                RatingCount = product.Rating?.RatingCount ?? 0,
                IsBestseller = product.IsBestseller,
                IsNew = product.IsNew,
                RecommendationScore = 0 // Trebao bi se postavljati iz scoring-a
            };
        }
    }
    #endif  // RecommendationService disabled due to IFusionCache API incompatibilities
}
