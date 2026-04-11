# SEO Public Contract Implementation - Tačne Izmene Po Fajlovima

## ✅ ZAVRŠENO

### 1. ✅ Program.cs - SEO endpoint-i
**Lokacija**: `TrendplusProdavnica.Api/Program.cs`
**Status**: ✅ DONE
- Dodao `/api/seo/categories` - vrataj sve kategorije
- Dodao `/api/seo/brands` - vrataj sve aktivne brendove
- Dodao `/api/seo/collections` - vrataj sve aktivne kolekcije
- Dodao `/api/seo/products` - vrataj sve vidljive/kupljive proizvode
- Dodao `/api/seo/stores` - vrataj sve aktivne prodavnice
- Dodao `/api/seo/editorial` - vrataj sav objavljeni editorial
- Svi endpoint-i su public, cached, vraćaju samo SEO-relevantne podatke

### 2. ✅ ProductListingQueryService - GetAllCategoriesForSeoAsync()
**Lokacija**: `TrendplusProdavnica.Infrastructure\Persistence\Queries\Catalog\ProductListingQueryService.cs`
**Status**: ✅ DONE
- Dodana metoda `GetAllCategoriesForSeoAsync()` koja vraća sve kategorije sa slug-om i updateTime

### 3. ✅ IProductListingQueryService - Interface update
**Lokacija**: `TrendplusProdavnica.Application\Catalog\Services\IProductListingQueryService.cs`
**Status**: ✅ DONE
- Dodat `CategorySeoDto` record
- Dodata metoda `GetAllCategoriesForSeoAsync()`

### 4. ✅ sitemap.ts - Koristi /api/seo/* umesto /admin/*
**Lokacija**: `TrendplusProdavnica.Web/src/lib/seo/sitemap.ts`
**Status**: ✅ DONE
- Zamenjeni svi `/admin/*` endpoint-i sa `/api/seo/*`
- Dodata `/api/seo/categories` u sitemap
- Prebačena ruta sa `/[categorySlug]` na `/kategorije/{slug}` u sitemap
- Dodat `/akcija` kao static entry
- Svi URL-ovi su sada konsistentni

---

## ⏳ OSTAJE DA SE URADI

### 5. Uskladi breadcrumb linkove u Product PDP
**Lokacija**: `TrendplusProdavnica.Web/src/app/proizvod/[slug]/page.tsx`

**Trenutno** (KRIVO):
```typescript
{ label: product.categoryName, url: `/${product.categorySlug}` },  // ❌ /cipele
```

**Trebalo biti**:
```typescript
{ label: product.categoryName, url: `/kategorije/${product.categorySlug}` },  // ✅ /kategorije/cipele
```

**Kako to izvršiti**: 
```bash
# Pronađi liniju sa breadcrumb-om
grep -n "categoryName.*url.*categorySlug" TrendplusProdavnica.Web/src/app/proizvod/\\[slug\\]/page.tsx

# Zameni sa novim URL-om
```

---

### 6. Kreiraj `/kategorije/` rute - OPCIONO (da li migrirati?)

**OPCIJA A - Preporučena**: Migracija na novi `/kategorije/{slug}` pattern
1. Kreiraj novi folder: `TrendplusProdavnica.Web/src/app/kategorije/`
2. Kreiraj `[slug]/page.tsx` sa sadržajem iz `[categorySlug]/page.tsx`
3. Update-uj API call sa `/api/listings/category/` (već postoji, OK je)
4. Obriši (ili archove) `[categorySlug]/page.tsx`
5. Dodaj 301 redirect u `next.config.js`:
   ```javascript
   async redirects() {
     return [
       {
         source: '/:categorySlug',
         destination: '/kategorije/:categorySlug',
         permanent: true, // 301 redirect
       }
     ]
   }
   ```

**OPCIJA B - Kompromis**: Drži oba absolutno identična
1. Kreiraj `kategorije/[slug]/page.tsx` kao kopiju `[categorySlug]/page.tsx`
2. Obriši `/[categorySlug]/page.tsx`
3. Update breadcrumb-e na novi URL

---

### 7. Dodaj canonical tagove za paginated URL-ove
**Lokacija**: `TrendplusProdavnica.Web/src/lib/seo/metadata.ts`

**Problem**: `/kategorije/cipele?page=2` je indexable - trebalo ne biti

**Rešenje**: U `buildMetadata()` funkciji:
```typescript
export function buildMetadata(options: BuildMetadataOptions): Metadata {
  const { path, ...rest } = options;
  
  // Ako URL ima ?page=, ?filter=, ?sort= - dodaj noindex
  const isParametrized = path.includes('?');
  const robots = isParametrized ? 'noindex' : undefined;
  
  return {
    ...rest,
    robots: robots ? { index: false, follow: true } : undefined,
    // ... ostatak
  };
}
```

---

### 8. Verifikuj default breadcrumb za root category route
**Lokacija**: `TrendplusProdavnica.Web/src/app/[categorySlug]/page.tsx` (ili `/kategorije/[slug]/page.tsx`)

**Trebalo biti**:
```typescript
// Breadcrumb za /kategorije/cipele:
const breadcrumbs = [
  { label: "Početna", url: "/" },
  { label: "Kategorije", url: "/kategorije" },  // ← Link do kategorija landing
  { label: "Cipele", url: "/kategorije/cipele" }
];
```

**Napomena**: Trebam da pronađem gde `getCategoryListing()` vraća breadcrumb-e i da se umjesto `/{slug}` koristi `/kategorije/{slug}`.

---

### 9. Seeduj kategorije sa canonical URL-ima (OPCIONO)
**Lokacija**: `TrendplusProdavnica.Infrastructure\Persistence\Seeding\DevelopmentDataSeeder*.cs`

**Trenutno**: Kategorije nisu seed-ovane sa SeoMetadata

**Trebalo**: Dodaj seed kategorija sa:
```csharp
category.Seo = new SeoMetadata
{
    SeoTitle = $"{seed.Name} zenska obuca - Trendplus",
    SeoDescription = seed.ShortDescription,
    CanonicalUrl = $"/kategorije/{seed.Slug}"
};
```

**Gde se kategorije kreiraju?** Trebam da pronađem inicijalizacijski seeder ili ručnu aplikaciju koja kreira kategorije.

---

## 🔍 Verifikacijski Checklist

### SEO Struktura
- [ ] `GET /api/seo/categories` vraća sve kategorije sa slug-om
- [ ] `GET /api/seo/brands` vraća sve brendove sa slug-om
- [ ] `GET /api/seo/collections` vraća sve kolekcije sa slug-om
- [ ] `GET /api/seo/products` vraća sve proizvode sa slug-om
- [ ] `GET /api/seo/stores` vraća sve prodavnice sa slug-om
- [ ] `GET /api/seo/editorial` vraća sav editorial sa slug-om
- [ ] Svi endpoint-i su cachkovani sa `seo-cache` TTL
- [ ] Nema `/admin/*` zavisnosti u javnom SEO sloju

### Sitemap
- [ ] Sitemap koristi `/api/seo/*` umesto `/admin/*`
- [ ] Sitemap uključuje `/kategorije/{slug}` URL-ove
- [ ] Sitemap NE uključuje `/[categorySlug]` (root dynamic) URL-ove
- [ ] Sitemap NE uključuje `?page=N` URL-ove
- [ ] Sitemap NE uključuje `?filter=*` URL-ove
- [ ] Sitemap NE uključuje `?sort=*` URL-ove
- [ ] Sve URL-ove su sa `https://` scheme absolućni

### Routing
- [ ] `/kategorije/{slug}` je primarna category landing
- [ ] `/{categorySlug}` je ili obrisana ili redirectuje na `/kategorije/`
- [ ] `/akcija/{categorySlug}` je za sale kategorije (OK je)
- [ ] `/brendovi/{slug}` je za brand stranice
- [ ] `/proizvod/{slug}` je za product detail
- [ ] `/prodavnice/{slug}` je za store detail

### Breadcrumb-i
- [ ] Product PDP breadcrumb ide na `/kategorije/{slug}` umesto `/{slug}`
- [ ] Category listing breadcrumb ide na `/kategorije` umesto drugog
- [ ] Sale breadcrumb ide na `/akcija` (OK je)
- [ ] Nema dead linkova u breadcrumb-ima

### Canonical URL-ovi
- [ ] Product canonical je `/proizvod/{slug}`
- [ ] Brand canonical je `/brendovi/{slug}`
- [ ] Collection canonical je `/kolekcije/{slug}`
- [ ] Store canonical je `/prodavnica/{slug}` ili `/prodavnice/{slug}`
- [ ] Category canonical je `/kategorije/{slug}`
- [ ] Paginated: `/kategorije/{slug}?page=2` ima canonical `/kategorije/{slug}` (bez ?page)

### Robot-i i Meta
- [ ] `robots.txt` dozvoljava indexiranje samo dozvoljenih putanja
- [ ] Paginated stranice imaju `rel="next"` i `rel="prev"` linkove (OR noindex)
- [ ] Filter/sort stranice imaju `noindex` meta tag
- [ ] Search strane imaju `noindex`
- [ ] Canonical tagovi su na svim stranicama

---

## 📋 TO-DO za Finalnu Implementaciju

1. **Odmah** (5-10 min)
   - [ ] Zameni breadcrumb URL u product/[slug]/page.tsx sa `/kategorije/`
   
2. **Uskoro** (20-30 min)
   - [ ] Kreiraj `/kategorije/[slug]/page.tsx` kao kopiju `[categorySlug]/page.tsx`
   - [ ] Obriši `[categorySlug]/page.tsx`
   - [ ] Dodaj 301 redirect u `next.config.js`

3. **Kasnije** (ako jeste potrebno)
   - [ ] Dodaj category seeding sa SeoMetadata
   - [ ] Dodaj noindex logiku za paginated/filter URL-ove
   - [ ] Dodaj rel="next"/"prev" linkove za paginated URL-ove
   - [ ] Testiraj sve rute i sitemap

---

## 🚀 Kako Testirati

### 1. Testiraj SEO endpoint-e
```bash
curl http://localhost:5000/api/seo/categories | jq '.'
curl http://localhost:5000/api/seo/brands | jq '.'
curl http://localhost:5000/api/seo/products | jq '.'
```

### 2. Testiraj sitemap
```bash
curl http://localhost:3000/sitemap.xml | head -50
# Trebalo da vidiš:
# - /kategorije/cipele
# - /brendovi/nike
# - /kolekcije/novo
# - /proizvod/pump-lara
# - NE /cipele (root dynamic)
# - NE /cipele?page=2
```

### 3. Testiraj breadcrumb-e
- Idi na `/proizvod/pump-lara`
- Klikni na breadcrumb za kategoriju
- Trebalo da ide na `/kategorije/cipele` (ne `/cipele`)

### 4. Testiraj redirect (ako je kreirate)
```bash
curl -I http://localhost:3000/cipele
# Trebalo: 301 → /kategorije/cipele
```

### 5. Testiraj canonical tagove
```bash
curl http://localhost:3000/kategorije/cipele | grep canonical
# Trebalo: <link rel="canonical" href="https://example.com/kategorije/cipele" />

curl "http://localhost:3000/kategorije/cipele?page=2" | grep canonical
# Trebalo: <link rel="canonical" href="https://example.com/kategorije/cipele" /> (bez ?page=2)
```

---

## 📚 Status Po Komponentama

| Komponenta | Status | Fajl |
|-----------|--------|------|
| **API SEO endpoint-i** | ✅ DONE | Program.cs |
| **Database query** | ✅ DONE | ProductListingQueryService.cs |
| **Interface** | ✅ DONE | IProductListingQueryService.cs |
| **Sitemap** | ✅ DONE | sitemap.ts |
| **Breadcrumb linkovi** | ⏳ TODO | proizvod/[slug]/page.tsx |
| **Routing** | ⏳ TODO | kategorije/[slug]/page.tsx + next.config.js |
| **Canonical tagovi** | ✅ READY | metadata.ts (already exists) |
| **Category seeding** | ❓ OPTIONAL | DevelopmentDataSeeder.cs |
| **Noindex logika** | ❓ OPTIONAL | metadata.ts |

---

## 🎯 Procena vremena

- **SEO endpoint-i**: ✅ 20 min (DONE)
- **Sitemap prebacivanje**: ✅ 15 min (DONE)
- **Breadcrumb fix**: ⏳ 5 min
- **Routing migracija**: ⏳ 20 min
- **Testing**: ⏳ 15 min
- **Optional (seeding, noindex)**: ❓ 30 min

**Ukupno**: ~75 min od početka do kraja

---

**Sledeća akcija**: Zameni breadcrumb URL u product/[slug]/page.tsx i kreiraj /kategorije/ rutu.
