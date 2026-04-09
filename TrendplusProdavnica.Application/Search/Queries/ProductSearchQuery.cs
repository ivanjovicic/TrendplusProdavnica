#nullable enable

namespace TrendplusProdavnica.Application.Search.Queries
{
    public record ProductSearchQuery(
        string? QueryText,
        int Page = 1,
        int PageSize = 24,
        string? Brand = null,
        string? Color = null,
        decimal? Size = null,
        bool? IsOnSale = null,
        bool? IsNew = null,
        bool? InStockOnly = null,
        string? Sort = "relevance");
}
