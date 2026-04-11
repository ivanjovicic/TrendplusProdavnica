#nullable enable

namespace TrendplusProdavnica.Application.Search.Queries
{
    public record ProductSearchQuery(
        string? QueryText,
        int Page = 1,
        int PageSize = 24,
        string[]? Brands = null,
        string[]? Colors = null,
        decimal[]? Sizes = null,
        decimal? MinPrice = null,
        decimal? MaxPrice = null,
        string[]? Availability = null,
        bool? IsOnSale = null,
        bool? IsNew = null,
        bool? InStockOnly = null,
        string? Sort = "relevance");
}
