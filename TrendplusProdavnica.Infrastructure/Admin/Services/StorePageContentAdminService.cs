#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrendplusProdavnica.Application.Admin.Common;
using TrendplusProdavnica.Application.Admin.Dtos;
using TrendplusProdavnica.Application.Admin.Services;
using TrendplusProdavnica.Application.Common.Caching;
using TrendplusProdavnica.Domain.Content;
using TrendplusProdavnica.Infrastructure.Admin.Common;
using TrendplusProdavnica.Infrastructure.Persistence;

namespace TrendplusProdavnica.Infrastructure.Admin.Services
{
    public class StorePageContentAdminService : IStorePageContentAdminService
    {
        private readonly TrendplusDbContext _db;
        private readonly IWebshopCacheInvalidationService _cacheInvalidationService;

        public StorePageContentAdminService(
            TrendplusDbContext db,
            IWebshopCacheInvalidationService cacheInvalidationService)
        {
            _db = db;
            _cacheInvalidationService = cacheInvalidationService;
        }

        public async Task<StorePageContentAdminDto> GetByStoreIdAsync(long storeId, CancellationToken cancellationToken = default)
        {
            var entity = await _db.StorePageContents.AsNoTracking()
                .FirstOrDefaultAsync(item => item.StoreId == storeId, cancellationToken);

            if (entity is null)
            {
                throw new AdminNotFoundException($"Store page content for store '{storeId}' was not found.");
            }

            return Map(entity);
        }

        public async Task<StorePageContentAdminDto> UpsertAsync(UpsertStorePageContentRequest request, CancellationToken cancellationToken = default)
        {
            await ValidateRequestAsync(request, cancellationToken);

            var entity = await _db.StorePageContents
                .FirstOrDefaultAsync(item => item.StoreId == request.StoreId, cancellationToken);
            var now = DateTimeOffset.UtcNow;

            if (entity is null)
            {
                entity = new StorePageContent
                {
                    StoreId = request.StoreId,
                    CreatedAtUtc = now
                };
                _db.StorePageContents.Add(entity);
            }

            entity.IsPublished = request.IsPublished;
            entity.HeroTitle = request.HeroTitle;
            entity.HeroSubtitle = request.HeroSubtitle;
            entity.IntroTitle = request.IntroTitle;
            entity.IntroText = request.IntroText;
            entity.SeoText = request.SeoText;
            entity.HeroImageUrl = request.HeroImageUrl;
            entity.Faq = AdminMappingHelper.ToFaqModels(request.Faq);
            entity.FeaturedLinks = AdminMappingHelper.ToFeaturedLinkModels(request.FeaturedLinks);
            entity.MerchBlocks = AdminMappingHelper.ToMerchBlockModels(request.MerchBlocks);
            entity.Seo = AdminMappingHelper.ToSeoModel(request.Seo);
            entity.UpdatedAtUtc = now;

            await _db.SaveChangesAsync(cancellationToken);
            await InvalidateStoreCacheAsync(request.StoreId, cancellationToken);
            return Map(entity);
        }

        public async Task<StorePageContentAdminDto> UnpublishAsync(long storeId, CancellationToken cancellationToken = default)
        {
            var entity = await _db.StorePageContents
                .FirstOrDefaultAsync(item => item.StoreId == storeId, cancellationToken);

            if (entity is null)
            {
                throw new AdminNotFoundException($"Store page content for store '{storeId}' was not found.");
            }

            entity.IsPublished = false;
            entity.UpdatedAtUtc = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            await InvalidateStoreCacheAsync(storeId, cancellationToken);
            return Map(entity);
        }

        private async Task InvalidateStoreCacheAsync(long storeId, CancellationToken cancellationToken)
        {
            var storeSlug = await _db.Stores.AsNoTracking()
                .Where(store => store.Id == storeId)
                .Select(store => store.Slug)
                .FirstOrDefaultAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(storeSlug))
            {
                await _cacheInvalidationService.InvalidateStoreBySlugAsync(storeSlug, cancellationToken);
            }
        }

        private async Task ValidateRequestAsync(UpsertStorePageContentRequest request, CancellationToken cancellationToken)
        {
            var errors = new Dictionary<string, string[]>();

            if (request.StoreId <= 0)
            {
                AdminValidationHelper.AddError(errors, nameof(request.StoreId), "StoreId must be greater than 0.");
            }

            if (!await _db.Stores.AsNoTracking().AnyAsync(item => item.Id == request.StoreId, cancellationToken))
            {
                AdminValidationHelper.AddError(errors, nameof(request.StoreId), $"Store '{request.StoreId}' does not exist.");
            }

            ValidateContentFields(
                request.HeroImageUrl,
                request.FeaturedLinks,
                request.Seo,
                errors);

            AdminValidationHelper.ThrowIfAny(errors, "Store page content request validation failed.");
        }

        private static void ValidateContentFields(
            string? heroImageUrl,
            FeaturedLinkAdminDto[]? featuredLinks,
            SeoAdminDto? seo,
            IDictionary<string, string[]> errors)
        {
            if (!string.IsNullOrWhiteSpace(heroImageUrl) && !AdminValidationHelper.IsValidAbsoluteUrl(heroImageUrl))
            {
                AdminValidationHelper.AddError(errors, nameof(heroImageUrl), "HeroImageUrl must be a valid absolute URL.");
            }

            foreach (var link in featuredLinks ?? Array.Empty<FeaturedLinkAdminDto>())
            {
                if (string.IsNullOrWhiteSpace(link.Title))
                {
                    AdminValidationHelper.AddError(errors, nameof(featuredLinks), "Each featured link must have a title.");
                }

                if (string.IsNullOrWhiteSpace(link.Url) || !AdminValidationHelper.IsValidAbsoluteUrl(link.Url))
                {
                    AdminValidationHelper.AddError(errors, nameof(featuredLinks), "Each featured link must have a valid absolute URL.");
                }

                if (!string.IsNullOrWhiteSpace(link.ImageUrl) && !AdminValidationHelper.IsValidAbsoluteUrl(link.ImageUrl))
                {
                    AdminValidationHelper.AddError(errors, nameof(featuredLinks), "Featured link image URL must be a valid absolute URL.");
                }
            }

            if (!string.IsNullOrWhiteSpace(seo?.OgImageUrl) && !AdminValidationHelper.IsValidAbsoluteUrl(seo.OgImageUrl))
            {
                AdminValidationHelper.AddError(errors, "seo.ogImageUrl", "Seo ogImageUrl must be a valid absolute URL.");
            }
        }

        private static StorePageContentAdminDto Map(StorePageContent entity)
        {
            return new StorePageContentAdminDto(
                entity.StoreId,
                entity.IsPublished,
                entity.HeroTitle,
                entity.HeroSubtitle,
                entity.IntroTitle,
                entity.IntroText,
                entity.SeoText,
                entity.HeroImageUrl,
                AdminMappingHelper.ToFaqDtos(entity.Faq),
                AdminMappingHelper.ToFeaturedLinkDtos(entity.FeaturedLinks),
                AdminMappingHelper.ToMerchBlockDtos(entity.MerchBlocks),
                AdminMappingHelper.ToSeoDto(entity.Seo),
                entity.CreatedAtUtc,
                entity.UpdatedAtUtc);
        }
    }
}
