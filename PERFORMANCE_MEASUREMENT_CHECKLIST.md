# Performance Measurement Checklist

Date: 2026-04-09

## Goal

Merenje pre i posle optimizacije treba da odgovori na tri pitanja:

1. da li je backend brzi na origin-u
2. da li je cache sloj efikasan na repeat request-ovima
3. da li je storefront brzi za korisnika na mobilnom web-u

## What Was Added For Baseline

Backend:

- `Server-Timing` header za hot read API rute
- structured log za spore hot request-e
- OpenTelemetry/Aspire service defaults hookup

Frontend:

- `WebVitalsReporter` koji opt-in salje `LCP`, `CLS`, `INP`, `FCP`, `TTFB` na `/api/telemetry/web-vitals`
- server-first `ProductCard` bez React state hydration-a

Supporting files:

- [StorefrontPerformanceTelemetryMiddleware.cs](C:\Users\Ivan\source\repos\TrendplusProdavnica\TrendplusProdavnica.Api\Infrastructure\Middleware\StorefrontPerformanceTelemetryMiddleware.cs)
- [product-card.tsx](C:\Users\Ivan\source\repos\TrendplusProdavnica\TrendplusProdavnica.Web\src\components\product-card.tsx)
- [web-vitals-reporter.tsx](C:\Users\Ivan\source\repos\TrendplusProdavnica\TrendplusProdavnica.Web\src\components\web-vitals-reporter.tsx)
- [route.ts](C:\Users\Ivan\source\repos\TrendplusProdavnica\TrendplusProdavnica.Web\src\app\api\telemetry\web-vitals\route.ts)
- [postgres-explain-baseline.sql](C:\Users\Ivan\source\repos\TrendplusProdavnica\scripts\performance\postgres-explain-baseline.sql)
- [measure-ttfb.ps1](C:\Users\Ivan\source\repos\TrendplusProdavnica\scripts\performance\measure-ttfb.ps1)

## Before / After Table

Popuni istu tabelu pre i posle narednih optimizacija.

| Metric | Before | After | Notes |
| --- | --- | --- | --- |
| Home API origin TTFB |  |  | `/api/pages/home` |
| PLP API origin TTFB |  |  | `/api/catalog/products` ili `/api/listings/category/{slug}` |
| PDP API origin TTFB |  |  | `/api/catalog/product/{slug}` |
| Home edge TTFB |  |  | storefront home |
| PLP edge TTFB |  |  | category / brand / collection listing |
| PDP edge TTFB |  |  | product detail |
| Home LCP mobile |  |  | Lighthouse / Web Vitals |
| PLP LCP mobile |  |  | Lighthouse / Web Vitals |
| PDP LCP mobile |  |  | Lighthouse / Web Vitals |

## Step 1: Enable Web Vitals Logging

U `TrendplusProdavnica.Web/.env.local` ili staging env-u:

```env
NEXT_PUBLIC_ENABLE_WEB_VITALS_LOGGING=1
```

Expected result:

- Next server log dobija `[web-vitals]` zapise dok browsujes storefront

## Step 2: Run PostgreSQL EXPLAIN ANALYZE

Ako imas `psql` i connection string:

```powershell
psql "<POSTGRES_CONNECTION_STRING>" -f ".\\scripts\\performance\\postgres-explain-baseline.sql"
```

Sta sacuvati:

- ukupno vreme svakog `EXPLAIN ANALYZE`
- da li plan koristi indekse ili pada na skupe sekvencijalne prolaze
- broj buffers hit/read

Obrati paznju na:

- `PLP total count`
- `PLP page query`
- `PDP variants/media/reviews`

Ako neka sekcija pokazuje znacajno veci cost od drugih, to je sledeci kandidat za tuning.

## Step 3: Measure API TTFB

Primer za lokalno/staging/prod:

```powershell
.\\scripts\\performance\\measure-ttfb.ps1 `
  -HomeUrl "https://localhost:7002/api/pages/home" `
  -PlpUrl "https://localhost:7002/api/catalog/products?category=cipele&page=1&pageSize=24" `
  -PdpUrl "https://localhost:7002/api/catalog/product/unesi-stvarni-slug"
```

Za storefront edge merenje:

```powershell
.\\scripts\\performance\\measure-ttfb.ps1 `
  -HomeUrl "https://www.trendplus.rs/" `
  -PlpUrl "https://www.trendplus.rs/cipele" `
  -PdpUrl "https://www.trendplus.rs/proizvod/unesi-stvarni-slug"
```

Sta gledamo:

- `ttfb`
- `total`
- `server-timing`
- `cache-control`
- `cf-cache-status`
- `age`

Interpretacija:

- pass 1 je cold-ish
- pass 2 je warm/cache scenario
- za storefront, `cf-cache-status: HIT` je vazan indikator da je edge radio posao

## Step 4: Check API Server-Timing

Na hot API GET rutama sada treba da postoji `Server-Timing` header.

Primer:

```powershell
curl.exe -I "https://localhost:7002/api/catalog/products?category=cipele&page=1&pageSize=24"
```

Expected:

- header oblika `Server-Timing: app;desc=\"plp\";dur=...`

To je brz nacin da vizuelno proverimo backend vreme bez dodatnog APM setup-a.

## Step 5: Measure LCP On Mobile

Recommended path:

1. Chrome DevTools
2. Lighthouse
3. mobile preset
4. testirati:
   - home
   - jedna category PLP strana
   - jedan PDP

Alternativno:

1. otvoriti stranicu na staging/prod
2. posmatrati `[web-vitals]` logove iz Next servera
3. zabeleziti `LCP`, `TTFB`, `CLS`, `INP`

Sta gledamo:

- LCP element
- image bytes
- render-blocking assets
- hydration/main-thread work

## Step 6: Compare Before / After

Posle svake optimizacije ponovi ista tri merenja:

1. `EXPLAIN ANALYZE`
2. `TTFB` skriptu
3. `LCP` proveru

Minimalni acceptance signal za ovu P0 turu:

- nema regresije na API TTFB
- PLP nema dodatni client hydration trosak od stare `ProductCard` verzije
- `Server-Timing` i `[web-vitals]` baseline su dostupni za dalje iteracije

## Recommended Next Measurement Sequence

1. izmeri stanje odmah posle ove promene
2. uradi naredni frontend hydration cleanup ako brojke to potvrde
3. zatim radi query tuning iskljucivo na osnovu `EXPLAIN` rezultata
