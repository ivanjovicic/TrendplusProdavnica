#nullable enable
using System.Threading;
using System.Threading.Tasks;

namespace TrendplusProdavnica.Application.Search.Services
{
    public interface IProductSearchIndexService
    {
        Task ReindexAllAsync(CancellationToken cancellationToken = default);
        Task ReindexProductAsync(long productId, CancellationToken cancellationToken = default);
        Task DeleteProductAsync(long productId, CancellationToken cancellationToken = default);
    }
}
