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

    public record SearchFacetValueDto(
        string Value,
        string Label,
        long Count,
        bool Selected);

    public record SearchPriceRangeFacetDto(
        decimal? Min,
        decimal? Max,
        decimal? SelectedMin,
        decimal? SelectedMax);

    public record SearchFacetsDto(
        SearchFacetValueDto[] Brands,
        SearchFacetValueDto[] Sizes,
        SearchFacetValueDto[] Colors,
        SearchPriceRangeFacetDto PriceRange,
        SearchFacetValueDto[] Availability,
        SearchFacetValueDto[] Sale,
        SearchFacetValueDto[] New);

    public record SearchPaginationDto(int Page, int PageSize, long TotalCount)
    {
        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    public record SearchResponseDto(
        ProductSearchItemDto[] Products,
        long TotalCount,
        int Page,
        int PageSize,
        SearchFacetsDto Facets)
    {
        public SearchPaginationDto Pagination => new(Page, PageSize, TotalCount);
    }

    // Backward-compatible alias for older callers that may still expect Items/Pagination.
    public record ProductSearchResultDto(
        long Total,
        ProductSearchItemDto[] Items,
        SearchPaginationDto Pagination,
        SearchFacetsDto Facets)
    {
        public SearchResponseDto ToSearchResponseDto() => new(Items, Total, Pagination.Page, Pagination.PageSize, Facets);
    }

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
