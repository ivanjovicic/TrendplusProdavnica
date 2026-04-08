#nullable enable
using System;
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Catalog.Dtos;
using TrendplusProdavnica.Application.Catalog.Queries;
using TrendplusProdavnica.Application.Catalog.Services;
using TrendplusProdavnica.Infrastructure.Persistence;

namespace TrendplusProdavnica.Application.Catalog.Services.Implementations
{
    public class ProductListingQueryService : IProductListingQueryService
    {
        private readonly TrendplusDbContext _db;
        public ProductListingQueryService(TrendplusDbContext db) => _db = db;

        public Task<ProductListingPageDto> GetCategoryListingAsync(GetCategoryListingQuery query) => throw new NotImplementedException();
        public Task<ProductListingPageDto> GetBrandListingAsync(GetBrandListingQuery query) => throw new NotImplementedException();
        public Task<ProductListingPageDto> GetCollectionListingAsync(GetCollectionListingQuery query) => throw new NotImplementedException();
        public Task<ProductListingPageDto> GetSaleListingAsync(GetSaleListingQuery query) => throw new NotImplementedException();
    }
}
