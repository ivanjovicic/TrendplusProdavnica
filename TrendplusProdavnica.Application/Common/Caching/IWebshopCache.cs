#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TrendplusProdavnica.Application.Common.Caching
{
    public interface IWebshopCache
    {
        Task<TValue> GetOrSetAsync<TValue>(
            string key,
            WebshopCacheProfile profile,
            Func<CancellationToken, Task<TValue>> factory,
            IReadOnlyCollection<string>? tags = null,
            CancellationToken cancellationToken = default);

        Task RemoveAsync(string key, CancellationToken cancellationToken = default);
        Task RemoveByTagAsync(string tag, CancellationToken cancellationToken = default);
    }
}
