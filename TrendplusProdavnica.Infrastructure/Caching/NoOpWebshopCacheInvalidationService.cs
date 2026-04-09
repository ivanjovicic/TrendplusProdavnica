#nullable enable
using System.Threading;
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Common.Caching;

namespace TrendplusProdavnica.Infrastructure.Caching
{
    internal sealed class NoOpWebshopCacheInvalidationService : IWebshopCacheInvalidationService
    {
        public Task InvalidateHomePageAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task InvalidateProductBySlugAsync(string slug, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task InvalidateBrandBySlugAsync(string slug, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task InvalidateCollectionBySlugAsync(string slug, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task InvalidateStoreBySlugAsync(string slug, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task InvalidateEditorialBySlugAsync(string slug, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task InvalidateEditorialListAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
