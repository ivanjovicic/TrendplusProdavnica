#nullable enable
using System;
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Catalog.Dtos;
using TrendplusProdavnica.Application.Catalog.Queries;
using TrendplusProdavnica.Application.Catalog.Services;
using TrendplusProdavnica.Infrastructure.Persistence;

namespace TrendplusProdavnica.Application.Catalog.Services.Implementations
{
    public class ProductDetailQueryService : IProductDetailQueryService
    {
        private readonly TrendplusDbContext _db;
        public ProductDetailQueryService(TrendplusDbContext db) => _db = db;

        public Task<ProductDetailDto> GetProductDetailAsync(GetProductDetailQuery query) => throw new NotImplementedException();
    }
}
