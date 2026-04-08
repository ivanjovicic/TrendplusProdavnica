#nullable enable
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Content.Dtos;
using TrendplusProdavnica.Application.Content.Queries;
using TrendplusProdavnica.Application.Content.Services;
using TrendplusProdavnica.Infrastructure.Persistence;

namespace TrendplusProdavnica.Application.Content.Services.Implementations
{
    public class CollectionPageQueryService : ICollectionPageQueryService
    {
        private readonly TrendplusDbContext _db;
        public CollectionPageQueryService(TrendplusDbContext db) => _db = db;

        public Task<CollectionPageDto> GetCollectionPageAsync(GetCollectionPageQuery query) => throw new System.NotImplementedException();
    }
}
