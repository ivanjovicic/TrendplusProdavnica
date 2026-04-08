#nullable enable
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Stores.Dtos;
using TrendplusProdavnica.Application.Stores.Queries;
using TrendplusProdavnica.Application.Stores.Services;
using TrendplusProdavnica.Infrastructure.Persistence;

namespace TrendplusProdavnica.Application.Stores.Services.Implementations
{
    public class StoreQueryService : IStoreQueryService
    {
        private readonly TrendplusDbContext _db;
        public StoreQueryService(TrendplusDbContext db) => _db = db;

        public Task<StoreCardDto[]> GetStoresAsync(GetStoresQuery query) => throw new System.NotImplementedException();
        public Task<StorePageDto> GetStorePageAsync(GetStorePageQuery query) => throw new System.NotImplementedException();
    }
}
