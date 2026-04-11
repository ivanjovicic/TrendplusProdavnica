# SEO/Public Contract Audit & Remediation Plan

## 🔴 KRITIČNI PROBLEMI PRONAĐENI

### Problem #1: Sitemap koristi ADMIN endpoint-e
**Lokacija**: `TrendplusProdavnica.Web/src/lib/seo/sitemap.ts` (lines 74-82)

**Trenutno**:
```typescript
const [brands, collections, products, stores, editorial] = await Promise.all([
  fetchSeoJson<BrandSitemapItem[]>('/admin/brands').catch(() => []),              // ❌ ADMIN
  fetchSeoJson<CollectionSitemapItem[]>('/admin/collections').catch(() => []),    // ❌ ADMIN
  fetchSeoJson<ProductSitemapItem[]>('/admin/products?status=Published')...       // ❌ ADMIN
  fetchSeoJson<StoreSitemapItem[]>('/admin/stores')...                            // ❌ ADMIN
```

**Problem**: 
- Javna SEO struktura zavisi od admin CRUD layera
- Admin endpoint-i mogu biti isključeni/zaštićeni/promenljivi
- Sitemap je javna SEO surface i ne sme biti krhka

**Rešenje**: 
- Kreiraj `/api/seo/sitemap/*` public endpoint-e
- Dedikovan response format samo za SEO
- Nisu dio admin CRUD-a

---

### Problem #2: Category routing je fragmentiran
**Lokacija**: Sve route-ove koriste različite pattern-e

**Trenutno stanje**:
```
Root dynamic route:      /[categorySlug]/page.tsx
  Koristi: /api/listings/category/{slug}
  URL: /{categorySlug} → /cipele, /patike, itd.
  
Legacy SEO landing:      /kategorija/[slug]/page.tsx
  Koristi: CategorySeoContent (novo)
  URL: /kategorija/{slug}
  
Sale by category:        /akcija/[categorySlug]/page.tsx
  Koristi: /api/listings/sale/{categorySlug}
  URL: /akcija/{categorySlug}
```

**Problem**:
- Dva različita puta za category landing
- `/{slug}` (root) vs `/kategorija/{slug}` (explicit)
- Breadcrumb links idu na `/{slug}` što je ZASTARELO
- Sitemap ne uključuje `/kategorija/{slug}` (SEO landing)
- Canonical URLs nisu konzistentni

**Rešenje** (tri opcije):

**OPCIJA A** (PREPORUČENA): Migriraj na `/kategorije/{slug}`
- `/[categorySlug]/page.tsx` delete
- Kreiraj `/kategorije/[slug]/page.tsx` (novo, main category listing)
- Drži `/kategorija/[slug]/page.tsx` (SEO landing sa sadržajem)
- Dodaj 301 redirect: `/{slug}` → `/kategorije/{slug}`

**OPCIJA B**: Prosledi `/[categorySlug]` kroz breadcrumb
- Drži root route kao je
- Update seeder canonical URLs na `/{slug}`
- Ukloni `/kategorija/{slug]` ili ga udeli sa root route-om
- Problem: Root-level dynamic route može kolizovati sa drugim rutama

**OPCIJA C**: Čini `/kategorija` primarna, migracija sa time
- Dosta rada
- Dugoročno najčistije, ali zahteva sve da updatira

---

### Problem #3: Breadcrumb linkovi vode na ZASTARELE rute
**Lokacija**: Više fajlova

```typescript
// TrendplusProdavnica.Web/src/app/proizvod/[slug]/page.tsx
{ label: product.categoryName, url: `/${product.categorySlug}` },  // ❌ legacy root dynamic

// TrendplusProdavnica.Web/src/app/akcija/[categorySlug]/page.tsx  
{ label: listing.title, url: `/akcija/${categorySlug}` },           // ✅ OK
```

**Problem**: Product PDP breadcrumb ide na `/{categorySlug}` umjesto `/kategorije/{categorySlug}`

---

### Problem #4: DevelopmentDataSeeder canonical URLs su NEUSKLAĐENI
**Lokacija**: `TrendplusProdavnica.Infrastructure\Persistence\Seeding\DevelopmentDataSeeder.cs`

**Trenutno** (iz seed-a):
```csharp
// Brand canonical
CanonicalUrl = $"/brend/{seed.Slug}"              // /brend/...
// Collection canonical
CanonicalUrl = $"/kolekcija/{seed.Slug}"           // /kolekcija/...
// Product canonical
CanonicalUrl = $"/proizvod/{seed.Slug}"            // /proizvod/...
// Store canonical
CanonicalUrl = $"/prodavnica/{seed.Slug}"          // /prodavnica/...
// Category - NIGDE NIJE EXPLICIT!
```

**Problem**: 
- Seed ne postavlja canonical za kategorije
- Različiti prefiks za svaki tip (/brend, /kolekcija, /proizvod, /prodavnica)
- Nema konzistencije sa front-end rutama

**Rešenje**:
- Dodaj category canonical URL-ove
- Potvrdi da se poklapa sa Front-end rutama

---

### Problem #5: Filter/Sort/Page query parametri nisu NOINDEX
**Lokacija**: Sve listing strane

**Trenutno** (sve su INDEXABLE):
```typescript
// kategorija/[slug]/page.tsx, akcija/[categorySlug]/page.tsx, itd.
path: page > 1 ? `/${categorySlug}?page=${page}` : `/${categorySlug}`,
```

**Problem**:
- Page parametri (`?page=2`) se indeksiraju
- Filter/sort parametri se mogu indeksirati
- Duplicate content između `/cipele`, `/cipele?page=2`, `/cipele?page=3`...
- SEO problem: Indexing svih paginated vari

**Rešenje**:
- Page 1 = indexable (bez ?page=1)
- Page 2+ = noindex ili rel="next"/"prev" (Google preferie next/prev za pagination)
- Search/filter params = noindex

---

### Problem #6: Seed URL struktura zavisi od Admin slugova
**Lokacija**: Fajlovi seeder-a

**Primer**:
```csharp
var slugs = productSeeds.Select(seed => seed.Slug).ToArray();
// Zatim koristi slugove iz seed definicije
// Ali ako admin promeni slug - seed je neusklađen!
```

**Problem**: Seed URL-ovi nisu "golden source" - mogu biti inconsistent sa stvarnim zapisima

---

## ✅ FINALNI TARGET MODEL

### 1. Public SEO Endpoint-i (novi)
```
GET /api/seo/categories - Za sitemap (samo slug + updateTime)
GET /api/seo/brands - Za sitemap (samo slug + updateTime)
GET /api/seo/collections - Za sitemap (samo slug + updateTime)
GET /api/seo/products - Za sitemap (samo slug + updateTime)
GET /api/seo/stores - Za sitemap (samo slug + updateTime)
GET /api/seo/editorial - Za sitemap (samo slug + updateTime)
```

**Format**:
```json
{
  "slug": "cipele",
  "isActive": true,
  "isPurchasable": true,
  "isVisible": true,
  "isIndexable": true,
  "updatedAtUtc": "2026-04-11T10:00:00Z"
}
```

**Zaštita**: Public, cached sa 1h TTL

---

### 2. Public Route struktura
```
LANDING PAGES (Indexable):
/                                - Home
/kategorije/{slug}              - Category listing (JA PRIMARNI)
/brendovi/{slug}                - Brand page
/kolekcije/{slug}               - Collection page
/akcija                         - Sale listing
/akcija/{categorySlug}          - Sale by category
/proizvod/{slug}                - Product detail
/prodavnice                     - Store listing
/prodavnice/{slug}              - Store detail
/editorial                       - Blog list
/editorial/{slug}               - Blog detail

LEGACY/COMPATIBILITY (REDIRECT):
/{categorySlug}                 → /kategorije/{categorySlug} (301)
/kategorija/{slug}              → /kategorije/{slug} (301)

NON-INDEXABLE:
/?page=2                        - noindex (or no pagination)
/kategorije/{slug}?page=2       - noindex (or rel="next"/rel="prev")
/kategorije/{slug}?sort=...     - noindex
/kategorije/{slug}?filter=...   - noindex
```

---

### 3. Canonical URL Specification
```
INDEXABLE PAGES:
  Brand:       GET https://example.com/brendovi/{slug}
  Collection:  GET https://example.com/kolekcije/{slug}
  Product:     GET https://example.com/proizvod/{slug}
  Store:       GET https://example.com/prodavnice/{slug}
  Editorial:   GET https://example.com/editorial/{slug}
  Category:    GET https://example.com/kategorije/{slug}   ← CHANGE from /brend→/brendovi etc
  Sale:        GET https://example.com/akcija/{slug}

CANONICAL RULES:
  - Page 1:   <link rel="canonical" href="/kategorije/{slug}" />
  - Page 2+:  <link rel="canonical" href="/kategorije/{slug}" /> (OR no pagination)
  - Filters:  NO canonical (noindex instead)
  - Sorts:    NO canonical (noindex instead)
```

---

### 4. Breadcrumb Consistency
```
Home > Kategorija > Proizvod:
  [
    { label: "Home", url: "/" },
    { label: "Kategorije", url: "/kategorije" },
    { label: "Cipele", url: "/kategorije/cipele" },     ← Changed from /cipele
    { label: "Pumpe kl", url: "/proizvod/pumpe-kl" }
  ]

Akcija > Kategorija > Proizvod:
  [
    { label: "Home", url: "/" },
    { label: "Akcije", url: "/akcija" },
    { label: "Cipele - akcija", url: "/akcija/cipele" },
    { label: "Pumpe 30%", url: "/proizvod/pumpe-30" }
  ]
```

---

### 5. Sitemap Structure
```xml
<?xml version="1.0" encoding="UTF-8"?>
<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
  <!-- Static pages -->
  <url>
    <loc>https://example.com/</loc>
    <lastmod>2026-04-11</lastmod>
    <changefreq>daily</changefreq>
    <priority>1.0</priority>
  </url>
  
  <!-- Category listing pages -->
  <url>
    <loc>https://example.com/kategorije/cipele</loc>
    <lastmod>2026-04-10</lastmod>
    <changefreq>daily</changefreq>
    <priority>0.8</priority>
  </url>
  
  <!-- Products -->
  <url>
    <loc>https://example.com/proizvod/lara-pump</loc>
    <lastmod>2026-04-09</lastmod>
    <changefreq>weekly</changefreq>
    <priority>0.9</priority>
  </url>
  
  <!-- NO pagination URLs -->
  <!-- NO filter URLs -->
  <!-- NO sort URLs -->
</urlset>
```

---

### 6. Seed URL Master Reference
```csharp
// For Categories
CanonicalUrl = $"/kategorije/{seed.Slug}"           // /kategorije/cipele

// For Brands
CanonicalUrl = $"/brendovi/{seed.Slug}"             // /brendovi/nike (changed from /brend)

// For Collections
CanonicalUrl = $"/kolekcije/{seed.Slug}"            // /kolekcije/prolece (changed from /kolekcija)

// For Products
CanonicalUrl = $"/proizvod/{seed.Slug}"             // /proizvod/pump-lara (OK)

// For Stores
CanonicalUrl = $"/prodavnica/{seed.Slug}"           // /prodavnica/bg-centar (OK)

// For Editorial
CanonicalUrl = $"/editorial/{seed.Slug}"            // /editorial/trend-report (OK)
```

---

## 🔧 TAČNE IZMENE PO FAJLOVIMA

### Fajl 1: sitemap.ts - Koristi SEO endpoint-e umesto admin

**Lokacija**: `TrendplusProdavnica.Web/src/lib/seo/sitemap.ts`

**Izmena**: Zameni admin endpoint-e sa `/api/seo/*`

---

### Fajl 2: Program.cs - Dodaj SEO endpoint-e

**Lokacija**: `TrendplusProdavnica.Api\Program.cs`

**Izmena**: Dodaj 6 novih GET endpoint-a za SEO sitemap

---

### Fajl 3: [categorySlug]/page.tsx - Obriši (migrate na /kategorije)

**Lokacija**: `TrendplusProdavnica.Web/src/app/[categorySlug]/page.tsx`

**Akcija**: Delete - zameni sa /kategorije-listing/[slug] ili rename folder

---

### Fajl 4: Dodaj /kategorije/[slug]/page.tsx - Nova category listing ruta

**Lokacija**: `TrendplusProdavnica.Web/src/app/kategorije/[slug]/page.tsx` (novo)

**Akcija**: Kreiraj (kopija iz [categorySlug] sa updateima)

---

### Fajl 5: DevelopmentDataSeeder.cs - Uskladi canonical URLs

**Lokacija**: `TrendplusProdavnica.Infrastructure\Persistence\Seeding\DevelopmentDataSeeder.cs`

**Izmene**:
1. Brand: `/brend/{slug}` → `/brendovi/{slug}`
2. Collection: `/kolekcija/{slug}` → `/kolekcije/{slug}`
3. Category: Dodaj `/kategorije/{slug}`
4. Sve ostale ostaju isto

---

### Fajl 6: product/[slug]/page.tsx - Uskladi breadcrumb

**Lokacija**: `TrendplusProdavnica.Web/src/app/proizvod/[slug]/page.tsx`

**Izmena**: Breadcrumb URL sa `/{categorySlug}` na `/kategorije/{categorySlug}`

---

### Fajl 7: metadata builder - Handle pagination/filters

**Lokacija**: `TrendplusProdavnica.Web/src/lib/seo/metadata.ts` (verovatno)

**Izmena**: Dodaj noindex logic za ?page=2+, ?filter=*, ?sort=*

---

### Fajl 8: kategorija/[slug]/page.tsx - OPT-OUT category SEO landing

**Lokacija**: `TrendplusProdavnica.Web/src/app/kategorija/[slug]/page.tsx`

**Opcija A**: Delete + redirect
**Opcija B**: Keep ali prosledi kroz `/kategorije/` ili gde je SEO landing

---

## 📋 VERIFIKACIJSKA CHECKLIST

### Sitemap korektnost
```
[ ] Sitemap koristi /api/seo/* endpoints (ne /admin/*)
[ ] Sve aktivne kategorije su u sitemap-u
[ ] Sve aktivne brendove su u sitemap-u
[ ] Sve aktivne kolekcije su u sitemap-u
[ ] Sve vidljive i kupljive proizvode su u sitemap-u
[ ] Sve Store-ove su u sitemap-u
[ ] Svi published editorial su u sitemap-u
[ ] Nema ?page=N URL-ova u sitemap-u
[ ] Nema filter/sort URL-ova u sitemap-u
```

### Canonical URL-ovi
```
[ ] GET /kategorije/cipele → <link rel="canonical" href="/kategorije/cipele" />
[ ] GET /brendovi/nike → <link rel="canonical" href="/brendovi/nike" />
[ ] GET /kolekcije/prolece → <link rel="canonical" href="/kolekcije/prolece" />
[ ] GET /proizvod/pump-lara → <link rel="canonical" href="/proizvod/pump-lara" />
[ ] GET /prodavnice/bg-centar → <link rel="canonical" href="/prodavnice/bg-centar" />
[ ] GET /editorial/trend-report → <link rel="canonical" href="/editorial/trend-report" />
[ ] Paginated: /kategorije/cipele?page=2 → canonical NEMA ?page (ili noindex)
```

### Breadcrumb linkovi
```
[ ] Product PDP ide na /kategorije/{slug} (ne /{slug})
[ ] Sale PDP ide na /akcija/{slug} (OK)
[ ] Sve breadcrumb linkove idu na ispravan endpoint
[ ] Nema dead linkova
```

### Redirect-i (ako je potrebno)
```
[ ] /{categorySlug} → /kategorije/{categorySlug} (301)
[ ] /kategorija/{slug} → /kategorije/{slug} (301) [ako migrirate]
[ ] Legacy URL-ovi se korektno redirectuju
```

### Legacy routes
```
[ ] /[categorySlug] rutira do /kategorije/[slug]
[ ] /kategorija/[slug] opcionalno: redirect ili delete
[ ] Nema 404-a na starim linkovima
```

### Noindex i pagination
```
[ ] ?page=2+ strane imaju rel="next"/"prev" linkove (ili noindex)
[ ] ?filter=* strane imaju noindex
[ ] ?sort=* strane imaju noindex
[ ] Meta robots="noindex" na parametriranom URL-ovima ako jeste potrebno
```

### Seed konsistencija
```
[ ] DevelopmentDataSeeder koristi `/kategorije/{slug}` canonical za kategorije
[ ] DevelopmentDataSeeder koristi `/brendovi/{slug}` canonical za brendove
[ ] DevelopmentDataSeeder koristi `/kolekcije/{slug}` canonical za kolekcije
[ ] Nema hardkodirane razlike između seeder canoncial-a i frontend ruta-a
```

---

## 🎯 SLEDEĆI KORACI

1. **Kreiraj /api/seo/* endpoint-e** (Program.cs)
   - Kopiraj logiku iz /admin/* endpoint-a
   - Vrati samo `{ slug, isActive/isVisible/isPurchasable, updatedAtUtc, isIndexable }`
   - Cache-uj sa 1h TTL
   - Public pristup (no auth)

2. **Prebaci sitemap na /api/seo/**  (sitemap.ts)
   - Zameni sve `/admin/*` sa `/api/seo/*`

3. **Migruj category routing**
   - Opcija A (preporučena): Prosledi `/[categorySlug]` → `/kategorije/[slug]/page.tsx`
   - Update breadcrumb links

4. **Uskladi seed canonical URL-ove** (DevelopmentDataSeeder.cs)
   - `/brend` → `/brendovi`
   - `/kolekcija` → `/kolekcije`
   - Dodaj `/kategorije` za kategorije

5. **Implementiraj noindex logic** (metadata.ts)
   - Page 2+: noindex OR rel="next"/"prev"
   - Filters: noindex
   - Sorts: noindex

6. **Testiraj i verifikuj** (videti checklist gore)
   - `robots.txt` dozvoljava indeksiranje samo datih putanja
   - `sitemap.xml` ne sadrži paginated/filter URL-ove
   - Canonical tagovi su korektni
   - Redirecti rade

---

## 📚 REFERENCIJE IZ KORISNIKA

- sitemap.ts (line 76) - Koristi /admin/brands, /admin/collections, /admin/products, /admin/stores ❌
- kategorija/[slug]/page.tsx (line 26) - Path je /kategorija/{slug} 
- kategorija/[slug]/page.tsx (line 43) - Koristi getCategoryListing
- kategorija/[slug]/page.tsx (line 156) - Pagination bez noindex
- kategorija/[slug]/page.tsx (line 175) - Breadcrumb nema
- ProductListingReadService.cs (line 420) - Get products logic
- ProductListingQueryService.cs (line 746, 801, 882) - Query logic
- DevelopmentDataSeeder.cs (line 78, 122, 257) - Seed canonical URLs

---

**Status**: 🔴 KRITIČNA - SEO struktura je fragmentirana i zavisi od admin layera
**Prioritet**: 🔴 VISOK - Utiče na organic search visibility
**Radi time**: 4-6 sati (plus QA/testing)
