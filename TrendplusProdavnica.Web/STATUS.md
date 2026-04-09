# TrendplusProdavnica.Web - Frontend Setup Progress

## ✅ Completed
- [x] Project structure initialization (package.json, tsconfig, next.config)
- [x] Tailwind CSS setup
- [x] Base API client layer with fetching wrapper
- [x] Complete TypeScript types from backend DTOs
- [x] Core components (Header, Footer, Container, ProductCard, ProductGrid)
- [x] Common UI components (Breadcrumbs, Pagination, Empty/Loading states)
- [x] Add to Cart functionality (client component with localStorage)
- [x] Root layout with metadata
- [x] Home page (server component with error handling)
- [x] Global styles (globals.css)

## 📋 Ready to Implement

### Routes to Create:
1. `/[categorySlug]` - Category listing page
2. `/brendovi/[slug]` - Brand detail page
3. `/kolekcije/[slug]` - Collection detail page
4. `/proizvod/[slug]` - Product detail page
5. `/editorial` - Articles list page
6. `/editorial/[slug]` - Article detail page
7. `/prodavnice` - Stores list page
8. `/prodavnice/[slug]` - Store detail page
9. `/korpa` - Shopping cart page
10. `/akcija` - Sale listing page
11. `/akcija/[categorySlug]` - Sale by category page

### API Functions Available:
- Pages: `getHomePage()`
- Listings: `getCategoryListing()`, `getBrandListing()`, `getCollectionListing()`, `getSaleListing()`, `getSaleCategoryListing()`
- Products: `getProductDetail()`
- Brands: `getBrandPage()`
- Collections: `getCollectionPage()`
- Editorial: `getEditorialList()`, `getEditorialArticle()`
- Stores: `getStores()`, `getStore()`
- Cart: `createCart()`, `getCart()`, `addToCart()`, `updateCartItem()`, `removeCartItem()`, `clearCart()`

## 🎯 Implementation Strategy

### Server vs Client Components:
- **Server Components**: All listing pages, product detail, brand/collection/editorial pages, store pages
  - Reason: SEO, static generation, backend data fetching
- **Client Components**: 
  - AddToCart button (state management)
  - Filter drawer (interactivity)
  - Cart page (full interactivity & state)
  - Size selector

### Data Fetching:
- All list pages use `getCategoryListing()` with URL search params
- Product details use `getProductDetail()` with slug
- Cart uses client-side API calls for CRUD operations
- Cart token stored in localStorage via `cart-storage.ts` utils

### Styling:
- Tailwind CSS for all components
- Custom globals.css for base styles
- `cn()` utility from helpers.ts for className merging

## 📦 Environment Setup

Create `.env.local` with:
```
NEXT_PUBLIC_API_BASE_URL=https://localhost:7002/api
NEXT_PUBLIC_SITE_URL=http://localhost:3000
```

## 🚀 Next Steps

1. Create remaining route files with server components
2. Implement dynamic routes with `generateStaticParams()` for SEO
3. Add meta tags via `generateMetadata()` for each page
4. Implement cart page with client-side state
5. Add filter UI for listing pages
6. Optional: Add image optimization, caching headers, pagination UI
