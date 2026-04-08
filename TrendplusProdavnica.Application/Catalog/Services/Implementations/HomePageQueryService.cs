#nullable enable
using System;
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Catalog.Dtos;
using TrendplusProdavnica.Application.Catalog.Services;
using TrendplusProdavnica.Infrastructure.Persistence;

namespace TrendplusProdavnica.Application.Catalog.Services.Implementations
{
    public class HomePageQueryService : IHomePageQueryService
    {
        private readonly TrendplusDbContext _db;

        public HomePageQueryService(TrendplusDbContext db) => _db = db;

        public Task<HomePageDto> GetHomePageAsync()
        {
            // Implementation pending: read from Content/HomePage and SiteSettings
            throw new NotImplementedException();
        }
    }
}
