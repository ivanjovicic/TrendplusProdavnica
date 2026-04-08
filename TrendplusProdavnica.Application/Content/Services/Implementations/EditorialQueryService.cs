#nullable enable
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Content.Dtos;
using TrendplusProdavnica.Application.Content.Queries;
using TrendplusProdavnica.Application.Content.Services;
using TrendplusProdavnica.Infrastructure.Persistence;

namespace TrendplusProdavnica.Application.Content.Services.Implementations
{
    public class EditorialQueryService : IEditorialQueryService
    {
        private readonly TrendplusDbContext _db;
        public EditorialQueryService(TrendplusDbContext db) => _db = db;

        public Task<EditorialArticleDto> GetEditorialArticleAsync(GetEditorialArticleQuery query) => throw new System.NotImplementedException();
    }
}
