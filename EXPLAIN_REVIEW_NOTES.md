# EXPLAIN Review Notes

Date: 2026-04-09
Database: `trendplus_prodavnica_dev`

## Scope

U ovom krugu je uradjeno sledece:

- lokalna development baza je stvarno napunjena seed podacima
- pokrenut je stvaran `EXPLAIN (ANALYZE, BUFFERS)` review nad hot read putanjama
- fokus je bio na `home`, `PLP` i `PDP`

## Seed Snapshot

Posle development seeda lokalna baza ima:

- `products=24`
- `variants=120`
- `media=100`
- `reviews=81`
- `brands=5`
- `collections=6`
- `stores=3`
- `editorial=5`
- `trust_pages=6`

Napomena:

- ovo je koristan development sample za pravi review
- i dalje je mali dataset, pa planner prirodno bira `Seq Scan` na tabelama koje su jos uvek vrlo male

## Fixtures Used By Review

Skripta je rezolovala sledece fixture vrednosti iz baze:

- `category_slug = cipele`
- `brand_slug = tamaris`
- `collection_slug = novo`
- `product_slug = tamaris-air-lite-lifestyle`

## Measured Results

### 01. Category lookup by slug

- plan: `Seq Scan on catalog.categories`
- execution time: `0.036 ms`
- finding: potpuno ocekivano za tabelu od 10 redova

### 02. Brand lookup by slug

- plan: `Seq Scan on catalog.brands`
- execution time: `0.023 ms`
- finding: potpuno ocekivano za tabelu od 5 redova

### 03. Collection lookup by slug

- plan: `Seq Scan on catalog.collections`
- execution time: `0.033 ms`
- finding: potpuno ocekivano za tabelu od 6 redova

### 04. Home page root query

- originalna benchmark skripta je ovde imala gresku: pokusavala je da cita nepostojecu kolonu `"Seo"`
- ispravljeno na flattenovana polja `Seo_SeoTitle` i `Seo_SeoDescription`

### 05. Home page new arrivals rail

- plan: `Seq Scan on catalog.products` + mali `Sort`
- execution time: `0.055 ms`
- finding: na seed dataset-u veoma brzo; na vecem katalogu treba pratiti da li parcijalni "live/new" indeks ostaje dovoljan

### 06. PLP total count for category scope

- plan:
  - `Seq Scan on catalog.products`
  - `Seq Scan on catalog.categories`
  - `Index Only Scan` na `PK_product_category_map`
  - `Index Scan` na `ix_product_variant_product_id`
- execution time: `0.732 ms`
- finding:
  - shape query-ja je zdrav
  - `product_category_map` i `product_variants` vec koriste indeks
  - glavni `Seq Scan` nad `products` je posledica malog broja redova, ne ocigledan problem sam po sebi

### 07. PLP page query with price, media and size aggregates

- plan:
  - `Seq Scan on catalog.products`
  - `Seq Scan on catalog.categories`
  - `Index Only Scan` na `PK_product_category_map`
  - `Index Scan` na `ix_product_variant_product_id`
  - `Index Scan` na `ix_product_media_product_sort_id`
  - vise korelisanih agregata nad `product_variants`
- execution time: `2.246 ms`
- finding:
  - za seed dataset query je veoma brz
  - najvazniji potencijalni hotspot na realno vecem katalogu nisu osnovni filteri, nego per-product lateral/korelisani agregati:
    - `min(price)`
    - `min(oldPrice)`
    - `primary media`
    - `secondary media`
    - `count(distinct size)`
  - ovo je i dalje razuman pragmatican shape za stranu od 24 proizvoda
  - vredi meriti opet na staging/prod kada broj proizvoda i varijanti poraste

### 08. PDP root query

- plan:
  - `Seq Scan on catalog.products`
  - `Seq Scan on catalog.categories`
  - `Seq Scan on catalog.brands`
- execution time: `0.138 ms`
- finding: odlican rezultat; tiny-table scan je ovde normalan

### 09. PDP variants query

- plan:
  - `Seq Scan on catalog.product_variants`
  - `Seq Scan on catalog.products`
- execution time: `0.095 ms`
- finding:
  - sa 120 varijanti globalni scan je i dalje jeftin
  - kad varijanti bude mnogo vise, treba proveriti da li se vise isplati produkt-orijentisani indeks sa `SortOrder`

### 10. PDP media query

- plan:
  - `Seq Scan on catalog.product_media`
  - `Seq Scan on catalog.products`
- execution time: `0.089 ms`
- finding:
  - trenutno bez problema
  - ako galerije znacajno porastu, korisno je meriti da li treba dodatni aktivni/media ordering indeks

### 11. PDP rating summary query

- plan:
  - `Seq Scan on catalog.product_ratings`
  - `Seq Scan on catalog.products`
- execution time: `0.048 ms`
- finding: odlican rezultat na ovom volumenu

### 12. PDP published reviews query

- plan:
  - `Seq Scan on catalog.product_reviews`
  - `Seq Scan on catalog.products`
  - mali `Sort`
- execution time: `0.124 ms`
- finding:
  - trenutno odlicno
  - kad review volume poraste, vredi proveriti indeks po `(ProductId, Status, PublishedAtUtc desc)`

## Overall Assessment

Trenutni rezultat na seedovanoj development bazi je veoma dobar:

- `home` hot read je ispod `0.1 ms` na SQL nivou za rail query
- `PLP count` je oko `0.7 ms`
- `PLP page` je oko `2.2 ms`
- `PDP` query-jevi su svi ispod `0.15 ms`

Za development sample ne vidim alarmantan SQL problem.

## Real Findings Worth Acting On

### 1. Benchmark skripta je imala realan bug

- `home_pages` nema kompozitnu kolonu `"Seo"`
- skripta je sada popravljena

### 2. PLP ostaje glavni kandidat za buduce ponovno merenje

Osnovni filteri su zdravi, ali per-card agregati ce prvi osetiti rast:

- cenovni agregati
- media lookup
- broj dostupnih velicina

### 3. Tiny-table `Seq Scan` ovde nije bug

Za:

- `categories`
- `brands`
- `collections`
- `products` sa 24 reda
- `product_variants` sa 120 redova

planner sasvim ispravno bira `Seq Scan`.

To ne treba prerano "leciti" dodatnim indeksima bez novog merenja na vecem volumenu.

## Recommended Next Review

Kad staging/prod baza bude dostupna ili kad development seed jos poraste, sledeci prioritet je:

1. ponoviti istu skriptu na vecem katalogu
2. dodati `pg_stat_statements` snapshot tokom PLP/PDP traffic prozora
3. uporediti da li `PLP page query` ostaje ispod prihvatljive granice pri realnom volumenu

## Conclusion

Na stvarno seedovanoj `trendplus_prodavnica_dev` bazi:

- query shape je zdrav
- indeksi koji vec postoje rade posao gde imaju smisla
- nema jasnog razloga za hitan novi DB indeks samo na osnovu ovog review-a
- glavni fokus za sledeci krug treba da ostane `PLP page query` na vecem volumenu podataka
