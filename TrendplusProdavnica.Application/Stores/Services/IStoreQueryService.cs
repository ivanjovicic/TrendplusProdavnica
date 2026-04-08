#nullable enable
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Stores.Dtos;
using TrendplusProdavnica.Application.Stores.Queries;

namespace TrendplusProdavnica.Application.Stores.Services
{
    public interface IStoreQueryService
    {
        Task<StoreCardDto[]> GetStoresAsync(GetStoresQuery query);
        Task<StorePageDto> GetStorePageAsync(GetStorePageQuery query);
    }
}
