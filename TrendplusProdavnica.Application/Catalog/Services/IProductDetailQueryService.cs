#nullable enable
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Catalog.Dtos;
using TrendplusProdavnica.Application.Catalog.Queries;

namespace TrendplusProdavnica.Application.Catalog.Services
{
    public interface IProductDetailQueryService
    {
        Task<ProductDetailDto> GetProductDetailAsync(GetProductDetailQuery query);
    }
}
