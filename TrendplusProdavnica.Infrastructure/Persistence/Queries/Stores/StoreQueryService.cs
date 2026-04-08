#nullable enable
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrendplusProdavnica.Application.Stores.Dtos;
using TrendplusProdavnica.Application.Stores.Queries;
using TrendplusProdavnica.Application.Stores.Services;
using TrendplusProdavnica.Infrastructure.Persistence;

namespace TrendplusProdavnica.Infrastructure.Persistence.Queries.Stores
{
    public class StoreQueryService : IStoreQueryService
    {
        private readonly TrendplusDbContext _db;
        public StoreQueryService(TrendplusDbContext db) => _db = db;

        public async Task<StoreCardDto[]> GetStoresAsync(GetStoresQuery query)
        {
            var q = _db.Stores.AsNoTracking().Where(s => s.IsActive);
            if (!string.IsNullOrWhiteSpace(query.City)) q = q.Where(s => s.City == query.City);

            var items = await q.OrderBy(s => s.Name)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(s => new StoreCardDto(
                    s.Name,
                    s.Slug,
                    s.City,
                    s.AddressLine1,
                    s.WorkingHoursText ?? string.Empty,
                    s.Phone ?? string.Empty,
                    s.CoverImageUrl
                ))
                .ToArrayAsync();

            return items;
        }

        public async Task<StorePageDto> GetStorePageAsync(GetStorePageQuery query)
        {
            var s = await _db.Stores.AsNoTracking()
                .Where(x => x.Slug == query.Slug && x.IsActive)
                .Select(x => new StorePageDto(
                    x.Name,
                    x.Slug,
                    x.City,
                    x.AddressLine1,
                    x.AddressLine2,
                    x.PostalCode ?? string.Empty,
                    x.MallName,
                    x.Phone ?? string.Empty,
                    x.Email ?? string.Empty,
                    x.Latitude ?? 0m,
                    x.Longitude ?? 0m,
                    x.WorkingHoursText ?? string.Empty,
                    x.ShortDescription ?? string.Empty,
                    x.CoverImageUrl ?? string.Empty,
                    new TrendplusProdavnica.Application.Catalog.Dtos.SeoDto(x.Seo?.SeoTitle ?? x.Name, x.Seo?.SeoDescription ?? string.Empty, x.Seo?.CanonicalUrl, null),
                    new object[0],
                    new object[0]
                ))
                .FirstOrDefaultAsync();

            if (s is null) throw new System.Collections.Generic.KeyNotFoundException("Store not found");
            return s;
        }
    }
}
