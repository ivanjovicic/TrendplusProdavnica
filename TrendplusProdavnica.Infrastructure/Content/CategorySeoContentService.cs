#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TrendplusProdavnica.Application.Content.CategorySeo;
using TrendplusProdavnica.Domain.Content;
using TrendplusProdavnica.Infrastructure.Persistence;
using ZiggyCreatures.Caching.Fusion;

namespace TrendplusProdavnica.Infrastructure.Content
{
    /// <summary>
    /// Implementacija servisa za upravljanje SEO landing stranicama kategorija
    /// </summary>
    public class CategorySeoContentService : ICategorySeoContentService
    {
        private readonly TrendplusDbContext _db;
        private readonly IFusionCache _cache;
        private readonly ILogger<CategorySeoContentService> _logger;

        private const string CacheKeyPrefix = "category_seo_";
        private const string CacheKeyAll = "category_seo_all";
        private const int CacheDurationMinutes = 30;

        public CategorySeoContentService(
            TrendplusDbContext db,
            IFusionCache cache,
            ILogger<CategorySeoContentService> logger)
        {
            _db = db;
            _cache = cache;
            _logger = logger;
        }

        public async Task<CategorySeoContentDto?> GetByCategoryIdAsync(long categoryId, bool useCache = true)
        {
            try
            {
                if (useCache)
                {
                    var cacheKey = $"{CacheKeyPrefix}{categoryId}";
                    return await _cache.GetOrSetAsync(
                        cacheKey,
                        async (ctx) =>
                        {
                            var entity = await _db.CategorySeoContents
                                .AsNoTracking()
                                .FirstOrDefaultAsync(x => x.CategoryId == categoryId);

                            return entity != null ? MapToDto(entity) : null;
                        },
                        new FusionCacheEntryOptions
                        {
                            Duration = TimeSpan.FromMinutes(CacheDurationMinutes)
                        });
                }

                var content = await _db.CategorySeoContents
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.CategoryId == categoryId);

                return content != null ? MapToDto(content) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri dohvati SEO sadržaja za kategoriju {CategoryId}", categoryId);
                return null;
            }
        }

        public async Task<IReadOnlyList<CategorySeoContentDto>> GetAllAsync(bool useCache = true)
        {
            try
            {
                if (useCache)
                {
                    return await _cache.GetOrSetAsync(
                        CacheKeyAll,
                        async (ctx) =>
                        {
                            var entities = await _db.CategorySeoContents
                                .AsNoTracking()
                                .ToListAsync();

                            return (IReadOnlyList<CategorySeoContentDto>)entities
                                .Select(MapToDto)
                                .ToList();
                        },
                        new FusionCacheEntryOptions
                        {
                            Duration = TimeSpan.FromMinutes(CacheDurationMinutes)
                        });
                }

                var all = await _db.CategorySeoContents
                    .AsNoTracking()
                    .ToListAsync();

                return all.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri dohvati svih SEO sadržaja");
                return new List<CategorySeoContentDto>();
            }
        }

        public async Task<CategorySeoContentDto> CreateAsync(
            CreateCategorySeoContentRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Provjeri da li već postoji
                var existing = await _db.CategorySeoContents
                    .FirstOrDefaultAsync(x => x.CategoryId == request.CategoryId, cancellationToken);

                if (existing != null)
                    throw new InvalidOperationException($"SEO sadržaj za kategoriju {request.CategoryId} već postoji");

                var entity = new CategorySeoContent(
                    request.CategoryId,
                    request.MetaTitle,
                    request.MetaDescription)
                {
                    IntroTitle = request.IntroTitle,
                    IntroText = request.IntroText,
                    MainContent = request.MainContent,
                    Faq = request.Faq
                };

                _db.CategorySeoContents.Add(entity);
                await _db.SaveChangesAsync(cancellationToken);

                await InvalidateCacheAsync();

                _logger.LogInformation(
                    "Kreiran SEO sadržaj za kategoriju {CategoryId}",
                    request.CategoryId);

                return MapToDto(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri kreiranju SEO sadržaja");
                throw;
            }
        }

        public async Task<CategorySeoContentDto> UpdateAsync(
            long categoryId,
            UpdateCategorySeoContentRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var entity = await _db.CategorySeoContents
                    .FirstOrDefaultAsync(x => x.CategoryId == categoryId, cancellationToken)
                    ?? throw new KeyNotFoundException($"SEO sadržaj za kategoriju {categoryId} nije pronađen");

                if (!string.IsNullOrEmpty(request.MetaTitle))
                    entity.MetaTitle = request.MetaTitle;

                if (!string.IsNullOrEmpty(request.MetaDescription))
                    entity.MetaDescription = request.MetaDescription;

                if (request.IntroTitle != null)
                    entity.IntroTitle = request.IntroTitle;

                if (request.IntroText != null)
                    entity.IntroText = request.IntroText;

                if (request.MainContent != null)
                    entity.MainContent = request.MainContent;

                if (request.Faq != null)
                    entity.Faq = request.Faq;

                await _db.SaveChangesAsync(cancellationToken);

                await InvalidateCacheAsync();

                _logger.LogInformation(
                    "Ažuriran SEO sadržaj za kategoriju {CategoryId}",
                    categoryId);

                return MapToDto(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri ažuriranju SEO sadržaja");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(long categoryId, CancellationToken cancellationToken = default)
        {
            try
            {
                var entity = await _db.CategorySeoContents
                    .FirstOrDefaultAsync(x => x.CategoryId == categoryId, cancellationToken);

                if (entity == null)
                    return false;

                _db.CategorySeoContents.Remove(entity);
                await _db.SaveChangesAsync(cancellationToken);

                await InvalidateCacheAsync();

                _logger.LogInformation(
                    "Obrisan SEO sadržaj za kategoriju {CategoryId}",
                    categoryId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri brisanju SEO sadržaja");
                throw;
            }
        }

        public async Task<CategorySeoContentDto> PublishAsync(
            long categoryId,
            bool isPublished,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var entity = await _db.CategorySeoContents
                    .FirstOrDefaultAsync(x => x.CategoryId == categoryId, cancellationToken)
                    ?? throw new KeyNotFoundException($"SEO sadržaj za kategoriju {categoryId} nije pronađen");

                if (isPublished)
                    entity.Publish();
                else
                    entity.Unpublish();

                await _db.SaveChangesAsync(cancellationToken);

                await InvalidateCacheAsync();

                _logger.LogInformation(
                    "SEO sadržaj za kategoriju {CategoryId} je sada {Status}",
                    categoryId,
                    isPublished ? "objavljen" : "neobjavljen");

                return MapToDto(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri promeni statusa publikovanja");
                throw;
            }
        }

        public async Task InvalidateCacheAsync()
        {
            try
            {
                await _cache.RemoveAsync(CacheKeyAll);
                // Invalidacija specifičnih cache ključeva bi se desila kroz pattern removal
                // FusionCache nema built-in pattern removal, pa je OKay da invalidiramo samo "all"
                _logger.LogInformation("Invalidiran cache SEO sadržaja");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Greška pri invalidaciji cache-a");
            }
        }

        private CategorySeoContentDto MapToDto(CategorySeoContent entity)
        {
            return new CategorySeoContentDto
            {
                Id = entity.Id,
                CategoryId = entity.CategoryId,
                MetaTitle = entity.MetaTitle,
                MetaDescription = entity.MetaDescription,
                IntroTitle = entity.IntroTitle,
                IntroText = entity.IntroText,
                MainContent = entity.MainContent,
                Faq = entity.Faq,
                IsPublished = entity.IsPublished,
                PublishedAtUtc = entity.PublishedAtUtc.UtcDateTime
            };
        }
    }
}
