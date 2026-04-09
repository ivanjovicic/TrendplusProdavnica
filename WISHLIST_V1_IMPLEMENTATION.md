# Wishlist V1 - Implementacija

## Pregled

Implementiran je **Wishlist V1** - jednostavan, anoniman sistem omiljenih proizvoda bez potrebe za prijavom korisnika. Sve funkcionalnosti koriste **token-based pristup** sličan cart sistemu.

---

## 1. DOMAIN MODEL

### Wishlist (Domain/Sales/Wishlist.cs)
```csharp
public class Wishlist
{
    public int Id { get; set; }
    public string WishlistToken { get; set; }  // Jedinstveni token (GUID)
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public ICollection<WishlistItem> Items { get; set; }
}
```

### WishlistItem (Domain/Sales/WishlistItem.cs)
```csharp
public class WishlistItem
{
    public int Id { get; set; }
    public int WishlistId { get; set; }
    public long ProductId { get; set; }  // FK na Product
    public DateTime AddedAtUtc { get; set; }
    public Wishlist? Wishlist { get; set; }
}
```

**Constraints:**
- Unique index na `Wishlist.WishlistToken`
- Unique index na `(WishlistItem.WishlistId, WishlistItem.ProductId)`
- Cascade delete: Wishlist.Items kada se Wishlist izbriše

---

## 2. PERSISTENCE

### EF Configurations

**WishlistConfiguration** (EntityConfigurations/WishlistConfiguration.cs)
- Schema: `sales.wishlists`
- Unique index: `WishlistToken`
- Decimal precision: N/A (samo timestamp i string)

**WishlistItemConfiguration** (EntityConfigurations/WishlistItemConfiguration.cs)
- Schema: `sales.wishlist_items`
- Unique composite index: `(WishlistId, ProductId)`
- FK: `WishlistId` → `Wishlist` (cascade delete)

### Migration
```
dotnet ef migrations add AddWishlistAndWishlistItem
```

Kreira tabele sa svim constraints-ima i indeksima.

---

## 3. APPLICATION LAYER

### DTOs

**WishlistItemDto** - Stavka u listi sa detaljima proizvoda
```csharp
public long ProductId { get; set; }
public string ProductSlug { get; set; }
public string ProductName { get; set; }
public string BrandName { get; set; }
public string? PrimaryImageUrl { get; set; }
public decimal Price { get; set; }           // Min cena iz varijanata
public decimal? OldPrice { get; set; }       // Min stara cena
public bool IsInStock { get; set; }          // Bilo koji variant ima stock
public DateTime AddedAtUtc { get; set; }
```

**WishlistDto** - Kompletna lista sa svim stavkama
```csharp
public string WishlistToken { get; set; }
public List<WishlistItemDto> Items { get; set; }
public int ItemCount { get; set; }
public DateTime CreatedAtUtc { get; set; }
public DateTime UpdatedAtUtc { get; set; }
```

**AddToWishlistRequest** - Zahtev za dodavanje
```csharp
public long ProductId { get; set; }
```

### Service Interface

**IWishlistService** (Application/Wishlist/Services/IWishlistService.cs)
```csharp
Task<WishlistDto> CreateWishlistAsync();
Task<WishlistDto?> GetWishlistAsync(string wishlistToken);
Task<WishlistDto> AddItemAsync(string wishlistToken, AddToWishlistRequest request);
Task<WishlistDto> RemoveItemAsync(string wishlistToken, long productId);
Task<WishlistDto> ClearAsync(string wishlistToken);
```

### Service Implementation

**WishlistService** (Infrastructure/Services/WishlistService.cs)
- Koristi EF Core sa Include() za Product.Variants i Product.Media
- Dohvata min cenu iz svih aktivnih varijanata
- Proverava stock dostupnost
- Sprečava duplikate u wishlist-u

---

## 4. API ENDPOINTS

| Metoda | Endpoint | Opis |
|--------|----------|------|
| POST | `/api/wishlist` | Kreira novi wishlist, vraća token |
| GET | `/api/wishlist/{wishlistToken}` | Dohvata wishlist sa stavkama |
| POST | `/api/wishlist/{wishlistToken}/items` | Dodaje proizvod u wishlist |
| DELETE | `/api/wishlist/{wishlistToken}/items/{productId}` | Uklanja proizvod |
| DELETE | `/api/wishlist/{wishlistToken}/items` | Briše sve stavke |

### Primeri zahteva

**POST /api/wishlist**
```json
// Request: (empty)
// Response 201:
{
  "wishlistToken": "a1b2c3d4e5f6...",
  "items": [],
  "itemCount": 0,
  "createdAtUtc": "2026-04-09T...",
  "updatedAtUtc": "2026-04-09T..."
}
```

**POST /api/wishlist/{token}/items**
```json
// Request:
{
  "productId": 123
}

// Response 200:
{
  "wishlistToken": "a1b2c3d4e5f6...",
  "items": [
    {
      "productId": 123,
      "productSlug": "cipela-crna",
      "productName": "Црне кожне ципле",
      "brandName": "Nike",
      "primaryImageUrl": "https://...",
      "price": 5999,
      "oldPrice": 7999,
      "isInStock": true,
      "addedAtUtc": "2026-04-09T..."
    }
  ],
  "itemCount": 1,
  ...
}
```

**DELETE /api/wishlist/{token}/items/{productId}**
```
Response 200: (updated WishlistDto)
```

---

## 5. FRONTEND ИНТЕГРАЦИЈА

### 1. Wishlist Storage Helper
`src/lib/utils/wishlist-storage.ts`
```typescript
getWishlistToken(): string | null
setWishlistToken(token: string): void
clearWishlistToken(): void
```

### 2. Wishlist API Client
`src/lib/api/wishlist.ts`
```typescript
createWishlist()
getWishlist(wishlistToken: string)
addToWishlist(wishlistToken: string, request: AddToWishlistRequest)
removeFromWishlist(wishlistToken: string, productId: number)
clearWishlist(wishlistToken: string)
```

### 3. Wishlist Página
`src/app/omiljeno/page.tsx`
- Prikazuje sve stavke u wishlist-u
- Moguće dodаvanje, brisanje, čišćenje
- Prazna lista poruka ako je lista prazna
- Product cards sa slikama, cenama, stanjem na zalihama

### 4. Wishlist Button Komponenta (Optional)
Može biti dodata u ProductCard i PDP:
```typescript
<AddToWishlistButton productId={id} />
```

### 5. Types
`src/lib/types/index.ts`
- `WishlistItemDto`
- `WishlistDto`
- `AddToWishlistRequest`

---

## 6. TOK KORIŠĆENJA

### Scenarij 1: Prvi put na sajtu
1. Korisnik stigne na stranicu
2. Krene po proizvod
3. Klikne "Dodaj u listu želja" → vytori se novi wishlist (POST /api/wishlist)
4. Wishlist token se čuva u `localStorage`
5. Stavka se dodaje (POST /api/wishlist/{token}/items)

### Scenarij 2: Povratak na sajt
1. Korisnik se vraća na sajt
2. Token se učitava iz `localStorage`
3. Wishlist se dohvata (GET /api/wishlist/{token})
4. Prikazuje se lista sa sačuvanim stavkama

### Scenarij 3: Prikaz liste
1. Korisnik ide na `/omiljeno`
2. Ako nema wishlist-a, sistem ga kreira automatski
3. Prikazuje se lista svih stavki
4. Može se obrisati pojedinačna stavka ili sve

### Scenarij 4: Čuvanje podataka
- Wishlist token se čuva **trajno** u `localStorage`
- Wishlist podaci se čuvaju na **serveru** u bazi
- Nema brisanja nakon određenog vremena (V1 je jednostavan)

---

## 7. RAZLIKE OD CHECKOUT-a

| Aspekt | Wishlist V1 | Checkout V1 |
|--------|------------|------------|
| Token | Čuva se trajno | Koristi se jednokratno za checkout |
| Stavke | Proizvodi (ProductId) | Varijante (ProductVariantId + quantity) |
| Brisanje | Može se brisati jedna po jedna | N/A |
| Cena | Minimalna cena iz varijanata | Tačna cena varijanty |
| Svrha | Omiljeni proizvodi | Kuповиње |

---

## 8. PRAGMATIČKI DIZAJN

Proizvod je dizajniran da bude:
- ✅ Jednostavan - bez auth zavisnosti
- ✅ Brz - direktan pristup bez redirecta
- ✅ Skalabilan - lako se proširiće sa user nalozima kasnije
- ✅ Clean - koristi existing patterns (cart token model)
- ✅ Fleksibilan - payment method placeholder za buduće integracije

---

## 9. SLEDEĆE FAZE (V2+)

- [ ] Vezu sa user nalozima (migracija wishlist-a kad se user registruje)
- [ ] Email notifications ("Proizvod je sada dostupan!")
- [ ] Share wishlist link
- [ ] Public wishlist viewing
- [ ] Wishlist recommendations (slični proizvodi korisnicima oko)
- [ ] Expiry na wishlist tokene (npr. 30 dana)

---

## BUILD & DEPLOYMENT

```bash
# Backend migration
dotnet ef database update

# Frontend build
npm run build

# Test
npm run dev
```

**Status:** ✅ Build successful, 0 errors, ready for testing
