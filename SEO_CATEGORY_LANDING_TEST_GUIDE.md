# SEO Category Landing - Testing Guide

## Lokalni Development Setup

### 1. Database Migration

```bash
# Navigate to API project
cd TrendplusProdavnica.Api

# Apply migrations
dotnet ef database update
```

**Expected:** Migration `20260410000001_AddCategorySeoContent` se primjenjuje sa tabelom `category_seo_content` u `content` schemi.

---

## API Testing

### 1. Create SEO Content for Category (POST)

**Request:**
```bash
curl -X POST "http://localhost:5000/api/admin/category-seo" \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "categoryId": 101,
    "metaTitle": "Salonke - Ženske elegantne cipele | TrendplusProdavnica",
    "metaDescription": "Pronađite kvalitetne salonke za žene. Velik izbor stilova i boja. Besplatna dostava na narudžbe preko 50KM.",
    "introTitle": "Kolekciјa ženske salonke",
    "introText": "Otkrijte našu ekskluzivu kolekciju salonki posebno dizajnirane za moderne žene. Svaki par je izabran sa pažnjom kako bi zadovoljio vaše potrebe za udobnošću, stilom i kvalitetom.",
    "mainContent": "<h2>Zašto izabrati naše salonke?</h2><ul><li><strong>Pravi materijali:</strong> Koristimo samo prirodne materijale kao što su koža i pamuk</li><li><strong>Ergonomski dizajn:</strong> Svaki par je dizajniran sa fokusom na udobnost tokom celog dana</li><li><strong>Dostupne cijene:</strong> Kvalitet bez gaženja budžeta</li><li><strong>Brza dostava:</strong> Dostavite na vašu adresu u roku od 24-48 sati</li></ul><h2>Kako odabrati tačnu veličinu?</h2><p>Naše tablice veličina su dostupne na stranici svake cipele. Ako imate pitanja, slobodno nas kontaktirajte.</p>",
    "faq": [
      {
        "question": "Koje su najbolje salonke za ljeto?",
        "answer": "Preporučujemo naše ljetne kolekcije sa otvorenim vrhom i prozračnim materijalom. Pogledajte filtriranje po sezonama na stranici kategorije."
      },
      {
        "question": "Kako se brinu salonke?",
        "answer": "Čišćenje: koristite meku tkaninu i blagu sapunsku vodu. Čuva se na suhom mjestu izvan direktne sunčeve svjetlosti."
      },
      {
        "question": "Koji je rok za vraćanja?",
        "answer": "Imamo 30-dnevnu politiku vraćanja za sve neizdane predmete. Vidi naš Povratni poliću za više informacija."
      },
      {
        "question": "Trebaju li salonke razgrevanje?",
        "answer": "Naše salonke su od prirodnih materijala i mogu trebati samo malo razgrevanja na početku. Nakon koje nedjelje, trebale bi biti idealno prilagođene vašim nogama."
      }
    ]
  }'
```

**Expected Response (201 Created):**
```json
{
  "id": 1,
  "categoryId": 101,
  "metaTitle": "Salonke - Ženske elegantne cipele | TrendplusProdavnica",
  "metaDescription": "Pronađite kvalitetne salonke za žene...",
  "introTitle": "Kolekciјa ženske salonke",
  "introText": "Otkrijte našu ekskluzivu kolekciju...",
  "mainContent": "<h2>Zašto izabrati naše salonke?</h2>...",
  "faq": [...],
  "isPublished": false,
  "publishedAtUtc": "2026-04-10T00:00:00Z"
}
```

---

### 2. Get SEO Content by Category ID (GET)

**Request:**
```bash
curl "http://localhost:5000/api/admin/category-seo/101"
```

**Expected Response (200 OK):**
```json
{
  "id": 1,
  "categoryId": 101,
  "metaTitle": "Salonke - Ženske elegantne cipele | TrendplusProdavnica",
  ...
}
```

**Cache Note:** Prvi zahtjev učituje iz baze, sljedeći zahtjevi (30 min) dolaze iz FusionCache.

---

### 3. Get All SEO Content (GET)

**Request:**
```bash
curl "http://localhost:5000/api/admin/category-seo"
```

**Expected Response (200 OK):**
```json
[
  {
    "id": 1,
    "categoryId": 101,
    ...
  },
  {
    "id": 2,
    "categoryId": 104,
    ...
  }
]
```

---

### 4. Update SEO Content (PUT)

**Request:**
```bash
curl -X PUT "http://localhost:5000/api/admin/category-seo/101" \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "metaTitle": "Salonke - Novi naslov sa ažuriranom verzijom",
    "introTitle": "Ažurirana kolekција"
  }'
```

**Expected Response (200 OK):**
```json
{
  "id": 1,
  "categoryId": 101,
  "metaTitle": "Salonke - Novi naslov sa ažuriranom verzijom",
  "introTitle": "Ažurirana kolekција",
  ...
}
```

**Cache Impact:** Cache se invalidira nakon Update-a.

---

### 5. Publish/Unpublish (PATCH)

**Request - Publish:**
```bash
curl -X PATCH "http://localhost:5000/api/admin/category-seo/101/publish" \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{"isPublished": true}'
```

**Expected Response (200 OK):**
```json
{
  "id": 1,
  "categoryId": 101,
  "isPublished": true,
  "publishedAtUtc": "2026-04-10T10:30:45Z",
  ...
}
```

**Request - Unpublish:**
```bash
curl -X PATCH "http://localhost:5000/api/admin/category-seo/101/publish" \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{"isPublished": false}'
```

---

### 6. Delete SEO Content (DELETE)

**Request:**
```bash
curl -X DELETE "http://localhost:5000/api/admin/category-seo/101" \
  -H "Authorization: Bearer {JWT_TOKEN}"
```

**Expected Response (204 No Content)**

---

### 7. Manual Cache Invalidation (POST)

**Request:**
```bash
curl -X POST "http://localhost:5000/api/admin/category-seo/cache/invalidate" \
  -H "Authorization: Bearer {JWT_TOKEN}"
```

**Expected Response (204 No Content)**

---

## Frontend (Next.js) Testing

### Preduslov: Publish SEO Content

```bash
# Publish content primeiro
curl -X PATCH "http://localhost:5000/api/admin/category-seo/101/publish" \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{"isPublished": true}'
```

### Test Landing Page

1. **Start Next.js dev server:**
```bash
cd TrendplusProdavnica.Web
npm run dev
```

2. **Navigate to:**
```
http://localhost:3000/salonke
```

3. **Expected:**
- Page title: "Salonke - Ženske elegantne cipele | TrendplusProdavnica"
- Meta description visible in browser dev tools
- Hero section sa `introTitle` i `introText`
- Main content sa HTML-om
- FAQ sekcija sa pitanjima/odgovorima
- CTA dugme "Pregledaj proizvode"

---

## Error Scenarios

### 1. Duplicate Category ID

**Request:**
```bash
curl -X POST "http://localhost:5000/api/admin/category-seo" \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "categoryId": 101,
    "metaTitle": "Duplicate",
    "metaDescription": "Već postoji"
  }'
```

**Expected Response (400 Bad Request):**
```json
{
  "error": "SEO sadržaj za kategoriju 101 već postoji"
}
```

---

### 2. Non-existent Category

**Request:**
```bash
curl "http://localhost:5000/api/admin/category-seo/99999"
```

**Expected Response (404 Not Found):**
```json
{
  "error": "SEO sadržaj nije pronađen"
}
```

---

### 3. Unpublished Content Access

**Request (Frontend):**
```
http://localhost:3000/salonke
```

**Expected Response:** 404 Not Found (ili redirect na početnu)

---

## Performance Testing

### Cache Hit Test

1. **First request (cache miss):**
```bash
time curl "http://localhost:5000/api/admin/category-seo/101"
# Expected: ~50-100ms (database query)
```

2. **Second request (cache hit):**
```bash
time curl "http://localhost:5000/api/admin/category-seo/101"
# Expected: ~5-10ms (from FusionCache)
```

### Load Test (100 concurrent requests)

```bash
ab -n 100 -c 20 "http://localhost:5000/api/admin/category-seo"
```

---

## Database Verification

### Check Created Tables

```sql
-- Connect to database
psql -U postgres -d trendplusprodavnica

-- List content schema tables
\dt content.*

-- Check category_seo_content table
SELECT * FROM content.category_seo_content;

-- Check indexes
\di content.category_seo_content*

-- Check data with FAQ
SELECT id, category_id, meta_title, faq FROM content.category_seo_content WHERE id = 1;
```

---

## Integration Checklist

- [ ] Migration applied successfully
- [ ] Table created in `content` schema
- [ ] Indexes created
- [ ] Can create SEO content via API
- [ ] Can retrieve content with cache
- [ ] Cache invalidation works
- [ ] Publish/unpublish works
- [ ] Next.js page displays correctly
- [ ] Meta tags visible in HTML source
- [ ] FAQ renders correctly
- [ ] 404 for unpublished content
- [ ] Performance meets expectations (>95% cache hit ratio)

---

## Troubleshoot

### Migration Failed

```bash
# Rollback last migration
dotnet ef migrations remove

# Check migration history
dotnet ef migrations list

# Detailed error
dotnet ef database update --verbose
```

### Cache Not Working

Check:
1. Redis connection (if backplane enabled)
2. FusionCache configuration
3. Cache keys pattern

### API Returns 404

- Verify JWT token validity
- Check category exists in database
- Verify correct category ID

### Next.js Page Not Rendering

- Check API_URL environment variable
- Verify SEO content is published
- Check browser console for errors

---

## Next Steps

1. ✅ Create sample SEO content for categories 101, 104, 105
2. Integrate with category listing pages
3. Add admin UI for SEO content management
4. Create automated content generation from templates
5. Add multi-language support
6. Implement A/B testing for meta descriptions
