# 📊 Demand Prediction API - Brza Referenca

## Setup

```csharp
// Već registrovano u DI Container
services.AddAnalyticsServices(); // Uključuje IDemandPredictionService
```

## 5 API Endpoints

### 1️⃣ Predviđanje za Proizvod
```bash
POST /api/analytics/demand-prediction/predict
Authorization: Bearer {TOKEN}
Content-Type: application/json

{
  "productId": 123,
  "historyMonthsCount": 12,
  "isFootwear": true
}

# ✅ Odgovor
{
  "productId": 123,
  "productName": "Adidas Superstar",
  "expectedMonthlySales": 45.5,
  "forecastNextMonth": 52.3,
  "confidenceScore": 87.5,
  "sizeDistribution": [
    { "size": 38, "unitsSold": 12, "percentageOfTotal": 26.7, "recommendedStockQuantity": 14 }
  ],
  "monthlySalesHistory": [...]
}
```

---

### 2️⃣ Bulk Predviđanja
```bash
POST /api/analytics/demand-prediction/predict-bulk
Authorization: Bearer {TOKEN}

{
  "productIds": [123, 456, 789],
  "historyMonthsCount": 12
}

# ✅ Odgovor
{
  "predictions": [...],
  "successCount": 3,
  "failureCount": 0
}
```

---

### 3️⃣ Preporuke za Nabavku
```bash
GET /api/analytics/demand-prediction/procurement/123?safetyStockPercentage=30
Authorization: Bearer {TOKEN}

# ✅ Odgovor - Lista veličina sa preporukom za nabavku
[
  { "size": 37, "unitsSold": 8, "percentageOfTotal": 17.8, "recommendedStockQuantity": 13 },
  { "size": 38, "unitsSold": 12, "percentageOfTotal": 26.7, "recommendedStockQuantity": 18 },
  { "size": 39, "unitsSold": 11, "percentageOfTotal": 24.4, "recommendedStockQuantity": 16 }
]
```

---

### 4️⃣ Sezonalni Trendovi
```bash
GET /api/analytics/demand-prediction/seasonality/5
Authorization: Bearer {TOKEN}

# ✅ Odgovor
[
  { "season": "SPRING", "seasonalIndex": 1.15, "averageMonthlyUnits": 52 },
  { "season": "SUMMER", "seasonalIndex": 1.45, "averageMonthlyUnits": 65 },
  { "season": "FALL", "seasonalIndex": 0.95, "averageMonthlyUnits": 43 },
  { "season": "WINTER", "seasonalIndex": 0.85, "averageMonthlyUnits": 38 }
]
```

---

### 5️⃣ Top Proizvodi
```bash
GET /api/analytics/demand-prediction/top-products?categoryId=5&limit=10
Authorization: Bearer {TOKEN}

# ✅ Odgovor - Proizvodi sortirani po godišnoj potražnji
[
  { "productId": 123, "productName": "Adidas Superstar", "expectedMonthlySales": 45.5, ... },
  { "productId": 456, "productName": "Nike Air Max", "expectedMonthlySales": 38.2, ... }
]
```

---

## 🎯 Tipični Scenariji

### Scenario 1: Planiranje Nabavke
```bash
# 1. Dobija preporuke za производ
GET /api/analytics/demand-prediction/procurement/123?safetyStockPercentage=25

# 2. Rezultat - koristi "recommendedStockQuantity" za PO
# EU 37: nabavi 10 komada
# EU 38: nabavi 15 komada
# EU 39: nabavi 14 komada
# ...
```

### Scenario 2: Analiza Sezonalnosti
```bash
# Pred letnju sezonu
GET /api/analytics/demand-prediction/seasonality/5

# Vidim da je SUMMER: seasonalIndex = 1.45
# Očekujem 45% veću potražnju nego prosečna sezona
# → Planiram 45% više nabavki
```

### Scenario 3: Monitoring Trendova
```bash
# Svaki dan - proveri top proizvode
GET /api/analytics/demand-prediction/top-products?categoryId=5&limit=20

# Ako "forecastNextMonth" pada za >30%, razmisli o:
# - Promociji za proizvod
# - Analizi konkurencije
# - Preispitivanju cene
```

---

## 📈 Šta Znače Vrednosti

| Vrednost | Opis | Akcija |
|---|---|---|
| **expectedMonthlySales** | Prosečna mesečna prodaja (12 meseci) | Osnov za planiranje |
| **forecastNextMonth** | Predviđanje za sledeći mesec | Uzmi u obzir trendove |
| **confidenceScore** | 0-100% pouzdanost analiza | >80% je OK, <50% je loš |
| **seasonalIndex** | 1.0 = prosek, >1.0 = peak | Pomnoži forecast sa indeksom |
| **recommendedStockQuantity** | Koliko komada nabaviti | Direktna preporuka |

---

## ⚠️ Limitacije

⚠️ **Novi Proizvodi**: Bez sales historije, ne može se predvideti
⚠️ **Minimalno 3 Meseca**: Potrebna je historija za validnu analizu
⚠️ **Outliers**: Veliki promotions iskrivljuju podatke
⚠️ **Admin Role**: Sve operacije zahtevaju Admin autentifikaciju

---

## 🔧 Troubleshooting

### "Product not found"
```
→ Proverite ProductId
→ Proizvod mora biti active (IsVisible = true)
```

### confidenceScore < 50%
```
→ Malo podataka ili veoma volatilna potražnja
→ Koristite sa pažnjom
```

### Nulls u sizeDistribution
```
→ Proizvod nije obuća ili IsFootwear = false
→ Ili nema sales podataka sa recorded size-ovima
```

---

## 💻 C# Primeri

```csharp
// Inject servis
private readonly IDemandPredictionService _demandPredictionService;

// Jedno predviđanje
var prediction = await _demandPredictionService.PredictDemandAsync(
    new DemandPredictionRequest { ProductId = 123 }
);

// Preporuke za nabavku
var recommendations = await _demandPredictionService
    .GetProcurementRecommendationsAsync(123, safetyStockPercentage: 30);

// Top proizvodi
var topProducts = await _demandPredictionService
    .GetTopDemandProductsAsync(categoryId: 5, limit: 10);

// Sezonalnost
var seasonality = await _demandPredictionService
    .GetCategorySeasonalityAsync(categoryId: 5);
```

---

## 📊 Data Sources

Svi podaci dolaze iz:
- `Order.PlacedAtUtc` - Kad je order kompletan
- `OrderItem.ProductId` - Proizvod
- `OrderItem.SizeEuSnapshot` - EU veličina
- `OrderItem.Quantity` - Koliko komada
- `Order.Status` - Filtrira Cancelled ordere

Periode:
- `12 meseci` - za monthly sales history
- `24 meseca` - za sezonalnost
- `X meseci` - customizable via HistoryMonthsCount
