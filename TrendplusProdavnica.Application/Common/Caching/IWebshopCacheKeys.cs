#nullable enable
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
    }
}
