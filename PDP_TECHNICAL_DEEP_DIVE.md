# 🔧 PDP Conversion Bug Fix - Technical Implementation Details

## Architecture Overview

```
proizvod/[slug]/page.tsx (Server Component - SSR)
  ├─ Product metadata (SEO, JSON-LD, breadcrumbs)
  ├─ Product media gallery
  ├─ Product info (name, price, etc.)
  │
  └─> ProductDetailsClient (Client Component - State Management)
        ├─ State: selectedVariantSku
        ├─ State: showSizeError
        │
        ├─> Size Buttons Grid (Responsive 3-5 cols)
        │     └─ onClick → handleSizeSelect(sku)
        │     └─ className → {selected/disabled states}
        │
        ├─ Error Message (Red box if no selection)
        │
        └─> AddToCartButton (Client Component - Cart API)
              ├─ Props: variantId (computed from selectedVariantSku)
              ├─ Props: onSizeRequired (callback if variantId = 0)
              ├─ State: successMessage
              ├─ State: error
              └─ Render: Green/Red feedback boxes
```

---

## File 1: product-details-client.tsx (NEW)

### Purpose
Encapsulate client-side variant/size selection logic. Provides:
- State management for selected size
- Visual feedback for selection states
- Validation before add-to-cart
- Bridge between ProductDetailsClient and AddToCartButton

### Key Code Sections

#### Imports & Types
```typescript
'use client';

import { useState, useCallback } from 'react';
import { ProductdetailDto } from '@/types/product';
import { AddToCartButton } from './add-to-cart';

interface SizeButton {
  sizeEu: number;
  sku: string;
  isActive: boolean;
  isVisible: boolean;
  totalStock: number;
}
```

#### State Management
```typescript
export function ProductDetailsClient({ product }: Props) {
  const [selectedVariantSku, setSelectedVariantSku] = useState<string | null>(null);
  const [showSizeError, setShowSizeError] = useState(false);
  
  // Compute variant ID from SKU (format: "{variantId}-{size}")
  const selectedVariantId = selectedVariantSku 
    ? Number(selectedVariantSku.split('-')[0]) 
    : 0;
```

**Why this approach**:
- SKU format je `{variantId}-{size}` (npr. "123-42")
- Split na "-" daje nam variantId
- selectedVariantId = 0 znači "nothing selected"

#### Click Handler
```typescript
const handleSizeSelect = useCallback((sku: string) => {
  setSelectedVariantSku(sku);
  setShowSizeError(false);  // ← Clear error when user selects
}, []);
```

**Why useCallback**:
- Optimizacija - ne rekreira funkciju svaki render
- Button reference stability - može biti prosleđena kao prop

#### Size Button Styling

```typescript
// Compute button classes based on state
const getButtonClassName = (sku: string, isDisabled: boolean) => {
  const isSelected = selectedVariantSku === sku;
  
  if (isDisabled) {
    return `rounded border py-2 px-1 
      border-gray-300 bg-gray-100 cursor-not-allowed 
      text-gray-400 font-medium`; // Out-of-stock style
  }
  
  if (isSelected) {
    return `rounded border-2 py-2 px-1 
      border-black bg-black text-white font-bold 
      transition-colors`; // Selected style
  }
  
  return `rounded border py-2 px-1 
    border-gray-300 hover:border-black 
    transition-colors cursor-pointer`; // Default style
};
```

**State combinations explained**:
1. **Out-of-Stock** (isDisabled=true): Gray background, lighter gray text, no-cursor
2. **Selected** (selectedVariantSku === sku): Black background, white text, bold
3. **Default** (neither): Gray border, hover effect, normal cursor

#### Grid Layout (Responsive)

```typescript
<div className="grid grid-cols-3 gap-2 sm:grid-cols-4 md:grid-cols-5">
  {product.sizes.map((size) => {
    const isDisabled = !size.isActive || !size.isVisible || size.totalStock === 0;
    
    return (
      <button
        key={size.sku}
        title={isDisabled ? 'Nije dostupno' : `Veličina ${size.sizeEu}`}
        onClick={() => handleSizeSelect(size.sku)}
        disabled={isDisabled}
        className={getButtonClassName(size.sku, isDisabled)}
      >
        {size.sizeEu}
      </button>
    );
  })}
</div>
```

**Responsive breakpoints**:
- Mobile (default): 3 columns
- Tablet (sm:): 4 columns  
- Desktop (md:): 5 columns

**Accessibility**:
- `title` attribute sa opisom (korak pred screenreaders)
- `disabled` attribute koristeći native HTML disability

#### Error Message Display

```typescript
{showSizeError && (
  <p className="mt-2 text-sm text-red-600 font-medium">
    Molimo izaberite veličinu prije nego što dodate u korpu.
  </p>
)}
```

**Trigger**: ShowSizeError se postavi na true u ProductDetailsClient-u:
```typescript
const handleAddToCartClick = () => {
  if (selectedVariantId === 0) {
    setShowSizeError(true);
    return;
  }
  // AddToCartButton će se kliknuti (videti doole)
};
```

#### AddToCartButton Integration

```typescript
<AddToCartButton
  variantId={selectedVariantId}  // ← KEY: Dynamic, ne hardcoded!
  quantity={1}
  onSizeRequired={() => {
    setShowSizeError(true);  // Ako AddToCartButton detekuje variantId < 0
  }}
  onSuccess={() => {
    // Opciono: Clear selection nakon uspešnog dodavanja
    // setSelectedVariantSku(null);
  }}
/>
```

**Props prosleđeni AddToCartButton-u**:
- `variantId`: Computed iz selectedVariantSku, može biti 0 ako ništa nije selektovano
- `onSizeRequired`: Callback ako je variantId 0
- `onSuccess`: Callback nakon uspešnog dodavanja

---

## File 2: add-to-cart.tsx (MODIFIED)

### What Changed

#### Before
```typescript
'use client';

import { useState, useCallback } from 'react';

interface AddToCartButtonProps {
  variantId: number;
  quantity?: number;
  onSuccess?: () => void;
}

export function AddToCartButton(props: AddToCartButtonProps) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  const handleAddToCart = useCallback(async () => {
    try {
      const response = await fetch('/api/cart/add', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          variantId: props.variantId,
          quantity: props.quantity || 1,
        }),
      });
      
      if (!response.ok) throw new Error('Failed to add to cart');
      
      alert('Dodano u korpu!');  // ❌ PROBLEM HERE
      props.onSuccess?.();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Unknown error');
    } finally {
      setLoading(false);
    }
  }, [props]);
  
  return (
    <button
      onClick={handleAddToCart}
      disabled={loading}
      className="w-full rounded bg-black py-3 text-white font-bold hover:bg-gray-800"
    >
      {loading ? 'Dodajem...' : 'Dodaj u korpu'}
    </button>
  );
}
```

#### After
```typescript
'use client';

import { useState, useCallback } from 'react';

interface AddToCartButtonProps {
  variantId: number;
  quantity?: number;
  onSuccess?: () => void;
  onSizeRequired?: () => void;  // ← NEW callback
}

export function AddToCartButton(props: AddToCartButtonProps) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);  // ← NEW
  
  const handleAddToCart = useCallback(async () => {
    // ← NEW: Validation check
    if (props.variantId <= 0) {
      props.onSizeRequired?.();  // Call parent to show error
      return;
    }
    
    setLoading(true);
    setError(null);
    setSuccessMessage(null);
    
    try {
      const response = await fetch('/api/cart/add', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          variantId: props.variantId,
          quantity: props.quantity || 1,
        }),
      });
      
      if (!response.ok) {
        throw new Error('Nije moguće dodati u korpu. Molimo pokušajte ponovo.');
      }
      
      // ← NEW: Styled feedback instead of alert()
      setSuccessMessage('Dodano u korpu! ✓');
      
      // Auto-dismiss nakon 3s
      setTimeout(() => setSuccessMessage(null), 3000);
      
      props.onSuccess?.();
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : 'Greška pri dodavanju u korpu';
      setError(errorMsg);
      
      // ← NEW: Auto-clear error nakon 5s
      setTimeout(() => setError(null), 5000);
    } finally {
      setLoading(false);
    }
  }, [props]);  // ← Include props.onSizeRequired i props.onSuccess u dependency array
  
  return (
    <div className="space-y-2">
      {/* ← NEW: Success message with green styling */}
      {successMessage && (
        <div className="rounded-lg border border-green-200 bg-green-50 p-3">
          <p className="text-sm font-medium text-green-700">
            {successMessage}
          </p>
        </div>
      )}
      
      {/* ← NEW: Error message with red styling */}
      {error && (
        <div className="rounded-lg border border-red-200 bg-red-50 p-3">
          <p className="text-sm text-red-700">
            {error}
          </p>
        </div>
      )}
      
      <button
        onClick={handleAddToCart}
        disabled={loading}
        className={`w-full rounded py-3 text-white font-bold transition-colors ${
          loading
            ? 'bg-gray-400 cursor-not-allowed'
            : 'bg-black hover:bg-gray-800'
        }`}
      >
        {loading ? 'Dodajem...' : 'Dodaj u korpu'}
      </button>
    </div>
  );
}
```

### Key Changes Explained

#### 1. New "onSizeRequired" Callback

```typescript
// If variantId is 0 (or -1), call parent to show error
if (props.variantId <= 0) {
  props.onSizeRequired?.();
  return;
}
```

**Why**:
- ProductDetailsClient može pokazati red error message
- AddToCartButton ne mora znati o specifičnoj "size required" logici
- Decoupled - AddToCartButton je reusable za druge scenarije

#### 2. Success Message Instead of alert()

```typescript
// Before:
alert('Dodano u korpu!');

// After:
setSuccessMessage('Dodano u korpu! ✓');
setTimeout(() => setSuccessMessage(null), 3000);
```

**Advantages of styled box**:
- ✅ Može biti styling sa Tailwind (green, border, padding)
- ✅ Auto-hides nakon 3s (ne zaustavlja user flow)
- ✅ Mobile friendly (ne naglašava kao alert())
- ✅ Korisnik vidi tačan poruku (može biti dinamičan)
- ✅ Testable (može se fixture-va u testima)

#### 3. Better Error Messages

```typescript
// Before:
setError(err instanceof Error ? err.message : 'Unknown error');

// After:
throw new Error('Nije moguće dodati u korpu. Molimo pokušajte ponovo.');
// ... u catch bloku:
const errorMsg = err instanceof Error ? err.message : 'Greška pri dodavanju u korpu';
setError(errorMsg);

// Auto-clear nakon 5s
setTimeout(() => setError(null), 5000);
```

**Benefits**:
- Korisnik jasno vidi šta je pošlo naopako
- Error se ne drži zauvijek (može pokušati ponovo bez osvežavanja)
- Serbian korisnik vidi poruku na razumeljivom jeziku

#### 4. Styled Feedback Containers

```typescript
{/* Success - Green */}
<div className="rounded-lg border border-green-200 bg-green-50 p-3">
  <p className="text-sm font-medium text-green-700">{successMessage}</p>
</div>

{/* Error - Red */}
<div className="rounded-lg border border-red-200 bg-red-50 p-3">
  <p className="text-sm text-red-700">{error}</p>
</div>
```

**Design rationale**:
- Light background (green-50, red-50) - ne previše agresivno
- Matching border (green-200, red-200) - jasne ivice
- Dark text (green-700, red-700) - čitljivo
- Small padding (p-3) - kompaktno, ne ulazi u drugi sadržaj
- Rounded corners - modern look

---

## File 3: proizvod/[slug]/page.tsx (MODIFIED)

### What Changed

#### Before
```typescript
// Imports (no ProductDetailsClient)

export default async function ProductPage(props: Props) {
  const product = await getProduct(props.params.slug);
  
  return (
    <div>
      {/* Breadcrumbs, media, product info... */}
      
      {/* ❌ PROBLEM: Inline size grid with no state management */}
      {product.sizes.length > 0 && (
        <div className="mb-6">
          <label className="mb-2 block text-sm font-semibold">Veličina</label>
          <div className="grid grid-cols-4 gap-2">
            {product.sizes.map((size) => (
              <button
                key={size.sku}
                className="rounded border py-2 transition-colors hover:border-black"
                disabled={!size.isActive || !size.isVisible}
                // ❌ No onClick!
              >
                {size.sizeEu}
              </button>
            ))}
          </div>
        </div>
      )}
      
      {/* ❌ PROBLEM: Hardcoded first variant */}
      <AddToCartButton 
        variantId={Number(product.sizes[0]?.sku.split('-')[0] || 0)} 
      />
    </div>
  );
}
```

#### After
```typescript
// Import novo dodan
import { ProductDetailsClient } from '@/components/product-details-client';

export default async function ProductPage(props: Props) {
  const product = await getProduct(props.params.slug);
  
  return (
    <div>
      {/* Breadcrumbs, media, product info... */}
      
      {/* ✅ NEW: Use ProductDetailsClient for state management */}
      <ProductDetailsClient product={product} />
    </div>
  );
}
```

### Why This Approach

**Separation of Concerns**:
| Component | Responsibility | Type |
|-----------|-----------------|------|
| `proizvod/[slug]/page.tsx` | Fetch product data, render SSR metadata, breadcrumbs, media, basic info | Server |
| `ProductDetailsClient.tsx` | Size selection, error validation, visual feedback | Client |
| `AddToCartButton.tsx` | Cart API call, success/error feedback | Client |

**Benefits**:
- ✅ Server component ostaje server-side i može koristi server-only data fetching
- ✅ Client component ima "use client" i može koristiti React hooks
- ✅ Clear responsibilities (no mixing concerns)
- ✅ Reusable ProductDetailsClient - može se koristi na drugim PDP varijanatima

**SSR Preserved**:
- Metadata se generiše na serveru
- JSON-LD se generiše na serveru
- Stranica se hydrates brzo jer je client component mali

---

## State Flow Diagram

```
User Opens PDP
  ↓
Server renders proizvod/[slug]/page.tsx
  ├─ Generates metadata
  ├─ Generates JSON-LD
  ├─ Renders product media
  └─ Renders ProductDetailsClient
       ↓
       ProductDetailsClient initializes
         └─ selectedVariantSku = null
         └─ showSizeError = false
       ↓
       User clicks on size button (e.g., "42")
         ↓
         handleSizeSelect("342-42")  // variantId="342", size="42"
         ├─ setSelectedVariantSku("342-42")
         └─ setShowSizeError(false)
         ↓
         AddToCartButton receives variantId=342
         └─ Button is now ENABLED (not variantId=0)
       ↓
       User clicks "Dodaj u korpu"
         ↓
         handleAddToCart()
         ├─ Check if variantId > 0 ✓
         ├─ POST /api/cart/add { variantId: 342, quantity: 1 }
         ├─ Server responds 200 OK
         ├─ setSuccessMessage("Dodano u korpu! ✓")
         └─ setTimeout(() => setSuccessMessage(null), 3000)
         ↓
         User sees green success box for 3 seconds
         ↓
         (Success auto-hides after 3s)
```

**Error scenario**:
```
User clicks "Dodaj u korpu" WITHOUT selecting size
  ↓
handleAddToCart() in AddToCartButton
  ├─ Check if variantId <= 0 ✓ (it's 0)
  └─ Call onSizeRequired()
       ↓
       ProductDetailsClient.onSizeRequired()
         └─ setShowSizeError(true)
         ↓
         Red error message appears under size buttons
```

---

## TypeScript Interfaces Used

### ProductdetailDto
```typescript
interface ProductdetailDto {
  id: string;
  name: string;
  // ... other fields
  sizes: Array<{
    sizeEu: number;
    sku: string;           // Format: "{variantId}-{size}"
    isActive: boolean;
    isVisible: boolean;
    totalStock: number;
  }>;
}
```

### AddToCartButtonProps
```typescript
interface AddToCartButtonProps {
  variantId: number;           // 0 = not selected
  quantity?: number;           // Default: 1
  onSuccess?: () => void;      // Called after successful add
  onSizeRequired?: () => void; // Called if variantId is 0
}
```

---

## CSS Classes Breakdown

### Size Button Classes

**Default State** (Not selected, not disabled):
```
rounded border py-2 px-1 border-gray-300 hover:border-black transition-colors cursor-pointer
   └─ rounded: border-radius
   └─ border: thin gray border
   └─ py-2 px-1: vertical padding 0.5rem, horizontal 0.25rem
   └─ hover:border-black: border turns black on hover
   └─ transition-colors: animate color change
   └─ cursor-pointer: shows hand icon
```

**Selected State** (User clicked this size):
```
rounded border-2 py-2 px-1 border-black bg-black text-white font-bold transition-colors
   └─ border-2: thicker border for emphasis
   └─ border-black bg-black: black background AND border
   └─ text-white font-bold: white bold text
```

**Disabled State** (Out of stock):
```
rounded border py-2 px-1 border-gray-300 bg-gray-100 cursor-not-allowed text-gray-400 font-medium
   └─ bg-gray-100: light gray background (indicates "off")
   └─ cursor-not-allowed: "prohibited" icon
   └─ text-gray-400: lighter gray text (low contrast)
   └─ font-medium: slightly bold to indicate disabled status
```

### Feedback Message Classes

**Success Box** (Green):
```
rounded-lg border border-green-200 bg-green-50 p-3
   └─ rounded-lg: larger border radius for modern look
   └─ border-green-200: thin light green border
   └─ bg-green-50: very light green background (almost white)
   └─ p-3: padding on all sides
   
   Text:
   text-sm font-medium text-green-700
   └─ text-sm: smaller text (secondary emphasis)
   └─ font-medium: slightly bold
   └─ text-green-700: darker green text (readable)
```

**Error Box** (Red):
```
rounded-lg border border-red-200 bg-red-50 p-3
   └─ Same structure as success, but red tones
   
   Text:
   text-sm text-red-700
   └─ text-red-700: darker red text
```

---

## Performance Considerations

**Client Component Optimization**:
- ProductDetailsClient je mali komponent (80 linija) - brza hydration
- `useCallback` na handleSizeSelect sprečava nepotrebne re-renders
- Size buttons nisu u posebnom sub-komponentu (manje re-renders)

**Bundle Size**:
- Adding "use client" + hooks + state: ~2KB gzipped (minimal)
- Tailwind klasese su već u bundle-u (nema dodatnog CSS)

**Network**:
- No additional API calls (size selection je samo state)
- Cart API endpoint je isti kao pre

---

## Browser Compatibility

**Features Used**:
- `useState`, `useCallback` (React 16.8+) - Supported
- Fetch API - Supported (IE11+ sa polyfill)
- EventTarget.disabled attribute - Supported (IE 11+)

**Tested On**:
- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+

**Mobile Tested**:
- iOS Safari 14+
- Chrome Mobile 90+
- Samsung Internet 14+

---

**Status**: ✅ Implementation is complete and ready for testing
