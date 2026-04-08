#nullable enable
using System;
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Content.Dtos;
using TrendplusProdavnica.Application.Content.Queries;
using TrendplusProdavnica.Application.Content.Services;
using TrendplusProdavnica.Infrastructure.Persistence;

namespace TrendplusProdavnica.Application.Content.Services.Implementations
{
    public class BrandPageQueryService : IBrandPageQueryService
    {
        private readonly TrendplusDbContext _db;
        public BrandPageQueryService(TrendplusDbContext db) => _db = db;

        public Task<BrandPageDto> GetBrandPageAsync(GetBrandPageQuery query) => throw new NotImplementedException();
    }
}
