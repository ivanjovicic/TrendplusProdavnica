#nullable enable
using System;

namespace TrendplusProdavnica.Application.Search.Dtos
{
    public record ProductSearchItemDto(
        long ProductId,
        string Slug,
        string BrandName,
        string Name,
        string? ShortDescription,
        string? PrimaryCategory,
        string[] SecondaryCategories,
        string? PrimaryColorName,
        bool IsNew,
        bool IsBestseller,
        bool IsOnSale,
        decimal? MinPrice,
        decimal? MaxPrice,
        decimal[] AvailableSizes,
        bool InStock,
        string? PrimaryImageUrl,
        DateTimeOffset? PublishedAtUtc,
        int SortRank);

    public record SearchFacetOptionDto(string Value, long Count);

    public record ProductSearchFacetsDto(
        SearchFacetOptionDto[] Brands,
        SearchFacetOptionDto[] Colors,
        SearchFacetOptionDto[] Sizes,
        SearchFacetOptionDto[] Sale,
        SearchFacetOptionDto[] New,
        SearchFacetOptionDto[] Stock);

    public record ProductSearchPaginationDto(int Page, int PageSize, long Total)
    {
        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)Total / PageSize);
    }

    public record ProductSearchResultDto(
        long Total,
        ProductSearchItemDto[] Items,
        ProductSearchPaginationDto Pagination,
        ProductSearchFacetsDto Facets);

    /// <summary>
    /// Autocomplete suggestion for product search
    /// </summary>
    public record ProductAutocompleteItemDto(
        long ProductId,
        string Slug,
        string Name,
        string BrandName,
        string? PrimaryImageUrl);

    /// <summary>
    /// Autocomplete result with product suggestions
    /// </summary>
    public record ProductAutocompleteResultDto(
        ProductAutocompleteItemDto[] Items);
}
