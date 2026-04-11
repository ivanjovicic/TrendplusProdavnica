# OpenSearch Search Benchmark Runbook

Ovaj runbook je namenjen za brzo merenje:

- direktnog OpenSearch search latency-ja
- faceted search latency-ja sa aggregations
- end-to-end API latency-ja za `/api/search`

Fokus je na lightweight, ponovljivom benchmark-u bez dodatnog alata van PowerShell-a i postojećih skripti u repou.

## Cilj

Za storefront product search cilj je:

- OpenSearch `took` za faceted search: `< 50 ms` na zagrejanom indeksu
- end-to-end `/api/search` TTFB: što bliže `< 200 ms`

## Preduslovi

- OpenSearch indeks je popunjen objavljenim proizvodima
- indeks i mapping odgovaraju aktuelnom `ProductSearchDocument`
- benchmark pokrećemo nad istim okruženjem koje koristi API
- cluster je zagrejan:
  - bar nekoliko warmup upita
  - bez paralelnog reindex-a tokom merenja

## Skripte

- Direktni OpenSearch benchmark:
  - [measure-opensearch-search.ps1](/C:/Users/Ivan/source/repos/TrendplusProdavnica/scripts/performance/measure-opensearch-search.ps1)
- API TTFB benchmark:
  - [measure-ttfb.ps1](/C:/Users/Ivan/source/repos/TrendplusProdavnica/scripts/performance/measure-ttfb.ps1)

## 1. Direktni OpenSearch benchmark

Skripta meri dva scenarija:

- `search-only`
- `search-with-facets`

Za oba scenarija meri:

- HTTP request trajanje
- OpenSearch `took`
- hit count
- timeout count

Na kraju izbacuje:

- `avg`
- `p50`
- `p95`
- prosecan facet overhead u odnosu na plain search

### Lokalni primer

```powershell
.\scripts\performance\measure-opensearch-search.ps1 `
  -OpenSearchUri 'http://localhost:9200' `
  -IndexName 'products' `
  -QueryText 'sandale' `
  -WarmupIterations 5 `
  -Iterations 20
```

### HTTPS / auth primer

```powershell
.\scripts\performance\measure-opensearch-search.ps1 `
  -OpenSearchUri 'https://opensearch-staging.trendplus.rs' `
  -IndexName 'trendplus-products-staging' `
  -Username 'benchmark' `
  -Password '***' `
  -SkipCertificateCheck `
  -QueryText 'salonke' `
  -WarmupIterations 5 `
  -Iterations 25
```

### Sta gledamo

- `search-with-facets` `TookP50Ms`
- `search-with-facets` `TookP95Ms`
- `Facet overhead (avg took)`

Pragmaticna meta za prvo ocenjivanje:

- `TookAvgMs < 50`
- `TookP95Ms < 80`
- facet overhead mali i stabilan izmedju pokretanja

## 2. End-to-end API benchmark

Za API sloj koristi postojeću skriptu:

```powershell
.\scripts\performance\measure-ttfb.ps1 `
  -SearchUrl 'https://localhost:7002/api/search?q=sandale&brands=Tamaris&availability=in_stock'
```

Ako želiš seriju merenja, pokreni skriptu više puta za tipične search kombinacije:

- samo tekst
- tekst + brand + color
- tekst + multi-size + price range
- bez teksta, samo faceti

## 3. Benchmark matrica

Preporučeni minimalni set:

1. `q=sandale`
2. `q=salonke`
3. `q=patike&brands=Tamaris`
4. `q=&availability=in_stock&isOnSale=true`
5. `q=cizme&colors=black&sizes=38&sizes=39`

Ako dataset još nije dovoljno velik, benchmark ponovi i na staging/prod indeksu.

## 4. Kako tumačiti nalaze

Ako je `search-only` brz, a `search-with-facets` spor:

- problem je verovatno u aggregations opterecenju
- proveri cardinality za `brandName.keyword`, `primaryColorName.keyword`, `availableSizes`
- proveri shard count i replica count

Ako su oba spora:

- proveri query DSL
- proveri cluster load, CPU i heap
- proveri da li se benchmark vrti dok traje reindex ili bulk ingest

Ako je OpenSearch brz, a API TTFB spor:

- problem je iznad search engine sloja
- proveri API serialization, networking, TLS i response cache headers

## 5. One-off profile kada latency ode previsoko

Za spor scenario uradi jedan profilisani upit direktno kroz OpenSearch Dev Tools ili HTTP:

- isti query body
- dodat `profile=true`

To ne treba koristiti za prosecan benchmark, samo za dijagnostiku.

## 6. Sta upisati u rezultat benchmark-a

Za svako okruženje zabeleži:

- datum i vreme
- indeks ime
- broj dokumenata
- query tekst
- `search-only` avg / p95
- `search-with-facets` avg / p95
- facet overhead
- `/api/search` TTFB

## 7. Preporučeni workflow

1. potvrdi da je indeks svež i zagrejan
2. pokreni direktni OpenSearch benchmark
3. pokreni API TTFB benchmark za iste query-je
4. uporedi `search-only` i `search-with-facets`
5. ako faceted query probija target, radi jedan `profile=true` pregled
