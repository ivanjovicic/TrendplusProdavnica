# TrendplusProdavnica Application Layer Guide

## Overview

The Application layer provides clean read/query contracts for the webshop front-end. It follows a **query-based architecture** optimized for serving data to web clients without exposing internal persistence details or Domain entities.

**Key principles:**
- DTO contracts are optimized for frontend consumption, not 1:1 domain models
- No write commands in this layer (read-only)
- No admin operations or internal persistence details in contracts
- Nullable reference types enabled (`#nullable enable`)
- Simple, production-ready code with clear separation of concerns

---

## Folder Structure

```
TrendplusProdavnica.Application
├── Common/
│   └── ServiceCollectionExtensions.cs       (DI registration)
├── Catalog/
│   ├── Dtos/
│   │   ├── CatalogDtos.cs                   (Product, SEO, Home page DTOs)
│   │   └── ListingDtos.cs                   (Product listing & detail DTOs)
│   ├── Queries/
│   │   ├── ListingQueries.cs                (Listing request models)
│   │   └── ProductQueries.cs                (Detail query model)
│   └── Services/
│       ├── IHomePageQueryService.cs
│       ├── IProductListingQueryService.cs
│       └── IProductDetailQueryService.cs
├── Content/
│   ├── Dtos/
│   │   └── ContentDtos.cs                   (Editorial, Brand, Collection DTOs)
│   ├── Queries/
│   │   └── ContentQueries.cs                (Content page query models)
│   └── Services/
│       ├── IBrandPageQueryService.cs
│       ├── ICollectionPageQueryService.cs
│       └── IEditorialQueryService.cs
└── Stores/
    ├── Dtos/
    │   └── StoreDtos.cs                     (Store DTOs)
    ├── Queries/
    │   └── StoreQueries.cs                  (Store query models)
    └── Services/
        └── IStoreQueryService.cs
```

---

## DTO Models by Feature

### 1. Home Page
**Screen:** `GET /` (Home)  
**DTO:** `HomePageDto`

**Response shape:**
```csharp
{
  "seo": { "title", "description", "canonicalUrl", Keywords[]? },
  "announcementBar": { "text", "backgroundColor"?, "textColor"?, "callToActionUrl"? }?,
  "heroSection": { "title", "subtitle", "imageUrl" },
  "categoryCards": CategoryCardDto[],      // { "name", "slug", "imageUrl"? }
  "newArrivals": ProductCardDto[],         // Product cards with badges, prices
  "featuredCollections": CollectionTeaserDto[],  // { "name", "slug", "coverImageUrl"?, "description"? }
  "bestsellers": ProductCardDto[],
  "brandWall": BrandWallItemDto[],         // { "brandName", "slug", "logoUrl"? }
  "editorialStatement": { "title", "text" }?,
  "storeTeaser": { "name", "slug", "coverImageUrl" }?,
  "trustItems": TrustItemDto[],            // { "title", "description" }
  "newsletter": { "title", "placeholder" }?
}
```

### 2. Product Listing Page (PLP)
**Screens:** 
- `GET /kategorija/{slug}?...` (Category listing)
- `GET /brend/{slug}?...` (Brand listing)
- `GET /kolekcija/{slug}?...` (Collection listing)
- `GET /sale?...` (Sale listing)

**DTO:** `ProductListingPageDto`

**Response shape:**
```csharp
{
  "title": "Category Name",
  "description": "Category description",
  "seo": SeoDto,
  "breadcrumbs": BreadcrumbItemDto[],      // { "label", "url" }
  "introTitle": "Intro Title"?,
  "introText": "Intro text"?,
  "products": ProductCardDto[],
  "facets": FilterFacetDto[],              // Aggregated counts for filters
  "appliedFilters": AppliedFilterDto[],    // Currently active filters
  "pagination": PaginationDto,             // { "page", "pageSize", "totalItems" }
  "merchBlocks": object[],                 // Dynamic content blocks
  "faq": object?                           // FAQ items if available
}
```

**Query Parameters:**
```
page=1
pageSize=24
sort=recommended|newest|price_asc|price_desc|bestsellers
sizes=35,36,37
colors=red,blue
brands=nike,adidas
priceFrom=5000
priceTo=15000
isOnSale=true
isNew=true
inStockOnly=true
```

### 3. Product Detail Page (PDP)
**Screen:** `GET /proizvod/{slug}`  
**DTO:** `ProductDetailDto`

**Response shape:**
```csharp
{
  "id": 12345,
  "slug": "nike-air-max-white",
  "brandName": "Nike",
  "name": "Air Max White",
  "subtitle": "Premium lifestyle shoe",
  "shortDescription": "Comfortable and stylish",
  "longDescription": "Detailed description...",
  "price": 12999.00M,
  "oldPrice": 16999.00M?,
  "currency": "RSD",
  "badges": [ "Novo", "Na sniženju" ],
  "breadcrumbs": BreadcrumbItemDto[],
  "media": ProductMediaDto[],
  "sizes": ProductSizeOptionDto[],
  "storeAvailabilitySummary": object?,
  "relatedProducts": ProductCardDto[],
  "similarProducts": ProductCardDto[],
  "seo": SeoDto,
  "deliveryInfo": "Delivery in 2-3 days",
  "returnInfo": "30 days returns",
  "sizeGuide": object?
}
```

### 4. Brand Page
**Screen:** `GET /brend/{slug}`  
**DTO:** `BrandPageDto`

**Response shape:**
```csharp
{
  "brandName": "Nike",
  "slug": "nike",
  "introText": "Nike is a global leader...",
  "seo": SeoDto,
  "featuredProducts": ProductCardDto[],
  "categoryLinks": BreadcrumbItemDto[],    // Categories with this brand
  "faq": FaqItemDto[]?                     // { "question", "answer" }
}
```

### 5. Collection Page
**Screen:** `GET /kolekcija/{slug}`  
**DTO:** `CollectionPageDto`

**Response shape:**
```csharp
{
  "name": "Summer Collection 2024",
  "slug": "summer-2024",
  "introText": "Discover our summer essentials...",
  "seo": SeoDto,
  "featuredProducts": ProductCardDto[],    // From collection map (pinned first)
  "merchBlocks": MerchBlockDto[],          // { "title", "html", "productSlugs" }
  "faq": FaqItemDto[]?
}
```

### 6. Editorial Article
**Screens:**
- `GET /blog` (Article list)
- `GET /blog/{slug}` (Article detail)

**DTOs:** 
- `EditorialArticleCardDto` (List view)
- `EditorialArticleDto` (Detail view)

**Card shape:**
```csharp
{
  "title": "Best Shoes of 2024",
  "slug": "best-shoes-2024",
  "excerpt": "5 must-have shoes for summer",
  "coverImageUrl": "https://...",
  "publishedAtUtc": "2024-04-08T10:30:00Z",
  "topic": "Fashion"
}
```

**Detail shape (extends card):**
```csharp
{
  ...card fields,
  "body": "HTML content",
  "authorName": "John Doe",
  "relatedProducts": ProductCardDto[],
  "relatedCollections": BreadcrumbItemDto[],
  "relatedCategories": BreadcrumbItemDto[],
  "relatedArticles": EditorialArticleCardDto[]
}
```

### 7. Stores
**Screens:**
- `GET /prodavnice` (Store list)
- `GET /prodavnica/{slug}` (Store detail)

**DTOs:**
- `StoreCardDto` (List view)
- `StorePageDto` (Detail view)

**Card shape:**
```csharp
{
  "name": "Nike Store Downtown",
  "slug": "nike-downtown",
  "city": "Belgrade",
  "addressLine1": "Kneza Miloša 5",
  "workingHoursText": "Mon-Sat: 10am-9pm",
  "phone": "+381 11 1234567",
  "coverImageUrl": "https://..."?
}
```

**Detail shape (extends card):**
```csharp
{
  ...card fields,
  "addressLine2": "2nd floor"?,
  "postalCode": "11000",
  "mallName": "Ušće Shopping Center"?,
  "email": "store@nike.com",
  "latitude": 44.8176,
  "longitude": 20.4762,
  "shortDescription": "Flagship Nike store",
  "seo": SeoDto,
  "featuredCategories": BreadcrumbItemDto[],
  "featuredBrands": BreadcrumbItemDto[]
}
```

---

## Core DTOs Reference

### SeoDto
```csharp
record SeoDto(
  string Title,
  string Description,
  string? CanonicalUrl,
  string[]? Keywords
);
```

### ProductCardDto
Primary DTO for product cards across all listing views.
```csharp
record ProductCardDto(
  long Id,
  string Slug,
  string BrandName,
  string Name,
  string PrimaryImageUrl,
  string? SecondaryImageUrl,
  decimal Price,
  decimal? OldPrice,
  string Currency,
  string[] Badges,              // ["Novo", "Na sniženju"]
  bool IsInStock,
  int AvailableSizesCount,
  string? ColorLabel
);
```

### PaginationDto
```csharp
record PaginationDto(int Page, int PageSize, long TotalItems)
{
  public int TotalPages { get; }      // Calculated
  public bool HasPrevious { get; }    // Calculated
  public bool HasNext { get; }        // Calculated
}
```

### FilterFacetDto
```csharp
record FilterFacetDto(
  string Key,                         // "sizes", "colors", "brands", "sale", "new", "stock"
  string Label,                       // Display name
  string Type,                        // "checkbox", "range", "toggle"
  FilterOptionDto[] Options
);

record FilterOptionDto(
  string Value,                       // "36", "red", "nike"
  string Label,                       // "EU 36", "Red", "Nike"
  int Count,                          // Number of products matching
  bool Selected,                      // Currently active
  bool Disabled                       // No products available
);
```

### BreadcrumbItemDto
```csharp
record BreadcrumbItemDto(string Label, string Url);
```

---

## Service Interfaces

### IHomePageQueryService
```csharp
Task<HomePageDto> GetHomePageAsync();
```
Location: `Catalog/Services/IHomePageQueryService.cs`

### IProductListingQueryService
```csharp
Task<ProductListingPageDto> GetCategoryListingAsync(GetCategoryListingQuery query);
Task<ProductListingPageDto> GetBrandListingAsync(GetBrandListingQuery query);
Task<ProductListingPageDto> GetCollectionListingAsync(GetCollectionListingQuery query);
Task<ProductListingPageDto> GetSaleListingAsync(GetSaleListingQuery query);
```
Location: `Catalog/Services/IProductListingQueryService.cs`

### IProductDetailQueryService
```csharp
Task<ProductDetailDto> GetProductDetailAsync(GetProductDetailQuery query);
```
Location: `Catalog/Services/IProductDetailQueryService.cs`

### IBrandPageQueryService
```csharp
Task<BrandPageDto> GetBrandPageAsync(GetBrandPageQuery query);
```
Location: `Content/Services/IBrandPageQueryService.cs`

### ICollectionPageQueryService
```csharp
Task<CollectionPageDto> GetCollectionPageAsync(GetCollectionPageQuery query);
```
Location: `Content/Services/ICollectionPageQueryService.cs`

### IEditorialQueryService
```csharp
Task<IReadOnlyList<EditorialArticleCardDto>> GetListAsync();
Task<EditorialArticleDto> GetEditorialArticleAsync(GetEditorialArticleQuery query);
```
Location: `Content/Services/IEditorialQueryService.cs`

### IStoreQueryService
```csharp
Task<StoreCardDto[]> GetStoresAsync(GetStoresQuery query);
Task<StorePageDto> GetStorePageAsync(GetStorePageQuery query);
```
Location: `Stores/Services/IStoreQueryService.cs`

---

## Query Request Models

### Listing Queries
Located in `Catalog/Queries/ListingQueries.cs`:

```csharp
record GetCategoryListingQuery(
  string Slug,
  int Page = 1,
  int PageSize = 24,
  string? Sort = null,
  long[]? Sizes = null,
  string[]? Colors = null,
  long[]? Brands = null,
  decimal? PriceFrom = null,
  decimal? PriceTo = null,
  bool? IsOnSale = null,
  bool? IsNew = null,
  bool? InStockOnly = null
);
// Similar structure for:
// - GetBrandListingQuery
// - GetCollectionListingQuery
// - GetSaleListingQuery
```

### Product Detail Query
Located in `Catalog/Queries/ProductQueries.cs`:
```csharp
record GetProductDetailQuery(string Slug);
```

### Content Queries
Located in `Content/Queries/ContentQueries.cs`:
```csharp
record GetBrandPageQuery(string Slug);
record GetCollectionPageQuery(string Slug);
record GetEditorialArticleQuery(string Slug);
```

### Store Queries
Located in `Stores/Queries/StoreQueries.cs`:
```csharp
record GetStoresQuery(string? City = null, int Page = 1, int PageSize = 20);
record GetStorePageQuery(string Slug);
```

---

## Dependency Injection

Register all Application services in `Common/ServiceCollectionExtensions.cs`:

```csharp
services.AddApplicationServices()
  // Registers:
  // - IHomePageQueryService
  // - IProductListingQueryService
  // - IProductDetailQueryService
  // - IBrandPageQueryService
  // - ICollectionPageQueryService
  // - IEditorialQueryService
  // - IStoreQueryService
```

**Usage in API:**
```csharp
var services = new ServiceCollection()
    .AddApplicationServices()  // Register Application layer
    .AddInfrastructureServices(connectionString)  // Register Infrastructure implementations
    .BuildServiceProvider();
```

---

## Design Patterns

### 1. Read-Only Operations
All services are read-only (no mutations). Data flows from DB → DTO → API response.

### 2. Server-Side Projections
Infrastructure layer uses **EF Core server-side projections** to map Domain entities to DTOs, avoiding client evaluation and unnecessary data transfer.

### 3. Faceted Navigation
Facet counts are calculated from the same filtered query used for pagination (pre-pagination), ensuring counts match available products.

### 4. Nullable Reference Types
All DTOs use `#nullable enable` and explicit `?` markers for optional fields.

### 5. Record-Based DTOs
All DTOs use `record` types for immutability, structural equality, and clean syntax.

---

## Common Filtering Challenges

### Size Filtering
- Sizes are variadic (`long[]` IDs from `SizeVariant`)
- Filter matches products where at least one variant has the given size
- Examples: `sizes=35,36,37` matches products with any of these sizes

### Price Range
- `priceFrom` / `priceTo` filter on active variant prices
- Prices are in the currency of the variant
- Currently assumes single currency (RSD) per product

### Stock Status
- `inStockOnly=true` filters for products with at least one visible, active variant in stock
- `StockStatus != OutOfStock` check per variant

### Sale Status
- `isOnSale=true` matches products with visible, active variants where `OldPrice` is not null
- Typically used for discount/sale sections

---

## Future Enhancements

- [ ] Add materialized view support for facet aggregations
- [ ] Implement caching layer for Home page
- [ ] Add support for A/B testing in listings
- [ ] Extend Editorial relationship models
- [ ] Add store inventory real-time availability
- [ ] Implement search/full-text support

---

## Notes for Frontend Teams

1. **Always check `HasNext`** in pagination before requesting the next page
2. **Badges array** is dynamic; clients should render whatever badges are included
3. **Nullable SEO fields** can be present or absent; use fallbacks
4. **ProductCardDto** is uniform across all listing contexts for consistency
5. **Filter values** (`colors`, `brands`) are returned in both filters and facets
6. **Breadcrumbs** include the current page (e.g., category page includes the category itself)
7. **Sort options** are:
   - `recommended` - default, combines bestseller status and recency
   - `newest` - newest products first
   - `price_asc` - lowest price first
   - `price_desc` - highest price first
   - `bestsellers` - bestseller status

---

Generated: April 2024 | .NET 10 | Entity Framework Core | Production Ready
