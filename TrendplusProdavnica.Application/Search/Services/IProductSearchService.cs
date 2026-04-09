#nullable enable
using System.Threading;
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Search.Dtos;
using TrendplusProdavnica.Application.Search.Queries;

namespace TrendplusProdavnica.Application.Search.Services
{
    public interface IProductSearchService
    {
        Task<ProductSearchResultDto> SearchProductsAsync(ProductSearchQuery query, CancellationToken cancellationToken = default);
    }
}
