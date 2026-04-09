# TrendplusProdavnica.Web - Frontend aplikacija

## Struktura Projekta

```
src/
├── app/                    # Next.js App Router stranice
│   ├── layout.tsx         # Root layout
│   ├── globals.css        # Global stilovi
│   ├── page.tsx           # Home page
│   ├── [categorySlug]/
│   │   └── page.tsx       # Category listing
│   ├── brendovi/
│   │   └── [slug]/page.tsx
│   ├── kolekcije/
│   │   └── [slug]/page.tsx
│   ├── proizvod/
│   │   └── [slug]/page.tsx
│   ├── editorial/
│   │   ├── page.tsx
│   │   └── [slug]/page.tsx
│   ├── prodavnice/
│   │   ├── page.tsx
│   │   └── [slug]/page.tsx
│   ├── akcija/
│   │   ├── page.tsx
│   │   └── [categorySlug]/page.tsx
│   └── korpa/
│       └── page.tsx
├── components/
│   ├── layout.tsx         # Header, Footer, Container
│   ├── product-card.tsx   # ProductCard, ProductGrid
│   ├── common.tsx         # Breadcrumbs, Pagination, Empty/Loading
│   └── add-to-cart.tsx    # AddToCartButton (client)
└── lib/
    ├── api/
    │   ├── api-client.ts  # Base API client
    │   ├── index.ts       # Sve API funkcije
    │   └── cart.ts        # Cart API funkcije
    ├── types/
    │   └── index.ts       # Svi TypeScript tipovi
    ├── config/
    │   └── env.ts         # Konfiguracija
    └── utils/
        ├── helpers.ts     # Utility funkcije
        └── cart-storage.ts # Cart localStorage helpers
```

## Postavka

### 1. Instaliraj závísnosti:
```bash
npm install
```

### 2. Kreiraj `.env.local`:
```env
NEXT_PUBLIC_API_BASE_URL=https://localhost:7002/api
NEXT_PUBLIC_SITE_URL=http://localhost:3000
```

### 3. Pokreni dev server:
```bash
npm run dev
```

Otvori [http://localhost:3000](http://localhost:3000)

## Arhitektura

### API Client Sloj
`src/lib/api/api-client.ts` - Centralizovani fetch wrapper:
- Automatski query parameters handling
- JSON parsing
- Error handling
- Typed responses

`src/lib/api/index.ts` - Sve API funkcije (pages, listings, products, brands, itd.)

`src/lib/api/cart.ts` - Cart CRUD operacije

### TypeScript Tipovi
`src/lib/types/index.ts` - Sadrži sve tipove iz backend DTO-a:
- Product, ProductCard, ProductDetail
- Home, Brand, Collection, Editorial, Store
- Cart, CartItem
- Svi ostali DTOi

### Komponente

**Server Components (SEO-friendly):**
- Sve listing stranice
- Product detail page
- Brand/Collection/Editorial/Store detail pages

**Client Components (za interakciju):**
- `AddToCartButton` - koristi cart API
- Filter UI (trebalo bi da se doda)
- Cart page - full client component

### Cart Menadžment

Cart token se čuva u `localStorage` putem `src/lib/utils/cart-storage.ts`:
```typescript
import { getCartToken, setCartToken } from '@/lib/utils/cart-storage';

// Dobij postojeći token
const token = getCartToken();

// Kreiraj novi
const newCart = await createCart();
setCartToken(newCart.cartToken);

// Dodaj u korpu
await addToCart(token, { productVariantId: 123, quantity: 1 });
```

## Routing Map

| Route | Komponenta | Opis |
|-------|-----------|------|
| `/` | Home | Home page |
| `/{categorySlug}` | CategoryPage | Listing po kategoriji |
| `/brendovi/{slug}` | BrandPage | Brand detalji |
| `/kolekcije/{slug}` | CollectionPage | Collection detalji |
| `/proizvod/{slug}` | ProductPage | Product detalji |
| `/editorial` | EditorialList | Članci listing |
| `/editorial/{slug}` | EditorialDetail | Članak detalji |
| `/prodavnice` | StoresList | Prodavnice listing |
| `/prodavnice/{slug}` | StoreDetail | Prodavnica detalji |
| `/akcija` | SaleListing | Svi proizvodi na sniženju |
| `/akcija/{categorySlug}` | SaleCategory | Sniženi po kategoriji |
| `/korpa` | Cart | Korpa |

## Data Fetching Strategija

### Server Components (default)
```typescript
// Koristi server-side data fetching za SEO
export default async function CategoryPage({ params, searchParams }) {
  const data = await getCategoryListing(params.slug, searchParams);
  
  return (
    <div>
      {/* JSX sa podacima */}
    </div>
  );
}
```

### Client Components
```typescript
'use client';

export default function AddToCartButton() {
  const [loading, setLoading] = useState(false);
  
  const handleClick = async () => {
    await addToCart(token, payload);
  };
  
  return <button onClick={handleClick}>{loading ? '...' : 'Add'}</button>;
}
```

## Stilizacija

- **Tailwind CSS** za sve stilove
- `src/app/globals.css` za globalne stilove
- `src/lib/utils/helpers.ts` - `cn()` funkcija za className merging

Primer:
```typescript
<button className={cn(
  'px-4 py-2 rounded',
  loading && 'opacity-50 cursor-not-allowed'
)}>
  Click me
</button>
```

## Sledeći koraci

### Za kompletniji frontend:
1. **Listing filteri** - Dodaj FilterSidebar komponentu
   - Filteruj po brandu, boji, veličini, ceni
   - Koristi URL query params

2. **Cart page** - Kompletan cart sa:
   - Lista stavki sa quantity controls
   - Remove item
   - Cart summary
   - Checkout button

3. **SEO optimizacija:**
   - Dodaj `generateMetadata()` za sve stranice
   - Open Graph meta tags
   - Structured data (JSON-LD)

4. **Image optimization:**
   - Koristi Next.js `Image` komponentu
   - Auto srcset gen

5. **Performance:**
   - Implementiraj `generateStaticParams()` za dynamic routes
   - ISR (Incremental Static Regeneration) caching

6. **UI/UX poboljšanja:**
   - Quantity selector na PDP
   - Size picker sa stock info
   - Wishlist
   - Product reviews/ratings
   - Related products

## Troubleshooting

### "API not found" greške:
- Proveri da je backend API pokrenut na `http://localhost:7002`
- Proveri `.env.local` konfiguraciju

### Cart token greške:
- Otvori browser console
- Proveri da li localStorage sprema token (Application tab)
- Resetuj: `localStorage.clear()`

### Build greške:
```bash
npm run type-check  # Check TypeScript errors
npm run lint        # Run linter
```

## API Reference

Sve API funkcije su dostupne iz `src/lib/api/index.ts`:

```typescript
// Pages
getHomePage()

// Listings (sa optional params: page, pageSize, sort, filters)
getCategoryListing(slug, params)
getBrandListing(slug, params)
getCollectionListing(slug, params)
getSaleListing(params)
getSaleCategoryListing(category, params)

// Details
getProductDetail(slug)
getBrandPage(slug)
getCollectionPage(slug)
getEditorialList()
getEditorialArticle(slug)
getStores(params)
getStore(slug)

// Cart
createCart()
getCart(cartToken)
addToCart(cartToken, {productVariantId, quantity})
updateCartItem(cartToken, itemId, {quantity})
removeCartItem(cartToken, itemId)
clearCart(cartToken)
```

## Napomene

- Sve stranice koriste server components za SEO
- Cart je client component jer zahteva interakciju
- API errorи se hvataü sa try/catch
- LoadingState komponenta se koristi tokom fetchinga
- URLs trebalo bi da budu SEO-friendly sa slugovima

## Support

Za probleme ili pitanja, kontaktiraj dev team.
