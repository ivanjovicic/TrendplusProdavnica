#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrendplusProdavnica.Application.Admin.Common;
using TrendplusProdavnica.Application.Admin.Dtos;
using TrendplusProdavnica.Application.Common.Caching;
using TrendplusProdavnica.Domain.Catalog;
using TrendplusProdavnica.Domain.Enums;
using TrendplusProdavnica.Infrastructure.Admin.Common;
using TrendplusProdavnica.Infrastructure.Persistence;
using ICollectionAdminService = TrendplusProdavnica.Application.Admin.Services.ICollectionAdminService;

namespace TrendplusProdavnica.Infrastructure.Admin.Services
{
    public class CollectionAdminService : ICollectionAdminService
    {
        private readonly TrendplusDbContext _db;
        private readonly IWebshopCacheInvalidationService _cacheInvalidationService;

        public CollectionAdminService(
            TrendplusDbContext db,
            IWebshopCacheInvalidationService cacheInvalidationService)
        {
            _db = db;
            _cacheInvalidationService = cacheInvalidationService;
        }

        public async Task<IReadOnlyList<CollectionAdminDto>> GetListAsync(CancellationToken cancellationToken = default)
        {
            return await _db.Collections.AsNoTracking()
                .OrderBy(entity => entity.SortOrder)
                .ThenBy(entity => entity.Name)
                .Select(entity => new CollectionAdminDto(
                    entity.Id,
                    entity.Name,
                    entity.Slug,
                    entity.CollectionType,
                    entity.ShortDescription,
                    entity.LongDescription,
                    entity.CoverImageUrl,
                    entity.ThumbnailImageUrl,
                    entity.BadgeText,
                    entity.StartAtUtc,
                    entity.EndAtUtc,
                    entity.IsFeatured,
                    entity.IsActive,
                    entity.SortOrder,
                    _db.ProductCollectionMaps.Count(map => map.CollectionId == entity.Id),
                    AdminMappingHelper.ToSeoDto(entity.Seo),
                    entity.CreatedAtUtc,
                    entity.UpdatedAtUtc))
                .ToArrayAsync(cancellationToken);
        }

        public async Task<CollectionAdminDto> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        {
            var entity = await _db.Collections.AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

            if (entity is null)
            {
                throw new AdminNotFoundException($"Collection with id '{id}' was not found.");
            }

            var productCount = await _db.ProductCollectionMaps.AsNoTracking()
                .CountAsync(map => map.CollectionId == entity.Id, cancellationToken);

            return Map(entity, productCount);
        }

        public async Task<CollectionAdminDto> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            var normalizedSlug = AdminValidationHelper.NormalizeSlug(slug);
            var entity = await _db.Collections.AsNoTracking()
                .FirstOrDefaultAsync(item => item.Slug == normalizedSlug, cancellationToken);

            if (entity is null)
            {
                throw new AdminNotFoundException($"Collection with slug '{slug}' was not found.");
            }

            var productCount = await _db.ProductCollectionMaps.AsNoTracking()
                .CountAsync(map => map.CollectionId == entity.Id, cancellationToken);

            return Map(entity, productCount);
        }

        public async Task<CollectionAdminDto> CreateAsync(TrendplusProdavnica.Application.Admin.Dtos.CreateCollectionRequest request, CancellationToken cancellationToken = default)
        {
            await ValidateAsync(
                request.Name,
                request.Slug,
                request.CollectionType,
                request.CoverImageUrl,
                request.ThumbnailImageUrl,
                request.StartAtUtc,
                request.EndAtUtc,
                request.Seo,
                null,
                cancellationToken);

            var now = DateTimeOffset.UtcNow;
            var entity = new Collection
            {
                Name = request.Name.Trim(),
                Slug = AdminValidationHelper.NormalizeSlug(request.Slug),
                CollectionType = request.CollectionType,
                ShortDescription = request.ShortDescription,
                LongDescription = request.LongDescription,
                CoverImageUrl = request.CoverImageUrl,
                ThumbnailImageUrl = request.ThumbnailImageUrl,
                BadgeText = request.BadgeText,
                StartAtUtc = request.StartAtUtc,
                EndAtUtc = request.EndAtUtc,
                IsFeatured = request.IsFeatured,
                IsActive = request.IsActive,
                SortOrder = request.SortOrder,
                Seo = AdminMappingHelper.ToSeoModel(request.Seo),
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            _db.Collections.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);
            await _cacheInvalidationService.InvalidateCollectionBySlugAsync(entity.Slug, cancellationToken);

            return Map(entity, 0);
        }

        public async Task<CollectionAdminDto> UpdateAsync(long id, TrendplusProdavnica.Application.Admin.Dtos.UpdateCollectionRequest request, CancellationToken cancellationToken = default)
        {
            var entity = await _db.Collections
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

            if (entity is null)
            {
                throw new AdminNotFoundException($"Collection with id '{id}' was not found.");
            }

            await ValidateAsync(
                request.Name,
                request.Slug,
                request.CollectionType,
                request.CoverImageUrl,
                request.ThumbnailImageUrl,
                request.StartAtUtc,
                request.EndAtUtc,
                request.Seo,
                id,
                cancellationToken);

            var previousSlug = entity.Slug;
            entity.Name = request.Name.Trim();
            entity.Slug = AdminValidationHelper.NormalizeSlug(request.Slug);
            entity.CollectionType = request.CollectionType;
            entity.ShortDescription = request.ShortDescription;
            entity.LongDescription = request.LongDescription;
            entity.CoverImageUrl = request.CoverImageUrl;
            entity.ThumbnailImageUrl = request.ThumbnailImageUrl;
            entity.BadgeText = request.BadgeText;
            entity.StartAtUtc = request.StartAtUtc;
            entity.EndAtUtc = request.EndAtUtc;
            entity.IsFeatured = request.IsFeatured;
            entity.IsActive = request.IsActive;
            entity.SortOrder = request.SortOrder;
            entity.Seo = AdminMappingHelper.ToSeoModel(request.Seo);
            entity.UpdatedAtUtc = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
            if (!string.Equals(previousSlug, entity.Slug, StringComparison.OrdinalIgnoreCase))
            {
                await _cacheInvalidationService.InvalidateCollectionBySlugAsync(previousSlug, cancellationToken);
            }
            await _cacheInvalidationService.InvalidateCollectionBySlugAsync(entity.Slug, cancellationToken);
            var productCount = await _db.ProductCollectionMaps.AsNoTracking()
                .CountAsync(map => map.CollectionId == entity.Id, cancellationToken);

            return Map(entity, productCount);
        }

        public async Task<CollectionAdminDto> ArchiveAsync(long id, CancellationToken cancellationToken = default)
        {
            var entity = await _db.Collections
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

            if (entity is null)
            {
                throw new AdminNotFoundException($"Collection with id '{id}' was not found.");
            }

            entity.IsActive = false;
            entity.UpdatedAtUtc = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            await _cacheInvalidationService.InvalidateCollectionBySlugAsync(entity.Slug, cancellationToken);
            var productCount = await _db.ProductCollectionMaps.AsNoTracking()
                .CountAsync(map => map.CollectionId == entity.Id, cancellationToken);

            return Map(entity, productCount);
        }

        public async Task<CollectionAdminDto> UnarchiveAsync(long id, CancellationToken cancellationToken = default)
        {
            var entity = await _db.Collections
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

            if (entity is null)
            {
                throw new AdminNotFoundException($"Collection with id '{id}' was not found.");
            }

            entity.IsActive = true;
            entity.UpdatedAtUtc = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            await _cacheInvalidationService.InvalidateCollectionBySlugAsync(entity.Slug, cancellationToken);
            var productCount = await _db.ProductCollectionMaps.AsNoTracking()
                .CountAsync(map => map.CollectionId == entity.Id, cancellationToken);

            return Map(entity, productCount);
        }

        private async Task ValidateAsync(
            string name,
            string slug,
            CollectionType collectionType,
            string? coverImageUrl,
            string? thumbnailImageUrl,
            DateTimeOffset? startAtUtc,
            DateTimeOffset? endAtUtc,
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

            if (!Enum.IsDefined(collectionType))
            {
                AdminValidationHelper.AddError(errors, nameof(collectionType), "CollectionType is not valid.");
            }

            if (startAtUtc.HasValue && endAtUtc.HasValue && startAtUtc.Value > endAtUtc.Value)
            {
                AdminValidationHelper.AddError(errors, "dateRange", "StartAtUtc must be earlier than or equal to EndAtUtc.");
            }

            if (!string.IsNullOrWhiteSpace(coverImageUrl) && !AdminValidationHelper.IsValidAbsoluteUrl(coverImageUrl))
            {
                AdminValidationHelper.AddError(errors, nameof(coverImageUrl), "CoverImageUrl must be a valid absolute URL.");
            }

            if (!string.IsNullOrWhiteSpace(thumbnailImageUrl) && !AdminValidationHelper.IsValidAbsoluteUrl(thumbnailImageUrl))
            {
                AdminValidationHelper.AddError(errors, nameof(thumbnailImageUrl), "ThumbnailImageUrl must be a valid absolute URL.");
            }

            if (!string.IsNullOrWhiteSpace(seo?.OgImageUrl) && !AdminValidationHelper.IsValidAbsoluteUrl(seo.OgImageUrl))
            {
                AdminValidationHelper.AddError(errors, "seo.ogImageUrl", "Seo ogImageUrl must be a valid absolute URL.");
            }

            AdminValidationHelper.ThrowIfAny(errors, "Collection request validation failed.");

            var normalizedSlug = AdminValidationHelper.NormalizeSlug(slug);
            var slugExists = await _db.Collections.AsNoTracking()
                .AnyAsync(item => item.Slug == normalizedSlug && (!excludeId.HasValue || item.Id != excludeId.Value), cancellationToken);

            if (slugExists)
            {
                throw new AdminConflictException($"Collection slug '{normalizedSlug}' already exists.");
            }
        }

        private static CollectionAdminDto Map(Collection entity, int productCount)
        {
            return new CollectionAdminDto(
                entity.Id,
                entity.Name,
                entity.Slug,
                entity.CollectionType,
                entity.ShortDescription,
                entity.LongDescription,
                entity.CoverImageUrl,
                entity.ThumbnailImageUrl,
                entity.BadgeText,
                entity.StartAtUtc,
                entity.EndAtUtc,
                entity.IsFeatured,
                entity.IsActive,
                entity.SortOrder,
                productCount,
                AdminMappingHelper.ToSeoDto(entity.Seo),
                entity.CreatedAtUtc,
                entity.UpdatedAtUtc);
        }
    }
}
