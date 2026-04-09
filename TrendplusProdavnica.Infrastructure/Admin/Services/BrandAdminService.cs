#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TrendplusProdavnica.Application.Admin.Common;
using TrendplusProdavnica.Application.Admin.Dtos;
using TrendplusProdavnica.Application.Admin.Services;
using TrendplusProdavnica.Application.Common.Caching;
using TrendplusProdavnica.Application.Search.Services;
using TrendplusProdavnica.Domain.Catalog;
using TrendplusProdavnica.Infrastructure.Admin.Common;
using TrendplusProdavnica.Infrastructure.Persistence;

namespace TrendplusProdavnica.Infrastructure.Admin.Services
{
    public class BrandAdminService : IBrandAdminService
    {
        private readonly TrendplusDbContext _db;
        private readonly IWebshopCacheInvalidationService _cacheInvalidationService;
        private readonly IProductSearchIndexService _searchIndexService;
        private readonly ILogger<BrandAdminService> _logger;

        public BrandAdminService(
            TrendplusDbContext db,
            IWebshopCacheInvalidationService cacheInvalidationService,
            IProductSearchIndexService searchIndexService,
            ILogger<BrandAdminService> logger)
        {
            _db = db;
            _cacheInvalidationService = cacheInvalidationService;
            _searchIndexService = searchIndexService;
            _logger = logger;
        }

        public async Task<IReadOnlyList<BrandAdminDto>> GetListAsync(CancellationToken cancellationToken = default)
        {
            var entities = await _db.Brands.AsNoTracking()
                .OrderBy(entity => entity.SortOrder)
                .ThenBy(entity => entity.Name)
                .ToArrayAsync(cancellationToken);

            return entities.Select(Map).ToArray();
        }

        public async Task<BrandAdminDto> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        {
            var entity = await _db.Brands.AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

            if (entity is null)
            {
                throw new AdminNotFoundException($"Brand with id '{id}' was not found.");
            }

            return Map(entity);
        }

        public async Task<BrandAdminDto> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            var normalizedSlug = AdminValidationHelper.NormalizeSlug(slug);
            var entity = await _db.Brands.AsNoTracking()
                .FirstOrDefaultAsync(item => item.Slug == normalizedSlug, cancellationToken);

            if (entity is null)
            {
                throw new AdminNotFoundException($"Brand with slug '{slug}' was not found.");
            }

            return Map(entity);
        }

        public async Task<BrandAdminDto> CreateAsync(CreateBrandRequest request, CancellationToken cancellationToken = default)
        {
            await ValidateAsync(request.Name, request.Slug, request.LogoUrl, request.CoverImageUrl, request.WebsiteUrl, request.Seo, null, cancellationToken);

            var now = DateTimeOffset.UtcNow;
            var entity = new Brand
            {
                Name = request.Name.Trim(),
                Slug = AdminValidationHelper.NormalizeSlug(request.Slug),
                ShortDescription = request.ShortDescription,
                LongDescription = request.LongDescription,
                LogoUrl = request.LogoUrl,
                CoverImageUrl = request.CoverImageUrl,
                WebsiteUrl = request.WebsiteUrl,
                IsFeatured = request.IsFeatured,
                IsActive = request.IsActive,
                SortOrder = request.SortOrder,
                Seo = AdminMappingHelper.ToSeoModel(request.Seo),
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            _db.Brands.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);
            await _cacheInvalidationService.InvalidateBrandBySlugAsync(entity.Slug, cancellationToken);

            return Map(entity);
        }

        public async Task<BrandAdminDto> UpdateAsync(long id, UpdateBrandRequest request, CancellationToken cancellationToken = default)
        {
            var entity = await _db.Brands
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

            if (entity is null)
            {
                throw new AdminNotFoundException($"Brand with id '{id}' was not found.");
            }

            await ValidateAsync(request.Name, request.Slug, request.LogoUrl, request.CoverImageUrl, request.WebsiteUrl, request.Seo, id, cancellationToken);

            var previousSlug = entity.Slug;
            entity.Name = request.Name.Trim();
            entity.Slug = AdminValidationHelper.NormalizeSlug(request.Slug);
            entity.ShortDescription = request.ShortDescription;
            entity.LongDescription = request.LongDescription;
            entity.LogoUrl = request.LogoUrl;
            entity.CoverImageUrl = request.CoverImageUrl;
            entity.WebsiteUrl = request.WebsiteUrl;
            entity.IsFeatured = request.IsFeatured;
            entity.IsActive = request.IsActive;
            entity.SortOrder = request.SortOrder;
            entity.Seo = AdminMappingHelper.ToSeoModel(request.Seo);
            entity.UpdatedAtUtc = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
            if (!string.Equals(previousSlug, entity.Slug, StringComparison.OrdinalIgnoreCase))
            {
                await _cacheInvalidationService.InvalidateBrandBySlugAsync(previousSlug, cancellationToken);
            }
            await _cacheInvalidationService.InvalidateBrandBySlugAsync(entity.Slug, cancellationToken);
            await TryReindexAllAsync(cancellationToken);
            return Map(entity);
        }

        public async Task<BrandAdminDto> DeactivateAsync(long id, CancellationToken cancellationToken = default)
        {
            var entity = await _db.Brands
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

            if (entity is null)
            {
                throw new AdminNotFoundException($"Brand with id '{id}' was not found.");
            }

            entity.IsActive = false;
            entity.UpdatedAtUtc = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            await _cacheInvalidationService.InvalidateBrandBySlugAsync(entity.Slug, cancellationToken);
            await TryReindexAllAsync(cancellationToken);

            return Map(entity);
        }

        private async Task TryReindexAllAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _searchIndexService.ReindexAllAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Full product reindex failed after brand change.");
            }
        }

        private async Task ValidateAsync(
            string name,
            string slug,
            string? logoUrl,
            string? coverImageUrl,
            string? websiteUrl,
            SeoAdminDto? seo,
            long? excludeId,
            CancellationToken cancellationToken)
        {
            var errors = new Dictionary<string, string[]>();

            if (string.IsNullOrWhiteSpace(name))
            {
                AdminValidationHelper.AddError(errors, nameof(name), "Name is required.");
            }

            if (string.IsNullOrWhiteSpace(slug))
            {
                AdminValidationHelper.AddError(errors, nameof(slug), "Slug is required.");
            }
            else if (!AdminValidationHelper.IsValidSlug(AdminValidationHelper.NormalizeSlug(slug)))
            {
                AdminValidationHelper.AddError(errors, nameof(slug), "Slug must contain only lowercase letters, numbers and hyphens.");
            }

            if (!string.IsNullOrWhiteSpace(logoUrl) && !AdminValidationHelper.IsValidAbsoluteUrl(logoUrl))
            {
                AdminValidationHelper.AddError(errors, nameof(logoUrl), "LogoUrl must be a valid absolute URL.");
            }

            if (!string.IsNullOrWhiteSpace(coverImageUrl) && !AdminValidationHelper.IsValidAbsoluteUrl(coverImageUrl))
            {
                AdminValidationHelper.AddError(errors, nameof(coverImageUrl), "CoverImageUrl must be a valid absolute URL.");
            }

            if (!string.IsNullOrWhiteSpace(websiteUrl) && !AdminValidationHelper.IsValidAbsoluteUrl(websiteUrl))
            {
                AdminValidationHelper.AddError(errors, nameof(websiteUrl), "WebsiteUrl must be a valid absolute URL.");
            }

            if (!string.IsNullOrWhiteSpace(seo?.OgImageUrl) && !AdminValidationHelper.IsValidAbsoluteUrl(seo.OgImageUrl))
            {
                AdminValidationHelper.AddError(errors, "seo.ogImageUrl", "Seo ogImageUrl must be a valid absolute URL.");
            }

            AdminValidationHelper.ThrowIfAny(errors, "Brand request validation failed.");

            var normalizedSlug = AdminValidationHelper.NormalizeSlug(slug);
            var slugExists = await _db.Brands.AsNoTracking()
                .AnyAsync(item => item.Slug == normalizedSlug && (!excludeId.HasValue || item.Id != excludeId.Value), cancellationToken);

            if (slugExists)
            {
                throw new AdminConflictException($"Brand slug '{normalizedSlug}' already exists.");
            }
        }

        private static BrandAdminDto Map(Brand entity)
        {
            return new BrandAdminDto(
                entity.Id,
                entity.Name,
                entity.Slug,
                entity.ShortDescription,
                entity.LongDescription,
                entity.LogoUrl,
                entity.CoverImageUrl,
                entity.WebsiteUrl,
                entity.IsFeatured,
                entity.IsActive,
                entity.SortOrder,
                AdminMappingHelper.ToSeoDto(entity.Seo),
                entity.CreatedAtUtc,
                entity.UpdatedAtUtc);
        }
    }
}
