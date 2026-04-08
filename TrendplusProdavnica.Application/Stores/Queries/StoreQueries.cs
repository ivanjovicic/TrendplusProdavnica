#nullable enable

namespace TrendplusProdavnica.Application.Stores.Queries
{
    public record GetStoresQuery(string? City = null, int Page = 1, int PageSize = 20);
    public record GetStorePageQuery(string Slug);
}
