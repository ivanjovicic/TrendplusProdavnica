#nullable enable
using System;
using System.Collections.Generic;

namespace TrendplusProdavnica.Application.Personalization
{
    /// <summary>Odgovore - korisnički profil sa signalima</summary>
    public class UserProfileDto
    {
        public Guid UserId { get; set; }
        public List<long> FavoriteBrandIds { get; set; } = new();
        public decimal PreferredPriceMin { get; set; }
        public decimal PreferredPriceMax { get; set; }
        public Dictionary<long, DateTimeOffset> RecentlyViewed { get; set; } = new();
        public List<long> PreferredCategoryIds { get; set; } = new();
        public DateTimeOffset LastUpdatedAtUtc { get; set; }
        public bool HasEnoughSignals { get; set; }
    }

    /// <summary>Zahtev za dodavanje proizvoda u recently viewed</summary>
    public class TrackProductViewRequest
    {
        public long ProductId { get; set; }
    }

    /// <summary>Zahtev za postavljanje omiljene marke</summary>
    public class SetFavoriteBrandRequest
    {
        public long BrandId { get; set; }
        public bool IsFavorite { get; set; }
    }

    /// <summary>Zahtev za postavljanje preferirane cjenovne grupe</summary>
    public class SetPreferredPriceRangeRequest
    {
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
    }

    /// <summary>Zahtev za postavljanje preferirane kategorije</summary>
    public class SetPreferredCategoryRequest
    {
        public long CategoryId { get; set; }
        public bool IsPreferred { get; set; }
    }

    /// <summary>Zahtev za dobijanje personalizirane home feed-a</summary>
    public class PersonalizedFeedRequest
    {
        public int PageSize { get; set; } = 20;
        public bool IncludeRecommendations { get; set; } = true;
        public bool IncludeNewArrivals { get; set; } = true;
    }

    /// <summary>Personalizovani proizvod u feed-u</summary>
    public class PersonalizedProductDto
    {
        public long ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public double? Rating { get; set; }
        public int ReviewCount { get; set; }
        public long BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public long CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        
        /// <summary>Razlozi zašto je prikazan (npr. "Omiljeni brend", "Nedavno pregledano")</summary>
        public List<string> PersonalizationReasons { get; set; } = new();
        
        /// <summary>Relevance score (0-100) za sortiranje</summary>
        public int RelevanceScore { get; set; }
    }

    /// <summary>Odgovore - personalizovana home feed</summary>
    public class PersonalizedFeedDto
    {
        public List<PersonalizedProductDto> Products { get; set; } = new();
        public int TotalCount { get; set; }
        public string[] AppliedSignals { get; set; } = new string[] { };
        public DateTimeOffset GeneratedAtUtc { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
