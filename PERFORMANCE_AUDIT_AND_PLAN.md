# Trendplus Performance Audit And Optimization Plan

Date: 2026-04-09

## Executive Summary

TrendplusProdavnica vec ima dobru performance osnovu:

- PostgreSQL ostaje source of truth
- kljucni read indeksi za PLP postoje
- EF read servisi uglavnom koriste `AsNoTracking`, projekcije i pagination
- FusionCache je uveden za hot read use-case-ove
- javni API GET endpoint-i koriste Output Cache
- Next.js storefront koristi server rendering, `revalidate`, `next/image` i Cloudflare edge strategiju

To znaci da nismo na "greenfield" pocetku. Glavni posao sada nije uvodjenje novih tehnologija, vec disciplinovano merenje, uklanjanje preostalog hydration/payload troska i dodatno suzavanje najskupljih query pattern-a.

## Target Metrics

Primary targets:

- TTFB `< 200ms` za cache hit javne read rute
- LCP `< 2s` za home, PLP i PDP na mobilnom web-u

Pragmaticno tumacenje:

- origin uncached TTFB ne mora uvek biti ispod 200ms
- edge-cache hit TTFB treba da bude blizu tog cilja
- LCP target zahteva istovremeno backend, image, frontend i CDN disciplinu

## Audit Summary

### 1. Database

Status: `Mostly complete`

Already present:

- product slug unique index: `ux_products_slug`
- product brand index: `ix_products_brand_id`
- product primary category index: `ix_products_primary_category_id`
- variant price index: `ix_product_variants_price`
- migration created for PLP read indexes

References:

- `TrendplusProdavnica.Infrastructure/Persistence/Configurations/ProductConfiguration.cs`
- `TrendplusProdavnica.Infrastructure/Persistence/Configurations/ProductVariantConfiguration.cs`
- `TrendplusProdavnica.Infrastructure/Migrations/20260409103636_AddPlpReadIndexes.cs`

Assessment:

- trazeni indeksi iz zadatka su vec uvedeni
- za PLP je ovo dobar minimum
- sledeci nivo optimizacije nije u osnovnim indeksima, vec u parcijalnim i kompozitnim indeksima za najcesce filtere i visibility uslove

Recommended next-level indexes after measurement:

- partial index za published/visible/purchasable products
- partial index za aktivne i vidljive varijante sa cenom i stanjem zaliha
- eventualni kompozitni indeks za sort po `SortRank` i `PublishedAtUtc` ako se pokaze kao hotspot

### 2. Query Layer

Status: `Good baseline, needs hotspot verification`

Observed strengths:

- query servisi koriste `AsNoTracking`
- hot read servisi koriste `Select` projekcije umesto sirovih `Include` lanaca
- listing koristi `Skip/Take` pagination
- PLP read servis kesira finalni DTO odgovor umesto niskog nivoa query fragmenata

Strong examples:

- `TrendplusProdavnica.Infrastructure/Persistence/Queries/Catalog/ProductListingReadService.cs`
- `TrendplusProdavnica.Infrastructure/Persistence/Queries/Catalog/ProductDetailQueryService.cs`
- `TrendplusProdavnica.Infrastructure/Persistence/Queries/Content/HomePageQueryService.cs`

Assessment:

- arhitektonski pravac je dobar
- najveci rizik nije "EF je spor", nego shape pojedinih PLP/PDP subquery-ja pri vecem obimu podataka
- potrebno je merenje sa `EXPLAIN ANALYZE` i `pg_stat_statements` pre dodatnog tuning-a

Specific observation:

- `ProductListingReadService` radi korektnu projekciju i pagination, ali po kartici racuna minimum cenu, staru cenu, broj velicina i slike kroz korelisane podupite
- to je prihvatljivo za stranu od 24 proizvoda, ali treba benchmark na realnom volumenu

### 3. Application Cache

Status: `Strong`

Observed strengths:

- FusionCache je registrovan kao glavni application cache
- Redis L2 i backplane su podrzani u infrastrukturi
- PLP read servis ima kratki TTL, eager refresh i fail-safe
- webshop cache abstraction postoji

References:

- `TrendplusProdavnica.Infrastructure/DependencyInjection/InfrastructureServiceCollectionExtensions.cs`
- `TrendplusProdavnica.Infrastructure/Caching/WebshopCache.cs`
- `TrendplusProdavnica.Infrastructure/Persistence/Queries/Catalog/ProductListingReadService.cs`

Assessment:

- cache smer je dobar i production-oriented
- naredni fokus treba da bude precizno merenje hit ratio-a i invalidacije, ne nova cache tehnologija

### 4. Public HTTP Caching

Status: `Strong`

Observed strengths:

- ASP.NET Output Cache postoji za javne GET endpoint-e
- custom middleware postavlja `Cache-Control` za public read API rute
- listing, product detail i entity pages imaju definisane output cache politike

References:

- `TrendplusProdavnica.Api/Program.cs`
- `TrendplusProdavnica.Api/Infrastructure/Middleware/PublicCacheHeadersMiddleware.cs`

Assessment:

- ovo znacajno pomaze TTFB na repeat request-ovima
- bitno je validirati da header-i i Cloudflare pravila ostanu uskladjeni posle svakog deploy-a

### 5. Frontend / Next.js

Status: `Good foundation, biggest remaining opportunity`

Observed strengths:

- App Router stranice su server-rendered
- kljucne javne stranice koriste `revalidate`
- koristi se `next/image`
- static assets imaju dug cache i CDN asset host
- image resizing ide kroz Cloudflare

References:

- `TrendplusProdavnica.Web/src/app/page.tsx`
- `TrendplusProdavnica.Web/src/app/[categorySlug]/page.tsx`
- `TrendplusProdavnica.Web/src/app/proizvod/[slug]/page.tsx`
- `TrendplusProdavnica.Web/next.config.js`
- `TrendplusProdavnica.Web/src/lib/cdn/cloudflare-image-loader.ts`
- `TrendplusProdavnica.Web/middleware.ts`

Important finding:

- `ProductCard` je `use client` komponenta
- to znaci da se ceo PLP grid hidrira na klijentu iako je velika vecina kartice staticka

Reference:

- `TrendplusProdavnica.Web/src/components/product-card.tsx`

Assessment:

- ovo je najverovatnije najveci sledeci frontend performance dobitak
- cilj treba da bude da kartica ostane server komponenta, a da se eventualni interaktivni delovi izdvoje u male client island-e

### 6. CDN / Edge

Status: `Strong`

Observed strengths:

- Cloudflare-first CDN strategija je dokumentovana
- asset host i image resizing postoje
- edge cache pravila za home i PLP su definisana
- private rute su `no-store`

References:

- `TrendplusProdavnica.Web/CDN_STRATEGY.md`
- `TrendplusProdavnica.Web/middleware.ts`
- `TrendplusProdavnica.Web/next.config.js`

Assessment:

- CDN sloj je dovoljno dobar da podrzi TTFB target na cache hit scenarijima
- glavni posao sada je staging/prod verifikacija `cf-cache-status`, `age`, `cache-control` i asset host upotrebe

## Main Risks

### 1. Missing measurement loop

Trenutno je najveca rupa observability, ne tehnologija.

Need next:

- `pg_stat_statements`
- `EXPLAIN ANALYZE` za home, PLP i PDP query-je
- server timing / request duration logging za hot API rute
- Web Vitals pracenje za `LCP`, `INP`, `CLS`

### 2. PLP hydration cost

`ProductCard` kao client komponenta verovatno trosi vise JS-a i hydration vremena nego sto je potrebno.

### 3. Query duplication / hot-path divergence

U repou postoje i `ProductListingQueryService` i `ProductListingReadService`.

To nije samo stilisticki problem.

Rizik:

- dva listing puta mogu vremenom divergovati po performansama, filterima i mapping-u
- audit i tuning postaju skuplji jer treba meriti oba puta

### 4. Need for real data benchmarking

Bez realisticnog volumena proizvoda, varijanti i media zapisa, optimization lako ode u pogresnom smeru.

## Optimization Plan

### Phase 1: Measure and lock baselines

Priority: `P0`

Goals:

- izmeriti stvarne bottleneck-e pre dodatnog tuninga
- postaviti objective baseline za TTFB i LCP

Actions:

1. ukljuciti `pg_stat_statements` na PostgreSQL instanci
2. pokrenuti `EXPLAIN (ANALYZE, BUFFERS)` za:
   - home page query
   - PLP query page 1
   - PDP query
3. dodati request timing logovanje za:
   - `/api/pages/home`
   - `/api/catalog/products`
   - `/api/catalog/product/{slug}`
   - `/api/listings/*`
4. ukljuciti frontend Web Vitals merenje za:
   - home
   - category PLP
   - PDP
5. dokumentovati baseline brojke za:
   - uncached origin TTFB
   - API output cache hit TTFB
   - Cloudflare edge hit TTFB
   - LCP mobile 4G

Expected result:

- jasan spisak top 3 bottleneck-a sa brojevima

### Phase 2: Remove unnecessary client hydration

Priority: `P0`

Goals:

- smanjiti JS payload i hydration trosak na PLP-u
- poboljsati LCP i responsiveness listing stranica

Actions:

1. refaktorisati `ProductCard` u server komponentu
2. izdvojiti eventualne interaktivne delove u male client island-e
3. ukloniti `useState` skeleton logiku sa cele kartice ako nije neophodna
4. zadrzati hover efekte u CSS-u gde je moguce
5. proveriti da below-the-fold slike ostanu lazy putem `next/image`

Expected result:

- manji JS bundle na PLP-u
- manje hydration rada na initial render-u
- bolji LCP i INP

### Phase 3: Query hotspot tuning

Priority: `P1`

Goals:

- dodatno sniziti origin vreme za PLP i PDP

Actions:

1. benchmark-ovati `ProductListingReadService` sa realnim volumenom
2. proveriti da li korelisani podupiti za cenu/slike/velicine traze dodatne indekse
3. po potrebi dodati partial index-e za:
   - published + visible + purchasable producte
   - aktivne/vidljive/in-stock varijante
4. po potrebi prelomiti deo agregacija u precomputed read model ili view samo ako merenje pokaze problem
5. proveriti da li facet query treba razdvojiti ili pojednostaviti pri velikim listing kombinacijama

Expected result:

- stabilniji uncached PLP response time pod vecim opterecenjem

### Phase 4: Cache hit ratio optimization

Priority: `P1`

Goals:

- povecati broj request-ova koji se sluze iz FusionCache / Output Cache / edge cache-a

Actions:

1. pratiti FusionCache hit ratio za home, PDP i PLP
2. potvrditi da su cache key-jevi stabilni i da query param ordering ne pravi cache fragmentation
3. meriti Output Cache hit ratio za listing endpoint-e
4. potvrditi Cloudflare HIT za:
   - `/`
   - category PLP
   - brand PLP
   - collection PLP
5. zadrzati kratak TTL za listing, srednji TTL za PDP/home/detail strane

Expected result:

- TTFB `< 200ms` na vecini repeat public request-ova

### Phase 5: Frontend media discipline

Priority: `P1`

Goals:

- spustiti LCP ispod 2 sekunde na mobilnom sajtu

Actions:

1. za hero i above-the-fold slike koristiti eksplicitno dimenzionisane `next/image` konfiguracije
2. koristiti `priority` samo za jednu do dve stvarno LCP-kljucne slike po stranici
3. proveriti da listing kartice ne dobijaju `priority`
4. zadrzati `sizes` atribute uskladjene sa realnim grid layout-om
5. validirati da Cloudflare image resizing vraca optimalne dimenzije po viewport-u

Expected result:

- nizi image bytes
- brzi LCP i manje layout shift-a

### Phase 6: Simplify hot-path architecture

Priority: `P2`

Goals:

- smanjiti dugorocni performance drift

Actions:

1. odabrati jedan primarni PLP hot-path servis
2. jasno dokumentovati koji listing servis koristi storefront/API hot path
3. spreciti paralelno odrzavanje dva skoro ista listing engine-a osim ako postoji stvarna potreba

Expected result:

- jednostavniji tuning
- manje regresija

## Concrete Recommendations By Layer

### Database

Do now:

- zadrzati postojece indekse
- potvrditi da je migracija primenjena u svim okruzenjima

Do next:

- meriti `EXPLAIN ANALYZE`
- dodavati samo one partial/composite indekse koje merenje opravda

### Backend Query Services

Do now:

- zadrzati projekcije, `AsNoTracking` i pagination kao standard
- kesirati finalne DTO rezultate na read use-case nivou

Do next:

- uvesti telemetry za duration i DB query count po hot ruti

### FusionCache

Do now:

- zadrzati kratki TTL za PLP
- fail-safe i eager refresh su dobar izbor

Do next:

- pratiti hit ratio i invalidation noise

### Frontend

Do now:

- zadrzati server components kao default
- zadrzati `next/image` i CDN loader

Do next:

- vratiti PLP kartice sto vise na server komponentu
- izbegavati nove velike client wrapper-e na listing i PDP kriticnim sekcijama

### CDN

Do now:

- zadrzati edge cache za home i PLP
- zadrzati immutable static asset policy

Do next:

- staging/prod proveriti `cf-cache-status` i `age`
- validirati realan edge hit ratio na glavnim landing stranicama

## Suggested Delivery Order

1. measurement baseline
2. PLP hydration reduction
3. query hotspot tuning after `EXPLAIN`
4. cache hit ratio tuning
5. media/LCP fine-tuning
6. architecture cleanup of duplicated listing hot paths

## Expected Outcome

Ako se plan odradi ovim redom, najrealniji rezultat je:

- vrlo dobar TTFB na cache hit javnim rutama
- znacajno bolji PLP rendering cost na frontendu
- stabilniji origin response times pod vecim opterecenjem
- realna sansa da home i PLP udju u `LCP < 2s`, uz dobar CDN hit ratio i disciplinovane LCP slike
