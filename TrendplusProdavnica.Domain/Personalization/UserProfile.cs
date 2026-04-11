#nullable enable
using System;
using System.Collections.Generic;
using TrendplusProdavnica.Domain.Common;

namespace TrendplusProdavnica.Domain.Personalization
{
    /// <summary>
    /// Korisnički profil sa personalizacionim signalima
    /// </summary>
    public class UserProfile : EntityBase
    {
        /// <summary>ID korisnika</summary>
        public Guid UserId { get; set; }

        /// <summary>Omiljene marke (lista ID-eva)</summary>
        public List<long> FavoriteBrandIds { get; set; } = new();

        /// <summary>Minimalna preferirana cijena</summary>
        public decimal PreferredPriceMin { get; set; } = 0;

        /// <summary>Maksimalna preferirana cijena</summary>
        public decimal PreferredPriceMax { get; set; } = decimal.MaxValue;

        /// <summary>Nedavno pregledani proizvodi (lista proizvoda sa vremenima)</summary>
        public Dictionary<long, DateTimeOffset> RecentlyViewed { get; set; } = new();

        /// <summary>Preferirane kategorije (lista ID-eva)</summary>
        public List<long> PreferredCategoryIds { get; set; } = new();

        /// <summary>Kada je profil zadnji put ažuriran</summary>
        public DateTimeOffset LastUpdatedAtUtc { get; set; }

        /// <summary>Kada je last time generisao personalnu preporuku</summary>
        public DateTimeOffset? LastPersonalizationAtUtc { get; set; }

        // EF Core parameterless constructor
        private UserProfile() { }

        public UserProfile(Guid userId)
        {
            UserId = userId;
            LastUpdatedAtUtc = DateTimeOffset.UtcNow;
        }

        /// <summary>Dodaj proizvod u recently viewed sa vremenskom oznakom</summary>
        public void AddRecentlyViewed(long productId, int maxItems = 20)
        {
            RecentlyViewed[productId] = DateTimeOffset.UtcNow;
            LastUpdatedAtUtc = DateTimeOffset.UtcNow;

            // Održavaj samo zadnjih N stavki
            if (RecentlyViewed.Count > maxItems)
            {
                var oldest = RecentlyViewed
                    .OrderBy(x => x.Value)
                    .First();
                RecentlyViewed.Remove(oldest.Key);
            }
        }

        /// <summary>Dodaj omiljenu marku</summary>
        public void AddFavoriteBrand(long brandId)
        {
            if (!FavoriteBrandIds.Contains(brandId))
            {
                FavoriteBrandIds.Add(brandId);
                LastUpdatedAtUtc = DateTimeOffset.UtcNow;
            }
        }

        /// <summary>Ukloni omiljenu marku</summary>
        public void RemoveFavoriteBrand(long brandId)
        {
            if (FavoriteBrandIds.Remove(brandId))
                LastUpdatedAtUtc = DateTimeOffset.UtcNow;
        }

        /// <summary>Postavi preferiranu cenuvnu grupu</summary>
        public void SetPreferredPriceRange(decimal minPrice, decimal maxPrice)
        {
            if (minPrice < 0 || maxPrice < 0 || minPrice > maxPrice)
                throw new ArgumentException("Nevaljani opseg cijena");

            PreferredPriceMin = minPrice;
            PreferredPriceMax = maxPrice;
            LastUpdatedAtUtc = DateTimeOffset.UtcNow;
        }

        /// <summary>Dodaj preferiranu kategoriju</summary>
        public void AddPreferredCategory(long categoryId)
        {
            if (!PreferredCategoryIds.Contains(categoryId))
            {
                PreferredCategoryIds.Add(categoryId);
                LastUpdatedAtUtc = DateTimeOffset.UtcNow;
            }
        }

        /// <summary>Ukloni preferiranu kategoriju</summary>
        public void RemovePreferredCategory(long categoryId)
        {
            if (PreferredCategoryIds.Remove(categoryId))
                LastUpdatedAtUtc = DateTimeOffset.UtcNow;
        }

        /// <summary>Očisti recently viewed proizvode (starije od N dana)</summary>
        public void ClearOldRecentlyViewed(int daysToKeep = 30)
        {
            var cutoffDate = DateTimeOffset.UtcNow.AddDays(-daysToKeep);
            var oldItems = RecentlyViewed
                .Where(x => x.Value < cutoffDate)
                .Select(x => x.Key)
                .ToList();

            foreach (var productId in oldItems)
                RecentlyViewed.Remove(productId);

            if (oldItems.Any())
                LastUpdatedAtUtc = DateTimeOffset.UtcNow;
        }

        /// <summary>Provjeri da li ima dovoljno signala za personalizaciju</summary>
        public bool HasEnoughSignals()
        {
            var signalCount = 0;
            if (RecentlyViewed.Any()) signalCount++;
            if (FavoriteBrandIds.Any()) signalCount++;
            if (PreferredPriceMin > 0 || PreferredPriceMax < decimal.MaxValue) signalCount++;
            if (PreferredCategoryIds.Any()) signalCount++;

            return signalCount >= 1;  // Trebam bar jedan signal
        }
    }
}
