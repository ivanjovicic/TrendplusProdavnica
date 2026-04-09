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
    public class HomePageAdminService : IHomePageAdminService
    {
        private readonly TrendplusDbContext _db;
        private readonly IWebshopCacheInvalidationService _cacheInvalidationService;

        public HomePageAdminService(
            TrendplusDbContext db,
            IWebshopCacheInvalidationService cacheInvalidationService)
        {
            _db = db;
            _cacheInvalidationService = cacheInvalidationService;
        }

        public async Task<HomePageAdminDto> GetCurrentAsync(CancellationToken cancellationToken = default)
        {
            var entity = await FindCurrentAsync(cancellationToken);

            if (entity is null)
            {
                throw new AdminNotFoundException("Home page was not found.");
            }

            return Map(entity);
        }

        public async Task<HomePageAdminDto> UpdateCurrentAsync(UpdateHomePageRequest request, CancellationToken cancellationToken = default)
        {
            ValidateRequest(request);

            var now = DateTimeOffset.UtcNow;
            var entity = await FindCurrentForUpdateAsync(cancellationToken);
            if (entity is null)
            {
                entity = new HomePage
                {
                    CreatedAtUtc = now
                };
                _db.HomePages.Add(entity);
            }

            entity.Title = request.Title.Trim();
            entity.Slug = request.Slug.Trim();
            entity.Seo = AdminMappingHelper.ToSeoModel(request.Seo);
            entity.Modules = AdminMappingHelper.ToHomeModuleModels(request.Modules ?? Array.Empty<HomeModuleAdminDto>());
            entity.UpdatedAtUtc = now;

            await _db.SaveChangesAsync(cancellationToken);
            await _cacheInvalidationService.InvalidateHomePageAsync(cancellationToken);
            return Map(entity);
        }

        public async Task<HomePageAdminDto> PublishCurrentAsync(CancellationToken cancellationToken = default)
        {
            var entity = await FindCurrentForUpdateAsync(cancellationToken);
            if (entity is null)
            {
                throw new AdminNotFoundException("Home page was not found.");
            }

            var now = DateTimeOffset.UtcNow;

            var currentlyPublished = await _db.HomePages
                .Where(item => item.IsPublished && item.Id != entity.Id)
                .ToListAsync(cancellationToken);

            foreach (var page in currentlyPublished)
            {
                page.IsPublished = false;
                page.UpdatedAtUtc = now;
            }

            entity.IsPublished = true;
            entity.PublishedAtUtc = now;
            entity.UpdatedAtUtc = now;

            await _db.SaveChangesAsync(cancellationToken);
            await _cacheInvalidationService.InvalidateHomePageAsync(cancellationToken);
            return Map(entity);
        }

        private static void ValidateRequest(UpdateHomePageRequest request)
        {
            var errors = new Dictionary<string, string[]>();

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                AdminValidationHelper.AddError(errors, nameof(request.Title), "Title is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Slug))
            {
                AdminValidationHelper.AddError(errors, nameof(request.Slug), "Slug is required.");
            }
            else if (request.Slug != "/" && !AdminValidationHelper.IsValidSlug(AdminValidationHelper.NormalizeSlug(request.Slug)))
            {
                AdminValidationHelper.AddError(errors, nameof(request.Slug), "Slug must be '/' or a valid slug.");
            }

            if (!string.IsNullOrWhiteSpace(request.Seo?.OgImageUrl) && !AdminValidationHelper.IsValidAbsoluteUrl(request.Seo.OgImageUrl))
            {
                AdminValidationHelper.AddError(errors, "seo.ogImageUrl", "Seo ogImageUrl must be a valid absolute URL.");
            }

            if ((request.Modules ?? Array.Empty<HomeModuleAdminDto>()).Any(item => string.IsNullOrWhiteSpace(item.Type)))
            {
                AdminValidationHelper.AddError(errors, nameof(request.Modules), "Each module must have a type.");
            }

            AdminValidationHelper.ThrowIfAny(errors, "Home page request validation failed.");
        }

        private async Task<HomePage?> FindCurrentAsync(CancellationToken cancellationToken)
        {
            return await _db.HomePages.AsNoTracking()
                .OrderByDescending(item => item.IsPublished)
                .ThenByDescending(item => item.UpdatedAtUtc)
                .ThenByDescending(item => item.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private async Task<HomePage?> FindCurrentForUpdateAsync(CancellationToken cancellationToken)
        {
            return await _db.HomePages
                .OrderByDescending(item => item.IsPublished)
                .ThenByDescending(item => item.UpdatedAtUtc)
                .ThenByDescending(item => item.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private static HomePageAdminDto Map(HomePage entity)
        {
            return new HomePageAdminDto(
                entity.Id,
                entity.Title,
                entity.Slug,
                entity.IsPublished,
                entity.PublishedAtUtc,
                AdminMappingHelper.ToSeoDto(entity.Seo),
                AdminMappingHelper.ToHomeModuleDtos(entity.Modules),
                entity.CreatedAtUtc,
                entity.UpdatedAtUtc);
        }
    }
}
