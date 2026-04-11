#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TrendplusProdavnica.Application.Content.CategorySeo
{
    /// <summary>
    /// Servis za upravljanje SEO landing stranicama kategorija
    /// </summary>
    public interface ICategorySeoContentService
    {
        /// <summary>
        /// Dohvata SEO sadržaj za kategoriju
        /// </summary>
        Task<CategorySeoContentDto?> GetByCategoryIdAsync(long categoryId, bool useCache = true);

        /// <summary>
        /// Dohvata sve SEO sadržaje
        /// </summary>
        Task<IReadOnlyList<CategorySeoContentDto>> GetAllAsync(bool useCache = true);

        /// <summary>
        /// Kreira novi SEO sadržaj
        /// </summary>
        Task<CategorySeoContentDto> CreateAsync(CreateCategorySeoContentRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Ažurira SEO sadržaj
        /// </summary>
        Task<CategorySeoContentDto> UpdateAsync(long categoryId, UpdateCategorySeoContentRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Briše SEO sadržaj
        /// </summary>
        Task<bool> DeleteAsync(long categoryId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Publikuje/unpublikuje SEO sadržaj
        /// </summary>
        Task<CategorySeoContentDto> PublishAsync(long categoryId, bool isPublished, CancellationToken cancellationToken = default);

        /// <summary>
        /// Invalidira cache
        /// </summary>
        Task InvalidateCacheAsync();
    }
}
