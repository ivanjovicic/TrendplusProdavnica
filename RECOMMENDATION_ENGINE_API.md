# Recommendation Engine API Dokumentacija

## Pregled

Recommendation engine je implementiran sa kombinacijom algoritama koji koriste:
- **Category similarity** (40% weight)
- **Brand similarity** (20% weight)  
- **Price proximity** (20% weight)
- **Popularity score** (20% weight)

Svi rezultati se keširaju u **FusionCache** sa TTL od **10 minuta**.

---

## API Endpoints

### 1. GET /api/recommendations/product/{productId}

Dohvata preporučene proizvode za specifičan proizvod (PDP - Product Detail Page).

**Parametri:**
```
productId (path) - ID proizvoda obavezno
limit (query) - broj preporuka (default: 8, max: 50)
type (query) - tip preporuke (default: RelatedProducts)
```

**Tipovi preporuka:**
- `RelatedProducts` = 1 - "Možda će vam se svideti"
- `CrossSell` = 2 - "Uz ovaj proizvod kupci često uzimaju"
- `Trending` = 3 - Trending proizvodi
- `NewArrivals` = 4 - Novi proizvodi

**Primjer zahtjeva:**
```
GET /api/recommendations/product/123?limit=8&type=1
```

**Odgovor (200):**
```json
{
  "sourceProductId": 123,
  "title": "Možda će vam se svideti",
  "items": [
    {
      "productId": 456,
      "slug": "proizvod-xyz",
      "name": "Proizvod XYZ",
      "brand": "Brand ABC",
      "price": 5999,
      "imageUrl": "https://cdn.example.com/image.webp",
      "mobileImageUrl": "https://cdn.example.com/image-mobile.webp",
      "averageRating": 4.5,
      "ratingCount": 128,
      "isBestseller": true,
      "isNew": false,
      "recommendationScore": 0.87
    }
    // ... do 8 proizvoda
  ]
}
```

**Kodovi greške:**
- `400` - Neispravan ID ili limit
- `404` - Proizvod nije pronađen / nema preporuka

---

### 2. GET /api/recommendations/homepage

Dohvata preporučene proizvode za homepage (bestselleri i top-rated).

**Parametri:**
```
limit (query) - broj preporuka (default: 8, max: 50)
```

**Primjer zahtjeva:**
```
GET /api/recommendations/homepage?limit=12
```

**Odgovor (200):**
```json
{
  "sourceProductId": 0,
  "title": "Trenutno preporučeni proizvodi",
  "items": [
    // ... niz RecommendedProductDto
  ]
}
```

**Cache TTL:** 5 minuta

---

### 3. GET /api/recommendations/debug/scoring/{productId}

**[ADMIN]** Dohvata detalje scoring-a za debug (kako se kalkulira score za svaki proizvod).

**Parametri:**
```
productId (path) - ID proizvoda obavezno
```

**Odgovor (200):**
```json
[
  {
    "productId": 456,
    "name": "Proizvod XYZ",
    "categoryScore": 0.85,    // Koliko su slične kategorije (0-1)
    "brandScore": 0.30,       // Slično li je brind (0-1)
    "priceScore": 0.92,       // Koliko je slična cijena (0-1)
    "popularityScore": 0.75,  // Koliko je popularan (0-1)
    "totalScore": 0.71        // Finalni score
  }
  // ... sortiran po totalScore descending
]
```

---

### 4. POST /api/recommendations/admin/invalidate-cache/{productId}

**[ADMIN]** Invalidira cache za specifičan proizvod (koristiti nakon što se proizvod ažurira).

**Parametri:**
```
productId (path) - ID proizvoda obavezno
```

**Odgovor (200):**
```json
{
  "message": "Cache invalidated"
}
```

---

## Scoring Algoritam

### Formula
```
TotalScore = 
  (CategoryScore * 0.40) +
  (BrandScore * 0.20) +
  (PriceScore * 0.20) +
  (PopularityScore * 0.20)
```

### Category Score
- **Calkulacija:** Jaccard similarity između set-ova kategorija
- **Range:** 0.0 - 1.0
- Ako proizvod nema kategorija: 0.5 (neutralno)

### Brand Score
- **Ista marka:** 0.7 (nižu vrijednost jer želimo raznolikost)
- **Različita marka:** 0.3

### Price Score
- **Calkulacija:** 1.0 - |priceDifference|
- **Range:** 0.0 - 1.0
- Ako je cijena ista: 1.0
- Ako je 50% razlika: ~0.5
- Ako je 100% razlika: 0.0

### Popularity Score
- **Base:** 0.5
- **+0.2** ako je bestseller
- **+0.1** ako je novi proizvod
- **+(rating/5) * 0.3** ako ima rating
- **Max:** 1.0

---

## Cache Strategija

### Ključevi
```
recommendations:{productId}:{type}     # Preporuke po proizvodu
recommendations:homepage               # Homepage preporuke
```

### TTL
- **Preporuke po proizvodu:** 10 minuta
- **Homepage preporuke:** 5 minuta

### Invalidacija
- Automatski se briše cache kada se proizvod ažurira (preko API-ja)
- Ručna invalidacija: `POST /api/recommendations/admin/invalidate-cache/{productId}`

---

## Performance

### Kompleksnost
- **Vrijeme izvršavanja:** ~200-500ms (prvi put)
- **S cache-om:** <10ms
- **Hit ratio:** 95%+ (zbog 10min TTL-a)

### Optimizacije
1. **NoTracking queries** - za brže dohvaćanje podataka
2. **Batch processing** - učitavanje svih kandidata odjednom
3. **FusionCache** - sa compressed storage
4. **SQL indexes** - na ProductCategoryMap i Brand

---

## Primjeri Korištenja

### Related Products na PDP
```typescript
// Frontend
const response = await fetch('/api/recommendations/product/123?limit=8&type=1');
const recommendations = await response.json();
```

### Homepage Recommendations
```typescript
// Frontend
const response = await fetch('/api/recommendations/homepage?limit=12');
const recommendations = await response.json();
```

### Cross-Sell u Cart-u
```typescript
// Frontend
const response = await fetch('/api/recommendations/product/456?limit=4&type=2');
```

---

## Budućna Poboljšanja

1. **Collaborative Filtering** - na osnovu kupovnih istorija
2. **Machine Learning** - treniranje modela na klikovima
3. **A/B Testing** - testiranje različitih scoring weights
4. **Personalization** - preporuke po korisniku
5. **Real-time Updates** - invalidacija cache-a na real-time dogajanja
