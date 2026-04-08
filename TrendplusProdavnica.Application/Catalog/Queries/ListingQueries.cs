#nullable enable
using System.Collections.Generic;

namespace TrendplusProdavnica.Application.Catalog.Queries
{
    public record GetHomePageQuery();

    public record GetCategoryListingQuery(
        string Slug,
        int Page = 1,
        int PageSize = 24,
        string? Sort = null,
        long[]? Sizes = null,
        string[]? Colors = null,
        long[]? Brands = null,
        decimal? PriceFrom = null,
        decimal? PriceTo = null,
        bool? IsOnSale = null,
        bool? IsNew = null,
        bool? InStockOnly = null
    );

    public record GetBrandListingQuery(
        string Slug,
        int Page = 1,
        int PageSize = 24,
        string? Sort = null,
        long[]? Sizes = null,
        string[]? Colors = null,
        long[]? Brands = null,
        decimal? PriceFrom = null,
        decimal? PriceTo = null,
        bool? IsOnSale = null,
        bool? IsNew = null,
        bool? InStockOnly = null
    );

    public record GetCollectionListingQuery(
        string Slug,
        int Page = 1,
        int PageSize = 24,
        string? Sort = null,
        long[]? Sizes = null,
        string[]? Colors = null,
        long[]? Brands = null,
        decimal? PriceFrom = null,
        decimal? PriceTo = null,
        bool? IsOnSale = null,
        bool? IsNew = null,
        bool? InStockOnly = null
    );

    public record GetSaleListingQuery(
        int Page = 1,
        int PageSize = 24,
        string? Sort = null,
        long[]? Sizes = null,
        string[]? Colors = null,
        long[]? Brands = null,
        decimal? PriceFrom = null,
        decimal? PriceTo = null,
        bool? IsOnSale = null,
        bool? IsNew = null,
        bool? InStockOnly = null
    );
}

