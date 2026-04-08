#nullable enable
using System.Collections.Generic;

namespace TrendplusProdavnica.Application.Stores.Dtos
{
    public record StoreCardDto(
        string Name,
        string Slug,
        string City,
        string AddressLine1,
        string WorkingHoursText,
        string Phone,
        string? CoverImageUrl
    );

    public record StorePageDto(
        string Name,
        string Slug,
        string City,
        string AddressLine1,
        string? AddressLine2,
        string PostalCode,
        string? MallName,
        string Phone,
        string Email,
        decimal Latitude,
        decimal Longitude,
        string WorkingHoursText,
        string ShortDescription,
        string CoverImageUrl,
        TrendplusProdavnica.Application.Catalog.Dtos.SeoDto Seo,
        object[] FeaturedCategories,
        object[] FeaturedBrands
    );
}
