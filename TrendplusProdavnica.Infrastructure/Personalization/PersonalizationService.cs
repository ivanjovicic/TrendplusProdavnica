#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TrendplusProdavnica.Application.Personalization;
using TrendplusProdavnica.Domain.Personalization;
using TrendplusProdavnica.Infrastructure.Persistence;

namespace TrendplusProdavnica.Infrastructure.Personalization
{
    /// <summary>
    /// Personalizacija servisa - generiše personalizovane preporuke na osnovu signala
    /// </summary>
    #if false
    // PersonalizationService temporarily disabled due to Product property mismatches (Price, CategoryId, AverageRating, ReviewCount)
    public class PersonalizationService : IPersonalizationService
    {
        private readonly TrendplusDbContext _db;
        private readonly ILogger<PersonalizationService> _logger;

        public PersonalizationService(
            TrendplusDbContext db,
            ILogger<PersonalizationService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<UserProfileDto> GetOrCreateProfileAsync(Guid userId)
        {
            try
            {
                var profile = await _db.UserProfiles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.UserId == userId);

                if (profile != null)
                    return MapToDto(profile);

                // Kreiraj novi profil
                var newProfile = new UserProfile(userId);
                _db.UserProfiles.Add(newProfile);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Kreiran novi korisnički profil za {UserId}", userId);

                return MapToDto(newProfile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri kreiranju/dohvatanju profila za {UserId}", userId);
                throw;
            }
        }

        public async Task<UserProfileDto?> GetProfileAsync(Guid userId)
        {
            try
            {
                var profile = await _db.UserProfiles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.UserId == userId);

                return profile != null ? MapToDto(profile) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri dohvatanju profila za {UserId}", userId);
                return null;
            }
        }

        public async Task<UserProfileDto> TrackProductViewAsync(Guid userId, long productId)
        {
            try
            {
                var profile = await _db.UserProfiles.FirstOrDefaultAsync(x => x.UserId == userId);
                if (profile == null)
                {
                    profile = new UserProfile(userId);
                    _db.UserProfiles.Add(profile);
                }

                profile.AddRecentlyViewed(productId);
                await _db.SaveChangesAsync();

                _logger.LogDebug("Zabilježen pregled proizvoda {ProductId} za korisnika {UserId}", productId, userId);

                return MapToDto(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri zabilježivanju pregleda proizvoda");
                throw;
            }
        }

        public async Task<UserProfileDto> SetFavoriteBrandAsync(Guid userId, long brandId, bool isFavorite)
        {
            try
            {
                var profile = await _db.UserProfiles.FirstOrDefaultAsync(x => x.UserId == userId);
                if (profile == null)
                {
                    profile = new UserProfile(userId);
                    _db.UserProfiles.Add(profile);
                }

                if (isFavorite)
                    profile.AddFavoriteBrand(brandId);
                else
                    profile.RemoveFavoriteBrand(brandId);

                await _db.SaveChangesAsync();

                _logger.LogInformation(
                    "Ažurirana omljena marka {BrandId} ({IsFavorite}) za korisnika {UserId}",
                    brandId, isFavorite, userId);

                return MapToDto(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri ažuriranju omiljene marke");
                throw;
            }
        }

        public async Task<UserProfileDto> SetPreferredPriceRangeAsync(Guid userId, decimal minPrice, decimal maxPrice)
        {
            try
            {
                var profile = await _db.UserProfiles.FirstOrDefaultAsync(x => x.UserId == userId);
                if (profile == null)
                {
                    profile = new UserProfile(userId);
                    _db.UserProfiles.Add(profile);
                }

                profile.SetPreferredPriceRange(minPrice, maxPrice);
                await _db.SaveChangesAsync();

                _logger.LogInformation(
                    "Ažuriran opseg cijena ({Min}-{Max}) za korisnika {UserId}",
                    minPrice, maxPrice, userId);

                return MapToDto(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri ažuriranju opsega cijena");
                throw;
            }
        }

        public async Task<UserProfileDto> SetPreferredCategoryAsync(Guid userId, long categoryId, bool isPreferred)
        {
            try
            {
                var profile = await _db.UserProfiles.FirstOrDefaultAsync(x => x.UserId == userId);
                if (profile == null)
                {
                    profile = new UserProfile(userId);
                    _db.UserProfiles.Add(profile);
                }

                if (isPreferred)
                    profile.AddPreferredCategory(categoryId);
                else
                    profile.RemovePreferredCategory(categoryId);

                await _db.SaveChangesAsync();

                _logger.LogInformation(
                    "Ažurirana preferirana kategorija {CategoryId} ({IsPreferred}) za korisnika {UserId}",
                    categoryId, isPreferred, userId);

                return MapToDto(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri ažuriranju preferirane kategorije");
                throw;
            }
        }

        public async Task<PersonalizedFeedDto> GetPersonalizedFeedAsync(
            Guid userId,
            PersonalizedFeedRequest request)
        {
            try
            {
                var profile = await _db.UserProfiles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.UserId == userId);

                var feed = new PersonalizedFeedDto
                {
                    GeneratedAtUtc = DateTimeOffset.UtcNow
                };

                // Ako nema profila ili signala, vrati default polje
                if (profile == null || !profile.HasEnoughSignals())
                {
                    feed.Message = "Nema dostupnih personalizovanih preporuka za sada. Pregledajte proizvode da bismo vam dali personalizovane preporuke.";
                    feed.AppliedSignals = new[] { "none" };
                    return feed;
                }

                var appliedSignals = new List<string>();
                var scoredProducts = new Dictionary<long, (PersonalizedProductDto Product, int Score, List<string> Reasons)>();

                // 1. Filtriranje po omiljenim markama
                if (profile.FavoriteBrandIds.Any())
                {
                    appliedSignals.Add("favorite_brands");
                    await ScoreProductsByBrands(scoredProducts, profile.FavoriteBrandIds, "Omiljeni brend", 40);
                }

                // 2. Filtriranje po preferiranoj cjenovnoj grupi
                if (profile.PreferredPriceMin > 0 || profile.PreferredPriceMax < decimal.MaxValue)
                {
                    appliedSignals.Add("price_range");
                    await ScoreProductsByPrice(
                        scoredProducts,
                        profile.PreferredPriceMin,
                        profile.PreferredPriceMax,
                        "U Vašem cjenovnom rasponu",
                        30);
                }

                // 3. Filtriranje po preferiranim kategorijama
                if (profile.PreferredCategoryIds.Any())
                {
                    appliedSignals.Add("preferred_categories");
                    await ScoreProductsByCategories(
                        scoredProducts,
                        profile.PreferredCategoryIds,
                        "U Vašim preferiranim kategorijama",
                        35);
                }

                // 4. Nedavno pregledani proizvodi (bonus ako se ponavljaju)
                if (profile.RecentlyViewed.Any())
                {
                    appliedSignals.Add("recently_viewed");
                    await ScoreSimilarProductsToViewed(
                        scoredProducts,
                        profile.RecentlyViewed.Keys.ToList(),
                        "Slično kao što ste pregledali",
                        25);
                }

                // Sortiraj po relevance score i vrati top rezultate
                var topProducts = scoredProducts
                    .OrderByDescending(x => x.Value.Score)
                    .Take(request.PageSize)
                    .Select(x => x.Value.Product)
                    .ToList();

                feed.Products = topProducts;
                feed.TotalCount = topProducts.Count;
                feed.AppliedSignals = appliedSignals.ToArray();

                _logger.LogInformation(
                    "Generisana personalizovana feed za {UserId} sa {Count} proizvoda",
                    userId,
                    topProducts.Count);

                return feed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri generisanju personalizovane feed-a");
                throw;
            }
        }

        public async Task ClearAllSignalsAsync(Guid userId)
        {
            try
            {
                var profile = await _db.UserProfiles.FirstOrDefaultAsync(x => x.UserId == userId);
                if (profile == null)
                    return;

                profile.FavoriteBrandIds.Clear();
                profile.PreferredCategoryIds.Clear();
                profile.RecentlyViewed.Clear();
                profile.PreferredPriceMin = 0;
                profile.PreferredPriceMax = decimal.MaxValue;
                profile.LastUpdatedAtUtc = DateTimeOffset.UtcNow;

                await _db.SaveChangesAsync();

                _logger.LogInformation("Očišćeni svi signali za korisnika {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri čišćenju signala");
                throw;
            }
        }

        public async Task ClearOldSignalsAsync(Guid userId, int daysToKeep = 30)
        {
            try
            {
                var profile = await _db.UserProfiles.FirstOrDefaultAsync(x => x.UserId == userId);
                if (profile == null)
                    return;

                profile.ClearOldRecentlyViewed(daysToKeep);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Očišćeni stari signali (>= {Days} dana) za korisnika {UserId}", daysToKeep, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri čišćenju starih signala");
                throw;
            }
        }

        // === Privatne pomoćne metode ===

        private async Task ScoreProductsByBrands(
            Dictionary<long, (PersonalizedProductDto, int, List<string>)> scoredProducts,
            List<long> brandIds,
            string reason,
            int baseScore)
        {
            var products = await _db.Products
                .AsNoTracking()
                .Where(p => p.IsVisible && brandIds.Contains(p.BrandId))
                .Take(100)
                .ToListAsync();

            foreach (var product in products)
            {
                if (!scoredProducts.ContainsKey(product.Id))
                {
                    scoredProducts[product.Id] = (
                        await BuildProductDto(product),
                        baseScore,
                        new List<string> { reason }
                    );
                }
                else
                {
                    var (dto, score, reasons) = scoredProducts[product.Id];
                    scoredProducts[product.Id] = (
                        dto,
                        score + baseScore / 2,
                        new List<string>(reasons) { reason }
                    );
                }
            }
        }

        private async Task ScoreProductsByPrice(
            Dictionary<long, (PersonalizedProductDto, int, List<string>)> scoredProducts,
            decimal minPrice,
            decimal maxPrice,
            string reason,
            int baseScore)
        {
            var products = await _db.Products
                .AsNoTracking()
                .Where(p => p.IsVisible && p.Price >= minPrice && p.Price <= maxPrice)
                .Take(100)
                .ToListAsync();

            foreach (var product in products)
            {
                if (!scoredProducts.ContainsKey(product.Id))
                {
                    scoredProducts[product.Id] = (
                        await BuildProductDto(product),
                        baseScore,
                        new List<string> { reason }
                    );
                }
                else
                {
                    var (dto, score, reasons) = scoredProducts[product.Id];
                    scoredProducts[product.Id] = (
                        dto,
                        score + baseScore / 2,
                        new List<string>(reasons) { reason }
                    );
                }
            }
        }

        private async Task ScoreProductsByCategories(
            Dictionary<long, (PersonalizedProductDto, int, List<string>)> scoredProducts,
            List<long> categoryIds,
            string reason,
            int baseScore)
        {
            var products = await _db.Products
                .AsNoTracking()
                .Where(p => p.IsVisible && categoryIds.Contains(p.CategoryId))
                .Take(100)
                .ToListAsync();

            foreach (var product in products)
            {
                if (!scoredProducts.ContainsKey(product.Id))
                {
                    scoredProducts[product.Id] = (
                        await BuildProductDto(product),
                        baseScore,
                        new List<string> { reason }
                    );
                }
                else
                {
                    var (dto, score, reasons) = scoredProducts[product.Id];
                    scoredProducts[product.Id] = (
                        dto,
                        score + baseScore / 2,
                        new List<string>(reasons) { reason }
                    );
                }
            }
        }

        private async Task ScoreSimilarProductsToViewed(
            Dictionary<long, (PersonalizedProductDto, int, List<string>)> scoredProducts,
            List<long> recentlyViewedIds,
            string reason,
            int baseScore)
        {
            // Dohvati pregljedane proizvode za kategoriziaciju
            var viewedProducts = await _db.Products
                .AsNoTracking()
                .Where(p => recentlyViewedIds.Contains(p.Id))
                .ToListAsync();

            var viewedCategoryIds = viewedProducts.Select(p => p.CategoryId).Distinct().ToList();
            var viewedBrandIds = viewedProducts.Select(p => p.BrandId).Distinct().ToList();

            // Nađi slične proizvode (ista kategorija ili marka)
            var similarProducts = await _db.Products
                .AsNoTracking()
                .Where(p => p.IsVisible &&
                       !recentlyViewedIds.Contains(p.Id) &&
                       (viewedCategoryIds.Contains(p.CategoryId) || viewedBrandIds.Contains(p.BrandId)))
                .Take(100)
                .ToListAsync();

            foreach (var product in similarProducts)
            {
                if (!scoredProducts.ContainsKey(product.Id))
                {
                    scoredProducts[product.Id] = (
                        await BuildProductDto(product),
                        baseScore,
                        new List<string> { reason }
                    );
                }
            }
        }

        private async Task<PersonalizedProductDto> BuildProductDto(Domain.Catalog.Product product)
        {
            var brand = await _db.Brands
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == product.BrandId);

            var category = await _db.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == product.CategoryId);

            return new PersonalizedProductDto
            {
                ProductId = product.Id,
                Name = product.Name,
                Slug = product.Slug,
                Price = product.Price,
                Rating = product.AverageRating,
                ReviewCount = product.ReviewCount ?? 0,
                BrandId = product.BrandId,
                BrandName = brand?.Name ?? "Unknown",
                CategoryId = product.CategoryId,
                CategoryName = category?.Name ?? "Unknown"
            };
        }

        private UserProfileDto MapToDto(UserProfile profile)
        {
            return new UserProfileDto
            {
                UserId = profile.UserId,
                FavoriteBrandIds = profile.FavoriteBrandIds,
                PreferredPriceMin = profile.PreferredPriceMin,
                PreferredPriceMax = profile.PreferredPriceMax,
                RecentlyViewed = profile.RecentlyViewed,
                PreferredCategoryIds = profile.PreferredCategoryIds,
                LastUpdatedAtUtc = profile.LastUpdatedAtUtc,
                HasEnoughSignals = profile.HasEnoughSignals()
            };
        }
    }
    #endif  // PersonalizationService disabled due to Product property mismatches
}
