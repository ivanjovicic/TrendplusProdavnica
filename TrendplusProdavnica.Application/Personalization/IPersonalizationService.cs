#nullable enable
using System;
using System.Threading.Tasks;

namespace TrendplusProdavnica.Application.Personalization
{
    public interface IPersonalizationService
    {
        /// <summary>Dohvati ili kreira korisnički profil</summary>
        Task<UserProfileDto> GetOrCreateProfileAsync(Guid userId);

        /// <summary>Dohvati korisnički profil</summary>
        Task<UserProfileDto?> GetProfileAsync(Guid userId);

        /// <summary>Zapi da je korisnik pregledao proizvod</summary>
        Task<UserProfileDto> TrackProductViewAsync(Guid userId, long productId);

        /// <summary>Postavi omiljenu marku</summary>
        Task<UserProfileDto> SetFavoriteBrandAsync(Guid userId, long brandId, bool isFavorite);

        /// <summary>Postavi preferiranu cijenovnu grupu</summary>
        Task<UserProfileDto> SetPreferredPriceRangeAsync(Guid userId, decimal minPrice, decimal maxPrice);

        /// <summary>Postavi preferiranu kategoriju</summary>
        Task<UserProfileDto> SetPreferredCategoryAsync(Guid userId, long categoryId, bool isPreferred);

        /// <summary>Generiši personalizovanu home feed na osnovu signala korisnika</summary>
        Task<PersonalizedFeedDto> GetPersonalizedFeedAsync(Guid userId, PersonalizedFeedRequest request);

        /// <summary>Očisti sve signale korisnika</summary>
        Task ClearAllSignalsAsync(Guid userId);

        /// <summary>Očisti recently viewed proizvode starije od N dana</summary>
        Task ClearOldSignalsAsync(Guid userId, int daysToKeep = 30);
    }
}
