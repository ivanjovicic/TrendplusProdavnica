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
using TrendplusProdavnica.Domain.Content;
using TrendplusProdavnica.Domain.Enums;
using TrendplusProdavnica.Infrastructure.Admin.Common;
using TrendplusProdavnica.Infrastructure.Persistence;

namespace TrendplusProdavnica.Infrastructure.Admin.Services
{
    public class TrustPageAdminService : ITrustPageAdminService
    {
        private readonly TrendplusDbContext _db;

        public TrustPageAdminService(TrendplusDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<TrustPageAdminDto>> GetListAsync(CancellationToken cancellationToken = default)
        {
            var entities = await _db.TrustPages.AsNoTracking()
                .OrderBy(entity => entity.PageKind)
                .ThenBy(entity => entity.Title)
                .ToArrayAsync(cancellationToken);

            return entities.Select(Map).ToArray();
        }

        public async Task<TrustPageAdminDto> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        {
            var entity = await _db.TrustPages.AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

            if (entity is null)
            {
                throw new AdminNotFoundException($"Trust page with id '{id}' was not found.");
            }

            return Map(entity);
        }

        public async Task<TrustPageAdminDto> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            var normalizedSlug = AdminValidationHelper.NormalizeSlug(slug);
            var entity = await _db.TrustPages.AsNoTracking()
                .FirstOrDefaultAsync(item => item.Slug == normalizedSlug, cancellationToken);

            if (entity is null)
            {
                throw new AdminNotFoundException($"Trust page with slug '{slug}' was not found.");
            }

            return Map(entity);
        }

        public async Task<TrustPageAdminDto> GetByKindAsync(TrustPageKind kind, CancellationToken cancellationToken = default)
        {
            var entity = await _db.TrustPages.AsNoTracking()
                .FirstOrDefaultAsync(item => item.PageKind == kind, cancellationToken);

            if (entity is null)
            {
                throw new AdminNotFoundException($"Trust page with kind '{kind}' was not found.");
            }

            return Map(entity);
        }

        public async Task<TrustPageAdminDto> CreateAsync(CreateTrustPageRequest request, CancellationToken cancellationToken = default)
        {
            await ValidateAsync(request, null, cancellationToken);

            var now = DateTimeOffset.UtcNow;
            var entity = new TrustPage
            {
                PageKind = request.PageKind,
                Title = request.Title.Trim(),
                Slug = AdminValidationHelper.NormalizeSlug(request.Slug),
                Body = request.Body,
                IsPublished = request.IsPublished,
                Seo = AdminMappingHelper.ToSeoModel(request.Seo),
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            _db.TrustPages.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);
            return Map(entity);
        }

        public async Task<TrustPageAdminDto> UpdateAsync(long id, UpdateTrustPageRequest request, CancellationToken cancellationToken = default)
        {
            var entity = await _db.TrustPages
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

            if (entity is null)
            {
                throw new AdminNotFoundException($"Trust page with id '{id}' was not found.");
            }

            await ValidateAsync(request, id, cancellationToken);

            entity.PageKind = request.PageKind;
            entity.Title = request.Title.Trim();
            entity.Slug = AdminValidationHelper.NormalizeSlug(request.Slug);
            entity.Body = request.Body;
            entity.IsPublished = request.IsPublished;
            entity.Seo = AdminMappingHelper.ToSeoModel(request.Seo);
            entity.UpdatedAtUtc = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
            return Map(entity);
        }

        public async Task<TrustPageAdminDto> UnpublishAsync(long id, CancellationToken cancellationToken = default)
        {
            var entity = await _db.TrustPages
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

            if (entity is null)
            {
                throw new AdminNotFoundException($"Trust page with id '{id}' was not found.");
            }

            entity.IsPublished = false;
            entity.UpdatedAtUtc = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            return Map(entity);
        }

        private async Task ValidateAsync(CreateTrustPageRequest request, long? excludeId, CancellationToken cancellationToken)
        {
            var errors = new Dictionary<string, string[]>();
            ValidateShared(request.PageKind, request.Title, request.Slug, request.Body, request.Seo, errors);
            AdminValidationHelper.ThrowIfAny(errors, "Trust page request validation failed.");

            await EnsureUniquenessAsync(request.PageKind, request.Slug, excludeId, cancellationToken);
        }

        private async Task ValidateAsync(UpdateTrustPageRequest request, long? excludeId, CancellationToken cancellationToken)
        {
            var errors = new Dictionary<string, string[]>();
            ValidateShared(request.PageKind, request.Title, request.Slug, request.Body, request.Seo, errors);
            AdminValidationHelper.ThrowIfAny(errors, "Trust page request validation failed.");

            await EnsureUniquenessAsync(request.PageKind, request.Slug, excludeId, cancellationToken);
        }

        private static void ValidateShared(
            TrustPageKind pageKind,
            string title,
            string slug,
            string body,
            SeoAdminDto? seo,
            IDictionary<string, string[]> errors)
        {
            if (!Enum.IsDefined(pageKind))
            {
                AdminValidationHelper.AddError(errors, nameof(pageKind), "PageKind is not valid.");
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                AdminValidationHelper.AddError(errors, nameof(title), "Title is required.");
            }

            if (string.IsNullOrWhiteSpace(slug))
            {
                AdminValidationHelper.AddError(errors, nameof(slug), "Slug is required.");
            }
            else if (!AdminValidationHelper.IsValidSlug(AdminValidationHelper.NormalizeSlug(slug)))
            {
                AdminValidationHelper.AddError(errors, nameof(slug), "Slug must contain only lowercase letters, numbers and hyphens.");
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                AdminValidationHelper.AddError(errors, nameof(body), "Body is required.");
            }

            if (!string.IsNullOrWhiteSpace(seo?.OgImageUrl) && !AdminValidationHelper.IsValidAbsoluteUrl(seo.OgImageUrl))
            {
                AdminValidationHelper.AddError(errors, "seo.ogImageUrl", "Seo ogImageUrl must be a valid absolute URL.");
            }
        }

        private async Task EnsureUniquenessAsync(TrustPageKind kind, string slug, long? excludeId, CancellationToken cancellationToken)
        {
            var normalizedSlug = AdminValidationHelper.NormalizeSlug(slug);

            var slugExists = await _db.TrustPages.AsNoTracking()
                .AnyAsync(item => item.Slug == normalizedSlug && (!excludeId.HasValue || item.Id != excludeId.Value), cancellationToken);
            if (slugExists)
            {
                throw new AdminConflictException($"Trust page slug '{normalizedSlug}' already exists.");
            }

            var kindExists = await _db.TrustPages.AsNoTracking()
                .AnyAsync(item => item.PageKind == kind && (!excludeId.HasValue || item.Id != excludeId.Value), cancellationToken);
            if (kindExists)
            {
                throw new AdminConflictException($"Trust page kind '{kind}' already exists.");
            }
        }

        private static TrustPageAdminDto Map(TrustPage entity)
        {
            return new TrustPageAdminDto(
                entity.Id,
                entity.PageKind,
                entity.Title,
                entity.Slug,
                entity.Body,
                entity.IsPublished,
                AdminMappingHelper.ToSeoDto(entity.Seo),
                entity.CreatedAtUtc,
                entity.UpdatedAtUtc);
        }
    }
}
