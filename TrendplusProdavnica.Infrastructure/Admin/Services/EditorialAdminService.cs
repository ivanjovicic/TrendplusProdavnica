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
using TrendplusProdavnica.Domain.Enums;
using TrendplusProdavnica.Infrastructure.Admin.Common;
using TrendplusProdavnica.Infrastructure.Persistence;

namespace TrendplusProdavnica.Infrastructure.Admin.Services
{
    public class EditorialAdminService : IEditorialAdminService
    {
        private readonly TrendplusDbContext _db;
        private readonly IWebshopCacheInvalidationService _cacheInvalidationService;

        public EditorialAdminService(
            TrendplusDbContext db,
            IWebshopCacheInvalidationService cacheInvalidationService)
        {
            _db = db;
            _cacheInvalidationService = cacheInvalidationService;
        }

        public async Task<IReadOnlyList<EditorialArticleAdminDto>> GetListAsync(CancellationToken cancellationToken = default)
        {
            var articles = await _db.EditorialArticles.AsNoTracking()
                .OrderByDescending(entity => entity.UpdatedAtUtc)
                .ThenByDescending(entity => entity.Id)
                .ToArrayAsync(cancellationToken);

            return await MapArticlesAsync(articles, cancellationToken);
        }

        public async Task<EditorialArticleAdminDto> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        {
            var article = await _db.EditorialArticles.AsNoTracking()
                .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

            if (article is null)
            {
                throw new AdminNotFoundException($"Editorial article with id '{id}' was not found.");
            }

            var dtos = await MapArticlesAsync(new[] { article }, cancellationToken);
            return dtos[0];
        }

        public async Task<EditorialArticleAdminDto> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            var normalizedSlug = AdminValidationHelper.NormalizeSlug(slug);
            var article = await _db.EditorialArticles.AsNoTracking()
                .FirstOrDefaultAsync(entity => entity.Slug == normalizedSlug, cancellationToken);

            if (article is null)
            {
                throw new AdminNotFoundException($"Editorial article with slug '{slug}' was not found.");
            }

            var dtos = await MapArticlesAsync(new[] { article }, cancellationToken);
            return dtos[0];
        }

        public async Task<EditorialArticleAdminDto> CreateAsync(CreateEditorialArticleRequest request, CancellationToken cancellationToken = default)
        {
            await ValidateRequestAsync(request, null, cancellationToken);

            var now = DateTimeOffset.UtcNow;
            var entity = new EditorialArticle
            {
                Title = request.Title.Trim(),
                Slug = AdminValidationHelper.NormalizeSlug(request.Slug),
                Excerpt = request.Excerpt,
                CoverImageUrl = request.CoverImageUrl,
                Body = request.Body,
                Topic = request.Topic,
                AuthorName = request.AuthorName,
                Status = request.Status,
                PublishedAtUtc = request.Status == ContentStatus.Published
                    ? (request.PublishedAtUtc ?? now)
                    : request.PublishedAtUtc,
                Seo = AdminMappingHelper.ToSeoModel(request.Seo),
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            _db.EditorialArticles.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);

            await ReplaceLinksAsync(entity.Id, request, now, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            await OnEditorialChangedAsync(null, entity.Slug, cancellationToken);

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<EditorialArticleAdminDto> UpdateAsync(long id, UpdateEditorialArticleRequest request, CancellationToken cancellationToken = default)
        {
            var entity = await _db.EditorialArticles
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

            if (entity is null)
            {
                throw new AdminNotFoundException($"Editorial article with id '{id}' was not found.");
            }

            await ValidateRequestAsync(request, id, cancellationToken);

            var previousSlug = entity.Slug;
            entity.Title = request.Title.Trim();
            entity.Slug = AdminValidationHelper.NormalizeSlug(request.Slug);
            entity.Excerpt = request.Excerpt;
            entity.CoverImageUrl = request.CoverImageUrl;
            entity.Body = request.Body;
            entity.Topic = request.Topic;
            entity.AuthorName = request.AuthorName;
            entity.Status = request.Status;
            entity.PublishedAtUtc = request.Status == ContentStatus.Published
                ? (request.PublishedAtUtc ?? entity.PublishedAtUtc ?? DateTimeOffset.UtcNow)
                : request.PublishedAtUtc;
            entity.Seo = AdminMappingHelper.ToSeoModel(request.Seo);
            entity.UpdatedAtUtc = DateTimeOffset.UtcNow;

            await ReplaceLinksAsync(entity.Id, request, entity.UpdatedAtUtc, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            await OnEditorialChangedAsync(previousSlug, entity.Slug, cancellationToken);

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<EditorialArticleAdminDto> PublishAsync(long id, CancellationToken cancellationToken = default)
        {
            var entity = await _db.EditorialArticles
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

            if (entity is null)
            {
                throw new AdminNotFoundException($"Editorial article with id '{id}' was not found.");
            }

            entity.Status = ContentStatus.Published;
            entity.PublishedAtUtc ??= DateTimeOffset.UtcNow;
            entity.UpdatedAtUtc = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
            await OnEditorialChangedAsync(entity.Slug, entity.Slug, cancellationToken);
            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<EditorialArticleAdminDto> ArchiveAsync(long id, CancellationToken cancellationToken = default)
        {
            var entity = await _db.EditorialArticles
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

            if (entity is null)
            {
                throw new AdminNotFoundException($"Editorial article with id '{id}' was not found.");
            }

            entity.Status = ContentStatus.Archived;
            entity.UpdatedAtUtc = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
            await OnEditorialChangedAsync(entity.Slug, entity.Slug, cancellationToken);
            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        private async Task OnEditorialChangedAsync(
            string? previousSlug,
            string currentSlug,
            CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(previousSlug) &&
                !string.Equals(previousSlug, currentSlug, StringComparison.OrdinalIgnoreCase))
            {
                await _cacheInvalidationService.InvalidateEditorialBySlugAsync(previousSlug, cancellationToken);
            }

            await _cacheInvalidationService.InvalidateEditorialBySlugAsync(currentSlug, cancellationToken);
            await _cacheInvalidationService.InvalidateEditorialListAsync(cancellationToken);
        }

        private async Task ValidateRequestAsync(CreateEditorialArticleRequest request, long? excludeId, CancellationToken cancellationToken)
        {
            var errors = new Dictionary<string, string[]>();
            ValidateSharedRequest(
                request.Title,
                request.Slug,
                request.Body,
                request.Status,
                request.CoverImageUrl,
                request.Seo,
                request.RelatedProductIds,
                request.RelatedCategoryIds,
                request.RelatedBrandIds,
                request.RelatedCollectionIds,
                errors);

            await ValidateReferenceIdsAsync(
                request.RelatedProductIds,
                request.RelatedCategoryIds,
                request.RelatedBrandIds,
                request.RelatedCollectionIds,
                errors,
                cancellationToken);

            AdminValidationHelper.ThrowIfAny(errors, "Editorial article request validation failed.");

            var normalizedSlug = AdminValidationHelper.NormalizeSlug(request.Slug);
            var slugExists = await _db.EditorialArticles.AsNoTracking()
                .AnyAsync(item => item.Slug == normalizedSlug && (!excludeId.HasValue || item.Id != excludeId.Value), cancellationToken);

            if (slugExists)
            {
                throw new AdminConflictException($"Editorial article slug '{normalizedSlug}' already exists.");
            }
        }

        private async Task ValidateRequestAsync(UpdateEditorialArticleRequest request, long? excludeId, CancellationToken cancellationToken)
        {
            var errors = new Dictionary<string, string[]>();
            ValidateSharedRequest(
                request.Title,
                request.Slug,
                request.Body,
                request.Status,
                request.CoverImageUrl,
                request.Seo,
                request.RelatedProductIds,
                request.RelatedCategoryIds,
                request.RelatedBrandIds,
                request.RelatedCollectionIds,
                errors);

            await ValidateReferenceIdsAsync(
                request.RelatedProductIds,
                request.RelatedCategoryIds,
                request.RelatedBrandIds,
                request.RelatedCollectionIds,
                errors,
                cancellationToken);

            AdminValidationHelper.ThrowIfAny(errors, "Editorial article request validation failed.");

            var normalizedSlug = AdminValidationHelper.NormalizeSlug(request.Slug);
            var slugExists = await _db.EditorialArticles.AsNoTracking()
                .AnyAsync(item => item.Slug == normalizedSlug && (!excludeId.HasValue || item.Id != excludeId.Value), cancellationToken);

            if (slugExists)
            {
                throw new AdminConflictException($"Editorial article slug '{normalizedSlug}' already exists.");
            }
        }

        private static void ValidateSharedRequest(
            string title,
            string slug,
            string body,
            ContentStatus status,
            string? coverImageUrl,
            SeoAdminDto? seo,
            long[] relatedProductIds,
            long[] relatedCategoryIds,
            long[] relatedBrandIds,
            long[] relatedCollectionIds,
            IDictionary<string, string[]> errors)
        {
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

            if (!Enum.IsDefined(status))
            {
                AdminValidationHelper.AddError(errors, nameof(status), "Status is not valid.");
            }

            if (!string.IsNullOrWhiteSpace(coverImageUrl) && !AdminValidationHelper.IsValidAbsoluteUrl(coverImageUrl))
            {
                AdminValidationHelper.AddError(errors, nameof(coverImageUrl), "CoverImageUrl must be a valid absolute URL.");
            }

            if (!string.IsNullOrWhiteSpace(seo?.OgImageUrl) && !AdminValidationHelper.IsValidAbsoluteUrl(seo.OgImageUrl))
            {
                AdminValidationHelper.AddError(errors, "seo.ogImageUrl", "Seo ogImageUrl must be a valid absolute URL.");
            }

            if (relatedProductIds.Any(id => id <= 0))
            {
                AdminValidationHelper.AddError(errors, nameof(relatedProductIds), "RelatedProductIds must contain only positive ids.");
            }

            if (relatedCategoryIds.Any(id => id <= 0))
            {
                AdminValidationHelper.AddError(errors, nameof(relatedCategoryIds), "RelatedCategoryIds must contain only positive ids.");
            }

            if (relatedBrandIds.Any(id => id <= 0))
            {
                AdminValidationHelper.AddError(errors, nameof(relatedBrandIds), "RelatedBrandIds must contain only positive ids.");
            }

            if (relatedCollectionIds.Any(id => id <= 0))
            {
                AdminValidationHelper.AddError(errors, nameof(relatedCollectionIds), "RelatedCollectionIds must contain only positive ids.");
            }
        }

        private async Task ValidateReferenceIdsAsync(
            long[] relatedProductIds,
            long[] relatedCategoryIds,
            long[] relatedBrandIds,
            long[] relatedCollectionIds,
            IDictionary<string, string[]> errors,
            CancellationToken cancellationToken)
        {
            var productIds = NormalizeIds(relatedProductIds);
            if (productIds.Length > 0)
            {
                var existingCount = await _db.Products.AsNoTracking()
                    .CountAsync(entity => productIds.Contains(entity.Id), cancellationToken);
                if (existingCount != productIds.Length)
                {
                    AdminValidationHelper.AddError(errors, nameof(relatedProductIds), "One or more related products do not exist.");
                }
            }

            var categoryIds = NormalizeIds(relatedCategoryIds);
            if (categoryIds.Length > 0)
            {
                var existingCount = await _db.Categories.AsNoTracking()
                    .CountAsync(entity => categoryIds.Contains(entity.Id), cancellationToken);
                if (existingCount != categoryIds.Length)
                {
                    AdminValidationHelper.AddError(errors, nameof(relatedCategoryIds), "One or more related categories do not exist.");
                }
            }

            var brandIds = NormalizeIds(relatedBrandIds);
            if (brandIds.Length > 0)
            {
                var existingCount = await _db.Brands.AsNoTracking()
                    .CountAsync(entity => brandIds.Contains(entity.Id), cancellationToken);
                if (existingCount != brandIds.Length)
                {
                    AdminValidationHelper.AddError(errors, nameof(relatedBrandIds), "One or more related brands do not exist.");
                }
            }

            var collectionIds = NormalizeIds(relatedCollectionIds);
            if (collectionIds.Length > 0)
            {
                var existingCount = await _db.Collections.AsNoTracking()
                    .CountAsync(entity => collectionIds.Contains(entity.Id), cancellationToken);
                if (existingCount != collectionIds.Length)
                {
                    AdminValidationHelper.AddError(errors, nameof(relatedCollectionIds), "One or more related collections do not exist.");
                }
            }
        }

        private async Task ReplaceLinksAsync(long articleId, CreateEditorialArticleRequest request, DateTimeOffset now, CancellationToken cancellationToken)
        {
            await ReplaceLinksCoreAsync(
                articleId,
                request.RelatedProductIds,
                request.RelatedCategoryIds,
                request.RelatedBrandIds,
                request.RelatedCollectionIds,
                now,
                cancellationToken);
        }

        private async Task ReplaceLinksAsync(long articleId, UpdateEditorialArticleRequest request, DateTimeOffset now, CancellationToken cancellationToken)
        {
            await ReplaceLinksCoreAsync(
                articleId,
                request.RelatedProductIds,
                request.RelatedCategoryIds,
                request.RelatedBrandIds,
                request.RelatedCollectionIds,
                now,
                cancellationToken);
        }

        private async Task ReplaceLinksCoreAsync(
            long articleId,
            long[] relatedProductIds,
            long[] relatedCategoryIds,
            long[] relatedBrandIds,
            long[] relatedCollectionIds,
            DateTimeOffset now,
            CancellationToken cancellationToken)
        {
            var existingProductLinks = await _db.Set<EditorialArticleProduct>()
                .Where(entity => entity.EditorialArticleId == articleId)
                .ToListAsync(cancellationToken);
            var existingCategoryLinks = await _db.Set<EditorialArticleCategory>()
                .Where(entity => entity.EditorialArticleId == articleId)
                .ToListAsync(cancellationToken);
            var existingBrandLinks = await _db.Set<EditorialArticleBrand>()
                .Where(entity => entity.EditorialArticleId == articleId)
                .ToListAsync(cancellationToken);
            var existingCollectionLinks = await _db.Set<EditorialArticleCollection>()
                .Where(entity => entity.EditorialArticleId == articleId)
                .ToListAsync(cancellationToken);

            _db.RemoveRange(existingProductLinks);
            _db.RemoveRange(existingCategoryLinks);
            _db.RemoveRange(existingBrandLinks);
            _db.RemoveRange(existingCollectionLinks);

            var productIds = NormalizeIds(relatedProductIds);
            for (var index = 0; index < productIds.Length; index++)
            {
                _db.Set<EditorialArticleProduct>().Add(new EditorialArticleProduct
                {
                    EditorialArticleId = articleId,
                    ProductId = productIds[index],
                    SortOrder = index + 1,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                });
            }

            foreach (var categoryId in NormalizeIds(relatedCategoryIds))
            {
                _db.Set<EditorialArticleCategory>().Add(new EditorialArticleCategory
                {
                    EditorialArticleId = articleId,
                    CategoryId = categoryId,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                });
            }

            foreach (var brandId in NormalizeIds(relatedBrandIds))
            {
                _db.Set<EditorialArticleBrand>().Add(new EditorialArticleBrand
                {
                    EditorialArticleId = articleId,
                    BrandId = brandId,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                });
            }

            foreach (var collectionId in NormalizeIds(relatedCollectionIds))
            {
                _db.Set<EditorialArticleCollection>().Add(new EditorialArticleCollection
                {
                    EditorialArticleId = articleId,
                    CollectionId = collectionId,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                });
            }
        }

        private async Task<EditorialArticleAdminDto[]> MapArticlesAsync(EditorialArticle[] articles, CancellationToken cancellationToken)
        {
            if (articles.Length == 0)
            {
                return Array.Empty<EditorialArticleAdminDto>();
            }

            var articleIds = articles.Select(item => item.Id).ToArray();

            var productLinks = await _db.Set<EditorialArticleProduct>().AsNoTracking()
                .Where(entity => articleIds.Contains(entity.EditorialArticleId))
                .OrderBy(entity => entity.SortOrder)
                .Select(entity => new { entity.EditorialArticleId, entity.ProductId })
                .ToArrayAsync(cancellationToken);

            var categoryLinks = await _db.Set<EditorialArticleCategory>().AsNoTracking()
                .Where(entity => articleIds.Contains(entity.EditorialArticleId))
                .Select(entity => new { entity.EditorialArticleId, entity.CategoryId })
                .ToArrayAsync(cancellationToken);

            var brandLinks = await _db.Set<EditorialArticleBrand>().AsNoTracking()
                .Where(entity => articleIds.Contains(entity.EditorialArticleId))
                .Select(entity => new { entity.EditorialArticleId, entity.BrandId })
                .ToArrayAsync(cancellationToken);

            var collectionLinks = await _db.Set<EditorialArticleCollection>().AsNoTracking()
                .Where(entity => articleIds.Contains(entity.EditorialArticleId))
                .Select(entity => new { entity.EditorialArticleId, entity.CollectionId })
                .ToArrayAsync(cancellationToken);

            var productLookup = productLinks
                .GroupBy(item => item.EditorialArticleId)
                .ToDictionary(group => group.Key, group => group.Select(item => item.ProductId).Distinct().ToArray());
            var categoryLookup = categoryLinks
                .GroupBy(item => item.EditorialArticleId)
                .ToDictionary(group => group.Key, group => group.Select(item => item.CategoryId).Distinct().ToArray());
            var brandLookup = brandLinks
                .GroupBy(item => item.EditorialArticleId)
                .ToDictionary(group => group.Key, group => group.Select(item => item.BrandId).Distinct().ToArray());
            var collectionLookup = collectionLinks
                .GroupBy(item => item.EditorialArticleId)
                .ToDictionary(group => group.Key, group => group.Select(item => item.CollectionId).Distinct().ToArray());

            return articles.Select(article => new EditorialArticleAdminDto(
                article.Id,
                article.Title,
                article.Slug,
                article.Excerpt,
                article.CoverImageUrl,
                article.Body,
                article.Topic,
                article.AuthorName,
                article.Status,
                article.PublishedAtUtc,
                AdminMappingHelper.ToSeoDto(article.Seo),
                productLookup.TryGetValue(article.Id, out var productIds) ? productIds : Array.Empty<long>(),
                categoryLookup.TryGetValue(article.Id, out var categoryIds) ? categoryIds : Array.Empty<long>(),
                brandLookup.TryGetValue(article.Id, out var brandIds) ? brandIds : Array.Empty<long>(),
                collectionLookup.TryGetValue(article.Id, out var collectionIds) ? collectionIds : Array.Empty<long>(),
                article.CreatedAtUtc,
                article.UpdatedAtUtc)).ToArray();
        }

        private static long[] NormalizeIds(long[]? ids)
        {
            return ids?
                .Where(id => id > 0)
                .Distinct()
                .ToArray()
                ?? Array.Empty<long>();
        }
    }
}
