#nullable enable
using System;
using System.Collections.Generic;

namespace TrendplusProdavnica.Application.Catalog.Dtos
{
    public record BreadcrumbItemDto(string Label, string Url);

    public record ProductMediaDto(
        string Url,
        string? MobileUrl,
        string? AltText,
        string? Title,
        string MediaType,
        string MediaRole,
        int SortOrder,
        bool IsPrimary
    );

    public record ProductSizeOptionDto(
        long VariantId,
        decimal SizeEu,
        string Label,
        bool IsInStock,
        bool IsLowStock,
        int TotalStock,
        decimal Price,
        decimal? OldPrice
    );

    public record ProductCardDto(
        long Id,
        string Slug,
        string BrandName,
        string Name,
        string PrimaryImageUrl,
        string? SecondaryImageUrl,
        decimal Price,
        decimal? OldPrice,
        string Currency,
        string[] Badges,
        bool IsInStock,
        int AvailableSizesCount,
        string? ColorLabel
    );

    public record PaginationDto(int Page, int PageSize, long TotalItems)
    {
        public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling((double)TotalItems / PageSize);
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;
    }

    public record FilterOptionDto(string Value, string Label, int Count, bool Selected, bool Disabled);
    public record FilterFacetDto(string Key, string Label, string Type, FilterOptionDto[] Options);
    public record AppliedFilterDto(string Key, string Label, string Value, string DisplayValue);
}
