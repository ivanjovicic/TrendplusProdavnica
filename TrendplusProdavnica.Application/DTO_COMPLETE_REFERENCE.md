# Complete DTO Reference - TrendplusProdavnica.Application

All DTOs are in the `TrendplusProdavnica.Application` namespace with their respective feature folders.

## Common DTOs (Shared across all features)

```csharp
namespace TrendplusProdavnica.Application.Catalog.Dtos
{
    /// SEO metadata for pages
    public record SeoDto(
        string Title,                           // Page title (50-60 chars)
        string Description,                     // Meta description (150-160 chars)
        string? CanonicalUrl,
        string[]? Keywords
    );

    /// Breadcrumb navigation item
    public record BreadcrumbItemDto(
        string Label,                           // Display text
        string Url                              // Navigation URL
    );

    /// Pagination information
    public record PaginationDto(
        int Page,
        int PageSize,
        long TotalItems
    )
    {
        public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling((double)TotalItems / PageSize);
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;
    };

    /// Product media (image, video, etc.)
    public record ProductMediaDto(
        string Url,
        string? MobileUrl,                      // Optimized for mobile
        string? AltText,                        // Accessibility
        string? Title,
        string MediaType,                       // "image", "video"
        string MediaRole,                       // "primary", "secondary", "video"
        int SortOrder,
        bool IsPrimary
    );

    /// Product size/variant option
    public record ProductSizeOptionDto(
        long VariantId,                         // For add-to-cart
        decimal SizeEu,                         // EU size
        string Label,                           // Display label "36 EU"
        bool IsInStock,
        bool IsLowStock,                        // < 5 items
        int TotalStock,
        decimal Price,
        decimal? OldPrice                       // If on sale
    );
}
```

---

## Catalog Feature DTOs

### Product DTOs

```csharp
namespace TrendplusProdavnica.Application.Catalog.Dtos
{
    /// Product card for listings (universal across all listing types)
    public record ProductCardDto(
        long Id,
        string Slug,                            // URL segment: /proizvod/{slug}
        string BrandName,
        string Name,
        string PrimaryImageUrl,                 // 1:1 aspect ratio recommended
        string? SecondaryImageUrl,              // For hover effect
        decimal Price,                          // Current price
        decimal? OldPrice,                      // If on sale
        string Currency,                        // "RSD"
        string[] Badges,                        // ["Novo", "Na sniženju"]
        bool IsInStock,                         // At least 1 variant in stock
        int AvailableSizesCount,                // Number of available sizes
        string? ColorLabel                      // Primary color name
    );

    /// Product detail view
    public record ProductDetailDto(
        long Id,
        string Slug,
        string BrandName,
        string Name,
        string? Subtitle,
        string ShortDescription,                // Short copy on PDP
        string? LongDescription,                // Rich HTML
        decimal Price,
        decimal? OldPrice,
        string Currency,
        string[] Badges,
        BreadcrumbItemDto[] Breadcrumbs,        // Category path
        ProductMediaDto[] Media,                // All images/videos
        ProductSizeOptionDto[] Sizes,           // Available options
        object? StoreAvailabilitySummary,
        ProductCardDto[] RelatedProducts,       // 4-6 items
        ProductCardDto[] SimilarProducts,       // By color/category
        SeoDto Seo,
        string DeliveryInfo,                    // "2-3 business days"
        string ReturnInfo,                      // "30 days returns"
        object? SizeGuide                       // Size chart object
    );
}
```

### Home Page DTOs

```csharp
namespace TrendplusProdavnica.Application.Catalog.Dtos
{
    /// Category card for home page category section
    public record CategoryCardDto(
        string Name,                            // "Women Sneakers"
        string Slug,                            // URL segment
        string? ImageUrl                        // Category hero image
    );

    /// Collection teaser for featured collections
    public record CollectionTeaserDto(
        string Name,
        string Slug,
        string? CoverImageUrl,
        string? Description                     // Short tagline
    );

    /// Brand in brand wall
    public record BrandWallItemDto(
        string BrandName,
        string Slug,
        string? LogoUrl                         // Brand logo
    );

    /// Announcement banner
    public record AnnouncementBarDto(
        string Text,
        string? BackgroundColor,                // Hex color
        string? TextColor,
        string? CallToActionUrl
    );

    /// Home page hero section
    public record HeroSectionDto(
        string Title,
        string Subtitle,
        string ImageUrl
    );

    /// Editorial statement section
    public record EditorialStatementDto(
        string Title,
        string Text
    );

    /// Store teaser section
    public record StoreTeaserDto(
        string Name,
        string Slug,
        string CoverImageUrl                    // Store image
    );

    /// Trust/social proof item
    public record TrustItemDto(
        string Title,                           // "Free Shipping"
        string Description
    );

    /// Newsletter subscription
    public record NewsletterDto(
        string Title,
        string Placeholder                      // "Enter your email"
    );

    /// Complete home page response
    public record HomePageDto(
        SeoDto Seo,
        AnnouncementBarDto? AnnouncementBar,
        HeroSectionDto HeroSection,
        CategoryCardDto[] CategoryCards,        // 4-6 categories
        ProductCardDto[] NewArrivals,           // 8-12 products
        CollectionTeaserDto[] FeaturedCollections,
        ProductCardDto[] Bestsellers,           // 8-12 products
        BrandWallItemDto[] BrandWall,           // 6-10 brands
        EditorialStatementDto? EditorialStatement,
        StoreTeaserDto? StoreTeaser,
        TrustItemDto[] TrustItems,              // 3-5 items
        NewsletterDto? Newsletter
    );
}
```

### Listing Page DTOs

```csharp
namespace TrendplusProdavnica.Application.Catalog.Dtos
{
    /// Filter facet option
    public record FilterOptionDto(
        string Value,                           // Raw value: "36", "red"
        string Label,                           // Display: "EU 36", "Red"
        int Count,                              // Products with this filter
        bool Selected,                          // Currently active
        bool Disabled                           // No products available
    );

    /// Filter facet (size, color, brand, etc.)
    public record FilterFacetDto(
        string Key,                             // "sizes", "colors", "brands"
        string Label,                           // "Size", "Color", "Brand"
        string Type,                            // "checkbox", "range"
        FilterOptionDto[] Options
    );

    /// Currently applied filter
    public record AppliedFilterDto(
        string Key,
        string Label,
        string Value,                           // Raw value
        string DisplayValue                     // Formatted display
    );

    /// Complete listing page response
    public record ProductListingPageDto(
        string Title,                           // Category/Brand name
        string Description,
        SeoDto Seo,
        BreadcrumbItemDto[] Breadcrumbs,
        string? IntroTitle,
        string? IntroText,
        ProductCardDto[] Products,              // Current page results
        FilterFacetDto[] Facets,                // All available filters
        AppliedFilterDto[] AppliedFilters,      // Currently active
        PaginationDto Pagination,
        object[] MerchBlocks,                   // Dynamic content
        object? Faq                             // Page-specific FAQ
    );
}
```

---

## Content Feature DTOs

```csharp
namespace TrendplusProdavnica.Application.Content.Dtos
{
    /// FAQ item
    public record FaqItemDto(
        string Question,
        string Answer                           // Rich HTML
    );

    /// Merchandise/promotional block
    public record MerchBlockDto(
        string Title,
        string Html,                            // Promotional HTML
        string[] ProductSlugs                   // Products to showcase
    );

    /// Editorial article card (for listing)
    public record EditorialArticleCardDto(
        string Title,
        string Slug,
        string Excerpt,
        string CoverImageUrl,
        DateTime PublishedAtUtc,
        string Topic                            // "Fashion", "Care", "Tips"
    );

    /// Full editorial article
    public record EditorialArticleDto(
        string Title,
        string Slug,
        string Excerpt,
        string CoverImageUrl,
        string Body,                            // Full HTML content
        DateTime PublishedAtUtc,
        string Topic,
        string AuthorName,
        TrendplusProdavnica.Application.Catalog.Dtos.SeoDto Seo,
        long[] RelatedProducts,                 // Product IDs
        long[] RelatedCollections,              // Collection IDs
        long[] RelatedCategories,               // Category IDs
        long[] RelatedArticles                  // Article IDs
    );

    /// Brand page response
    public record BrandPageDto(
        string BrandName,
        string Slug,
        string IntroText,                       // Brand story
        TrendplusProdavnica.Application.Catalog.Dtos.SeoDto Seo,
        TrendplusProdavnica.Application.Catalog.Dtos.ProductCardDto[] FeaturedProducts,
        TrendplusProdavnica.Application.Catalog.Dtos.BreadcrumbItemDto[] CategoryLinks,
        FaqItemDto[]? Faq
    );

    /// Collection page response
    public record CollectionPageDto(
        string Name,
        string Slug,
        string IntroText,
        TrendplusProdavnica.Application.Catalog.Dtos.SeoDto Seo,
        TrendplusProdavnica.Application.Catalog.Dtos.ProductCardDto[] FeaturedProducts,
        MerchBlockDto[] MerchBlocks,            // Promotional blocks
        FaqItemDto[]? Faq
    );
}
```

---

## Store Feature DTOs

```csharp
namespace TrendplusProdavnica.Application.Stores.Dtos
{
    /// Store card (for store listing)
    public record StoreCardDto(
        string Name,
        string Slug,
        string City,
        string AddressLine1,
        string WorkingHoursText,                // "Mon-Sat: 10am-9pm"
        string Phone,                           // "+381 11 1234567"
        string? CoverImageUrl
    );

    /// Full store page response
    public record StorePageDto(
        string Name,
        string Slug,
        string City,
        string AddressLine1,
        string? AddressLine2,                   // Apt/Suite
        string PostalCode,
        string? MallName,                       // "Ušće Shopping Center"
        string Phone,
        string Email,
        decimal Latitude,
        decimal Longitude,
        string WorkingHoursText,
        string ShortDescription,                // Store info
        string CoverImageUrl,
        TrendplusProdavnica.Application.Catalog.Dtos.SeoDto Seo,
        TrendplusProdavnica.Application.Catalog.Dtos.BreadcrumbItemDto[] FeaturedCategories,
        TrendplusProdavnica.Application.Catalog.Dtos.BreadcrumbItemDto[] FeaturedBrands
    );
}
```

---

## Query Request Models

### Catalog Queries

```csharp
namespace TrendplusProdavnica.Application.Catalog.Queries
{
    public record GetHomePageQuery();

    public record GetCategoryListingQuery(
        string Slug,
        int Page = 1,
        int PageSize = 24,
        string? Sort = null,                    // "recommended"|"newest"|"price_asc"|"price_desc"|"bestsellers"
        long[]? Sizes = null,                   // Size variant IDs
        string[]? Colors = null,                // Color names
        long[]? Brands = null,                  // Brand IDs
        decimal? PriceFrom = null,
        decimal? PriceTo = null,
        bool? IsOnSale = null,
        bool? IsNew = null,
        bool? InStockOnly = null
    );

    // Similar structure for:
    public record GetBrandListingQuery(...);
    public record GetCollectionListingQuery(...);
    public record GetSaleListingQuery(...);    // No slug, category filtering optional

    public record GetProductDetailQuery(
        string Slug
    );
}
```

### Content Queries

```csharp
namespace TrendplusProdavnica.Application.Content.Queries
{
    public record GetBrandPageQuery(string Slug);
    public record GetCollectionPageQuery(string Slug);
    public record GetEditorialArticleQuery(string Slug);
}
```

### Store Queries

```csharp
namespace TrendplusProdavnica.Application.Stores.Queries
{
    public record GetStoresQuery(
        string? City = null,                    // Optional city filter
        int Page = 1,
        int PageSize = 20
    );

    public record GetStorePageQuery(
        string Slug
    );
}
```

---

## Service Interfaces

```csharp
namespace TrendplusProdavnica.Application.Catalog.Services
{
    public interface IHomePageQueryService
    {
        Task<HomePageDto> GetHomePageAsync();
    }

    public interface IProductListingQueryService
    {
        Task<ProductListingPageDto> GetCategoryListingAsync(GetCategoryListingQuery query);
        Task<ProductListingPageDto> GetBrandListingAsync(GetBrandListingQuery query);
        Task<ProductListingPageDto> GetCollectionListingAsync(GetCollectionListingQuery query);
        Task<ProductListingPageDto> GetSaleListingAsync(GetSaleListingQuery query);
    }

    public interface IProductDetailQueryService
    {
        Task<ProductDetailDto> GetProductDetailAsync(GetProductDetailQuery query);
    }
}

namespace TrendplusProdavnica.Application.Content.Services
{
    public interface IBrandPageQueryService
    {
        Task<BrandPageDto> GetBrandPageAsync(GetBrandPageQuery query);
    }

    public interface ICollectionPageQueryService
    {
        Task<CollectionPageDto> GetCollectionPageAsync(GetCollectionPageQuery query);
    }

    public interface IEditorialQueryService
    {
        Task<IReadOnlyList<EditorialArticleCardDto>> GetListAsync();
        Task<EditorialArticleDto> GetEditorialArticleAsync(GetEditorialArticleQuery query);
    }
}

namespace TrendplusProdavnica.Application.Stores.Services
{
    public interface IStoreQueryService
    {
        Task<StoreCardDto[]> GetStoresAsync(GetStoresQuery query);
        Task<StorePageDto> GetStorePageAsync(GetStorePageQuery query);
    }
}
```

---

## DTO Usage by Screen

| Screen | Primary DTO | Supporting DTOs | Query Model |
|--------|-------------|-----------------|-------------|
| **Home** | `HomePageDto` | `ProductCardDto`, `CategoryCardDto`, `CollectionTeaserDto` | `GetHomePageQuery` |
| **PLP Category** | `ProductListingPageDto` | `ProductCardDto`, `FilterFacetDto`, `AppliedFilterDto` | `GetCategoryListingQuery` |
| **PLP Brand** | `ProductListingPageDto` | same | `GetBrandListingQuery` |
| **PLP Collection** | `ProductListingPageDto` | same | `GetCollectionListingQuery` |
| **PLP Sale** | `ProductListingPageDto` | same | `GetSaleListingQuery` |
| **PDP** | `ProductDetailDto` | `ProductCardDto`, `ProductMediaDto`, `ProductSizeOptionDto` | `GetProductDetailQuery` |
| **Brand Page** | `BrandPageDto` | `ProductCardDto`, `BreadcrumbItemDto` | `GetBrandPageQuery` |
| **Collection Page** | `CollectionPageDto` | `ProductCardDto`, `MerchBlockDto`, `FaqItemDto` | `GetCollectionPageQuery` |
| **Article List** | `EditorialArticleCardDto[]` | - | (no query) |
| **Article Detail** | `EditorialArticleDto` | `ProductCardDto`, `BreadcrumbItemDto` | `GetEditorialArticleQuery` |
| **Store List** | `StoreCardDto[]` | - | `GetStoresQuery` |
| **Store Detail** | `StorePageDto` | `BreadcrumbItemDto` | `GetStorePageQuery` |

---

## Notes

- All DTOs use **records** for immutability and structural equality
- All files have `#nullable enable` for strict null checking
- Objects use **snake_case** in JSON serialization (configured in API layer)
- No internal persistence details (IDs for navigation, no DbContext references)
- Front-end receives clean, typed contracts ready for UI rendering
- Currency always "RSD" (future-proof for multi-currency)

---

*Last Updated: April 2024*
