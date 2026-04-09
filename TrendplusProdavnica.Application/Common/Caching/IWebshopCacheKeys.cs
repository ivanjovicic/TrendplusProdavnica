#nullable enable
using TrendplusProdavnica.Application.Catalog.Queries;

namespace TrendplusProdavnica.Application.Common.Caching
{
    public interface IWebshopCacheKeys
    {
        string HomePage();
        string ProductDetail(string slug);
        string BrandPage(string slug);
        string CollectionPage(string slug);
        string StorePage(string slug);
        string EditorialDetail(string slug);
        string EditorialList();
        string? CategoryListing(GetCategoryListingQuery query);
        string? BrandListing(GetBrandListingQuery query);
        string? CollectionListing(GetCollectionListingQuery query);
        string? SaleListing(GetSaleListingQuery query);
    }
}
