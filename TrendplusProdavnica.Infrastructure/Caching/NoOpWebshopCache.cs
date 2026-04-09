#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Common.Caching;

namespace TrendplusProdavnica.Infrastructure.Caching
{
    internal sealed class NoOpWebshopCache : IWebshopCache
    {
        public Task<TValue> GetOrSetAsync<TValue>(
            string key,
            WebshopCacheProfile profile,
            Func<CancellationToken, Task<TValue>> factory,
            IReadOnlyCollection<string>? tags = null,
            CancellationToken cancellationToken = default)
        {
            return factory(cancellationToken);
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task RemoveByTagAsync(string tag, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
