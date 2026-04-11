# Trendplus Next.js Storefront Arhitektura

Ovo je premium ecommerce storefront implementacija inspirisan sa ALOHAS, NAKED Copenhagen, On, i Axel Arigato. Arhitektura je fokusirana na čistim linijama, typography-first pristupom i minimalističkim dizajnom.

## 📁 Folder Struktura

```
src/
├── app/                                    # Next.js App Router
│   ├── layout.tsx                         # Root layout sa Header i Footer
│   ├── page.tsx                           # Home page sa svim sekcijama
│   ├── [categorySlug]/
│   │   ├── page.tsx                       # Category listing (cipele, patike, cizme, itd.)
│   │   └── [subcategory]/     
│   │       └── page.tsx                   # Subcategory listing
│   ├── brendovi/
│   │   └── [slug]/
│   │       └── page.tsx                   # Brand detail page
│   ├── kolekcije/
│   │   └── [slug]/
│   │       └── page.tsx                   # Collection detail page
│   ├── proizvod/
│   │   └── [slug]/
│   │       └── page.tsx                   # Product detail page (PDP)
│   ├── editorial/
│   │   ├── page.tsx                       # Editorial listing
│   │   └── [slug]/
│   │       └── page.tsx                   # Editorial detail page
│   ├── prodavnice/
│   │   ├── page.tsx                       # Store locator
│   │   └── [slug]/
│   │       └── page.tsx                   # Store detail page
│   ├── akcija/
│   │   └── page.tsx                       # Sale listing page
│   ├── korpa/
│   │   └── page.tsx                       # Shopping cart
│   ├── omiljeno/
│   │   └── page.tsx                       # Wishlist
│   ├── globals.css                        # Global styles + Tailwind imports
│   └── layout.tsx
│
├── components/                             # Reusable UI components
│   ├── index.tsx                          # Centralized exports
│   ├── layout.tsx                         # Header, Footer, Container
│   ├── product-card.tsx                   # Product card sa slike, cene, badgea
│   ├── product-grid.tsx                   # Responsive product grid (2/3/4 kolone)
│   ├── category-grid.tsx                  # Category tiles sa slikama
│   ├── brand-grid.tsx                     # Brand wall sa logotipima
│   ├── editorial-block.tsx                # Editorial content sa featured layout
│   ├── store-locator.tsx                  # Store listing sa informacijama
│   ├── hero-section.tsx                   # Typography-first hero sekcija
│   ├── section.tsx                        # Section wrapper sa spacing
│   ├── add-to-cart.tsx                    # Add to cart button
│   ├── add-to-wishlist-button.tsx         # Wishlist button
│   └── common.tsx                         # Breadcrumbs, Pagination, EmptyState, LoadingState
│
├── lib/
│   ├── api.ts                             # API client functions
│   ├── types.ts                           # TypeScript types i interfaces
│   └── utils/
│       └── helpers.ts                     # Utility functions (formatPrice, itd.)
│
└── styles/
    └── globals.css                        # Global styles
```

## 🎨 Komponente

### ProductCard
Premium product card sa slike, brenda, imena, cene i badgea (novo, akcija).

```typescript
<ProductCard product={productData} />
```

**Props:**
- `product: ProductCardDto` - Product data

**Features:**
- Next.js Image optimization
- Image lazy loading
- Hover effects
- Sale/New badges
- Price with old price

### ProductGrid
Responsive product grid sa mobile-first pristupom.

```typescript
<ProductGrid 
  products={products}
  columns={4}         // 4 kolone na desktop-u
  gap="md"            // gap: sm | md | lg
/>
```

**Responsive Breakpoints:**
- Mobile: 2 kolone
- Tablet: 3 kolone
- Desktop: 4 kolone (default) ili custom

### CategoryGrid
Grid sa kategoriama sa slikama i broj proizvoda.

```typescript
<CategoryGrid 
  categories={categories}
  columns={3}  // 3 kolone na desktop-u
/>
```

**Features:**
- Responsive 2/3/4 kolone
- Image overlay sa hover efektom
- Product count prikazan
- Link na category page

### BrandGrid
Brand wall sa logotipima ili tekstom.

```typescript
<BrandGrid 
  brands={brands}
  columns={6}  // 6 kolona na desktop-u
/>
```

**Features:**
- Responsive grid
- Logo images ili brand name fallback
- Border hover effect

### EditorialBlock
Editorial content sa featured layout opcijom.

```typescript
<EditorialBlock 
  items={editorial}
  layout="featured"  // featured | grid | list
  columns={3}        // za grid/list layout
/>
```

**Features:**
- Featured item sa većom slikom i tekstom
- Rest items u grid-u
- Publish date prikazan
- Excerpt text
- Link na detail page

### HeroSection
Typography-first hero sekcija bez velikih slika.

```typescript
<HeroSection
  title="Pronađi svoju stilsku obuću"
  subtitle="Premium obuća"
  description="..."
  cta={{ label: "Pregledaj", href: "/" }}
  align="center"      // left | center | right
  maxWidth="lg"       // sm | md | lg
/>
```

**Features:**
- Light typography weight
- Optional subtitle (eyebrow)
- Optional description
- Optional CTA button
- Text alignment
- Max width control

### Section
Wrapper komponenta za sve sekcije sa kontrolom spacinga.

```typescript
<Section 
  spacingTop="lg"     // sm | md | lg | xl
  spacingBottom="md"
  maxWidth="lg"
>
  {children}
</Section>
```

### StoreLocator
Store listing sa grid ili list layout.

```typescript
<StoreLocator 
  stores={stores}
  layout="grid"  // grid | list
/>
```

**Features:**
- Store name, address, city
- Phone i email sa links
- Radno vreme
- Link na store detail page

### Header
Sticky header sa navigacijom.

**Features:**
- Logo
- Desktop navigation
- Mobile menu dropdown
- Wishlist i cart links
- Responsive design

### Footer
Premium footer sa svim sekcijama.

**Features:**
- About sekcija
- Shop links
- Help links
- Newsletter signup
- Privacy/Terms links
- Copyright

## 🔄 Data Fetching

### Server Components
Sve stranice koriste Next.js Server Components za data fetching:

```typescript
export default async function CategoryPage() {
  // Fetch data on server
  const products = await fetch(`/api/catalog/products?category=${slug}`);
  
  return <ProductGrid products={products} />;
}
```

### Cache Strategy
- **Product Listings**: 60 seconds ISR revalidation
- **Product Detail**: 300 seconds ISR revalidation
- **HomePage**: 3600 seconds (1 sati) cache
- **Real-time**: Shopping cart (no cache)

### API Endpoints (iz backend-a)

- `GET /api/catalog/products` - Product listing sa filters/sort
- `GET /api/catalog/product/{slug}` - Product detail
- `GET /api/content/homepage` - Homepage data
- `GET /api/catalog/brands/{slug}` - Brand products
- `GET /api/catalog/collections/{slug}` - Collection products
- `GET /api/content/editorial` - Editorial listing
- `GET /api/stores` - Store locator
- `GET /api/cart` - Shopping cart
- `POST /api/cart/add` - Add to cart
- `GET /api/search` - Product search

## 🎯 Performance Features

### Image Optimization
```typescript
<Image
  src={url}
  alt={alt}
  fill
  className="object-cover"
  sizes="(max-width: 640px) 50vw, 25vw"
  priority={false}  // lazy load
/>
```

### React Server Components
- Svi data fetching na serveru
- No JavaScript za listing stranice
- Streaming za brže first paint

### Lazy Loading
- Images sa `priority={false}` za lazy loading
- Components sa `dynamic` imports za code splitting

### CSS Optimization
- Tailwind CSS sa purging unused styles
- Critical CSS inline-ovano
- PostCSS za CSS optimization

## 📱 Responsive Design

### Mobile First Approach

```css
/* Mobile (default) */
.grid-cols-2 md:grid-cols-3 lg:grid-cols-4

/* Breakpoints */
sm:  640px  (tablet)
md:  768px  (tablet landscape)
lg:  1024px (desktop)
xl:  1280px (large desktop)
```

### Spacing System

```
sm: 8/12px  (py-8 md:py-12)
md: 12/16px (py-12 md:py-16)
lg: 16/24px (py-16 md:py-24)
xl: 20/32px (py-20 md:py-32)
```

## 🎨 Design System

### Typography
- **Font**: System fonts (font-sans)
- **Weights**: Light (300), Regular (400), Medium (500)
- **Sizes**: Tailwind defaults
- **Line Heights**: Tight (snug), Normal, Relaxed

### Colors
- **Primary**: Gray (900 = black)
- **Secondary**: Gray scale (100-900)
- **Accents**: Red (sale), Blue (new)

### Spacing
- 8px grid
- Padding: 4, 6, 8, 12, 16, 20, 24, 32
- Gaps: 4, 6, 8, 10, 12, 16

### Radius
- No radius on most elements
- Minimal rounded corners

## 🚀 Page Templates

Svaka stranica koristi template pattern sa:
1. SEO metadata (`generateMetadata`)
2. Data fetching (async server component)
3. Error handling
4. Loading states
5. Empty states

### Template Files

- `page.template.tsx` - Template sa TODO komentarima za implementaciju
- Kopiraj template -> `page.tsx` -> dodaj tvoje podatke

## 🔧 Setup

```bash
# Install dependencies
npm install

# Development server
npm run dev

# Build
npm run build

# Production
npm run start

# Type checking
npm run type-check
```

## 📋 Checklist za novu stranicu

- [ ] Create `app/[route]/page.tsx`
- [ ] Add `generateMetadata` function
- [ ] Fetch data sa backend API-ja
- [ ] Handle errors i loading states
- [ ] Use `Section` wrapper za spacing
- [ ] Use appropriate components (ProductGrid, CategoryGrid, itd.)
- [ ] Add breadcrumbs
- [ ] Test responsive design
- [ ] Check performance sa Lighthouse

## 🎯 Best Practices

1. **Always use Server Components** - Smanjuje JavaScript bundle
2. **Use Next.js Image** - Automatic optimization
3. **Lazy load images** - `priority={false}` ili `sizes`
4. **Use Section wrapper** - Consistent spacing
5. **Export from components/index.tsx** - Centralized imports
6. **Use TypeScript types** - Type safety
7. **Implement ISR** - Cache where appropriate
8. **Mobile first** - Design za mobile prvo

## 📚 Inspiracija

- **ALOHAS** - Minimalni dizajn, čista tipografija
- **NAKED Copenhagen** - Premium positioning, small details
- **On** - Modern, inovativni pristup
- **Axel Arigato** - Subtle elegancija, attention to detail

---

Zadnje ažurirano: April 2026
