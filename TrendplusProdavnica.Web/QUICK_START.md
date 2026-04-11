# Next.js Premium Storefront - Quick Start Guide

## 🚀 Brz početak

### 1. Import Komponenti

```typescript
import {
  Section,
  Container,
  Header,
  Footer,
  HeroSection,
  ProductCard,
  ProductGrid,
  CategoryGrid,
  BrandGrid,
  EditorialBlock,
  StoreLocator,
} from '@/components';
```

### 2. Basic Page Setup

```typescript
import type { Metadata } from 'next';

export async function generateMetadata(): Promise<Metadata> {
  return {
    title: 'Page Title | Trendplus',
    description: 'Page description',
  };
}

export default async function Page() {
  // Fetch data
  const data = await fetch('/api/...');

  return (
    <div>
      {/* Header je u root layout */}
      
      {/* Hero Section - typography first */}
      <HeroSection
        title="Naslov"
        description="Opis"
        cta={{ label: 'CTA', href: '/' }}
      />

      {/* Sekcije sa spacing */}
      <Section spacingTop="lg" spacingBottom="lg">
        <h2 className="text-3xl font-light mb-8">Sekcija 1</h2>
        <ProductGrid products={products} />
      </Section>

      <Section spacingTop="lg" spacingBottom="lg">
        <h2 className="text-3xl font-light mb-8">Sekcija 2</h2>
        <CategoryGrid categories={categories} />
      </Section>

      {/* Footer je u root layout */}
    </div>
  );
}
```

## 📦 Često korišćeni primeri

### ProductCard

```typescript
const product = {
  id: '1',
  name: 'Premium Cipele',
  slug: 'premium-cipele',
  brandName: 'Nike',
  price: 12000,
  oldPrice: 15000,
  primaryImageUrl: '/images/shoes.jpg',
  isNew: true,
  isOnSale: false,
  subtitle: 'Running shoes',
};

<ProductCard product={product} />
```

### ProductGrid

```typescript
// 2 kolone na mobilnom, 3 na tablet-u, 4 na desktop-u
<ProductGrid products={products} />

// Sa custom spacing
<ProductGrid products={products} gap="lg" />

// Sa custom brojem kolona
<ProductGrid products={products} columns={3} />
```

### CategoryGrid

```typescript
const categories = [
  {
    id: '1',
    name: 'Cipele',
    slug: 'cipele',
    image: '/images/category-shoes.jpg',
    productCount: 245,
  },
  // ...
];

<CategoryGrid categories={categories} columns={3} />
```

### BrandGrid

```typescript
const brands = [
  {
    id: '1',
    name: 'Nike',
    slug: 'nike',
    logo: '/logos/nike.svg',
  },
  // ...
];

<BrandGrid brands={brands} columns={6} />
```

### EditorialBlock

```typescript
// Featured layout (prvi item veci, ostali u gridu)
<EditorialBlock 
  items={editorial}
  layout="featured"
/>

// Grid layout (svi isti размер)
<EditorialBlock 
  items={editorial}
  layout="grid"
  columns={3}
/>

// List layout (vertikalni red)
<EditorialBlock 
  items={editorial}
  layout="list"
/>
```

### HeroSection

```typescript
<HeroSection
  subtitle="Premium"
  title="Pronađi svoju stilsku obuću"
  description="Opis sa više detalja..."
  cta={{ label: 'Pregledaj', href: '/cipele' }}
  align="center"
  maxWidth="lg"
/>
```

### StoreLocator

```typescript
// Grid pregled
<StoreLocator stores={stores} layout="grid" />

// List pregled
<StoreLocator stores={stores} layout="list" />
```

### Section

```typescript
// Default spacing
<Section>
  {children}
</Section>

// Custom spacing
<Section spacingTop="xl" spacingBottom="sm">
  {children}
</Section>

// Custom max width
<Section maxWidth="sm">
  {children}
</Section>
```

## 🎯 Common Layouts

### Sekcija sa naslovom i grid-om

```typescript
<Section spacingTop="lg" spacingBottom="lg">
  <div>
    <h2 className="text-3xl md:text-4xl font-light text-gray-900 mb-2">
      Naslov
    </h2>
    <p className="text-gray-600 mb-12">
      Opis sekcije sa dodatnim informacijama.
    </p>
    <ProductGrid products={products} />
  </div>
</Section>
```

### Dva reda sa različitim sadržajem

```typescript
<Section spacingTop="lg" spacingBottom="lg">
  <h2 className="text-3xl font-light mb-12">Brendovi</h2>
  <BrandGrid brands={brands} columns={6} />
</Section>

<Section spacingTop="lg" spacingBottom="lg">
  <h2 className="text-3xl font-light mb-12">Popularne kategorije</h2>
  <CategoryGrid categories={categories} columns={4} />
</Section>
```

### Featured sekcija

```typescript
<Section spacingTop="lg" spacingBottom="lg">
  <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
    {/* Leva strana */}
    <div>
      <h2 className="text-4xl font-light mb-4">Featured</h2>
      <p className="text-gray-600 text-lg mb-8">Opis...</p>
      <Link href="/..." className="inline-block...">
        Pročitaj više
      </Link>
    </div>

    {/* Desna strana - grid ili image */}
    <ProductGrid products={products} columns={2} />
  </div>
</Section>
```

### Sa inline CTA-om

```typescript
<Section spacingTop="lg" spacingBottom="lg">
  <div className="text-center">
    <h2 className="text-3xl font-light mb-4">Naslov</h2>
    <p className="text-gray-600 mb-8">Opis</p>
    <Link
      href={ "/..." }
      className="inline-block px-8 py-3 border border-gray-900 text-gray-900 hover:bg-gray-900 hover:text-white transition-colors"
    >
      CTA Label
    </Link>
  </div>
</Section>
```

## 🎨 Tailwind Classes

### Typography

```typescript
// Headings
className="text-4xl md:text-5xl font-light leading-tight"
className="text-3xl md:text-4xl font-light"
className="text-xl md:text-2xl font-medium"

// Body text
className="text-base text-gray-900"
className="text-sm text-gray-600"
className="text-xs text-gray-500 uppercase tracking-widest"

// Lists
className="space-y-2"      // gap između items
className="space-y-4"
className="space-y-8"
```

### Spacing

```typescript
// Padding
className="p-4"     // 16px
className="px-6"    // 24px horizontal
className="py-8"    // 32px vertical

// Margins
className="mb-4"    // 16px bottom
className="mt-8"    // 32px top
className="gap-6"   // 24px gap između flex/grid items

// Responsive
className="py-8 md:py-12 lg:py-16"
className="px-4 sm:px-6 lg:px-8"
```

### Colors

```typescript
// Text colors
className="text-gray-900"  // Black
className="text-gray-600"  // Dark gray
className="text-gray-500"  // Medium gray

// Background
className="bg-gray-50"     // Light gray
className="bg-white"       // White
className="bg-gray-900"    // Black

// Borders
className="border-gray-200"
className="border-gray-300"
className="border-gray-900"
```

### Grid

```typescript
// Responsive grid
className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4"

// With gap
className="grid grid-cols-2 md:grid-cols-3 gap-4 md:gap-6"

// Flex
className="flex gap-4"
className="flex flex-col gap-2"
className="flex justify-between items-center"
```

### Hover & Transitions

```typescript
className="hover:text-gray-600 transition-colors"
className="hover:scale-105 transition-transform duration-300"
className="group-hover:opacity-75"
```

## 📋 Types

```typescript
// Product
interface ProductCardDto {
  id: string;
  name: string;
  slug: string;
  brandName: string;
  price: number;
  oldPrice?: number;
  primaryImageUrl?: string;
  subtitle?: string;
  isNew?: boolean;
  isOnSale?: boolean;
}

// Category
interface CategoryItem {
  id: string;
  name: string;
  slug: string;
  image?: string;
  productCount?: number;
}

// Brand
interface BrandItem {
  id: string;
  name: string;
  slug: string;
  logo?: string;
}

// Editorial
interface EditorialBlockItem {
  id: string;
  title: string;
  slug: string;
  subtitle?: string;
  excerpt?: string;
  image?: string;
  publishedAt?: string;
}

// Store
interface StoreLocation {
  id: string;
  name: string;
  slug: string;
  address: string;
  city: string;
  phone?: string;
  email?: string;
  hours?: {
    monday?: string;
    tuesday?: string;
    // ...
  };
}
```

## 🔍 SEO

### generateMetadata

```typescript
export async function generateMetadata(
  props: PageProps
): Promise<Metadata> {
  const params = await props.params;

  return {
    title: 'Page Title | Trendplus',
    description: 'Meta description',
    openGraph: {
      title: 'OG Title',
      description: 'OG Description',
      images: [{ url: '/image.jpg' }],
    },
  };
}
```

### Structured Data

```typescript
// U component-u, dodaj JSON-LD
<script
  type="application/ld+json"
  dangerouslySetInnerHTML={{
    __html: JSON.stringify({
      '@context': 'https://schema.org',
      '@type': 'Product',
      name: product.name,
      price: product.price,
      // ...
    }),
  }}
/>
```

## 🚨 Common Mistakes

❌ Ne koristi `sm:grid-cols-1` - mobile je default
```typescript
// Wrong
className="sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4"

// Right
className="grid-cols-2 md:grid-cols-3 lg:grid-cols-4"
```

❌ Ne zaboravi `priority={false}` za lazy loading
```typescript
// Images automatski će biti lazy loaded
// Za hero images, koristi priority={true}
```

❌ Ne miksaj responsive utilities
```typescript
// Wrong
className="w-1/2 md:w-1/3"  // neusaglašeni breakpoints

// Right
className="grid-cols-2 md:grid-cols-3"  // consistent
```

✅ Always use `Section` wrapper
```typescript
// Right
<Section spacingTop="lg">
  {children}
</Section>
```

---

Za detalje, vidi `STOREFRONT_ARCHITECTURE.md`
