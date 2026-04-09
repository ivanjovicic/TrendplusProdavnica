#nullable enable
using System.Threading;
using System.Threading.Tasks;

namespace TrendplusProdavnica.Application.Catalog.Listing
{
    public interface IProductListingReadService
    {
        Task<ProductListingResponse> GetProductsAsync(ProductListingQuery query, CancellationToken cancellationToken = default);
    }
}
