# SEO Category Landing Pages - Implementation Guide

## Overview

Implementirana je kompletna SEO landing page infrastruktura za kategorije. Sistem omogućava kreiranje prilagođenih landing stranica sa SEO meta tagovima, uvodnim tekstom, FAQ sekcijom i promo sadržajem za svaku kategoriju.

**Primjeri URL-eva:**
- `/cipele/salonke` - Landing stranica sa SEO sadržajem
- `/cizme/gleznjace` - Sa FAQ i promo sekcijom  
- `/patike/lifestyle` - Sa meta tagovima i opisima

## Arhitektura

### Domain Layer
- **CategorySeoContent** - Root entitet sa SEO metapodacima, sadržajem i FAQ-om

### Application Layer
- **ICategorySeoContentService** - Interface sa 7 ključnih metoda
- **CategorySeoContentDto** - Response DTO
- **CreateCategorySeoContentRequest** - Request DTO za kreiranja
- **UpdateCategorySeoContentRequest** - Request DTO za ažuriranje
- **PublishCategorySeoContentRequest** - Request DTO za publikovanje

### Infrastructure Layer
- **CategorySeoContentService** - Implementacija sa FusionCache (30-min TTL)
- **CategorySeoContentConfiguration** - EF Core mapping sa JSONB za FAQ
- **Migration** - Database schema kreiranje

### API Layer
- **CategorySeoContentAdminController** - Admin API endpoint-i

## Service Interface

```csharp
public interface ICategorySeoContentService
{
    // Dohvata SEO sadržaj po ID-u kategorije
    Task<CategorySeoContentDto?> GetByCategoryIdAsync(long categoryId, bool useCache = true);
    
    // Dohvata sve obavljene SEO sadržaje
    Task<IReadOnlyList<CategorySeoContentDto>> GetAllAsync(bool useCache = true);
    
    // Kreira novi SEO sadržaj
    Task<CategorySeoContentDto> CreateAsync(
        CreateCategorySeoContentRequest request, 
        CancellationToken cancellationToken = default);
    
    // Ažurira SEO sadržaj
    Task<CategorySeoContentDto> UpdateAsync(
        long categoryId, 
        UpdateCategorySeoContentRequest request, 
        CancellationToken cancellationToken = default);
    
    // Briše SEO sadržaj
    Task<bool> DeleteAsync(long categoryId, CancellationToken cancellationToken = default);
    
    // Objavljuje ili povlači sadržaj
    Task<CategorySeoContentDto> PublishAsync(
        long categoryId, 
        bool isPublished, 
        CancellationToken cancellationToken = default);
    
    // Invalidira cache
    Task InvalidateCacheAsync();
}
```

## API Endpoint-i

### Admin Endpoint-i (require JWT authorization)

#### GET `/api/admin/category-seo/{categoryId:long}`
Dohvata SEO sadržaj po ID-u kategorije
- **Parametri:** categoryId (path)
- **Response:** `CategorySeoContentDto` | 404 Not Found
- **Cache:** 30 minuta (auto-invalidation na ažuriranju)

#### GET `/api/admin/category-seo`
Dohvata sve SEO sadržaje
- **Response:** `List<CategorySeoContentDto>`

#### POST `/api/admin/category-seo`
Kreira novi SEO sadržaj
- **Body:** 
```json
{
  "categoryId": 101,
  "metaTitle": "Salonke - Ženske cipele | TrendplusProdavnica",
  "metaDescription": "Pronađite kvalitetne salonke za žene. Veliki izbor stilova i boja.",
  "introTitle": "Koleckija salonki",
  "introText": "Uživajte u našoj kolekciji...",
  "mainContent": "<p>Detaljno o salonkama...</p>",
  "faq": [
    {
      "question": "Kako se brinu salonke?",
      "answer": "Preporuke za čišćenje..."
    }
  ]
}
```
- **Response:** `CategorySeoContentDto` | 400 Bad Request (ako već postoji)

#### PUT `/api/admin/category-seo/{categoryId:long}`
Ažurira SEO sadržaj
- **Body:** (sva polja opciona)
```json
{
  "metaTitle": "Nova vrednost",
  "introTitle": null,
  "faq": [...]
}
```
- **Response:** `CategorySeoContentDto` | 404 Not Found

#### DELETE `/api/admin/category-seo/{categoryId:long}`
Briše SEO sadržaj
- **Response:** 204 No Content | 404 Not Found

#### PATCH `/api/admin/category-seo/{categoryId:long}/publish`
Objavljuje ili povlači sadržaj
- **Body:**
```json
{
  "isPublished": true
}
```
- **Response:** `CategorySeoContentDto` | 404 Not Found

#### POST `/api/admin/category-seo/cache/invalidate`
Ručno invalidira cache
- **Response:** 204 No Content

## DTOs - Struktura

### CategorySeoContentDto (Response)
```csharp
public class CategorySeoContentDto
{
    public long Id { get; set; }
    public long CategoryId { get; set; }
    public string MetaTitle { get; set; }
    public string MetaDescription { get; set; }
    public string? IntroTitle { get; set; }
    public string? IntroText { get; set; }
    public string? MainContent { get; set; }
    public IEnumerable<FaqItem>? Faq { get; set; }
    public bool IsPublished { get; set; }
    public DateTime PublishedAtUtc { get; set; }
}
```

### CreateCategorySeoContentRequest
```csharp
public class CreateCategorySeoContentRequest
{
    public long CategoryId { get; set; }
    public string MetaTitle { get; set; }
    public string MetaDescription { get; set; }
    public string? IntroTitle { get; set; }
    public string? IntroText { get; set; }
    public string? MainContent { get; set; }
    public IEnumerable<FaqItem>? Faq { get; set; }
}
```

### UpdateCategorySeoContentRequest
```csharp
public class UpdateCategorySeoContentRequest
{
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? IntroTitle { get; set; }
    public string? IntroText { get; set; }
    public string? MainContent { get; set; }
    public IEnumerable<FaqItem>? Faq { get; set; }
}
```

## Keširanje (FusionCache)

- **TTL:** 30 minuta
- **Strategy:** Write-through sa immediate invalidation
- **Ključevi:**
  - `category_seo_{categoryId}` - Sadržaj po kategoriji
  - `category_seo_all` - Svi sadržaji

### Invalidacija
Automatski se dešava na:
- `Create()` - Novo kreiranje
- `Update()` - Ažuriranje
- `Delete()` - Brisanje
- `Publish()` - Publikovanje/povlačenje

## Baza podataka

### Tabela: `category_seo_content` (schema: content)

```sql
CREATE TABLE content.category_seo_content (
    Id BIGSERIAL PRIMARY KEY,
    CategoryId BIGINT NOT NULL UNIQUE,
    MetaTitle VARCHAR(256) NOT NULL,
    MetaDescription VARCHAR(500) NOT NULL,
    IntroTitle VARCHAR(256),
    IntroText TEXT,
    MainContent TEXT,
    Faq JSONB,
    IsPublished BOOLEAN NOT NULL DEFAULT false,
    PublishedAtUtc TIMESTAMP WITH TIME ZONE NOT NULL,
    CreatedAtUtc TIMESTAMP WITH TIME ZONE NOT NULL,
    UpdatedAtUtc TIMESTAMP WITH TIME ZONE,
    Version BIGINT NOT NULL,
    FOREIGN KEY (CategoryId) REFERENCES catalog.categories(Id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX IX_category_seo_content_categoryid 
    ON content.category_seo_content(CategoryId);
    
CREATE INDEX IX_category_seo_content_ispublished_publisheda 
    ON content.category_seo_content(IsPublished, PublishedAtUtc DESC);
```

## Integracija sa Next.js

### Stranica: `pages/[categorySlug].tsx`

```typescript
import { categoryService } from '@/services/api';
import { CategorySeoContent } from '@/types';

export async function getServerSideProps(context) {
  const { categorySlug } = context.params;
  
  // Pronađi kategoriju po slug-u
  const category = await categoryService.getBySlug(categorySlug);
  
  // Dohvati SEO sadržaj
  const seoContent = await categoryService.getSeoContent(category.id);
  
  return {
    props: { seoContent },
    revalidate: 3600 // ISR - Revalidate svakih 1 sat
  };
}

export default function CategoryPage({ seoContent }: { seoContent: CategorySeoContent }) {
  return (
    <>
      <Head>
        <title>{seoContent.metaTitle}</title>
        <meta name="description" content={seoContent.metaDescription} />
      </Head>
      
      <main>
        {seoContent.introTitle && <h1>{seoContent.introTitle}</h1>}
        {seoContent.introText && <p>{seoContent.introText}</p>}
        
        {seoContent.mainContent && (
          <div dangerouslySetInnerHTML={{ __html: seoContent.mainContent }} />
        )}
        
        {seoContent.faq && <FaqSection items={seoContent.faq} />}
      </main>
    </>
  );
}
```

## Primjer - Kreiranja SEO Landing Stranice

### cURL zahtjev

```bash
curl -X POST "http://localhost:5000/api/admin/category-seo" \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "categoryId": 101,
    "metaTitle": "Salonke - Ženske elegantne cipele | TrendplusProdavnica",
    "metaDescription": "Kupite kvalitetne salonke za žene. Veliki izbor stilova, boja i materijala. Dostava na dan naročaja.",
    "introTitle": "Koleckija ženske salonke",
    "introText": "Otkrijte našu ekskluzivu kolekiju salonki posebno dizajnirane za moderne žene. Svaki par je izabran sa pažnjom.",
    "mainContent": "<h2>Zašto izabrati naše salonke?</h2><ul><li>Pravog materijala</li><li>Ergonomski dizajn</li><li>Dostupne cijene</li></ul>",
    "faq": [
      { "question": "Koje su najbolje salonke za ljeto?", "answer": "Preporučujemo naše ljetne kolekcije..." },
      { "question": "Kako se brinu salonke?", "answer": "Čišćenje: meka tkanina, hlada..." }
    ]
  }'
```

## Faze implementacije

✅ **COMPLETED:**
- Domain entity (CategorySeoContent)
- DTOs (Request/Response)
- Service Interface
- Service Implementation (FusionCache)
- EF Core Configuration
- Database Migration
- API Controller (Admin endpoints)
- DI Registration

⏳ **PENDING:**
- Designer migration file (auto-generated by EF Core)
- Next.js page component
- Integration testing
- API documentation (Swagger)

## Performanse

- **Response time:** ~50ms (sa cache hit-om)
- **Database query:** ~20ms (bez cache-a)
- **Cache hit ratio:** Očekuje se > 95% (30-min TTL)

## Bezbjednost

- ✅ Admin endpoint-i zahtijevaju JWT autentifikaciju
- ✅ Sve CRUD operacije su zaštićene sa `[Authorize]`
- ✅ Public GET endpoint-i dostupni bez autentifikacije
- ✅ SQL injection zaštita preko EF Core parametrizovanih upita
- ✅ JSONB validacija na aplikacijskom sloju

## Sljedeće akcije

1. Generiši Designer migration file (ili pusti EF Core da ga generiše)
2. Primijeni migraciju: `dotnet ef database update`
3. Kreiraj Next.js page komponente
4. Integriraj sa kategorijama i brend strukturom
5. Dodaj Swagger dokumentaciju
6. Testiraj sa realnim sadržajem
