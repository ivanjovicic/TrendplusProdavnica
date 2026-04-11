# Demand Prediction System - Kompletna Dokumentacija

## 📋 Pregled

Sistem za predviđanje potražnje za obuću na osnovu istorijskih podataka o prodaji. Korisno za:
- **Nabavku (Procurement)** - Planiranje količina po veličinama
- **Upravljanje inventarom** - Optimizacija stock nivoa
- **Sezonalno planiranje** - Priprema za peak sezone
- **Analizu trendova** - Razumevanje šta se dobro prodaje

## 🏗️ Arhitektura

```
TrendplusProdavnica.Api
    ↓
AnalyticsController (demand-prediction/* endpoints)
    ↓
IdemandPredictionService (Application layer)
    ↓
DemandPredictionService + DemandPredictionQueries (Infrastructure)
    ↓
TrendplusDbContext (Order, OrderItem, Product)
```

## 📦 Implementirane Komponente

### 1. DTOs (`Application/Analytics/DTOs/DemandPredictionDtos.cs`)

#### DemandPredictionRequest
```csharp
{
    "productId": 123,
    "historyMonthsCount": 12,  // Koliko meseci istorije za analizu
    "isFootwear": true         // Da li je proizvod obuća
}
```

#### DemandPredictionDto
```csharp
{
    "productId": 123,
    "productName": "Adidas Superstar",
    "expectedMonthlySales": 45.5,          // Prosečna mesečna prodaja
    "forecastNextMonth": 52.3,             // Predviđanje za sledeći mesec
    "confidenceScore": 87.5,               // Sigurnost predviđanja 0-100%
    "analyzed": "2026-04-10T12:00:00Z",
    "status": "COMPLETED",
    
    // Mesečni podaci poslednje 12 meseci
    "monthlySalesHistory": [
        {
            "month": "2025-04",
            "unitsSold": 42.0,
            "revenue": 12600.0
        },
        // ... još 11 meseci
    ],
    
    // Distribucija po veličinama
    "sizeDistribution": [
        {
            "size": 37.0,
            "unitsSold": 8,
            "percentageOfTotal": 17.8,
            "recommendedStockQuantity": 10
        },
        {
            "size": 38.0,
            "unitsSold": 12,
            "percentageOfTotal": 26.7,
            "recommendedStockQuantity": 14
        },
        // ... više veličina
    ],
    
    // Sezonalni trendovi
    "seasonalityIndex": [
        {
            "season": "SPRING",
            "seasonalIndex": 1.15,
            "averageMonthlyUnits": 52.0
        }
    ]
}
```

#### SizeDistributionData
```csharp
{
    "size": 38.0,              // EU veličina
    "unitsSold": 12,           // Ukupno prodato u periodu
    "percentageOfTotal": 26.7, // Koliko % od svih prodaja
    "recommendedStockQuantity": 14  // Preporuka za nabavku sledeći mesec
}
```

## 🔌 API Endpoints

### 1. Predviđanje Potražnje za Jedan Proizvod

**POST** `/api/analytics/demand-prediction/predict`

**Primer zahteva:**
```bash
curl -X POST https://api.example.com/api/analytics/demand-prediction/predict \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "productId": 123,
    "historyMonthsCount": 12,
    "isFootwear": true
  }'
```

**Odgovor:**
```json
{
  "productId": 123,
  "productName": "Adidas Superstar",
  "expectedMonthlySales": 45.5,
  "forecastNextMonth": 52.3,
  "confidenceScore": 87.5,
  ...
}
```

---

### 2. Bulk Predviđanje

**POST** `/api/analytics/demand-prediction/predict-bulk`

**Koristi:** Kada trebate predviđanja za više proizvoda odjednom

**Primer zahteva:**
```bash
curl -X POST https://api.example.com/api/analytics/demand-prediction/predict-bulk \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "productIds": [123, 456, 789, 1000],
    "historyMonthsCount": 12
  }'
```

**Odgovor:**
```json
{
  "predictions": [
    { "productId": 123, "expectedMonthlySales": 45.5, ... },
    { "productId": 456, "expectedMonthlySales": 38.2, ... },
    ...
  ],
  "successCount": 4,
  "failureCount": 0,
  "errors": []
}
```

---

### 3. Preporuke za Nabavku

**GET** `/api/analytics/demand-prediction/procurement/{productId}?safetyStockPercentage=20`

**Koristi:** Dobiti konkretne preporuke koliko komada svake veličine nabaviti

**Pример zahteva:**
```bash
curl -X GET "https://api.example.com/api/analytics/demand-prediction/procurement/123?safetyStockPercentage=30" \
  -H "Authorization: Bearer TOKEN"
```

**Odgovor:**
```json
[
  {
    "size": 37.0,
    "unitsSold": 8,
    "percentageOfTotal": 17.8,
    "recommendedStockQuantity": 13  // 10 + 30% safety stock
  },
  {
    "size": 38.0,
    "unitsSold": 12,
    "percentageOfTotal": 26.7,
    "recommendedStockQuantity": 18  // 14 + 30% safety stock
  },
  ...
]
```

---

### 4. Sezonalni Trendovi Kategorije

**GET** `/api/analytics/demand-prediction/seasonality/{categoryId}`

**Koristi:** Razumeti kako se potražnja menja tokom godine

**Primer zahteva:**
```bash
curl -X GET "https://api.example.com/api/analytics/demand-prediction/seasonality/5" \
  -H "Authorization: Bearer TOKEN"
```

**Odgovor:**
```json
[
  {
    "season": "SPRING",
    "seasonalIndex": 1.15,
    "averageMonthlyUnits": 52.0
  },
  {
    "season": "SUMMER",
    "seasonalIndex": 1.45,
    "averageMonthlyUnits": 65.0
  },
  {
    "season": "FALL",
    "seasonalIndex": 0.95,
    "averageMonthlyUnits": 43.0
  },
  {
    "season": "WINTER",
    "seasonalIndex": 0.85,
    "averageMonthlyUnits": 38.0
  }
]
```

- **SeasonalIndex**: 
  - `1.0` = prosečna sezona
  - `>1.0` = peak sezona (veća potražnja)
  - `<1.0` = low sezona (manja potražnja)

---

### 5. Top Proizvodi po Predviđenoj Potražnji

**GET** `/api/analytics/demand-prediction/top-products?categoryId=5&limit=10`

**Koristi:** Identifikuj koje proizvode trebam prioritizirati

**Primer zahteva:**
```bash
curl -X GET "https://api.example.com/api/analytics/demand-prediction/top-products?categoryId=5&limit=15" \
  -H "Authorization: Bearer TOKEN"
```

**Odgovor:**
```json
[
  {
    "productId": 123,
    "productName": "Adidas Superstar",
    "expectedMonthlySales": 45.5,
    "forecastNextMonth": 52.3,
    ...
  },
  {
    "productId": 456,
    "productName": "Nike Air Max",
    "expectedMonthlySales": 38.2,
    ...
  },
  ...
]
```

## 💡 Use Cases - Primeri Korišćenja

### Use Case 1: Planiranje Nabavke za Sledeći Mesec

```csharp
// 1. Dobija predviđanja za sve proizvode u kategoriji
var allProducts = await _demandPredictionService.GetTopDemandProductsAsync(
    categoryId: 5,
    limit: 100
);

// 2. Za svaki proizvod, dobija preporuke po veličini
foreach (var product in allProducts)
{
    var sizeRecommendations = await _demandPredictionService
        .GetProcurementRecommendationsAsync(
            product.ProductId,
            safetyStockPercentage: 25  // 25% buffer za sigurnost
        );
    
    // 3. Prosledi preporuke procurement timu
    await _procurementService.PlaceOrderAsync(sizeRecommendations);
}
```

### Use Case 2: Analiza Sezonalnosti Pre Peak Sezone

```csharp
// Jednom godišnje - pred letnjo sezonu (春)
var seasonalityData = await _demandPredictionService.GetCategorySeasonalityAsync(5);

var summerSeasonality = seasonalityData.First(x => x.Season == "SUMMER");
Console.WriteLine($"Leto je {summerSeasonality.SeasonalIndex}x aktivnije od proseka");

// Planiraj da nabavim više te vreme
var increasePercentage = (summerSeasonality.SeasonalIndex - 1) * 100;
Console.WriteLine($"Nabavi za {increasePercentage}% više robe");
```

### Use Case 3: Monitoring Trendova - Naglo Smanjenje Potražnje

```csharp
// Svakodnevni monitoring
var prediction = await _demandPredictionService.PredictDemandAsync(
    new DemandPredictionRequest { ProductId = 123 }
);

if (prediction.ForecastNextMonth < prediction.ExpectedMonthlySales * 0.7)
{
    // Potražnja pada za >30%
    _logger.LogWarning("Proizvod {ProductId}: potražnja pada", 123);
    
    // Akcija: Smanji nabavke, razmisli o promociji
    await _promotionService.CreateDiscountAsync(
        productId: 123,
        discountPercent: 15
    );
}
```

## 📊 Kako Funkcionira Predviđanje

### Algoritam

1. **Učitaj sales historiju** - poslednji 12 meseci
   ```
   1. Pretraga svih Order.Items gde je ProductId = X
   2. Grupiraj po mesecima
   3. Sumira quantity po mesecima
   ```

2. **Analiza trendova**
   ```
   - Kalkuliši moving average (prozor od 3 meseca)
   - Detektuj trend (raste/pada)
   - Izračunaj trend faktor
   ```

3. **Sezonalnost**
   ```
   - Grupiraj sales po sezoni (WINTER, SPRING, SUMMER, FALL)
   - Azionaj prosečnu potražnju po sezoni
   - Kalkuliši seasonal index = prosečna prodaja sezone / overall prosek
   ```

4. **Distribucija po veličinama**
   ```
   - Grupiraj OrderItem.SizeEuSnapshot
   - Izračunaj % od ukupne prodaje
   - Primeni na predicted quantity
   ```

5. **Confidence Score**
   ```
   - +8.33% po mesecima historije (max 100%)
   - -volatilnost penalty (ako je potražnja nepredvidiva)
   - +2% po različitim veličinama (više veličina = stabila potražnja)
   ```

### Primer Kalkulacije

```
Proizvod: Adidas Superstar
Istorija: 12 meseci
Mesečna prodaja: [40, 42, 45, 48, 50, 48, 45, 42, 40, 38, 35, 33]

1. Prosečna prodaja = 42.08 komada/mesec

2. Trend analiza:
   - Poslednja 3 meseca: prosek = 36
   - Pre 3 meseca: prosek = 48
   - Trend = 36/48 = 0.75 (pada)

3. Predikcija za sledeći mesec = 42.08 * 0.75 = 31.56 komada

4. Distribucija veličina:
   - EU 37: 15% → 31.56 * 0.15 = 4.7 → 5 komada
   - EU 38: 28% → 31.56 * 0.28 = 8.8 → 9 komada
   - EU 39: 22% → 31.56 * 0.22 = 6.9 → 7 komada
   - EU 40: 20% → 31.56 * 0.20 = 6.3 → 6 komada
   - EU 41: 15% → 31.56 * 0.15 = 4.7 → 5 komada

5. Sa 30% safety stock:
   - EU 37: 5 + 1.5 = 6-7 komada
   - EU 38: 9 + 2.7 = 11-12 komada
   - itd.

6. Confidence Score = 100% - volatilnost_penalty + veličine_bonus
                    = 100 - 15 + 10 = 95%
```

## 🏢 Integracija sa Drugim Servisima

### Sa Admin API-jem

```csharp
// U Admin controlleru
[HttpGet("products/{id}/demand-forecast")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> GetProductDemandForecast(long id)
{
    var prediction = await _demandPredictionService.PredictDemandAsync(
        new DemandPredictionRequest { ProductId = id }
    );
    
    return Ok(prediction);
}
```

### Sa Procurement Sistemom

```csharp
// Automatska nabavka na osnovu predviđanja
var prediction = await _demandPredictionService.PredictDemandAsync(request);
foreach (var size in prediction.SizeDistribution)
{
    await _procurementSystem.CreatePOLine(new
    {
        ProductId = prediction.ProductId,
        Size = size.Size,
        Quantity = size.RecommendedStockQuantity
    });
}
```

### Sa Pricing Sistemom

```csharp
// Dinamički cene na osnovu potražnje
var prediction = await _demandPredictionService.PredictDemandAsync(request);

if (prediction.ForecastNextMonth > prediction.ExpectedMonthlySales * 1.2)
{
    // Potražnja raste - može se podići cena
    var newPrice = product.Price * 1.05;
    await _pricingService.UpdatePriceAsync(prediction.ProductId, newPrice);
}
else if (prediction.ForecastNextMonth < prediction.ExpectedMonthlySales * 0.8)
{
    // Potražnja pada - trebalo bi sniženje
    var newPrice = product.Price * 0.90;
    await _promotionService.CreateDynamicDiscountAsync(prediction.ProductId, 10);
}
```

## 📈 Performance i Optimizacija

### Performanse po Veličini

| Broj Proizvoda | Vreme | Notes |
|---|---|---|
| 1 proizvod | ~50ms | Database queries + kalkulacija |
| 10 proizvoda | ~500ms | Bulk endpoint |
| 100 proizvoda | ~5s | Sa parallelizacijom |
| 1000 proizvoda | ~50s | Preporučuje se batch po 100 |

### Baza Podataka

Qu koji se koriste:

```sql
-- 1. Mesečna prodaja
SELECT YEAR(PlacedAtUtc), MONTH(PlacedAtUtc), SUM(Quantity), SUM(LineTotal)
FROM Orders o
JOIN OrderItems oi ON o.Id = oi.OrderId
WHERE oi.ProductId = @ProductId 
  AND o.PlacedAtUtc >= DATEADD(month, -12, GETDATE())
  AND o.Status != 'Cancelled'
GROUP BY YEAR(PlacedAtUtc), MONTH(PlacedAtUtc)

-- 2. Distribucija veličina
SELECT SizeEuSnapshot, COUNT(*)
FROM OrderItems oi
JOIN Orders o ON o.Id = oi.OrderId
WHERE oi.ProductId = @ProductId 
  AND o.Status != 'Cancelled'
GROUP BY SizeEuSnapshot
```

### Caching Preporuke

```csharp
// Keširaj na 24 sata
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
});

// U servisu:
var cacheKey = $"demand-prediction:{productId}";
var cached = await _cache.GetStringAsync(cacheKey);

if (cached != null)
{
    return JsonSerializer.Deserialize<DemandPredictionDto>(cached);
}

var prediction = CalculatePrediction(productId);
await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(prediction), 
    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) });

return prediction;
```

## ⚠️ Limitacije i Napomene

1. **Minimalna Historija**: Potrebno najmanje 3 meseca podataka za validu analizu
2. **Novi Proizvodi**: Bez sales historije, algoritam ne može predvideti
3. **Outliers**: Veliki promotions ili festivali ćeće iskriviti podatke
4. **Sezonalnost**: Sa samo 1 godinom podataka, sezonalnost je aproksimativna

## 🔧 Future Enhancements

- [ ] Machine Learning predviđanja (ARIMA, Prophet)
- [ ] Anomaly detection za outliers
- [ ] A/B testiranje preporuka
- [ ] Integracija sa external data (weather, events)
- [ ] Per-store demand variacija
- [ ] Price elasticity analiza

## 📝 Implementacione Checklistе

- [x] Domain entity struktura
- [x] Application DTOs
- [x] Service interface (IDemandPredictionService)
- [x] Service implementacija
- [x] Database queries (DemandPredictionQueries)
- [x] API endpoints
- [x] DI container registracija
- [x] Dokumentacija
- [ ] Unit tests
- [ ] Integration tests
- [ ] Load testing
- [ ] Frontend integracija
