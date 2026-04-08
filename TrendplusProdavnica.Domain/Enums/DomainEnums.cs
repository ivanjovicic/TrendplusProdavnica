#nullable enable
namespace TrendplusProdavnica.Domain.Enums
{
    public enum CategoryType : short
    {
        Root = 1,
        Subcategory = 2
    }

    public enum ProductStatus : short
    {
        Draft = 0,
        Published = 1,
        Archived = 2
    }

    public enum CollectionType : short
    {
        Manual = 1,
        RuleBased = 2,
        Seasonal = 3,
        Campaign = 4
    }

    public enum StockStatus : short
    {
        OutOfStock = 0,
        InStock = 1,
        LowStock = 2,
        BackSoon = 3
    }

    public enum MediaType : short
    {
        Image = 1,
        Video = 2
    }

    public enum MediaRole : short
    {
        Listing = 1,
        Gallery = 2,
        Hero = 3,
        Thumbnail = 4,
        OpenGraph = 5
    }

    public enum ProductRelationType : short
    {
        Similar = 1,
        SameBrand = 2,
        Recommended = 3
    }

    public enum PromotionDiscountType : short
    {
        Percent = 1,
        FixedAmount = 2
    }

    public enum MenuLocation : short
    {
        Header = 1,
        Footer = 2,
        Mobile = 3
    }

    public enum ContentStatus : short
    {
        Draft = 0,
        Published = 1,
        Archived = 2
    }

    public enum TrustPageKind : short
    {
        Delivery = 1,
        Returns = 2,
        SizeGuide = 3,
        Payments = 4,
        About = 5,
        Contact = 6
    }

    public enum SlugRedirectEntityType : short
    {
        Category = 1,
        Brand = 2,
        Collection = 3,
        Product = 4,
        EditorialArticle = 5,
        Store = 6,
        TrustPage = 7,
        SalePage = 8
    }

    public enum CartStatus : short
    {
        Active = 1,
        Abandoned = 2,
        Converted = 3
    }
}
