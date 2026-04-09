#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrendplusProdavnica.Application.Admin.Common;
using TrendplusProdavnica.Application.Admin.Dtos;
using TrendplusProdavnica.Application.Admin.Services;
using TrendplusProdavnica.Application.Common.Caching;
using TrendplusProdavnica.Domain.Inventory;
using TrendplusProdavnica.Infrastructure.Admin.Common;
using TrendplusProdavnica.Infrastructure.Persistence;

namespace TrendplusProdavnica.Infrastructure.Admin.Services
{
    public class StoreAdminService : IStoreAdminService
    {
        private static readonly Regex PhoneRegex = new("^[0-9+()\\-\\s]{6,40}$", RegexOptions.Compiled);
        private readonly TrendplusDbContext _db;
        private readonly IWebshopCacheInvalidationService _cacheInvalidationService;

        public StoreAdminService(
            TrendplusDbContext db,
            IWebshopCacheInvalidationService cacheInvalidationService)
        {
            _db = db;
            _cacheInvalidationService = cacheInvalidationService;
        }

        public async Task<IReadOnlyList<StoreAdminDto>> GetListAsync(CancellationToken cancellationToken = default)
        {
            var entities = await _db.Stores.AsNoTracking()
                .OrderBy(entity => entity.SortOrder)
                .ThenBy(entity => entity.Name)
                .ToArrayAsync(cancellationToken);

            return entities.Select(Map).ToArray();
        }

        public async Task<StoreAdminDto> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        {
            var entity = await _db.Stores.AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

            if (entity is null)
            {
                throw new AdminNotFoundException($"Store with id '{id}' was not found.");
            }

            return Map(entity);
        }

        public async Task<StoreAdminDto> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            var normalizedSlug = AdminValidationHelper.NormalizeSlug(slug);
            var entity = await _db.Stores.AsNoTracking()
                .FirstOrDefaultAsync(item => item.Slug == normalizedSlug, cancellationToken);

            if (entity is null)
            {
                throw new AdminNotFoundException($"Store with slug '{slug}' was not found.");
            }

            return Map(entity);
        }

        public async Task<StoreAdminDto> CreateAsync(CreateStoreRequest request, CancellationToken cancellationToken = default)
        {
            await ValidateAsync(request, null, cancellationToken);

            var now = DateTimeOffset.UtcNow;
            var entity = new Store
            {
                Name = request.Name.Trim(),
                Slug = AdminValidationHelper.NormalizeSlug(request.Slug),
                City = request.City.Trim(),
                AddressLine1 = request.AddressLine1.Trim(),
                AddressLine2 = request.AddressLine2,
                PostalCode = request.PostalCode,
                MallName = request.MallName,
                Phone = request.Phone,
                Email = request.Email,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                WorkingHoursText = request.WorkingHoursText,
                ShortDescription = request.ShortDescription,
                CoverImageUrl = request.CoverImageUrl,
                DirectionsUrl = request.DirectionsUrl,
                IsActive = request.IsActive,
                SortOrder = request.SortOrder,
                Seo = AdminMappingHelper.ToSeoModel(request.Seo),
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            _db.Stores.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);
            await _cacheInvalidationService.InvalidateStoreBySlugAsync(entity.Slug, cancellationToken);
            return Map(entity);
        }

        public async Task<StoreAdminDto> UpdateAsync(long id, UpdateStoreRequest request, CancellationToken cancellationToken = default)
        {
            var entity = await _db.Stores
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

            if (entity is null)
            {
                throw new AdminNotFoundException($"Store with id '{id}' was not found.");
            }

            await ValidateAsync(request, id, cancellationToken);

            var previousSlug = entity.Slug;
            entity.Name = request.Name.Trim();
            entity.Slug = AdminValidationHelper.NormalizeSlug(request.Slug);
            entity.City = request.City.Trim();
            entity.AddressLine1 = request.AddressLine1.Trim();
            entity.AddressLine2 = request.AddressLine2;
            entity.PostalCode = request.PostalCode;
            entity.MallName = request.MallName;
            entity.Phone = request.Phone;
            entity.Email = request.Email;
            entity.Latitude = request.Latitude;
            entity.Longitude = request.Longitude;
            entity.WorkingHoursText = request.WorkingHoursText;
            entity.ShortDescription = request.ShortDescription;
            entity.CoverImageUrl = request.CoverImageUrl;
            entity.DirectionsUrl = request.DirectionsUrl;
            entity.IsActive = request.IsActive;
            entity.SortOrder = request.SortOrder;
            entity.Seo = AdminMappingHelper.ToSeoModel(request.Seo);
            entity.UpdatedAtUtc = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
            if (!string.Equals(previousSlug, entity.Slug, StringComparison.OrdinalIgnoreCase))
            {
                await _cacheInvalidationService.InvalidateStoreBySlugAsync(previousSlug, cancellationToken);
            }
            await _cacheInvalidationService.InvalidateStoreBySlugAsync(entity.Slug, cancellationToken);
            return Map(entity);
        }

        public async Task<StoreAdminDto> DeactivateAsync(long id, CancellationToken cancellationToken = default)
        {
            var entity = await _db.Stores
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

            if (entity is null)
            {
                throw new AdminNotFoundException($"Store with id '{id}' was not found.");
            }

            entity.IsActive = false;
            entity.UpdatedAtUtc = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            await _cacheInvalidationService.InvalidateStoreBySlugAsync(entity.Slug, cancellationToken);
            return Map(entity);
        }

        private async Task ValidateAsync(CreateStoreRequest request, long? excludeId, CancellationToken cancellationToken)
        {
            var errors = new Dictionary<string, string[]>();
            ValidateSharedFields(
                request.Name,
                request.Slug,
                request.City,
                request.AddressLine1,
                request.Phone,
                request.Email,
                request.Latitude,
                request.Longitude,
                request.CoverImageUrl,
                request.DirectionsUrl,
                request.Seo,
                errors);

            AdminValidationHelper.ThrowIfAny(errors, "Store request validation failed.");
            await EnsureUniqueSlugAsync(request.Slug, excludeId, cancellationToken);
        }

        private async Task ValidateAsync(UpdateStoreRequest request, long? excludeId, CancellationToken cancellationToken)
        {
            var errors = new Dictionary<string, string[]>();
            ValidateSharedFields(
                request.Name,
                request.Slug,
                request.City,
                request.AddressLine1,
                request.Phone,
                request.Email,
                request.Latitude,
                request.Longitude,
                request.CoverImageUrl,
                request.DirectionsUrl,
                request.Seo,
                errors);

            AdminValidationHelper.ThrowIfAny(errors, "Store request validation failed.");
            await EnsureUniqueSlugAsync(request.Slug, excludeId, cancellationToken);
        }

        private static void ValidateSharedFields(
            string name,
            string slug,
            string city,
            string addressLine1,
            string? phone,
            string? email,
            decimal? latitude,
            decimal? longitude,
            string? coverImageUrl,
            string? directionsUrl,
            SeoAdminDto? seo,
            IDictionary<string, string[]> errors)
        {
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

            if (string.IsNullOrWhiteSpace(city))
            {
                AdminValidationHelper.AddError(errors, nameof(city), "City is required.");
            }

            if (string.IsNullOrWhiteSpace(addressLine1))
            {
                AdminValidationHelper.AddError(errors, nameof(addressLine1), "AddressLine1 is required.");
            }

            if (!string.IsNullOrWhiteSpace(phone) && !PhoneRegex.IsMatch(phone))
            {
                AdminValidationHelper.AddError(errors, nameof(phone), "Phone format is not valid.");
            }

            if (!string.IsNullOrWhiteSpace(email) && !AdminValidationHelper.IsValidEmail(email))
            {
                AdminValidationHelper.AddError(errors, nameof(email), "Email format is not valid.");
            }

            if (latitude.HasValue && !AdminValidationHelper.IsValidLatitude(latitude.Value))
            {
                AdminValidationHelper.AddError(errors, nameof(latitude), "Latitude must be between -90 and 90.");
            }

            if (longitude.HasValue && !AdminValidationHelper.IsValidLongitude(longitude.Value))
            {
                AdminValidationHelper.AddError(errors, nameof(longitude), "Longitude must be between -180 and 180.");
            }

            if (!string.IsNullOrWhiteSpace(coverImageUrl) && !AdminValidationHelper.IsValidAbsoluteUrl(coverImageUrl))
            {
                AdminValidationHelper.AddError(errors, nameof(coverImageUrl), "CoverImageUrl must be a valid absolute URL.");
            }

            if (!string.IsNullOrWhiteSpace(directionsUrl) && !AdminValidationHelper.IsValidAbsoluteUrl(directionsUrl))
            {
                AdminValidationHelper.AddError(errors, nameof(directionsUrl), "DirectionsUrl must be a valid absolute URL.");
            }

            if (!string.IsNullOrWhiteSpace(seo?.OgImageUrl) && !AdminValidationHelper.IsValidAbsoluteUrl(seo.OgImageUrl))
            {
                AdminValidationHelper.AddError(errors, "seo.ogImageUrl", "Seo ogImageUrl must be a valid absolute URL.");
            }
        }

        private async Task EnsureUniqueSlugAsync(string slug, long? excludeId, CancellationToken cancellationToken)
        {
            var normalizedSlug = AdminValidationHelper.NormalizeSlug(slug);
            var exists = await _db.Stores.AsNoTracking()
                .AnyAsync(item => item.Slug == normalizedSlug && (!excludeId.HasValue || item.Id != excludeId.Value), cancellationToken);

            if (exists)
            {
                throw new AdminConflictException($"Store slug '{normalizedSlug}' already exists.");
            }
        }

        private static StoreAdminDto Map(Store entity)
        {
            return new StoreAdminDto(
                entity.Id,
                entity.Name,
                entity.Slug,
                entity.City,
                entity.AddressLine1,
                entity.AddressLine2,
                entity.PostalCode,
                entity.MallName,
                entity.Phone,
                entity.Email,
                entity.Latitude,
                entity.Longitude,
                entity.WorkingHoursText,
                entity.ShortDescription,
                entity.CoverImageUrl,
                entity.DirectionsUrl,
                entity.IsActive,
                entity.SortOrder,
                AdminMappingHelper.ToSeoDto(entity.Seo),
                entity.CreatedAtUtc,
                entity.UpdatedAtUtc);
        }
    }
}
