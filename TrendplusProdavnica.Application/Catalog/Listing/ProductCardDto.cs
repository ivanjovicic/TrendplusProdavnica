#nullable enable

namespace TrendplusProdavnica.Application.Catalog.Listing
{
    public sealed record ProductCardDto(
        long ProductId,
        string Slug,
        string Name,
        string BrandName,
        decimal Price,
        decimal? OldPrice,
        int? DiscountPercent,
        string PrimaryImageUrl,
        string? SecondaryImageUrl,
        int AvailableSizesCount,
        bool IsNew,
        bool IsOnSale,
        string? Color);
}
