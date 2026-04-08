#nullable enable
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrendplusProdavnica.Application.Catalog.Dtos;
using TrendplusProdavnica.Application.Catalog.Services;
using TrendplusProdavnica.Infrastructure.Persistence;

namespace TrendplusProdavnica.Infrastructure.Persistence.Queries.Content
{
    public class HomePageQueryService : IHomePageQueryService
    {
        private readonly TrendplusDbContext _db;
        public HomePageQueryService(TrendplusDbContext db) => _db = db;

        public async Task<HomePageDto> GetHomePageAsync()
        {
            var page = await _db.HomePages.AsNoTracking()
                .Where(h => h.IsPublished)
                .OrderByDescending(h => h.PublishedAtUtc)
                .Select(h => new
                {
                    h.Seo,
                    h.Title,
                    Modules = h.Modules
                })
                .FirstOrDefaultAsync();

            if (page is null)
            {
                return new HomePageDto(
                    new SeoDto("", "", null, null),
                    null,
                    new HeroSectionDto("", "", ""),
                    new ProductCardDto[0],
                    new ProductCardDto[0],
                    new ProductCardDto[0],
                    new ProductCardDto[0],
                    new string[0],
                    null,
                    null,
                    new TrustItemDto[0],
                    null
                );
            }

            var seo = new SeoDto(page.Seo?.SeoTitle ?? page.Title, page.Seo?.SeoDescription ?? string.Empty, page.Seo?.CanonicalUrl, null);

            // Minimal mapping: modules payloads are domain objects; for now map only basic sections
            return new HomePageDto(
                seo,
                null,
                new HeroSectionDto(page.Title, string.Empty, string.Empty),
                new ProductCardDto[0],
                new ProductCardDto[0],
                new ProductCardDto[0],
                new ProductCardDto[0],
                new string[0],
                null,
                null,
                new TrustItemDto[0],
                null
            );
        }
    }
}
