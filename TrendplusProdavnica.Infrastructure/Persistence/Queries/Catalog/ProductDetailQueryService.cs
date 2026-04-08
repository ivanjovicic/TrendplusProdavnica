#nullable enable
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrendplusProdavnica.Application.Catalog.Dtos;
using TrendplusProdavnica.Application.Catalog.Queries;
using TrendplusProdavnica.Application.Catalog.Services;
using TrendplusProdavnica.Infrastructure.Persistence;

namespace TrendplusProdavnica.Infrastructure.Persistence.Queries.Catalog
{
    public class ProductDetailQueryService : IProductDetailQueryService
    {
        private readonly TrendplusDbContext _db;
        public ProductDetailQueryService(TrendplusDbContext db) => _db = db;

        public async Task<ProductDetailDto> GetProductDetailAsync(GetProductDetailQuery query)
        {
            var dto = await _db.Products.AsNoTracking()
                .Where(p => p.Slug == query.Slug && p.IsVisible && p.IsPurchasable && p.Status == Domain.Enums.ProductStatus.Published)
                .Select(p => new ProductDetailDto(
                    p.Id,
                    p.Slug,
                    _db.Brands.Where(b => b.Id == p.BrandId).Select(b => b.Name).FirstOrDefault() ?? string.Empty,
                    p.Name,
                    p.Subtitle,
                    p.ShortDescription,
                    p.LongDescription,
                    p.Variants.OrderBy(v => v.Price).Select(v => v.Price).FirstOrDefault(),
                    p.Variants.OrderBy(v => v.Price).Select(v => v.OldPrice).FirstOrDefault(),
                    p.Variants.Select(v => v.Currency).FirstOrDefault() ?? "RSD",
                    p.IsBestseller ? new string[] { "Bestseller" } : new string[0],
                    new BreadcrumbItemDto[0],
                    p.Media.OrderBy(m => m.SortOrder).Select(m => new ProductMediaDto(
                        m.Url,
                        m.MobileUrl,
                        m.AltText,
                        m.Title,
                        m.MediaType.ToString(),
                        m.MediaRole.ToString(),
                        m.SortOrder,
                        m.IsPrimary
                    )).ToArray(),
                    p.Variants.OrderBy(v => v.SortOrder).Select(v => new ProductSizeOptionDto(
                        v.Id,
                        v.SizeEu,
                        v.SizeEu.ToString(),
                        v.StockStatus != Domain.Enums.StockStatus.OutOfStock,
                        v.TotalStock <= v.LowStockThreshold,
                        v.TotalStock,
                        v.Price,
                        v.OldPrice
                    )).ToArray(),
                    null,
                    new ProductCardDto[0],
                    new ProductCardDto[0],
                    new TrendplusProdavnica.Application.Catalog.Dtos.SeoDto(
                        p.Seo != null ? p.Seo.SeoTitle ?? p.Name : p.Name,
                        p.Seo != null ? p.Seo.SeoDescription ?? string.Empty : string.Empty,
                        p.Seo != null ? p.Seo.CanonicalUrl : null,
                        null
                    ),
                    "Dostava: standard",
                    "Povrat: 14 dana",
                    null
                ))
                .FirstOrDefaultAsync();

            if (dto is null)
                throw new System.Collections.Generic.KeyNotFoundException("Product not found");

            return dto;
        }
    }
}
