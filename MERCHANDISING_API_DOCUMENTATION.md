# Merchandising Rules Engine - API Documentation

## Overview
Merchandising Rules Engine omogućava ručno upravljanje redosledom proizvoda u listingu kroz systeme Pin, Boost, i Demote pravila.

## Base URL
```
/api/admin/merchandising
```

## Authentication
Svi endpoint-i zahtevaju JWT token u Authorization header-u:
```
Authorization: Bearer {token}
```

## Endpoints

### 1. GET /api/admin/merchandising
Dohvata sva merchandising pravila sa mogućnostima filtriranja.

**Query Parameters:**
- `categoryId` (int?, optional): Filtrira po kategoriji
- `brandId` (int?, optional): Filtrira po marki
- `onlyActive` (bool?, optional): Ako je true, dohvata samo aktivna pravila

**Response:**
```json
[
  {
    "id": 1,
    "name": "Summer Sale Pin - Dresses",
    "description": "Pin best-selling dresses on top",
    "ruleType": 1,
    "ruleTypeName": "Pin",
    "categoryId": 5,
    "brandId": null,
    "productId": 123,
    "boostScore": 0,
    "startDate": "2024-06-01T00:00:00Z",
    "endDate": "2024-08-31T23:59:59Z",
    "isActive": true,
    "priority": 100,
    "createdByUserId": 1,
    "createdAtUtc": "2024-05-15T10:30:00Z",
    "updatedByUserId": null,
    "updatedAtUtc": null
  }
]
```

### 2. GET /api/admin/merchandising/{id}
Dohvata specifično pravilo.

**Parameters:**
- `id` (long, required): ID pravila

**Response:** Pojedinačno pravilo (vidi primer kod GET /api/admin/merchandising)

**Error Codes:**
- 404: Pravilo nije pronađeno

### 3. POST /api/admin/merchandising
Kreira novo merchandising pravilo.

**Request Body:**
```json
{
  "name": "Black Friday Boost - Electronics",
  "description": "Boost electronics during Black Friday",
  "ruleType": 2,
  "categoryId": 8,
  "brandId": null,
  "productId": null,
  "boostScore": 30,
  "startDate": "2024-11-24T00:00:00Z",
  "endDate": "2024-11-28T23:59:59Z",
  "priority": 200
}
```

**Request Field Descriptions:**
- `name` (string, required): Naziv pravila (max 256 karaktera)
- `description` (string, optional): Detaljan opis
- `ruleType` (short, required): 
  - 1 = Pin (pinuj na vrh)
  - 2 = Boost (pojačaj vidljivost)
  - 3 = Demote (smanji vidljivost)
- `categoryId` (long?, optional): ID kategorije za filtering
- `brandId` (long?, optional): ID marke za filtering
- `productId` (long?, optional): ID proizvoda za direktno pinovanje
- `boostScore` (decimal, required): 
  - Za Boost: pozitivan procenat (20 = +20%)
  - Za Demote: pozitivan procenat koji će biti oduzet (20 = -20%)
  - Za Pin: ignorisano (postavi na 0)
- `startDate` (DateTime, required): Početak primene pravila
- `endDate` (DateTime?, optional): Kraj primene pravila
- `priority` (int, optional): Prioritet (viši broj = viša prioriteta, default 100)

**Response:**
```json
{
  "id": 2,
  "name": "Black Friday Boost - Electronics",
  "description": "Boost electronics during Black Friday",
  "ruleType": 2,
  "ruleTypeName": "Boost",
  "categoryId": 8,
  "brandId": null,
  "productId": null,
  "boostScore": 30,
  "startDate": "2024-11-24T00:00:00Z",
  "endDate": "2024-11-28T23:59:59Z",
  "isActive": true,
  "priority": 200,
  "createdByUserId": 1,
  "createdAtUtc": "2024-05-20T14:15:00Z",
  "updatedByUserId": null,
  "updatedAtUtc": null
}
```

**Response Codes:**
- 201 Created: Pravilo je uspešno kreirano
- 400 Bad Request: Validacijska greška

### 4. PUT /api/admin/merchandising/{id}
Ažurira postojeće pravilo.

**Parameters:**
- `id` (long, required): ID pravila za ažuriranje

**Request Body:**
```json
{
  "name": "Updated Boost Name",
  "description": "Updated description",
  "ruleType": 2,
  "categoryId": 8,
  "brandId": null,
  "productId": null,
  "boostScore": 25,
  "startDate": "2024-11-24T00:00:00Z",
  "endDate": "2024-11-28T23:59:59Z",
  "isActive": true,
  "priority": 150
}
```

**Note:** Samo polja koja su zavoljena će biti ažurirana. Null vrednosti znače da se neće menjati.

**Response:**
Ažurirano pravilo (vidi format kod POST)

**Response Codes:**
- 200 OK: Pravilo je ažurirano
- 404 Not Found: Pravilo ne postoji
- 400 Bad Request: Validacijska greška

### 5. DELETE /api/admin/merchandising/{id}
Briše merchandising pravilo.

**Parameters:**
- `id` (long, required): ID pravila za brisanje

**Response Codes:**
- 204 No Content: Pravilo je uspešno obrisano
- 404 Not Found: Pravilo ne postoji

### 6. POST /api/admin/merchandising/cache/invalidate
Invalidira cache svih merchandising pravila.

**Response:**
```json
{
  "message": "Cache je invalidiran"
}
```

**Note:** Automatski se invalidira nakon Create/Update/Delete operacija.

## Rule Types Explanation

### Pin (RuleType = 1)
- **Namena**: Fiksno pinovanje proizvoda na vrh liste
- **Ponašanje**: Proizvod se pojavljuje sa fiksnom visokom scenom (10000 + priority)
- **Targeting**: Obavezno ProductId, ili kombinacija CategoryId + ProductId
- **Primer**: Pin najbolje prodavance na vrhu kategorije tokom promocije

### Boost (RuleType = 2)
- **Namena**: Povećanje vidljivosti proizvoda
- **Ponašanje**: Povećava skor proizvoda za procenat
- **Targeting**: Može biti po ProductId, CategoryId, BrandId, ili globalno
- **Primer**: +30% boost za sve proizvode marke Nike tokom kampanje

### Demote (RuleType = 3)
- **Namena**: Smanjenje vidljivosti proizvoda
- **Ponašanje**: Smanjuje skor proizvoda za procenat
- **Targeting**: Može biti po ProductId, CategoryId, BrandId, ili globalno
- **Primer**: -50% demote za diskontinuirane proizvode

## Scoring Algorithm

1. **Evaluacija Pin Pravila** (prioritet 1)
   ```
   Ako postoji aktivno Pin pravilo za proizvod:
     Score = 10000 + Priority
     STOP (pin ima maksimalnu prioritetu)
   ```

2. **Evaluacija Boost/Demote Pravila** (prioritet 2)
   ```
   TotalBoost = 0
   
   Za svako aktivno Boost pravilo koje se primenjuje:
     TotalBoost += BoostScore
   
   Za svako aktivno Demote pravilo koje se primenjuje:
     TotalBoost -= BoostScore
   
   FinalScore = OriginalScore * (1 + (TotalBoost / 100))
   FinalScore = Max(0, FinalScore)  // Minimalno 0
   ```

3. **Redoslednost**
   - Pravila se prvo sortiraju po Priority (opadajuće)
   - Zatim po RuleType (Pin > Boost > Demote)
   - Aktivnost se proverava: IsActive = true AND CurrentTime u [StartDate, EndDate]

## Examples

### Primer 1: Pinovanje produkra tokom Black Fridia-ja
```bash
curl -X POST /api/admin/merchandising \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Black Friday Top Seller Pin",
    "description": "Pin top selling laptop on entire listing",
    "ruleType": 1,
    "productId": 456,
    "boostScore": 0,
    "startDate": "2024-11-24T00:00:00Z",
    "endDate": "2024-11-28T23:59:59Z",
    "priority": 300
  }'
```

### Primer 2: Boost za novu kolekciju
```bash
curl -X POST /api/admin/merchandising \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "New Collection Boost",
    "description": "Boost new summer collection for 2 weeks",
    "ruleType": 2,
    "categoryId": 12,
    "boostScore": 50,
    "startDate": "2024-06-01T00:00:00Z",
    "endDate": "2024-06-14T23:59:59Z",
    "priority": 150
  }'
```

### Primer 3: Demote starih modela
```bash
curl -X POST /api/admin/merchandising \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Demote Discontinued Models",
    "description": "Push discontinued items to bottom",
    "ruleType": 3,
    "brandId": 7,
    "boostScore": 75,
    "startDate": "2024-01-01T00:00:00Z",
    "endDate": null,
    "priority": 50
  }'
```

## Error Handling

### Common Error Responses

**400 Bad Request:**
```json
{
  "error": "Rule name is required and must not exceed 256 characters"
}
```

**404 Not Found:**
```json
{
  "error": "Rule with ID 999 was not found"
}
```

**500 Internal Server Error:**
```json
{
  "error": "Error occurred while processing merchandising rules"
}
```

## Caching

- Sve pravila se čuvaju u cache-u sa 30-minutnim TTL-om
- Cache se automatski invalidira nakon Create/Update/Delete
- Evaluation koristi cache za bolje performanse
- Možete ručno invalidirati cache kroz `POST /cache/invalidate`

## Performance Considerations

- Prioritet > RuleType > StartDate evaluacija je optimizovana
- Bulk operations preporučeni za veliki broj pravila
- Pin pravila se evaluiraju prvi (early exit ako pronađeno)
- Kategorija + Brand filteri smanjuju broj evaluacija

## Best Practices

1. **Vremenski Periodi**: Uvek postavite EndDate da izbegnete vječna pravila
2. **Prioriteti**: Koristite različite prioritete za detaljan kontrolu redosleda
3. **Specifičnost**: Budi specifičan - pin einzelni proizvod umesto cele kategorije ako je moguće
4. **Monitoring**: Redovito proveravaj活 pravila da se nisu istekla
5. **Testing**: Testiraj sa `onlyActive=false` pre nego što publikuješ promociju

## Integration Points

### ProductListingQueryService
Merchandising servise se poziva pre finalnog sortiranja:
```csharp
var rules = await merchandisingService.EvaluateRulesAsync(products);
// Primeni scored adjustments pre than default sorting
```

### Recommendation Engine
Merchandising Pin pravila imaju prioritet nad recommendation scoring.

### Cache Invalidation
Automatski se integriše sa WebshopCache invalidacijom kroz Domain Events.
