#nullable enable
using System.Threading;
using System.Threading.Tasks;

namespace TrendplusProdavnica.Application.Common.Caching
{
    public interface IWebshopCacheInvalidationService
    {
        Task InvalidateHomePageAsync(CancellationToken cancellationToken = default);
        Task InvalidateProductBySlugAsync(string slug, CancellationToken cancellationToken = default);
        Task InvalidateBrandBySlugAsync(string slug, CancellationToken cancellationToken = default);
        Task InvalidateCollectionBySlugAsync(string slug, CancellationToken cancellationToken = default);
        Task InvalidateStoreBySlugAsync(string slug, CancellationToken cancellationToken = default);
        Task InvalidateEditorialBySlugAsync(string slug, CancellationToken cancellationToken = default);
        Task InvalidateEditorialListAsync(CancellationToken cancellationToken = default);
    }
}
