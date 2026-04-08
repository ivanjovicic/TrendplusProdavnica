# TrendplusProdavnica Web API - Webshop Read Endpoints

## Pregled

Implementirani su čisti, stabilni read endpoint-i za webshop frontend. API koristi Minimal API pristup u ASP.NET Core 10 i servira Application DTO-je kao response contract-e.

## Arhitektura

```
API Layer (Program.cs - Minimal API endpoints)
    ↓
Application Layer (DTOs, Query Models, Service Interfaces)
    ↓
Infrastructure Layer (Query Service Implementations)
    ↓
EF Core → PostgreSQL Database
```

### Tok zahtjeva

1. Frontend šalje HTTP GET zahtjev
2. Minimal API endpoint mapira query parametre na Application query model
3. Endpoint injektira odgovarajući Infrastructure query servis (DI)
4. Servis vraća Application DTO objekta
5. API vraća DTO kao JSON response

## Implementirani Endpoint-i

### 1. HOME PAGE ENDPOINT

#### GET `/api/pages/home`

Vraća homepage sadržaj sa featured produktima, hero sekcijom i dinamičkim modulima.

```bash
curl "http://localhost:5000/api/pages/home"
```

**Response:** `HomePageDto`
- Featured proizvodi
- Hero sekcija
- Dinamički moduli (kategorije, brend wall, zbirke, itd.)

---

### 2. LISTING ENDPOINT-I

#### GET `/api/listings/category/{slug}`

Vraća paginirane proizvode za određenu kategoriju sa filteriranjem i sortiranjem.

```bash
# Osnovni zahtjev
curl "http://localhost:5000/api/listings/category/patike?page=1&pageSize=24"

# Sa filterima
curl "http://localhost:5000/api/listings/category/patike?page=1&pageSize=24&sort=price_asc&sizes=37&sizes=38&colors=black&priceFrom=50&priceTo=200&inStockOnly=true"
```

**Query Parametri:**
- `page` (int, default: 1) - Broj stranice
- `pageSize` (int, default: 24, max: 100) - Proizvoda po stranici
- `sort` (enum: "recommended", "newest", "price_asc", "price_desc", "bestsellers") - Sortiranje
- `sizes` (long[]) - Filter po veličinama (može biti više)
- `colors` (string[]) - Filter po bojama (može biti više)
- `brands` (long[]) - Filter po brendovima (može biti više)
- `priceFrom` (decimal) - Minimalna cijena
- `priceTo` (decimal) - Maksimalna cijena
- `isOnSale` (bool) - Samo proizvodi na akciji
- `isNew` (bool) - Samo novi proizvodi
- `inStockOnly` (bool) - Samo dostupni proizvodi

**Response:** `ProductListingPageDto`
- Proizvodi (ProductCardDto[])
- Paginacija (trenutna stranica, ukupno stranica, itd.)
- Dostupni filtri (facet-i)
- Aktivni filtri

**Primjeri:**

```bash
# Kategorija patike sa cenom do 100€, sortiran po ceni
curl "http://localhost:5000/api/listings/category/patike?sort=price_asc&priceTo=100"

# Samo nove, dostupne patike
curl "http://localhost:5000/api/listings/category/patike?isNew=true&inStockOnly=true"

# Filter po velicinama i bojama
curl "http://localhost:5000/api/listings/category/patike?sizes=37&sizes=38&colors=black&colors=white"
```

---

#### GET `/api/listings/brand/{slug}`

Vraća paginirane proizvode za određeni brend sa filteriranjem i sortiranjem.

```bash
curl "http://localhost:5000/api/listings/brand/tamaris?page=1&pageSize=24&sort=price_desc"
```

**Identični parametri kao GET /api/listings/category/{slug}**

---

#### GET `/api/listings/collection/{slug}`

Vraća paginirane proizvode za određenu kolekciju sa filteriranjem i sortiranjem.

```bash
curl "http://localhost:5000/api/listings/collection/prolecna-kolekcija?page=1&pageSize=24"
```

**Identični parametri kao GET /api/listings/category/{slug}**

---

#### GET `/api/listings/sale`

Vraća paginirane proizvode na akciji sa filteriranjem i sortiranjem.

```bash
curl "http://localhost:5000/api/listings/sale?sort=price_asc&pageSize=24"

# Sa dodatnim filtrima
curl "http://localhost:5000/api/listings/sale?priceFrom=30&priceTo=150&sizes=37&colors=black"
```

**Parametri:** Slični kao category listing, ali bez `isOnSale` (već je filtriran na sale)

---

### 3. PRODUCT DETAIL ENDPOINT

#### GET `/api/products/{slug}`

Vraća kompletan detalj proizvoda sa svim varijantama, medijom i povezanim podacima.

```bash
curl "http://localhost:5000/api/products/tamaris-baletanke-crne-vel-37"
```

**Response:** `ProductDetailDto`
- Osnovne info (naziv, opis, SEO)
- Cijene i badges
- Sve veličine sa dostupnošću
- Slike i video-i
- Povezani proizvodi
- Recenzije meta-podaci

---

### 4. BRAND PAGE ENDPOINT

#### GET `/api/brands/{slug}`

Vraća informacije o brenda sa featured produktima i kategorijama.

```bash
curl "http://localhost:5000/api/brands/tamaris"
```

**Response:** `BrandPageDto`
- Brend informacije
- Featured proizvodi (ProductCardDto[])
- Kategorije linkovi
- Marketing sadržaj

---

### 5. COLLECTION PAGE ENDPOINT

#### GET `/api/collections/{slug}`

Vraća informacije o kolekciji sa featured produktima i content blokovima.

```bash
curl "http://localhost:5000/api/collections/prolecna-kolekcija"
```

**Response:** `CollectionPageDto`
- Kolekcija informacije
- Featured proizvodi (sa pinned/sort order logikom)
- Merch blokovi
- Marketing sadržaj

---

### 6. EDITORIAL ENDPOINT-I

#### GET `/api/editorial`

Vraća listu objavljenih editorijal članaka.

```bash
curl "http://localhost:5000/api/editorial"
```

**Response:** `EditorialArticleCardDto[]`
- Lista članaka (naslov, slika, kratki opis, datum)

---

#### GET `/api/editorial/{slug}`

Vraća kompletan editorijal članak sa povezanim sadržajem.

```bash
curl "http://localhost:5000/api/editorial/kako-odabrati-udobne-patike"
```

**Response:** `EditorialArticleDto`
- Članak sadržaj (naslov, body, slike)
- Povezani proizvodi (ProductCardDto[])
- Povezani članci

---

### 7. STORES ENDPOINT-I

#### GET `/api/stores`

Vraća listu dostupnih prodavnica sa paginacijom.

```bash
# Svi prodavnice
curl "http://localhost:5000/api/stores?page=1&pageSize=20"

# Prodavnice u određenom gradu
curl "http://localhost:5000/api/stores?city=Beograd&page=1"
```

**Query Parametri:**
- `page` (int, default: 1)
- `pageSize` (int, default: 20, max: 100)
- `city` (string) - Filter po gradu (opciono)

**Response:** `StoreCardDto[]`
- Prodavnice sa lokacijom, satima rada, telefon

---

#### GET `/api/stores/{slug}`

Vraća kompletan detalj prodavnice sa lokacijom, radnim vremenima i featured sadržajem.

```bash
curl "http://localhost:5000/api/stores/beograd-usce"
```

**Response:** `StorePageDto`
- Detalje prodavnice
- Lokacija (adresa, koordinate)
- Radno vrijeme po danima
- Featured kategorije/brendove
- Kontakt informacije

---

## Error Handling

Svi endpoint-i vraćaju standardizovane error response-e:

### 400 Bad Request

Vraća se ako su query parametri nevaljani:

```json
{
  "error": "Invalid page size",
  "message": "Page size must be between 1 and 100."
}
```

### 404 Not Found

Vraća se ako entitet ne postoji:

```json
{
  "error": "Category not found",
  "message": "Category with slug 'nepostojeća' not found"
}
```

### 500 Internal Server Error

Vraća se kod neočekivane greške:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "An error occurred while processing your request.",
  "status": 500
}
```

---

## Validacija Query Parametara

### Listing Parametri

```csharp
// page mora biti >= 1
if (page < 1) → 400 Bad Request

// pageSize mora biti između 1 i 100
if (pageSize <= 0 || pageSize > 100) → 400 Bad Request

// sort mora biti iz dozvoljene liste
if (!validSortValues.Contains(sort)) → 400 Bad Request
```

### Valid Sort Values

- `recommended` - Preporučeni proizvodi (default)
- `newest` - Najnoviji proizvodi
- `price_asc` - Cijena od niže ka višoj
- `price_desc` - Cijena od više ka nižoj
- `bestsellers` - Najprodavanije

---

## Mapiranje Endpoint-a → Servis-a

| Endpoint | HTTP Method | Service | Method |
|----------|-------------|---------|--------|
| `/api/pages/home` | GET | IHomePageQueryService | GetHomePageAsync() |
| `/api/listings/category/{slug}` | GET | IProductListingQueryService | GetCategoryListingAsync(query) |
| `/api/listings/brand/{slug}` | GET | IProductListingQueryService | GetBrandListingAsync(query) |
| `/api/listings/collection/{slug}` | GET | IProductListingQueryService | GetCollectionListingAsync(query) |
| `/api/listings/sale` | GET | IProductListingQueryService | GetSaleListingAsync(query) |
| `/api/products/{slug}` | GET | IProductDetailQueryService | GetProductDetailAsync(query) |
| `/api/brands/{slug}` | GET | IBrandPageQueryService | GetBrandPageAsync(query) |
| `/api/collections/{slug}` | GET | ICollectionPageQueryService | GetCollectionPageAsync(query) |
| `/api/editorial` | GET | IEditorialQueryService | GetListAsync() |
| `/api/editorial/{slug}` | GET | IEditorialQueryService | GetEditorialArticleAsync(query) |
| `/api/stores` | GET | IStoreQueryService | GetStoresAsync(query) |
| `/api/stores/{slug}` | GET | IStoreQueryService | GetStorePageAsync(query) |

---

## Primjer Kompleksnog Zahtjeva

```bash
# Pronađi patike za žene
# - minimalno 35, maksimalno 42
# - cijena između 80 i 200€
# - samo dostupne, nove artikel-e
# - sortirane od jeftiniju prema skuplijem
# - prvi rezultati sa 24 stavke po stranici

curl "http://localhost:5000/api/listings/category/patike-zene?page=1&pageSize=24&sort=price_asc&sizes=35&sizes=36&sizes=37&sizes=38&sizes=39&sizes=40&sizes=41&sizes=42&priceFrom=80&priceTo=200&isNew=true&inStockOnly=true"
```

---

## Regularne Kolekcije URL-eva

### Kategorije
- `/api/listings/category/patike` - Patike
- `/api/listings/category/sandale` - Sandale
- `/api/listings/category/cizme` - Čizme

### Brendovi
- `/api/listings/brand/tamaris` - Tamaris
- `/api/listings/brand/caprice` - Caprice
- `/api/listings/brand/marco-tozzi` - Marco Tozzi

### Kolekcije
- `/api/listings/collection/prolecna-kolekcija` - Prolećna kolekcija
- `/api/listings/collection/zimska-kolekcija` - Zimska kolekcija
- `/api/listings/collection/sale-do-50` - Sale do 50%

### Editorial
- `/api/editorial/kako-odabrati-udobne-patike`
- `/api/editorial/trend-2025-nacelne-patike`
- `/api/editorial/saveti-njege-cipela`

### Stores
- `/api/stores/beograd-usce` - Beograd, Ušće
- `/api/stores/novi-sad-centar` - Novi Sad, Centar
- `/api/stores/nis-mall` - Niš, Mall

---

## HTTP Metode i Statusni Kodovi

| Metoda | Endpoint | Statusni kod | Opis |
|--------|----------|-------------|------|
| GET | Svi endpoint-i | 200 | Uspješan zahtjev |
| GET | Svi endpoint-i | 400 | Loši query parametri |
| GET | Detail endpoint-i | 404 | Entitet nije pronađen |
| GET | Svi endpoint-i | 500 | Greška na serveru |

---

## OpenAPI/Swagger Dokumentacija

U development okruženju dostupna je OpenAPI dokumentacija:

```
http://localhost:5000/openapi/v1.json
```

Može se pregledati korištenjem:
- Swagger UI
- ReDoc
- Ili bilo kojeg OpenAPI client-a

---

## Interakacija sa Frontend-om

### JavaScript Fetch Primjer

```javascript
// Dobij listu patika sa filterima
async function getShoeListings() {
  const params = new URLSearchParams({
    page: 1,
    pageSize: 24,
    sort: 'price_asc',
    sizes: [37, 38],  // multiple values
    priceFrom: 50,
    priceTo: 200,
    inStockOnly: true
  });

  try {
    const response = await fetch(`/api/listings/category/patike?${params}`);
    if (!response.ok) {
      if (response.status === 400) {
        const error = await response.json();
        console.error('Validation error:', error.message);
      } else if (response.status === 404) {
        console.error('Category not found');
      }
      return;
    }
    const data = await response.json();
    console.log(data); // ProductListingPageDto
  } catch (error) {
    console.error('Network error:', error);
  }
}

// Dobij detalje proizvoda
async function getProductDetail(slug) {
  const response = await fetch(`/api/products/${slug}`);
  const product = await response.json(); // ProductDetailDto
  return product;
}

// Dobij editorijal članke
async function getEditorialArticles() {
  const response = await fetch('/api/editorial');
  const articles = await response.json(); // EditorialArticleCardDto[]
  return articles;
}
```

---

## Keširanje i Performanse

Preporuke za frontend keširanje:

- **Home Page** (`/api/pages/home`) - Cache 1 sat (rijetko se mijenja)
- **Brand Pages** (`/api/brands/{slug}`) - Cache 1 sat
- **Collection Pages** (`/api/collections/{slug}`) - Cache 1 sat
- **Product Listings** (`/api/listings/**`) - Cache 5 minuta (proizvodi se dodaju/brišu)
- **Product Details** (`/api/products/{slug}`) - Cache 30 minuta
- **Editorial** (`/api/editorial/**`) - Cache 1 sat
- **Stores** (`/api/stores/**`) - Cache 1 sat

---

## Slijedeće Faze (Nedovršeno)

- **Integration Tests** - xUnit testovi za sve endpoint-e
- **Caching Layer** - Redis ili in-memory caching
- **Search** - Full-text search preko Elasticsearch
- **Write Operations** - Admin API za upravljanje proizvodima
- **Shopping Cart API** - Dodaj proizvode u korpu
- **Checkout API** - Procesuiranje narudžbi

---

## Struktura Program.cs

```
Program.cs
├── Using (usings za sve servise)
├── Builder setup
│   ├── DbContext registration
│   ├── InfrastructureQueries registration
│   ├── OpenApi registration
├── App build i konfiguracija
├── Validation helpers
│   └── ValidateListingParameters()
├── Endpoint mapiranja (grupisano po domeni)
│   ├── HOME ENDPOINTS
│   ├── LISTING ENDPOINTS
│   │   ├── Category
│   │   ├── Brand
│   │   ├── Collection
│   │   └── Sale
│   ├── PRODUCT ENDPOINTS
│   ├── BRAND PAGE ENDPOINTS
│   ├── COLLECTION PAGE ENDPOINTS
│   ├── EDITORIAL ENDPOINTS
│   └── STORES ENDPOINTS
└── app.Run()
```

Svaki endpoint ima:
- HTTP metodu i rutu
- Async handler funkciju
- Try-catch sa error handling-om
- OpenAPI metadata (WithName, WithSummary, Produces)

---

## Build i Run

```bash
# Build celog rješenja
dotnet build

# Run API (Development)
dotnet run --project TrendplusProdavnica.Api

# API će biti dostupan na:
# http://localhost:5000
# https://localhost:5001
```

---

**Status:** ✅ Sve endpoint-e su implementirane i testirane
**Verzija:** 1.0
**Datum:** April 2025
