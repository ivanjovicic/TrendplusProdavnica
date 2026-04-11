# Design System & Guidelines

Premium ecommerce storefront design system inspirisan sa ALOHAS, NAKED Copenhagen, On, i Axel Arigato.

## 🎨 Design Tokens

### Typography

```
Font Family: System Stack
  - macOS/iOS: -apple-system, BlinkMacSystemFont, "Segoe UI"
  - Android: "Roboto", sans-serif
  - Windows: "Segoe UI", sans-serif
  - Fallback: sans-serif

Font Weights:
  - Light (300): Headings, large text
  - Regular (400): Body text
  - Medium (500): Emphasis, labels
  - Bold (700): Never (too heavy for premium feel)

Line Heights:
  - Tight (1.1): Headings
  - Snug (1.375): Subheadings
  - Normal (1.5): Body text
  - Relaxed (1.625): Long content
  - Loose (2): Description text
```

### Color Palette

```
Grayscale:
  - 50: #fafafa      (backgrounds)
  - 100: #f5f5f5     (light backgrounds)
  - 200: #e5e5e5     (borders light)
  - 300: #d4d4d4     (borders)
  - 400: #a3a3a3     (secondary text)
  - 500: #737373     (muted text)
  - 600: #525252     (text)
  - 700: #404040     (dark text)
  - 800: #262626     (dark text)
  - 900: #171717     (black, primary)

Accents:
  - Sale: #dc2626    (red)
  - New: #2563eb     (blue)
  - Success: #16a34a (green)

Usage:
  - Text: gray-900 (primary), gray-600 (secondary), gray-500 (tertiary)
  - Borders: gray-200 (light), gray-300 (normal), gray-900 (strong)
  - Backgrounds: white (primary), gray-50 (secondary)
```

### Spacing Scale

```
Base: 4px

Spacing:
  2 = 8px    (tight)
  3 = 12px   (compact)
  4 = 16px   (default)
  6 = 24px   (comfortable)
  8 = 32px   (relaxed)
  12 = 48px  (generous)
  16 = 64px  (spacious)
  20 = 80px  (very spacious)
  24 = 96px  (extra spacious)
  32 = 128px (massive)

Recommendations:
  - Between elements: 4, 6, 8 (px-4, px-6, px-8)
  - Section padding: py-12, py-16, py-24
  - Grid gaps: gap-4, gap-6, gap-8
  - Text spacing: mb-2, mb-4, mb-8
```

### Border Radius

```
Minimal:
  - No radius (default)
  - sm: 0.125rem (2px) - rarely used
  - md: 0.375rem (6px) - only for specific cases

Philosophy: Clean lines, no rounded corners unless needed
```

### Shadows

```
None or very subtle for premium feel
- No heavy shadows
- Minimal elevation
- Use borders instead when needed
```

### Breakpoints

```
Mobile: < 640px (sm)
Tablet: >= 640px (sm)
Desktop: >= 768px (md)
Large: >= 1024px (lg)
Extra: >= 1280px (xl)

Grid Columns:
- Mobile: 2 kolone
- Tablet: 3 kolone (sa adjustments)
- Desktop: 4 kolone
- Full: 6 kolone (brand wall)
```

## 🎯 Component Guidelines

### ProductCard

```
Layout:
  - Image: 1:1 aspect ratio
  - Space below image: mb-4
  - Brand: text-xs, uppercase, tracking-widest
  - Name: text-base or text-lg
  - Price: text-sm, bold, with old price line-through
  - Badges: top-right corner, minimal styling

Hover State:
  - Image opacity decrease
  - Subtle scale on image
  - No other changes (minimize movement)

Spacing:
  - Container: 2 kolone mobile, 3 tablet, 4 desktop
  - Gap: 6 (24px) or 8 (32px)
```

### HeroSection

```
Structure:
  - Subtitle (eyebrow): text-xs, uppercase, tracking-widest
  - Title: text-5xl/text-6xl, font-light
  - Description: text-lg, text-gray-600
  - CTA: minimal button, text-sm, uppercase, thin border

No Background Image: Typography-first approach

Alignment:
  - Center: default (for most pages)
  - Left: specific use cases
  - Right: rare

Max Width:
  - sm (max-w-md): very narrow
  - md (max-w-2xl): narrow
  - lg (max-w-4xl): wide (default)
```

### Buttons

```
Primary Button:
  - Border: border-gray-900
  - Text: text-sm, uppercase, tracking-wide
  - Padding: px-8, py-3
  - Hover: bg-gray-900, text-white
  - Transition: smooth 200ms

Secondary Button:
  - Border: border-gray-300
  - Text: text-sm
  - Hover: border-gray-900
  - Transition: smooth 200ms

Disabled:
  - opacity-50
  - cursor-not-allowed
```

### Navigation

```
Header:
  - Sticky position: z-50
  - Height: 64px (h-16)
  - Font weight: light (logo)
  - Spacing: gap-8 (desktop)

Footer:
  - Generous padding: py-16/py-20
  - Multiple columns: md:grid-cols-4
  - Newsletter signup: bottom section

Mobile Menu:
  - Dropdown, no modal
  - Border-top separator
  - Simple animation (slide/fade)
```

## 📐 Layout Patterns

### Single Column (Full Width)

```html
<Container>
  <div className="max-w-2xl">
    {children}
  </div>
</Container>
```

### Two Column (50/50)

```html
<Section>
  <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
    <div>{left}</div>
    <div>{right}</div>
  </div>
</Section>
```

### Three Column Grid

```html
<Section>
  <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
    {items.map(item => <>{item}</>)}
  </div>
</Section>
```

### Featured + Grid

```html
<Section>
  <div className="grid grid-cols-1 md:grid-cols-2 gap-12">
    <div>{featured}</div>
    <div className="grid grid-cols-2 gap-6">
      {rest.map(item => <>{item}</>)}
    </div>
  </div>
</Section>
```

### Hero + Below Content

```html
<HeroSection ... />
<Section spacingTop="lg">
  {content}
</Section>
```

## 🎭 Visual Hierarchy

### Text Sizes

```
Display (Hero): 48-64px, font-light
  - Large headings
  - Page titles

Heading 1: 36-48px, font-light
  - Section headings

Heading 2: 24-32px, font-light
  - Subsection headings

Heading 3: 20-24px, font-medium
  - Card titles, highlights

Body: 16px, font-regular
  - Main content

Small: 14px, font-regular
  - Secondary content

Caption: 12px, font-regular
  - Fine print, labels
```

### Weight Hierarchy

```
Light (300): Large headings, premium feel
Regular (400): Body text, default
Medium (500): Emphasis, labels, CTAs
Bold (700): NEVER (too heavy)

Never use bold for premium feel - use size/color instead
```

### Color Hierarchy

```
Primary: gray-900 (headlines, primary actions)
Secondary: gray-600 (body text, secondary content)
Tertiary: gray-500 (labels, fine print)
Disabled: gray-400 (inactive states)

Accents:
  - Red (#dc2626): Sale/urgency
  - Blue (#2563eb): New/featured
```

## 🔄 Interaction States

### Links

```
Default: text-gray-900, underline (optional)
Hover: text-gray-600, underline
Active: text-gray-700
Visited: text-gray-600 (optional)

Transition: all 200ms ease-in-out
```

### Buttons

```
Default: border-gray-900, text-gray-900
Hover: bg-gray-900, text-white
Active: bg-gray-800
Disabled: opacity-50, cursor-not-allowed
Focus: ring (for accessibility)

Transition: all 200ms ease-in-out
```

### Cards

```
Default: subtle border-gray-200
Hover: darker border-gray-300 or border-gray-900
Active: border-gray-900, small shadow

Transition: all 200ms ease-in-out
```

## 📱 Responsive Guidelines

### Images

```
- Hero images: 16:9 aspect ratio
- Product images: 1:1 aspect ratio
- Category images: 1:1 or 4:3
- Editorial images: 4:3 or 3:2
- Always use Next.js Image component
- Always use sizes prop for responsive images
- Lazy load by default (priority={false})
```

### Text

```
- Base size: 16px
- Line height: 1.5 (body), 1.2 (headings)
- Never go below 14px (12px only for caption)
- Increase heading size on larger screens
- Maintain readability on mobile
```

### Grid Columns

```
Mobile: 2 columns
Tablet: 3 columns (some 2)
Desktop: 4 columns (some 3, 6 for brands)
Max containers: 1280px
```

### Spacing

```
Mobile: tighter (gap-4, py-8)
Tablet: moderate (gap-6, py-12)
Desktop: relaxed (gap-8, py-16)

Padding: px-4 mobile, px-6 tablet, px-8 desktop
```

## ✅ QA Checklist

- [ ] All fonts displayed correctly
- [ ] Colors match design spec
- [ ] Spacing consistent with grid
- [ ] Buttons accessible (size, contrast)
- [ ] Images optimized and lazy loaded
- [ ] Mobile responsive (2/3/4 columns)
- [ ] Touch targets min 44px x 44px
- [ ] No horizontal scroll on mobile
- [ ] Links have clear hover state
- [ ] Loading states visible
- [ ] Error states clear
- [ ] Empty states friendl

---

Version: 1.0
Last Updated: April 2026
