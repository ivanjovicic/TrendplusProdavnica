# pg_stat_statements Runbook

Date: 2026-04-09

## Goal

Uvesti pouzdano merenje PostgreSQL hot query pattern-a pre daljeg query tuning-a.

Ovo je posebno vazno za:

- home page read path
- PLP count + page query
- PDP detail, variants, media i reviews
- search/listing query-je kada opterecenje poraste

## Why This Matters

Bez `pg_stat_statements` lako optimizujemo "napamet".

Sa njim dobijamo:

- koje query forme zaista trose najvise ukupnog vremena
- koji query ima los `mean execution time`
- da li je problem u velikom broju poziva ili u sporim pojedinacnim pozivima

## 1. Enable Extension

### postgresql.conf

Na PostgreSQL serveru dodati ili potvrditi:

```conf
shared_preload_libraries = 'pg_stat_statements'
pg_stat_statements.max = 10000
pg_stat_statements.track = all
pg_stat_statements.save = on
```

Ako vec postoji drugi preload library, samo dodaj `pg_stat_statements` u listu.

Primer:

```conf
shared_preload_libraries = 'pg_stat_statements,auto_explain'
```

### restart servera

Posle izmene `shared_preload_libraries` potreban je restart PostgreSQL instance.

## 2. Create Extension In Database

Za svaku bazu koju merimo:

```sql
CREATE EXTENSION IF NOT EXISTS pg_stat_statements;
```

To tipicno radimo najmanje na:

- dev
- staging
- production

## 3. Basic Health Check

```sql
SELECT *
FROM pg_extension
WHERE extname = 'pg_stat_statements';
```

```sql
SELECT count(*) AS statement_count
FROM pg_stat_statements;
```

Ako je broj `0`, jos nema dovoljno workload-a ili extension nije pravilno aktiviran.

## 4. Collect Top Statements

Koristi pripremljenu skriptu:

```powershell
psql "<POSTGRES_CONNECTION_STRING>" -f ".\\scripts\\performance\\pg-stat-statements-top.sql"
```

Ona vraca:

- top query-je po `total_exec_time`
- top query-je po `mean_exec_time`

## 5. Reset Window Before Focused Test

Za cisto merenje pre i posle jedne optimizacije:

```sql
SELECT pg_stat_statements_reset();
```

Zatim:

1. pokreni ciljane request-e
2. sacuvaj `pg_stat_statements` output
3. uporedi sa prethodnim snimkom

## 6. Recommended Test Windows

### Home baseline

Pokreni 20-50 request-ova na:

- `/api/pages/home`
- storefront `/`

### PLP baseline

Pokreni 20-50 request-ova na:

- `/api/catalog/products?category=cipele&page=1&pageSize=24`
- `/api/listings/category/{slug}`
- storefront `/cipele`

### PDP baseline

Pokreni 20-50 request-ova na:

- `/api/catalog/product/{slug}`
- storefront `/proizvod/{slug}`

## 7. What To Look For

### Red flags

- visok `mean_exec_time` na PLP count query-ju
- visok `shared_blks_read` umesto `shared_blks_hit`
- cesti temp blokovi
- previse slicnih listing query formi sa malim razlikama u filterima

### Good signs

- home, PLP i PDP hot query-ji se vide jasno i pregledno
- `calls` i `total_exec_time` su u skladu sa realnim prometom
- posle optimizacije pada `mean_exec_time` ili `total_exec_time`

## 8. Production-Safe Workflow

Preporuceni redosled:

1. prvo ukljuciti na staging
2. proveriti da overhead bude prihvatljiv
3. zatim ukljuciti na production
4. reset koristiti samo planski i kada je tim usaglasen

## 9. Trendplus-Specific Review Focus

Za ovaj projekat prvo gledati:

1. PLP count query
2. PLP page query sa agregacijom cene/slika/velicina
3. PDP variants query
4. PDP media query
5. home page module/product rail query-je

## 10. Pairing With EXPLAIN ANALYZE

`pg_stat_statements` ti kaze *sta* je skupo.

`EXPLAIN ANALYZE` ti kaze *zasto* je skupo.

Najbolji workflow je:

1. nadji query u `pg_stat_statements`
2. reprodukuj ga kroz `EXPLAIN (ANALYZE, BUFFERS)`
3. tek onda menjaj indeks ili query shape
