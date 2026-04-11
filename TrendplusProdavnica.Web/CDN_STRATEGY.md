# CDN Strategy

## Provider

Primarni izbor za Trendplus storefront je **Cloudflare**.

Razlog:

- jedan vendor pokriva edge cache, image resizing, Brotli, HTTP/3 i osnovni WAF
- jednostavan rollout za Next.js storefront + ASP.NET API
- dobar odnos kompleksnosti i performansi za ecommerce sa mnogo slika i PLP/PDP saobracajem

Fastly ostaje validna alternativa, ali za ovu bazu koda Cloudflare je pragmaticniji prvi korak.

## Arhitektura

- `www.trendplus.rs`
  - Next.js storefront HTML
  - Cloudflare edge cache za javne SSR/ISR stranice
- `static.trendplus.rs`
  - `_next/static`, JS chunk-ovi, CSS i fontovi preko `CDN_ASSET_PREFIX`
- `www.trendplus.rs/cdn-cgi/image/...`
  - Cloudflare Image Resizing za produkt, brand, collection, editorial i store slike
- `api.trendplus.rs`
  - ASP.NET Core API
  - origin source za query/read podatke
  - Output Cache + FusionCache ostaju origin-side slojevi, CDN je iznad njih

## Odgovornosti slojeva

- PostgreSQL
  - source of truth
- FusionCache + Redis
  - application/query cache u backendu
- ASP.NET Output Cache
  - short-lived HTTP cache na origin API sloju
- Cloudflare CDN
  - edge cache za javni HTML i static assets
  - image resizing i optimizacija na ivici mreze

## Implementirano u kodu

- `next.config.js`
  - `assetPrefix` preko `CDN_ASSET_PREFIX`
  - custom Cloudflare image loader
  - immutable cache headers za `/_next/static`, `/fonts`, `/images`
- `middleware.ts`
  - short-cache za public HTML
  - `no-store` za private/storefront-sensitive rute
- public API cache headers middleware
  - `Cache-Control` za home, PLP, PDP i entity API rute
- `revalidate`
  - `home`: 300s
  - PLP rute: 120s
  - PDP/store/entity detail: 300s

## Cache Headers

### Static assets

Za hashed static assets i public static fajlove:

```http
Cache-Control: public, max-age=31536000, immutable
```

Primena:

- `/_next/static/*`
- `/fonts/*`
- `/images/*`

### Home HTML

```http
Cache-Control: public, max-age=0, s-maxage=300, stale-while-revalidate=60
```

### PLP HTML

```http
Cache-Control: public, max-age=0, s-maxage=120, stale-while-revalidate=30
```

Primena:

- `/akcija`
- `/akcija/*`
- `/brendovi/*`
- `/kolekcije/*`
- top-level category landing stranice kao `/cipele`, `/patike`, `/cizme`, `/sandale`, `/papuce`

### PDP / editorial / stores HTML

```http
Cache-Control: public, max-age=0, s-maxage=300, stale-while-revalidate=60
```

### Private pages

```http
Cache-Control: private, no-store, max-age=0
```

Primena:

- `/korpa`
- `/checkout`
- `/account`
- `/omiljeno`
- `/search`
- `/admin`

### Public API

API dodatno vraca kratke public cache headere:

- `/api/pages/home`
  - `max-age=30, s-maxage=300`
- `/api/listings/*` i `/api/catalog/products`
  - `max-age=15, s-maxage=120`
- `/api/catalog/product/{slug}`
  - `max-age=30, s-maxage=300`
- `/api/brands/{slug}`, `/api/collections/{slug}`, `/api/stores/{slug}`
  - `max-age=30, s-maxage=30`

## Edge Rules

### 1. Bypass private and mutable routes

Cloudflare Cache Rule:

- Bypass cache for:
  - `/korpa*`
  - `/checkout*`
  - `/account*`
  - `/omiljeno*`
  - `/search*`
  - `/admin*`
  - `/api/cart*`
  - `/api/checkout*`
  - `/api/wishlist*`
  - svaki request sa `Authorization` header-om ili `Set-Cookie` odgovorom

### 2. Cache static assets aggressively

- Cache everything for:
  - `/_next/static/*`
  - `/fonts/*`
  - `/images/*`
- TTL:
  - 1 year

### 3. Cache homepage at the edge

- Match:
  - `/`
- Mode:
  - Eligible for cache
- Edge TTL:
  - 300s
- Browser TTL:
  - respect origin

### 4. Cache PLP at the edge

- Match:
  - `/akcija*`
  - `/brendovi/*`
  - `/kolekcije/*`
  - top-level category routes
- Edge TTL:
  - 120s
- Recommended:
  - u prvoj iteraciji favorizovati page 1 i glavne landing kombinacije

### 5. Cache PDP / editorial / store detail

- Match:
  - `/proizvod/*`
  - `/editorial/*`
  - `/prodavnice/*`
- Edge TTL:
  - 300s

## Cloudflare Dashboard Rules

Ispod su pravila spremna za unos kroz `Rules > Overview` u Cloudflare dashboard-u.

### Cache Rules

#### Rule 01: bypass-private-storefront

Purpose:

- ne kesirati privatne i korisnicki-specficne storefront rute

Custom filter expression:

```txt
(http.host eq "www.trendplus.rs" and (
  starts_with(http.request.uri.path, "/korpa") or
  starts_with(http.request.uri.path, "/checkout") or
  starts_with(http.request.uri.path, "/account") or
  starts_with(http.request.uri.path, "/omiljeno") or
  starts_with(http.request.uri.path, "/search") or
  starts_with(http.request.uri.path, "/admin")
))
```

Then:

- Cache eligibility: `Bypass cache`

#### Rule 02: bypass-mutable-api

Purpose:

- ne kesirati mutable API rute

Custom filter expression:

```txt
(http.host eq "api.trendplus.rs" and (
  starts_with(http.request.uri.path, "/api/cart") or
  starts_with(http.request.uri.path, "/api/checkout") or
  starts_with(http.request.uri.path, "/api/wishlist")
))
```

Then:

- Cache eligibility: `Bypass cache`

#### Rule 03: bypass-authenticated-api

Purpose:

- ne kesirati API request-e sa `Authorization` header-om

Custom filter expression:

```txt
(http.host eq "api.trendplus.rs" and any(lower(http.request.headers.names[*])[*] eq "authorization"))
```

Then:

- Cache eligibility: `Bypass cache`

#### Rule 04: cache-static-assets

Purpose:

- agresivno kesiranje build asseta na `static` subdomenu

Custom filter expression:

```txt
(http.host eq "static.trendplus.rs" and (
  starts_with(http.request.uri.path, "/_next/static/") or
  starts_with(http.request.uri.path, "/fonts/") or
  starts_with(http.request.uri.path, "/images/")
))
```

Then:

- Cache eligibility: `Eligible for cache`
- Edge TTL: `Ignore cache-control header and use this TTL`
- Edge TTL value: `1 year`
- Browser Cache TTL: `Respect existing headers`

#### Rule 05: cache-home-html

Purpose:

- edge cache za homepage HTML

Custom filter expression:

```txt
(http.host eq "www.trendplus.rs" and http.request.uri.path eq "/")
```

Then:

- Cache eligibility: `Eligible for cache`
- Edge TTL: `Use cache-control header if present, use default Cloudflare caching behavior if not`
- Browser Cache TTL: `Respect existing headers`

#### Rule 06: cache-plp-html

Purpose:

- edge cache za category, brand, collection i akcija listing rute

Custom filter expression:

```txt
(http.host eq "www.trendplus.rs" and (
  http.request.uri.path eq "/akcija" or
  starts_with(http.request.uri.path, "/akcija/") or
  starts_with(http.request.uri.path, "/brendovi/") or
  starts_with(http.request.uri.path, "/kolekcije/") or
  raw.http.request.uri.path matches "^/(cipele|patike|cizme|sandale|papuce)(/.*)?$"
))
```

Then:

- Cache eligibility: `Eligible for cache`
- Edge TTL: `Use cache-control header if present, use default Cloudflare caching behavior if not`
- Browser Cache TTL: `Respect existing headers`

#### Rule 07: cache-detail-html

Purpose:

- edge cache za PDP, editorial i store detail

Custom filter expression:

```txt
(http.host eq "www.trendplus.rs" and (
  starts_with(http.request.uri.path, "/proizvod/") or
  starts_with(http.request.uri.path, "/editorial/") or
  starts_with(http.request.uri.path, "/prodavnice/")
))
```

Then:

- Cache eligibility: `Eligible for cache`
- Edge TTL: `Use cache-control header if present, use default Cloudflare caching behavior if not`
- Browser Cache TTL: `Respect existing headers`

#### Rule 08: cache-public-read-api

Purpose:

- dodatni edge cache za javne read API rute

Custom filter expression:

```txt
(http.host eq "api.trendplus.rs" and (
  http.request.uri.path eq "/api/pages/home" or
  starts_with(http.request.uri.path, "/api/listings/") or
  http.request.uri.path eq "/api/catalog/products" or
  starts_with(http.request.uri.path, "/api/catalog/product/") or
  starts_with(http.request.uri.path, "/api/brands/") or
  starts_with(http.request.uri.path, "/api/collections/") or
  starts_with(http.request.uri.path, "/api/stores/")
))
```

Then:

- Cache eligibility: `Eligible for cache`
- Edge TTL: `Use cache-control header if present, use default Cloudflare caching behavior if not`
- Browser Cache TTL: `Respect existing headers`

### Transform Rules

#### Request Header Transform 01: strip-cookie-static-host

Purpose:

- ukloniti `cookie` header sa asset hosta radi cistijeg cache key-ja i boljeg hit ratio-a

Custom filter expression:

```txt
(http.host eq "static.trendplus.rs")
```

Modify request header:

- Operation: `Remove`
- Header name: `cookie`

#### Request Header Transform 02: strip-cookie-public-api-read

Purpose:

- ukloniti `cookie` header sa javnih read API ruta
- ostaviti mutable API rute netaknute

Custom filter expression:

```txt
(http.host eq "api.trendplus.rs" and (
  http.request.uri.path eq "/api/pages/home" or
  starts_with(http.request.uri.path, "/api/listings/") or
  http.request.uri.path eq "/api/catalog/products" or
  starts_with(http.request.uri.path, "/api/catalog/product/") or
  starts_with(http.request.uri.path, "/api/brands/") or
  starts_with(http.request.uri.path, "/api/collections/") or
  starts_with(http.request.uri.path, "/api/stores/")
))
```

Modify request header:

- Operation: `Remove`
- Header name: `cookie`

#### Response Header Transform 03: noindex-nofollow-nonprod

Purpose:

- zastita `dev` i `staging` hostova od indeksacije

Custom filter expression:

```txt
(http.host eq "dev.trendplus.rs" or http.host eq "staging.trendplus.rs")
```

Modify response header:

- Operation: `Set`
- Header name: `x-robots-tag`
- Value: `noindex, nofollow, noarchive`

### Optional Origin Rules

Ovo nije obavezno ako `static.trendplus.rs` i `api.trendplus.rs` direktno pokazuju na svoje origin hostove.

Ako vendor ili hosting trazi poseban `Host` header, koristi `Origin Rules`, ne `URL Rewrite Rules`.

Recommended optional rule:

- `static.trendplus.rs` -> Host Header override na stvarni storage/app origin
- `api.trendplus.rs` -> Host Header override na stvarni app-service origin

## SSL/TLS Baseline

Za ecommerce storefront preporuka je da Cloudflare bude postavljen konzervativno i bez “mixed mode” kompromisa.

### Recommended baseline settings

| Area | Setting | Recommended value | Why |
| --- | --- | --- | --- |
| `SSL/TLS > Overview` | Encryption mode | `Full (strict)` | validacija origin sertifikata i kraj “flexible” rizika |
| `SSL/TLS > Edge Certificates` | Always Use HTTPS | `On` | forsira HTTPS na svim hostovima |
| `SSL/TLS > Edge Certificates` | Automatic HTTPS Rewrites | `On` | pomaze oko mixed-content legacy linkova |
| `SSL/TLS > Edge Certificates` | Minimum TLS Version | `1.2` | uskladjeno sa savremenim security i PCI preporukama |
| `SSL/TLS > Edge Certificates` | TLS 1.3 | `On` | bolje performanse i moderniji TLS handshake |
| `SSL/TLS > Edge Certificates` | HSTS | `Off` na pocetku, pa postepeno ukljuciti | izbeci lock-in dok ne prodje puna validacija |
| `SSL/TLS > Origin Server` | Origin CA ili valid public cert | `Required` | origin mora imati validan sertifikat za `Full (strict)` |
| `SSL/TLS > Origin Server` | Authenticated Origin Pulls | `On` kada origin podrzi enforcement | dodatna zastita da origin prima samo Cloudflare origin pull-ove |

### Production guidance

- ne koristiti `Flexible`
- ne koristiti `Full` osim kao vrlo kratku prelaznu fazu
- na produkciji svi javni hostovi treba da budu iza `Full (strict)`
- `HSTS` ukljuciti tek nakon sto su `www`, `static`, `api`, `staging`, `static-staging` i `api-staging` validirani

### HSTS rollout recommendation

Preporuka je fazni rollout:

1. `max-age=300`
2. `max-age=86400`
3. `max-age=2592000`
4. tek nakon stabilnog perioda razmotriti `6-12 months`
5. `includeSubDomains` ukljuciti tek kada su i staging i svi dodatni hostovi spremni
6. `preload` koristiti samo kada je cela zona zaista spremna za trajni HTTPS-only rezim

### Authenticated Origin Pulls

Za ovaj projekat AOP je preporucen za:

- `www.trendplus.rs`
- `static.trendplus.rs`
- `api.trendplus.rs`
- staging hostove nakon pocetne validacije

Ako origin podrzava mTLS enforcement, to je jedan od najvrednijih koraka posle `Full (strict)`, jer sprecava direktno gadjanje origin-a mimo Cloudflare sloja.

## WAF Baseline

WAF baseline za ecommerce treba da bude dovoljno strog za checkout, cart i account tokove, ali bez nepotrebnog lomljenja SEO i browsing iskustva na javnim listing/detail stranicama.

### Recommended baseline settings

| Area | Setting | Recommended value | Notes |
| --- | --- | --- | --- |
| `Security > WAF > Managed rules` | Cloudflare Managed Ruleset | `On` | osnovni baseline za web app zastitu |
| `Security > WAF > Managed rules` | OWASP Core Ruleset | `Off` ili `Log/Challenge only` u startu | Cloudflare navodi da je sklon false positive-ima i da cesto donosi ogranicenu dodatnu korist preko glavnog ruleset-a |
| `Security > Settings` | Browser Integrity Check | `On` na storefront hostovima | dobar low-friction signal protiv losih klijenata |
| `Security > Settings` | Security Level | `Medium` | pragmatican default za ecommerce |
| `Security > Settings` | Bot Fight Mode | `On` samo ako ste na Free planu | ako postoji SBFM, njega koristiti umesto BFM |
| `Security > WAF > Rate limiting rules` | rate limits za mutable rute | `On` | posebno za cart, checkout i auth osetljive rute |
| `Security > Events` | monitoring | `Daily during rollout` | posmatrati false positive-e i challenge solve rate |

### Plan-aware bot recommendation

- `Free`: koristiti `Bot Fight Mode`
- `Pro/Business`: ako je dostupan `Super Bot Fight Mode`, koristiti njega umesto `Bot Fight Mode`
- `Enterprise`: koristiti `Bot Management` / bot score pravila ako je add-on dostupan

Za storefront SEO i UX je vazno:

- verified bots ostaviti da prolaze
- ne challenge-ovati javne HTML rute “naslepo” samo zbog crawler pattern-a
- checkout/cart/account rute mogu imati agresivnija pravila od home/PLP/PDP ruta

### WAF custom rule baseline

Ispod je preporuceni minimum za custom pravila.

#### Rule A: Challenge sensitive storefront paths

Purpose:

- dodatna zastita za checkout i account-sensitive stranice

Expression:

```txt
(http.host in {"www.trendplus.rs" "staging.trendplus.rs"} and (
  starts_with(http.request.uri.path, "/checkout") or
  starts_with(http.request.uri.path, "/account") or
  starts_with(http.request.uri.path, "/admin")
))
```

Action:

- `Managed Challenge`

#### Rule B: Challenge sensitive API mutations

Purpose:

- dodatna zastita za cart/checkout i buduce auth-sensitive API rute

Expression:

```txt
(http.host in {"api.trendplus.rs" "api-staging.trendplus.rs"} and http.request.method in {"POST" "PUT" "PATCH" "DELETE"} and (
  starts_with(http.request.uri.path, "/api/cart") or
  starts_with(http.request.uri.path, "/api/checkout") or
  starts_with(http.request.uri.path, "/api/account") or
  starts_with(http.request.uri.path, "/api/admin")
))
```

Action:

- `Managed Challenge`

#### Rule C: Block obvious admin exposure on public storefront host

Purpose:

- ne dozvoliti da javni storefront host postane fallback za admin/API probe-ove

Expression:

```txt
(http.host in {"www.trendplus.rs" "staging.trendplus.rs"} and (
  starts_with(http.request.uri.path, "/wp-admin") or
  starts_with(http.request.uri.path, "/wp-login.php") or
  starts_with(http.request.uri.path, "/.env") or
  starts_with(http.request.uri.path, "/phpmyadmin") or
  starts_with(http.request.uri.path, "/vendor/")
))
```

Action:

- `Block`

### Rate limiting baseline

Rate limiting preporuka za ecommerce nije da se “guši” sav traffic, vec da se uspori abuse na mutating tokovima.

#### Rate Limit 01: Checkout submit

- Match:
  - host `api.trendplus.rs`, `api-staging.trendplus.rs`
  - path starts with `/api/checkout`
  - method `POST`
- Threshold:
  - `10 requests / 1 minute / per IP`
- Action:
  - `Managed Challenge`
- Duration:
  - `10 minutes`

#### Rate Limit 02: Cart mutation

- Match:
  - host `api.trendplus.rs`, `api-staging.trendplus.rs`
  - path starts with `/api/cart`
  - method in `POST`, `PUT`, `PATCH`, `DELETE`
- Threshold:
  - `60 requests / 1 minute / per IP`
- Action:
  - `Managed Challenge`
- Duration:
  - `10 minutes`

#### Rate Limit 03: Search or listing abuse

- Match:
  - host `api.trendplus.rs`
  - path starts with `/api/catalog/products`
- Threshold:
  - `240 requests / 1 minute / per IP`
- Action:
  - `Managed Challenge`
- Duration:
  - `5 minutes`

### Host-specific configuration guidance

Preporuceni split po hostovima:

| Host | Browser Integrity Check | Security Level | Extra notes |
| --- | --- | --- | --- |
| `www.trendplus.rs` | `On` | `Medium` | glavni storefront |
| `static.trendplus.rs` | `Off` | n/a | asset host ne treba BIC/challenge logiku |
| `api.trendplus.rs` | `Off` ili selektivno `Off` kroz Configuration Rule | `Medium` | API-ju cesce smeta browser heuristika nego HTML sajtu |
| `staging.trendplus.rs` | `On` | `Medium` | uz `x-robots-tag: noindex` |
| `static-staging.trendplus.rs` | `Off` | n/a | staging asset host |
| `api-staging.trendplus.rs` | `Off` ili selektivno `Off` | `Medium` | staging API |

Za API host je cesto bolje da `Browser Integrity Check` bude iskljucen per-hostname ili per `/api/*` konfiguracionim pravilom, a da se zastita resava kroz `Managed Rules`, `Custom Rules` i `Rate Limiting`.

## Deployment Matrix

## Image CDN Strategy

Next.js koristi custom loader koji sliku preusmerava na Cloudflare Image Resizing:

```text
https://www.trendplus.rs/cdn-cgi/image/format=auto,metadata=none,fit=cover,width=800,quality=85/https://origin.example.com/image.jpg
```

Prednosti:

- WebP/AVIF auto-format kad je podrzan
- resize po komponenti i viewport-u
- manji origin bandwidth
- bolji LCP na PLP/PDP

## Environment Variables

Minimalni env za storefront:

```env
NEXT_PUBLIC_SITE_URL=https://www.trendplus.rs
NEXT_PUBLIC_API_BASE_URL=https://api.trendplus.rs/api
CDN_ASSET_PREFIX=https://static.trendplus.rs
NEXT_PUBLIC_CDN_IMAGE_BASE_URL=https://www.trendplus.rs/cdn-cgi/image
```

### Dev / Staging / Prod Matrix

| Environment | Storefront host | Static host | API host | `NEXT_PUBLIC_SITE_URL` | `NEXT_PUBLIC_API_BASE_URL` | `CDN_ASSET_PREFIX` | `NEXT_PUBLIC_CDN_IMAGE_BASE_URL` |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Dev local | `localhost:3000` | none ili `localhost:3000` | `localhost:7002` | `http://localhost:3000` | `https://localhost:7002/api` | prazno | prazno ili `http://localhost:3000/cdn-cgi/image` |
| Staging | `staging.trendplus.rs` | `static-staging.trendplus.rs` | `api-staging.trendplus.rs` | `https://staging.trendplus.rs` | `https://api-staging.trendplus.rs/api` | `https://static-staging.trendplus.rs` | `https://staging.trendplus.rs/cdn-cgi/image` |
| Prod | `www.trendplus.rs` | `static.trendplus.rs` | `api.trendplus.rs` | `https://www.trendplus.rs` | `https://api.trendplus.rs/api` | `https://static.trendplus.rs` | `https://www.trendplus.rs/cdn-cgi/image` |

### DNS / Proxy Matrix

| Environment | Hostname | Cloudflare proxy | Target origin |
| --- | --- | --- | --- |
| Dev local | `localhost` | no | lokalni dev proces |
| Staging | `staging.trendplus.rs` | yes | staging Next.js origin |
| Staging | `static-staging.trendplus.rs` | yes | staging static asset origin |
| Staging | `api-staging.trendplus.rs` | yes | staging ASP.NET API origin |
| Prod | `www.trendplus.rs` | yes | production Next.js origin |
| Prod | `static.trendplus.rs` | yes | production static asset origin |
| Prod | `api.trendplus.rs` | yes | production ASP.NET API origin |

## DNS Record Proposals

Preporuka je da javni hostovi budu `Proxied`, a da stvarni app origin-i budu sakriveni iza zasebnih `DNS only` alias zapisa.

Time dobijamo:

- cistiju zamenu origin-a bez promene javnog hosta
- odvojene Cloudflare rule-ove po javnom hostu
- jednostavniji rollback ako se menja vendor ili hosting

### Public Proxied Records

Ovo su javni hostovi koje koristi storefront, asset delivery i API.

| Name | Type | Target | Proxy status | TTL | Purpose |
| --- | --- | --- | --- | --- | --- |
| `www` | `CNAME` | `origin-web-prod.trendplus.rs` | `Proxied` | `Auto` | primarni storefront host |
| `static` | `CNAME` | `origin-web-prod.trendplus.rs` | `Proxied` | `Auto` | static asset host za `/_next/static`, CSS, fontove i javne slike |
| `api` | `CNAME` | `origin-api-prod.trendplus.rs` | `Proxied` | `Auto` | javni API host |
| `staging` | `CNAME` | `origin-web-staging.trendplus.rs` | `Proxied` | `Auto` | staging storefront |
| `static-staging` | `CNAME` | `origin-web-staging.trendplus.rs` | `Proxied` | `Auto` | staging static asset host |
| `api-staging` | `CNAME` | `origin-api-staging.trendplus.rs` | `Proxied` | `Auto` | staging API host |

### Internal Origin Alias Records

Ovo su pomocni aliasi unutar iste zone. Njih ne reklamiramo javno kao application entrypoint-e.

| Name | Type | Target | Proxy status | TTL | Notes |
| --- | --- | --- | --- | --- | --- |
| `origin-web-prod` | `CNAME` | `<prod-next-origin-host>` | `DNS only` | `Auto` | npr. Vercel / Azure App Service / container ingress host |
| `origin-api-prod` | `CNAME` | `<prod-api-origin-host>` | `DNS only` | `Auto` | ASP.NET API origin |
| `origin-web-staging` | `CNAME` | `<staging-next-origin-host>` | `DNS only` | `Auto` | staging Next.js origin |
| `origin-api-staging` | `CNAME` | `<staging-api-origin-host>` | `DNS only` | `Auto` | staging ASP.NET API origin |

### Why `static` points to the same web origin

U ovoj verziji projekta `static.trendplus.rs` i `static-staging.trendplus.rs` treba da pokazuju na isti Next.js origin kao i pripadajuci storefront host.

Razlog:

- Next build generise `/_next/static/*` assete na web origin-u
- `assetPrefix` samo menja host sa kog ih browser trazi
- Cloudflare zatim preuzima agresivno kesiranje na edge-u

Ako kasnije prebacimo statiku na poseban object storage ili dedicated asset origin, menjamo samo target `origin-web-*` ili uvodimo poseban `origin-static-*` alias.

### Dashboard Entry Examples

Ako zapis unosis direktno kroz Cloudflare DNS ekran:

```text
Type: CNAME
Name: www
Target: origin-web-prod.trendplus.rs
Proxy status: Proxied
TTL: Auto
```

```text
Type: CNAME
Name: static
Target: origin-web-prod.trendplus.rs
Proxy status: Proxied
TTL: Auto
```

```text
Type: CNAME
Name: api
Target: origin-api-prod.trendplus.rs
Proxy status: Proxied
TTL: Auto
```

```text
Type: CNAME
Name: staging
Target: origin-web-staging.trendplus.rs
Proxy status: Proxied
TTL: Auto
```

```text
Type: CNAME
Name: static-staging
Target: origin-web-staging.trendplus.rs
Proxy status: Proxied
TTL: Auto
```

```text
Type: CNAME
Name: api-staging
Target: origin-api-staging.trendplus.rs
Proxy status: Proxied
TTL: Auto
```

### Recommended rollout order

1. Deploy `api-staging.trendplus.rs`
2. Deploy `staging.trendplus.rs`
3. Verify staging cache rules + `x-robots-tag`
4. Deploy `api.trendplus.rs`
5. Deploy `static.trendplus.rs`
6. Deploy `www.trendplus.rs`

## Operativne preporuke

- ukljuciti Cloudflare Brotli i HTTP/3
- ukljuciti Polish i Mirage ako budzet/paket to dozvoljava
- ne koristiti Cloudflare Auto Minify za vec build-ovane Next assete ako pravi diff debugging problem
- pratiti:
  - edge cache hit ratio
  - LCP na home/PLP/PDP
  - origin requests per minute
  - image bytes saved

## Prvi rollout

1. postaviti `CDN_ASSET_PREFIX` na `static` subdomain
2. postaviti `NEXT_PUBLIC_CDN_IMAGE_BASE_URL`
3. deploy-ovati storefront sa novim loader-om i middleware-om
4. aktivirati Cloudflare cache rules iznad
5. proveriti:
   - home HTML cache hit
   - PLP HTML cache hit
   - `/_next/static/*` immutable hit
   - image resize URL output u browser network tabu

## Cloudflare Zone Rollout Checklist

Checklist ispod je pisan za jednu Cloudflare zonu: `trendplus.rs`.

Preporuceni redosled je:

1. prvo staging hostovi
2. zatim prod hostovi
3. tek onda opcioni hardening kao HSTS preload

### Phase 0: Prerequisites

- [ ] potvrditi koji su stvarni vendor origin hostovi za Next.js i ASP.NET API
- [ ] obezbediti valid TLS sertifikat na svim origin-ima
- [ ] obezbediti da origin prihvata `Host` header za javni host ili spremiti `Origin Rule` override
- [ ] potvrditi da staging i prod koriste odvojene baze, Redis instance i OpenSearch indekse gde je potrebno

### Phase 1: DNS Setup

- [ ] dodati `origin-web-prod`, `origin-api-prod`, `origin-web-staging`, `origin-api-staging` kao `DNS only`
- [ ] dodati javne `Proxied` CNAME zapise: `www`, `static`, `api`, `staging`, `static-staging`, `api-staging`
- [ ] proveriti da `www` i `static` trenutno gadjaju isti web origin
- [ ] proveriti da `staging` i `static-staging` trenutno gadjaju isti staging web origin
- [ ] sacekati DNS propagaciju i potvrditi rezoluciju

Validation:

```powershell
nslookup www.trendplus.rs
nslookup static.trendplus.rs
nslookup api.trendplus.rs
nslookup staging.trendplus.rs
nslookup static-staging.trendplus.rs
nslookup api-staging.trendplus.rs
```

### Phase 2: SSL/TLS and Edge Platform Settings

- [ ] `SSL/TLS > Overview > Encryption mode` postaviti na `Full (strict)`
- [ ] `SSL/TLS > Edge Certificates > Always Use HTTPS` ukljuciti
- [ ] `SSL/TLS > Edge Certificates > Automatic HTTPS Rewrites` ukljuciti
- [ ] `SSL/TLS > Edge Certificates > Minimum TLS Version` postaviti na `TLS 1.2` ili vise
- [ ] `SSL/TLS > Edge Certificates > TLS 1.3` ukljuciti
- [ ] `Network > HTTP/3 (with QUIC)` ukljuciti
- [ ] `Speed > Optimization > Brotli` ukljuciti
- [ ] `Rocket Loader` ostaviti iskljucen za Next.js storefront
- [ ] `HSTS` ostaviti iskljucen dok staging i prod ne prodju punu validaciju
- [ ] pripremiti `Authenticated Origin Pulls` za produkciju ako origin podrzava enforcement

### Phase 3: Cache Rules

- [ ] dodati `Rule 01: bypass-private-storefront`
- [ ] dodati `Rule 02: bypass-mutable-api`
- [ ] dodati `Rule 03: bypass-authenticated-api`
- [ ] dodati `Rule 04: cache-static-assets`
- [ ] dodati `Rule 05: cache-home-html`
- [ ] dodati `Rule 06: cache-plp-html`
- [ ] dodati `Rule 07: cache-detail-html`
- [ ] dodati `Rule 08: cache-public-read-api`
- [ ] potvrditi da je redosled pravila identican kao iznad

### Phase 4: Transform and Origin Rules

- [ ] dodati `Request Header Transform 01: strip-cookie-static-host`
- [ ] dodati `Request Header Transform 02: strip-cookie-public-api-read`
- [ ] dodati `Response Header Transform 03: noindex-nofollow-nonprod`
- [ ] ako origin zahteva poseban host, dodati `Origin Rule` za `static` host
- [ ] ako origin zahteva poseban host, dodati `Origin Rule` za `api` host

### Phase 4.5: WAF and Rate Limiting

- [ ] ukljuciti `Cloudflare Managed Ruleset`
- [ ] `OWASP Core Ruleset` ostaviti `Off` ili `Log/Challenge only` u prvoj iteraciji
- [ ] `Browser Integrity Check` ukljuciti za `www` i `staging`
- [ ] za `api` host proveriti da li `Browser Integrity Check` treba ostati iskljucen
- [ ] postaviti `Security Level` na `Medium`
- [ ] ako plan dozvoljava, ukljuciti `Super Bot Fight Mode`; u suprotnom razmotriti `Bot Fight Mode`
- [ ] dodati rate limit za `POST /api/checkout`
- [ ] dodati rate limit za mutacije na `/api/cart`
- [ ] dodati rate limit za `GET /api/catalog/products`
- [ ] pregledati `Security Events` posle prvih testova i potvrditi da nema false positive spike-a

### Phase 5: App Configuration

- [ ] staging storefront env postaviti na:

```env
NEXT_PUBLIC_SITE_URL=https://staging.trendplus.rs
NEXT_PUBLIC_API_BASE_URL=https://api-staging.trendplus.rs/api
CDN_ASSET_PREFIX=https://static-staging.trendplus.rs
NEXT_PUBLIC_CDN_IMAGE_BASE_URL=https://staging.trendplus.rs/cdn-cgi/image
```

- [ ] production storefront env postaviti na:

```env
NEXT_PUBLIC_SITE_URL=https://www.trendplus.rs
NEXT_PUBLIC_API_BASE_URL=https://api.trendplus.rs/api
CDN_ASSET_PREFIX=https://static.trendplus.rs
NEXT_PUBLIC_CDN_IMAGE_BASE_URL=https://www.trendplus.rs/cdn-cgi/image
```

- [ ] redeploy staging storefront
- [ ] redeploy production storefront
- [ ] potvrditi da browser trazi `/_next/static/*` sa `static` hosta

### Phase 6: Validation

- [ ] otvoriti homepage dva puta i potvrditi `cf-cache-status: HIT` na drugom request-u
- [ ] otvoriti jednu PLP stranicu dva puta i potvrditi `cf-cache-status: HIT`
- [ ] proveriti da private rute vracaju `Cache-Control: private, no-store`
- [ ] proveriti da staging host vraca `x-robots-tag: noindex, nofollow, noarchive`
- [ ] proveriti da `static` asseti vracaju `Cache-Control: public, max-age=31536000, immutable`
- [ ] proveriti da image resizing URL radi i vraca optimizovan format

Validation examples:

```powershell
curl.exe -I https://www.trendplus.rs/
curl.exe -I https://www.trendplus.rs/cipele
curl.exe -I https://static.trendplus.rs/_next/static/chunks/main.js
curl.exe -I https://api.trendplus.rs/api/pages/home
curl.exe -I https://staging.trendplus.rs/
```

Sta gledamo:

- `cf-cache-status`
- `cache-control`
- `age`
- `x-robots-tag` na staging hostovima

### Phase 7: Launch and Hardening

- [ ] tek nakon stabilne validacije ukljuciti `HSTS` sa niskim `max-age`
- [ ] posle nekoliko dana bez problema povecati `max-age`
- [ ] razmotriti `Polish` i `Mirage` ako plan i budzet to dozvoljavaju
- [ ] pratiti Cloudflare Analytics za `Cache Hit Ratio`, `Origin Egress` i `Response Status Codes`

### Rollback Plan

- [ ] prvo disable-ovati `Rule 05` do `Rule 08` ako HTML/API cache pravi problem
- [ ] zatim disable-ovati request header transform za public API ako request-i neocekivano zavise od kolacica
- [ ] ako asset host pravi problem, privremeno vratiti `CDN_ASSET_PREFIX` na prazno i redeploy storefront
- [ ] tek na kraju menjati DNS targete
