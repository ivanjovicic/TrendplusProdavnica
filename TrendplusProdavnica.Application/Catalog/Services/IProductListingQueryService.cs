#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Catalog.Dtos;
using TrendplusProdavnica.Application.Catalog.Queries;

namespace TrendplusProdavnica.Application.Catalog.Services
{
    public record CategorySeoDto(string Slug, DateTimeOffset UpdatedAtUtc);

    public interface IProductListingQueryService
    {
        Task<ProductListingPageDto> GetCategoryListingAsync(GetCategoryListingQuery query);
        Task<ProductListingPageDto> GetBrandListingAsync(GetBrandListingQuery query);
        Task<ProductListingPageDto> GetCollectionListingAsync(GetCollectionListingQuery query);
        Task<ProductListingPageDto> GetSaleListingAsync(GetSaleListingQuery query);
        Task<List<CategorySeoDto>> GetAllCategoriesForSeoAsync();
    }
}
