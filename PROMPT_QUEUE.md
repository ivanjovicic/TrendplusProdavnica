# Trendplus Prompt Queue

Last reviewed: 2026-08-19

Ovo je jedini aktivni backlog za prompt-sized rad u Trendplus repozitorijumu. Stari planovi, implementation guide fajlovi i sacuvani build logovi su izvori konteksta, ali nisu queue.

## Pravila queue-a

- Statusi su `READY`, `BLOCKED`, `DONE` i `REMOVED`.
- `READY` znaci da prompt moze odmah da se radi bez dodatne odluke ili produkcionog pristupa.
- Radi se jedan prompt po grani/PR-u, osim ako prompt izricito kaze drugacije.
- Po zavrsetku se prompt skracuje na jednu stavku u `Done` arhivi sa datumom i linkom na PR/commit.
- Novi prompt mora imati cilj, obim, acceptance criteria i verifikaciju.
- Ne dodavati query tuning ili nove indekse bez izmerenog baseline-a.

## Ready pregled

| Red | ID | Prioritet | Velicina | Tema | Zasto sada |
| --- | --- | --- | --- | --- | --- |
| 1 | TP-027 | P0 | M | Order lookup PII exposure | Predvidiv order number javno vraca pune podatke kupca |
| 2 | TP-028 | P0 | S | Public business analytics | Revenue/order izvestaji nemaju authorization policy |
| 3 | TP-015 | P0 | L | Admin auth hardening | Refresh prihvata proizvoljan dug token i nema revocation |
| 4 | TP-026 | P0 | M | Admin login/session reachability | Login layout vraca `null`; API origin/env ugovor je neuskladjen |
| 5 | TP-017 | P0 | M | Checkout locking | Metoda nazvana lock radi obican SELECT; provider test je preskocen |
| 6 | TP-001 | P0 | S | Frontend type-check | Trenutno pada na 2 greske |
| 7 | TP-002 | P0 | M | Ranjive .NET zavisnosti | Build prijavljuje 23 NU1902/NU1903 upozorenja |
| 8 | TP-014 | P0 | M | Inventory/recommendation runtime | Kontroleri postoje, ali DI registracije su iskljucene |
| 9 | TP-003 | P0 | M | SEO query URL indexacija | Listing varijante emituju indexable `?page=` canonical URL-ove |
| 10 | TP-004 | P1 | M | Legacy i encoded Next.js rute | Postoje duple/stare route putanje i template artefakti |
| 11 | TP-005 | P1 | S | Repository hygiene | Tracked logovi i generated/starter fajlovi stvaraju lazno stanje |
| 12 | TP-006 | P1 | M | A/B assignment hardening | Nema testova; hash nije stabilan izmedju procesa |
| 13 | TP-007 | P1 | L | A/B conversion analytics | Metoda je jos TODO, storefront ne salje evente |
| 14 | TP-016 | P1 | M | Backend CI quality gate | Repozitorijum nema GitHub Actions workflow |
| 15 | TP-029 | P1 | M | Public API error disclosure | Desetine endpoint-a vracaju `ex.Message` klijentu |
| 16 | TP-030 | P1 | M | Checkout delivery contract | StorePickup trazi adresu i prikazuje pogresan total |
| 17 | TP-031 | P1 | S | Order category snapshots | Svaka kategorija se cuva pod imenom `Default` |
| 18 | TP-032 | P1 | M | Analytics ingestion hardening | Anonymous event endpoint nema rate/size/schema zastitu |
| 19 | TP-034 | P1 | M | Order lifecycle state machine | API dozvoljava proizvoljne status skokove; UI enum je pogresan |
| 20 | TP-018 | P1 | L | Experiments admin UI | Kompletan admin API nema radnu povrsinu |
| 21 | TP-019 | P1 | L | Merchandising admin UI | Rules API radi, ali nema UI za trgovce |
| 22 | TP-020 | P1 | L | Content/SEO admin workspace | Vise content API-ja nema odgovarajuce ekrane |
| 23 | TP-021 | P1 | L | Product review workflow | Reviews se samo seed-uju/citaju; nema submit/moderation toka |
| 24 | TP-008 | P1 | L | Observability metrics path | Infra postoji, ali aplikacione metrike/export nisu povezani end-to-end |
| 25 | TP-033 | P2 | M | Postojeci admin UI ugovori | Dead linkovi, netacni KPI-jevi, statusi i valuta |
| 26 | TP-022 | P2 | M | Admin audit trail | Mutacije nisu objedinjeno auditovane |
| 27 | TP-023 | P2 | L | Demand planning admin UI | Demand API postoji, ali nije dostupan operativnom timu |
| 28 | TP-009 | P2 | M | Logs, alerting i runbooks | Seq/Slack dokumentacija nije uskladjena sa stvarnim runtime wiring-om |
| 29 | TP-010 | P2 | M | Frontend automated quality gate | Nema test komande ni test framework-a |

## READY TP-001 - Vrati frontend type-check na zeleno

```text
U TrendplusProdavnica repozitorijumu popravi trenutni frontend TypeScript quality gate.

Kontekst:
- `npm run type-check` pada u `TrendplusProdavnica.Web/src/lib/api/analytics.ts` na pozivima za shoe-type i supplier analytics.
- `AnalyticsQueryParams` nije kompatibilan sa `Record<string, SearchParamValue>` jer nema index signature.

Zadatak:
1. Pregledaj tipove API klijenta i analytics parametara i izaberi type-safe resenje bez `any`, nepotrebnih cast-ova i slabljenja zajednickog API ugovora.
2. Ispravi oba analytics poziva.
3. Proveri da ostali API moduli i dalje koriste isti searchParams ugovor.
4. Dodaj mali type-level ili unit test samo ako postojeca test infrastruktura to podrzava bez uvodjenja novog framework-a.

Acceptance criteria:
- `npm run type-check` prolazi bez gresaka.
- Nema `any`, `@ts-ignore` ili dupliranja serializer logike.
- Ponasanje query parametara ostaje isto za `from`, `to` i `brandId`.

Verifikacija:
- `npm run type-check`
- `npm run build`
- prikazi kratak diff i objasni zasto je izabrani tip bezbedan.
```

## READY TP-002 - Ukloni poznate ranjivosti iz .NET dependency grafa

```text
U TrendplusProdavnica repozitorijumu ukloni aktuelna NU1902/NU1903 upozorenja bez nekontrolisanog major upgrade-a.

Kontekst:
- `dotnet build TrendplusProdavnica.slnx --no-restore` prolazi, ali prijavljuje 23 upozorenja.
- High severity: transitive `Microsoft.OpenApi 2.0.0` i `System.Security.Cryptography.Xml 9.0.0`.
- Moderate severity: `OpenTelemetry.Api 1.15.0` i `OpenTelemetry.Exporter.OpenTelemetryProtocol 1.15.0`.

Zadatak:
1. Pokreni `dotnet list package --vulnerable --include-transitive` i utvrdi tacne parent pakete.
2. Nadji najmanje bezbedne kompatibilne verzije koje uklanjaju advisories i ostaju uskladjene sa `net10.0` i Aspire paketima.
3. Azuriraj direktne PackageReference stavke ili dodaj eksplicitne transitive override-e samo kada je potrebno.
4. Ne potiskuj NU1902/NU1903 i ne iskljucuj audit.
5. Dokumentuj ako advisory nema dostupnu kompatibilnu zakrpu i izdvoji ga kao blokiran rizik.

Acceptance criteria:
- `dotnet restore` i build prolaze.
- `dotnet list package --vulnerable --include-transitive` nema poznate high/moderate ranjivosti, ili je svaki preostali slucaj precizno dokumentovan sa razlogom.
- Svi testovi prolaze.
- OpenTelemetry export i API startup nisu regresirani.

Verifikacija:
- `dotnet restore TrendplusProdavnica.slnx`
- `dotnet build TrendplusProdavnica.slnx --no-restore`
- `dotnet test TrendplusProdavnica.Tests/TrendplusProdavnica.Tests.csproj --no-build`
- `dotnet list TrendplusProdavnica.slnx package --vulnerable --include-transitive`
```

## READY TP-003 - Uredi canonical/noindex pravila za listing query URL-ove

```text
Implementiraj jedinstven SEO ugovor za paginaciju, filtere i sortiranje na Trendplus storefront listing stranicama.

Kontekst:
- Category, brand, collection i sale stranice trenutno grade canonical URL sa `?page=N` za page > 1.
- `buildMetadata` vec podrzava `noIndex`, ali listing stranice ga ne koriste za query varijante.
- Search strana nema eksplicitan metadata/noindex ugovor.

Zadatak:
1. Primeni i dokumentuj pravilo: page 1 je `index, follow` sa canonical landing URL-om; cista page > 1 paginacija je `noindex, follow` sa normalizovanim self-canonical URL-om; search/filter/sort kombinacije su `noindex, follow` sa canonical URL-om na odgovarajucu nefiltriranu landing stranicu.
2. Implementiraj zajednicki helper u `src/lib/seo` umesto kopiranja uslova po stranicama.
3. Primeni ga na `/kategorije/[slug]`, brand, collection, sale, sale-category i search stranice.
4. Sacuvaj query parametre u korisnickoj navigaciji, ali ne dozvoli canonical fragmentaciju.
5. Dodaj fokusirane testove za page 1, page > 1, sort/filter i nevalidan page parametar.
6. Ne dodavati `rel=next/prev`; fokus je metadata ugovor koji Next.js stvarno emituje.

Acceptance criteria:
- Svaka obuhvacena ruta ima deterministicki canonical i robots rezultat.
- Filter/sort/search URL-ovi nisu indexable.
- Page 1 nema dupli `?page=1` canonical.
- Nevalidni i negativni page parametri se normalizuju.
- Type-check i production build prolaze.

Verifikacija:
- `npm run type-check`
- `npm run build`
- pregled renderovanog metadata izlaza za reprezentativne URL-ove.
```

## READY TP-004 - Ukloni legacy i pogresno encoded Next.js route artefakte

```text
Ocisti Trendplus Next.js App Router stablo tako da postoji samo jedan kanonski storefront put za svaku stranicu.

Kontekst:
- SEO migracija je standardizovala kategorije na `/kategorije/[slug]` i legacy putanje treba da budu redirect-i.
- U stablu i dalje postoje `(storefront)/[categorySlug]`, `kategorija/[slug]`, kao i literalni `%5Bslug%5D` direktorijumi za kategorije, proizvode i brendove.
- Postoje `page.template.tsx` fajlovi sa starim TODO primerima koji izgledaju kao nedovrsen aktivni kod.

Zadatak:
1. Napravi mapu svih javnih ruta i potvrdi koje stranice su kanonske, koje su redirect-only, a koje su mrtvi artefakti.
2. Ukloni literalne `%5Bslug%5D` direktorijume i zastarele template fajlove ako nemaju aktivnu upotrebu.
3. Ukloni duple legacy page implementacije; zadrzi permanent redirect pravila u `next.config.js` za stare javne URL-ove.
4. Proveri da catch-all dinamicka ruta vise ne moze da proguta rezervisane top-level putanje.
5. Azuriraj `TrendplusProdavnica.Web/STATUS.md` da opisuje stvarno stanje ili ga jasno oznaci kao istorijski dokument.
6. Dodaj route smoke proveru za home, category, PDP, brand, collection, sale, editorial, stores, cart i admin.

Acceptance criteria:
- Jedna implementacija po javnoj stranici.
- Nema tracked `%5Bslug%5D` direktorijuma ni aktivnih TODO template stranica.
- Legacy category/product URL-ovi vracaju ocekivani permanent redirect.
- Kanonske rute rade bez route collision-a.
- Frontend type-check i build prolaze.

Verifikacija:
- `npm run type-check`
- `npm run build`
- izlistaj finalnu route mapu i rezultate smoke provere.
```

## READY TP-005 - Ukloni tracked build/log/starter artefakte

```text
Ocisti tracked repository artefakte koji stvaraju lazne greske i dupliraju izvor istine, bez brisanja korisnickog ili produkcionog koda.

Kontekst:
- Root sadrzi tracked `build_output.txt`, `_audit_build_output.txt` i `_audit_tests_output.txt` sa zastarelim rezultatima.
- `TrendplusProdavnica.Web/tsconfig.tsbuildinfo` je tracked generated fajl.
- `TrendplusProdavnica.Tests/Trendplus.Tests.csproj` je drugi csproj u istom folderu, ali nije u solution-u.
- `TrendplusProdavnica.Tests/UnitTest1.cs` izgleda kao starter test.

Zadatak:
1. Potvrdi da nijedan CI/script ne cita ove fajlove.
2. Ukloni zastarele tracked logove i generated tsbuildinfo; prosiri `.gitignore` za `*.tsbuildinfo` i repo-root audit/build output obrasce.
3. Uporedi oba test csproj fajla. Zadrzi kanonski `TrendplusProdavnica.Tests.csproj`, a drugi ukloni samo ako nema jedinstvene reference ili konfiguraciju.
4. Ukloni ili preimenuj `UnitTest1.cs` ako je samo placeholder; ne uklanjaj pravi coverage.
5. Azuriraj dokumentaciju koja linkuje obrisane artefakte.

Acceptance criteria:
- `git status` posle build/test komandi ne dobija generated izmene.
- Solution ima jedan nameran test projekat za taj folder.
- Nema zastarelih logova koji se mogu pogresno tumaciti kao aktuelni rezultat.
- Build i testovi prolaze.

Verifikacija:
- `dotnet build TrendplusProdavnica.slnx`
- `dotnet test TrendplusProdavnica.Tests/TrendplusProdavnica.Tests.csproj --no-build`
- `npm run type-check`
- `git status --short`
```

## READY TP-006 - Harden A/B assignment i pokrij ga testovima

```text
Ucini A/B dodelu varijante deterministickom, concurrency-safe i testiranom.

Kontekst:
- `ExperimentService.DetermineVariant` koristi `string.GetHashCode()`, koji nije stabilan ugovor izmedju procesa/runtime instanci.
- Get-then-insert tok moze da napravi race pri paralelnim zahtevima.
- Postojeca dokumentacija navodi testove, ali repo nema fokusiran `ExperimentService` test suite.
- EF migracije za Experiment i ExperimentAssignment vec postoje u snapshot-u; ne generisi duplu migraciju iz starog guide-a.

Zadatak:
1. Zameni runtime hash stabilnim, dokumentovanim hash algoritmom i sacuvaj zadati traffic split.
2. Proveri unique constraints za user/session assignment i ucini paralelni tok idempotentnim.
3. Jasno validiraj slucaj kada nema ni userId ni sessionId.
4. Dodaj testove za sticky assignment, 60/40 distribuciju sa razumnom tolerancijom, inactive experiment, user/session lookup, invalid split i paralelnu dodelu.
5. Ne menjaj javni API bez potrebe; ako je promena neophodna, dokumentuj kompatibilnost.

Acceptance criteria:
- Isti identifikator daje istu varijantu na razlicitim procesima i restartima.
- Paralelni zahtevi ne prave duple assignment zapise.
- Testovi ne zavise od slucajnog runtime seed-a i nisu flaky.
- Postojeci experiment endpoint-i ostaju kompatibilni.

Verifikacija:
- fokusirani ExperimentService testovi
- kompletan `dotnet test TrendplusProdavnica.Tests/TrendplusProdavnica.Tests.csproj`
- `dotnet build TrendplusProdavnica.slnx --no-restore`
```

## READY TP-007 - Zavrsi A/B conversion analytics end-to-end

```text
Povezi A/B assignment sa analytics eventima i izracunavanjem konverzija, od storefront-a do admin rezultata.

Kontekst:
- `ExperimentService.CalculateConversionRatesAsync` je TODO/no-op.
- `GetResultsAsync` trenutno vraca samo broj assignment-a, bez conversion rate-a po varijanti.
- Frontend nema storefront analytics event tracking; postojeci analytics TS modul sluzi admin izvestajima.

Zadatak:
1. Definisi minimalan event ugovor za experiment assignment, exposure i conversion, sa experimentId, variant, user/session korelacijom i timestamp-om.
2. Iskoristi postojeci analytics pipeline; ne uvodi drugi event store.
3. Posalji exposure tek kada je varijanta stvarno prikazana i conversion na dogovorenom poslovnom dogadjaju (npr. completed order), uz idempotency.
4. Implementiraj conversion rate po varijanti u service/results DTO-u i admin endpoint-u.
5. Dodaj storefront integration za assignment/exposure bez izlaganja admin endpoint-a javnom klijentu.
6. Dodaj testove za anonimnu sesiju, prijavljenog korisnika, dupli event, nula assignment-a i conversion attribution.

Acceptance criteria:
- Admin rezultat prikazuje assignments, conversions i conversion rate za A i B.
- Dupli refresh/order ne naduvava metrike.
- PII se ne upisuje u event payload.
- Failure analytics pipeline-a ne prekida checkout ili render storefront-a.
- API, frontend type-check/build i testovi prolaze.

Verifikacija:
- unit/integration testovi za event attribution i idempotency
- `dotnet test TrendplusProdavnica.Tests/TrendplusProdavnica.Tests.csproj`
- `npm run type-check`
- `npm run build`
```

## READY TP-008 - Povezi observability metrics end-to-end

```text
Uskladi Trendplus aplikacionu telemetriju sa postojecim Prometheus/Grafana/OTel stack-om i uvedi merljive business/hot-path metrike.

Kontekst:
- Docker observability konfiguracija, dashboard-i i alert rules postoje.
- ServiceDefaults vec ukljucuje OpenTelemetry logging/metrics/tracing i OTLP exporter.
- API ima `Server-Timing`, a frontend Web Vitals reporter.
- Nema jasnog aplikacionog meter-a/custom metrics wiring-a koji odgovara imenima iz dashboard-a i alert pravila.

Zadatak:
1. Uporedi metric names u Prometheus rules/Grafana dashboard-u sa onim sto runtime stvarno emituje.
2. Izaberi jedan export put (OTLP collector/Prometheus scrape) i ucini ga funkcionalnim lokalno; ukloni ili ispravi mrtvu konfiguraciju.
3. Dodaj low-cardinality metrike za hot API latency/count, DB query duration, cache hit/miss, analytics backlog, search queue/DLQ i checkout failures.
4. Ne koristiti productId, userId, slug, orderNumber ili raw URL kao metric label.
5. Dodaj konfiguracione opcije, health signal i kratko lokalno uputstvo.
6. Dodaj test ili smoke skriptu koja dokazuje da kljucne metrike stizu do izabranog backend-a.

Acceptance criteria:
- Lokalni stack prima stvarne metrike iz API-ja.
- Dashboard queries i alert rules koriste postojeca metric names.
- Label cardinality je ogranicena i dokumentovana.
- Aplikacija radi i kada telemetry backend nije dostupan.
- Build/test prolaze.

Verifikacija:
- pokreni observability stack i API
- generisi kontrolisani test traffic
- prikazi representative metric query rezultate
- `dotnet build` i `dotnet test`
```

## READY TP-009 - Uskladi structured logs, Seq, alerting i runbooks

```text
Zavrsi operativni observability sloj tako da dokumentacija, runtime log pipeline i alerting opisuju isti sistem.

Kontekst:
- Dokumentacija tvrdi da je Seq/Slack infrastruktura spremna, ali kod nema Serilog/Seq wiring.
- ServiceDefaults vec salje OpenTelemetry logove preko OTLP-a kada je endpoint konfigurisan.
- Slack webhook je produkcioni secret i ne sme biti hardkodovan.

Zadatak:
1. Donesi i dokumentuj jednu odluku za log transport: OTel collector do log backend-a ili Serilog Seq sink. Ne odrzavaj dva paralelna puta bez razloga.
2. Implementiraj izabrani put sa structured properties, correlation/trace ID i redaction pravilima.
3. Uskladi docker compose, appsettings primere i observability dokumente sa stvarnim wiring-om.
4. Proveri AlertManager rute i dodaj bezbedan placeholder/env secret za Slack webhook.
5. Za svaki aktivni alert obezbedi runbook link i test proceduru; ukloni alert rules za metrike koje ne postoje.
6. Dodaj outage scenario: aplikacija mora nastaviti da radi kada Seq/collector/Slack nisu dostupni.

Acceptance criteria:
- Jedan jasan log pipeline radi lokalno end-to-end.
- Nema tajni, tokena, PII ili payment podataka u repozitorijumu/logovima.
- Alert test stize do test receiver-a kada je webhook obezbedjen.
- Svaki aktivni alert upucuje na postojeci runbook.
- Dokumentacija nema netacne "completed" tvrdnje.

Verifikacija:
- generisi request sa poznatim correlation ID-em i pronadji ga u log backend-u
- simuliraj nedostupan backend
- validiraj AlertManager config
- `dotnet build` i `dotnet test`
```

## READY TP-010 - Uvedi frontend automated quality gate

```text
Uvedi minimalan, brz i pouzdan frontend test/CI quality gate za Trendplus Next.js aplikaciju.

Kontekst:
- `package.json` ima build, lint i type-check skripte, ali nema test skriptu ni test framework.
- SEO metadata, URL normalizacija, API searchParams i cart state imaju dovoljno ciste logike za brze unit testove.
- Cilj nije veliki E2E suite u prvoj turi.

Zadatak:
1. Proveri kompatibilnost trenutne Next/React/TypeScript verzije i izaberi lagan unit test setup.
2. Dodaj `test` i `test:ci` skripte bez watch moda u CI-u.
3. Pokrij SEO helper-e, query param serialization/normalization, cart storage reducer/helper-e i jednu server metadata funkciju.
4. Dodaj GitHub Actions job koji radi install sa lockfile-om, type-check, test i build.
5. Ne uvoditi browser E2E framework dok unit gate nije stabilan.

Acceptance criteria:
- `npm run test:ci`, `npm run type-check` i `npm run build` prolaze lokalno i u CI-u.
- Testovi su deterministicki i ne zahtevaju spoljne servise.
- CI koristi cache i reproducibilan install.
- Dokumentovan je nacin dodavanja novih testova.

Verifikacija:
- pokreni sva tri frontend quality gate koraka
- prikazi CI workflow diff i test summary.
```

## READY TP-014 - Vrati Inventory i Recommendations API u funkcionalan runtime

```text
Uskladi javni API ugovor sa dependency injection konfiguracijom za inventory i recommendations module.

Kontekst:
- `InventoryController` i `RecommendationsController` su aktivni i dokumentuju javne/admin endpoint-e.
- `Program.cs` ima komentarisane pozive za module, a `AddInventoryServices` i `AddRecommendationServices` imaju komentarisane registracije `IInventoryService` i `IRecommendationService`.
- Kod implementacija postoji, ali endpoint aktivacija trenutno nije dokazana integration testom.

Zadatak:
1. Pregledaj razloge zbog kojih su registracije iskljucene i popravi stvarne compile/runtime ugovore u servisima.
2. Registruj servise i samo one hosted worker-e koji mogu bezbedno da rade sa konfiguracijom po okruzenju.
3. Ako neki endpoint nije spreman za produkciju, ukloni ga iz aktivnog API ugovora ili vrati eksplicitan `503` feature-disabled odgovor; ne ostavljaj DI 500.
4. Dodaj startup/endpoint integration testove koji razresavaju oba kontrolera i pozivaju po jedan read endpoint.
5. Potvrdi da su inventory write i recommendation debug/cache endpoint-i i dalje pod odgovarajucom authorization policy.

Acceptance criteria:
- Nijedan deklarisani endpoint ne pada zbog neregistrovanog servisa.
- Public read endpoint-i imaju definisano success/empty/error ponasanje.
- Worker se ne pokrece bez potrebne konfiguracije i ne obara startup.
- Authorization testovi pokrivaju sve mutacije i operational endpoint-e.
- Build i kompletni testovi prolaze.

Verifikacija:
- API startup u Development okruzenju
- fokusirani integration testovi za inventory/recommendations
- `dotnet build TrendplusProdavnica.slnx --no-restore`
- `dotnet test TrendplusProdavnica.Tests/TrendplusProdavnica.Tests.csproj`
```

## READY TP-015 - Harden admin login, refresh, logout i browser session

```text
Zameni privremeni Trendplus admin auth mehanizam produkciono bezbednim session/token tokom.

Kontekst:
- `AdminAuthService` poredi konfiguracioni email/password i vraca hardkodovanog user-a ID 1.
- `RefreshTokenAsync` ne proverava token store, korisnika ni opoziv; bilo koji neprazan token moze dobiti novi admin access token.
- `ValidateRefreshToken` proverava samo duzinu.
- Frontend cuva access token u `localStorage`, ne obnavlja pouzdano korisnika na reload-u i nema backend logout/revoke.

Zadatak:
1. Uvedi persistent admin korisnike sa jakim password hashing-om, statusom i ulogama; bez default produkcionih kredencijala u kodu.
2. Uvedi hashed refresh token store sa expiry, rotation, reuse detection i revocation; dodaj logout/revoke endpoint.
3. Login zastiti rate limiting-om i generickim greskama koje ne otkrivaju postojanje naloga.
4. Izaberi bezbedan browser transport: preferiraj `HttpOnly`, `Secure`, `SameSite` cookie za refresh/session i kratko-ziveci access token; dodaj CSRF zastitu ako cookie ucestvuje u mutacijama.
5. Frontend na mount-u mora validirati/obnoviti sesiju preko `/me`, a istekao ili opozvan token mora zavrsiti na login stranici.
6. Dodaj bootstrap proceduru za prvog admina preko secret/env ili jednokratne komande, bez seedovanog password-a.
7. Dodaj integration testove za login, pogresan password, disabled user, valid refresh, rotation, reuse, expiry, logout i role policy.

Acceptance criteria:
- Proizvoljan refresh token nikada ne izdaje admin JWT.
- Logout i password/user disable opozivaju aktivne refresh sesije.
- Access/refresh tokeni nisu dostupni kroz `localStorage`.
- Nema hardkodovanih credential-a, JWT secret-a ili produkcionog admin identiteta.
- Authorization integration testovi i frontend auth tok prolaze.

Verifikacija:
- fokusirani auth unit/integration testovi
- rucni login-refresh-reload-logout smoke tok
- `dotnet test` i frontend `type-check`/`build`
```

## READY TP-016 - Dodaj backend CI quality gate

```text
Dodaj GitHub Actions quality gate za Trendplus .NET solution tako da build, test i dependency rizici vise ne zavise od lokalnih log fajlova.

Kontekst:
- `.github/workflows` nema aktivan workflow.
- Solution sadrzi API, AppHost, Application, Domain, Infrastructure, ServiceDefaults i kanonski test projekat.
- TP-002 resava trenutne vulnerability warning-e; CI posle toga ne sme da ih ignorise.

Zadatak:
1. Dodaj workflow za pull_request i push na main sa minimalnim permissions.
2. Koristi verziju .NET SDK-a uskladjenu sa repo target framework-om i reproducibilan restore/cache.
3. Pokreni restore, build bez ponovnog restore-a i kanonski test projekat sa TRX rezultatom.
4. Dodaj dependency vulnerability audit koji pada na high/critical i jasno prijavljuje moderate nalaze prema dogovorenoj politici.
5. Objavi test rezultate/coverage samo ako se to moze uraditi bez tajni i nepouzdanih third-party action-a.
6. Ne pokretati provider PostgreSQL test dok TP-017 ne obezbedi ephemeral bazu.

Acceptance criteria:
- Workflow radi na cistom GitHub runner-u bez lokalnih fajlova i servisa.
- Minimalne dozvole su eksplicitne.
- Build/test failure i high vulnerability zaustavljaju PR gate.
- Nema duplog test projekta ili stale log artefakata u CI-u.

Verifikacija:
- validiraj workflow sintaksu
- pokreni lokalni ekvivalent svih koraka
- nakon push-a potvrdi green Actions run i zabelezi trajanje/cache hit.
```

## READY TP-017 - Implementiraj stvarni PostgreSQL checkout lock i ukljuci concurrency test

```text
Popravi checkout concurrency zastitu i ukljuci pravi PostgreSQL integration test za paralelne zahteve.

Kontekst:
- `CheckoutConcurrencyPostgresIntegrationTests.PlaceOrder_ParallelSameRequest_OnPostgreSql_CreatesSingleOrder` je preskocen.
- Skip razlog navodi nestabilan EF model bootstrap oko `orders.UpdatedAtUtc`/store-generated metadata.
- In-memory test ne moze verno dokazati PostgreSQL locking i transakcijsko ponasanje.
- `CheckoutService.LockProductVariantsAsync` trenutno izvrsava obican LINQ `ToListAsync`; nema `SELECT ... FOR UPDATE` niti drugi provider lock, iako komentari tvrde da je tok race-proof.

Zadatak:
1. Implementiraj stvarni PostgreSQL row-level lock za sve varijante u stabilnom ID redosledu unutar iste transakcije; ne oslanjaj se na tracking kao lock.
2. Reprodukuj provider bootstrap gresku i popravi EF model/migraciju bez test-only odstupanja od produkcionog schema ugovora.
3. Zameni hardkodovanu lokalnu PostgreSQL konekciju izolovanim ephemeral pristupom pogodnim za lokalni rad i CI (service container ili ekvivalent).
4. Ukloni `Skip` i obezbedi pouzdano kreiranje i gasenje test baze/schema-e.
5. Pokrij paralelni isti request, paralelne razlicite cart/idempotency zahteve za isti SKU, insufficient stock, lock timeout i rollback nakon greske.
6. Testovi moraju biti paralelno bezbedni i ne smeju dirati development/production bazu.

Acceptance criteria:
- Provider test za isti request daje jedan `Success`, jedan `AlreadyProcessed` i tacno jednu porudzbinu.
- Dva razlicita checkout-a ne mogu prodati vise od raspolozivog stock-a; stock nikad nije negativan.
- SQL/provider dokaz potvrdjuje da je lock zaista uzet pre stock provere i izmene.
- Test radi lokalno i u CI-u bez hardkodovanog password-a.
- Cleanup radi i posle failed assertion-a.

Verifikacija:
- fokusirani PostgreSQL test suite ponovljen najmanje 10 puta bez flaky rezultata
- kompletan `dotnet test`
- dokumentuj kako se provider test pokrece lokalno.
```

## READY TP-018 - Napravi admin workspace za A/B eksperimente

```text
Dodaj Trendplus admin radnu povrsinu za kreiranje, upravljanje i pracenje A/B eksperimenata koristeci postojeci admin API.

Kontekst:
- `ExperimentsAdminController` vec ima list/detail/create/update/activate/pause/complete/cancel/results endpoint-e.
- Admin sidebar i frontend API client nemaju experiments sekciju.
- TP-006 hardenuje assignment, a TP-007 dodaje conversion metrike; UI mora degradirati kada ta polja jos nisu dostupna.

Zadatak:
1. Dodaj typed admin API klijent i `/admin/experiments` list/detail/create-edit stranice.
2. Prikazi status, tip, traffic split, varijante, datume i lifecycle akcije sa jasnim confirmation koracima.
3. Results view prikazuje assignments i dostupne conversion metrike bez izmisljanja statisticke znacajnosti.
4. Validiraj traffic split, nazive varijanti i dozvoljene status tranzicije i na klijentu i kroz API greske.
5. Sacuvaj filtere/paginaciju u URL-u i koristi postojeci admin vizuelni jezik.
6. Dodaj testove za DTO mapping, status akcije, loading/empty/error i unauthorized stanje.

Acceptance criteria:
- Admin moze zavrsiti ceo lifecycle bez direktnih API poziva.
- Nedozvoljene tranzicije nisu dostupne i server greske su razumljivo prikazane.
- Refresh/deep link cuvaju aktivni eksperiment i filtere.
- Nema optimistic prikaza koji tvrdi uspeh pre server potvrde.
- Type-check, testovi i build prolaze.

Verifikacija:
- rucni draft-activate-pause-complete/cancel tok
- frontend testovi
- `npm run type-check` i `npm run build`
```

## READY TP-019 - Napravi merchandising rules admin workspace

```text
Dodaj operativni admin UI za Trendplus merchandising pravila, sa bezbednim preview-em uticaja na listing.

Kontekst:
- `MerchandisingRulesAdminController` podrzava list/detail/create/update/delete i cache invalidate.
- Listing servis vec primenjuje merchandising pravila.
- Admin panel nema ekran za pravila, pa se feature moze koristiti samo direktnim API pozivima.

Zadatak:
1. Dodaj typed API klijent i `/admin/merchandising` list/create/edit ekran.
2. Podrzi tipove pravila, prioritet, aktivnost, vremenski prozor i product/category/brand targeting prema stvarnim DTO ugovorima.
3. Dodaj konflikt upozorenja za preklopljena pin/boost/bury pravila i jasno prikazi red evaluacije.
4. Dodaj read-only preview rezultata za izabranu listing kategoriju; ako backend nema preview endpoint, uvedi uzak admin endpoint koji ne menja podatke.
5. Mutacije zahtevaju confirmation za delete/deactivate i moraju osveziti cache kroz jedan kanonski servisni tok.
6. Dodaj loading/empty/error/unauthorized i validation testove.

Acceptance criteria:
- Trgovac moze kreirati, pregledati, izmeniti, deaktivirati i obrisati pravilo iz UI-a.
- Preview jasno pokazuje koje je pravilo promenilo redosled proizvoda.
- Invalidni target-i i datumi se odbijaju pre mutacije i na serveru.
- Cache se invalidira tacno jednom posle uspesne izmene.
- Frontend i backend testovi prolaze.

Verifikacija:
- smoke tok za pin, boost i bury pravilo
- potvrdi listing rezultat pre/posle aktivacije
- `dotnet test`, `npm run type-check`, `npm run build`
```

## READY TP-020 - Objedini content i SEO operacije u admin panelu

```text
Napravi koherentan Trendplus admin workspace za category SEO i page content koji vec imaju backend kontrolere, ali nemaju kompletne UI povrsine.

Kontekst:
- Postoje admin kontroleri za CategorySeoContent, BrandPageContent, CollectionPageContent, StorePageContent, Stores i TrustPages.
- Admin trenutno ima stranice za brands/collections, ali nema jedinstven content/SEO tok niti stranice za stores/trust/category SEO.

Zadatak:
1. Inventarisi stvarne DTO ugovore i napravi typed klijent bez `unknown` payload-a.
2. Dodaj `/admin/content` ulaz sa sekcijama Category SEO, Brand, Collection, Stores i Trust pages.
3. Omoguci draft/edit/publish gde API to podrzava, uz canonical preview, robots polje, title/description length upozorenja i strukturisan FAQ/module editor.
4. Dodaj preview link ka javnoj kanonskoj stranici i jasno razdvoji save od publish akcije.
5. Sanitizuj rich HTML/JSON sadrzaj na serveru; ne renderuj neproveren admin unos preko nesigurnog HTML-a.
6. Dodaj dirty-form guard, validation, loading/empty/error i role testove.

Acceptance criteria:
- Svi navedeni content API-ji imaju dostupnu i pronalazivu admin povrsinu.
- Draft izmene nisu javne pre publish-a.
- Canonical/robots/SEO metadata preview odgovara storefront helper-ima.
- Nevalidan ili opasan content payload se odbija/sanitizuje.
- Type-check, testovi i build prolaze.

Verifikacija:
- napravi i objavi po jedan category/store/trust content primer
- proveri javni metadata i prikaz
- `dotnet test`, `npm run type-check`, `npm run build`
```

## READY TP-021 - Uvedi product review submission i moderation tok

```text
Zavrsi Trendplus product reviews feature od verifikovanog poziva do objave i rating agregata.

Kontekst:
- `ProductReview`, `ProductRating`, migracije, seed i read projekcija na PDP-u vec postoje.
- Nema public submit API-ja, moderation admin API-ja ni storefront forme; trenutni reviews dolaze samo iz seed podataka.

Zadatak:
1. Definisi review invitation/verification tok vezan za delivered order item i potpisan, ogranicen token; ne oslanjaj se samo na javni order number/email.
2. Dodaj idempotentan submit endpoint sa rating validacijom, optional title/body, consent i anti-abuse/rate limit zastitom.
3. Novi review je `Pending`; dodaj admin moderation list/detail/publish/reject akcije sa razlogom.
4. Rating agregat azuriraj transakcijski samo iz Published reviews i ispravno ga preracunaj pri publish/reject promeni.
5. Dodaj PDP formu za validan invitation token i admin moderation ekran.
6. Sanitizuj tekst, ne izlagati PII, i dodaj testove za replay, pogresan proizvod, unpublished review i agregat.

Acceptance criteria:
- Samo validan, neistekao invitation moze poslati jedan review za kupljeni proizvod/order item.
- Pending/rejected reviews nisu javni niti ulaze u JSON-LD/rating.
- Publish/reject je auditabilan i agregat ostaje tacan pri konkurentnim akcijama.
- Storefront ima jasna success/error/expired stanja.
- Backend/frontend testovi prolaze.

Verifikacija:
- end-to-end delivered-order -> invitation -> submit -> publish -> PDP tok
- concurrency/idempotency testovi
- `dotnet test`, `npm run type-check`, `npm run build`
```

## READY TP-022 - Dodaj centralni admin audit trail

```text
Uvedi neizmenjiv i pretraziv audit trail za sve Trendplus admin mutacije.

Kontekst:
- Admin kontroleri menjaju proizvode, varijante, medije, porudzbine, content, merchandising i eksperimente.
- Trenutno ne postoji zajednicki AuditLog model niti admin ekran za istragu promena.

Zadatak:
1. Definisi append-only audit model sa actor ID/role, action, resource type/id, timestamp, correlation ID, outcome i bezbednim before/after summary-jem.
2. Implementiraj centralni filter/middleware ili application decorator tako da se audit ne kopira rucno u svaki kontroler.
3. Redactuj password-e, token-e, PII, payment i velike content payload-e; cuvaj samo dozvoljene promene.
4. Audit zapis i poslovna mutacija moraju imati jasno definisanu transakcijsku semantiku i failure ponasanje.
5. Dodaj read-only admin endpoint/UI sa filterima po actor-u, akciji, resursu, datumu i correlation ID-u.
6. Zabrani update/delete audit zapisa kroz aplikaciju i dokumentuj retention/export politiku.

Acceptance criteria:
- Sve uspesne i odbijene admin mutacije daju audit dogadjaj bez dupliranja.
- Tajne i osetljivi podaci nisu prisutni u zapisu.
- Audit citanje je admin-only, paginirano i ograniceno.
- Testovi dokazuju coverage za najmanje product, order, content i auth akcije.

Verifikacija:
- integration testovi za success/failure/redaction
- rucno pronadji jednu promenu preko correlation ID-a
- `dotnet build` i `dotnet test`
```

## READY TP-023 - Napravi demand planning admin workspace

```text
Izlozi postojeci Trendplus demand prediction servis kroz operativni admin ekran za nabavku, bez predstavljanja aproksimacija kao garantovane prognoze.

Kontekst:
- Admin-protected analytics endpoint-i vec podrzavaju single/bulk prediction, procurement po proizvodu, seasonality po kategoriji i top-demand listu.
- Web admin nema demand/procurement ekran.
- Dokumentacija navodi ogranicenja za kratku istoriju, nove proizvode, outlier-e i sezonalnost.

Zadatak:
1. Dodaj typed frontend API klijent i `/admin/analytics/demand` workspace.
2. Prikazi top-demand proizvode, prognozu po periodu, size distribution/procurement preporuku i category seasonality.
3. Svaka prognoza mora prikazati horizon, istorijski prozor, confidence/quality signal i upozorenje kada nema dovoljno podataka.
4. Dodaj bulk input sa limitima i CSV export rezultata; ne dodavati import/mutaciju zaliha u ovoj turi.
5. Filtere i period cuvaj u URL-u i dodaj loading/empty/error/unauthorized stanja.
6. Dodaj testove za DTO mapping, decimal/date format, insufficient-history i partial bulk failure.

Acceptance criteria:
- Nabavka moze dobiti objasnjivu preporuku bez direktnog API poziva.
- UI jasno razlikuje istorijske podatke, prognozu i safety-stock dodatak.
- Nema automatske izmene inventory-ja.
- Veliki bulk zahtevi su ograniceni i ne blokiraju UI bez feedback-a.
- Type-check, testovi i build prolaze.

Verifikacija:
- single, bulk, seasonality i insufficient-data smoke scenariji
- `npm run type-check`, frontend testovi i `npm run build`
```

## READY TP-026 - Popravi admin login, session hydration i API origin ugovor

```text
Popravi neposredne Trendplus admin frontend bagove zbog kojih login i reload sesije trenutno nisu pouzdano dostupni. Ovo je funkcionalna popravka pre sireg auth hardening-a iz TP-015.

Kontekst:
- Zajednicki `/admin` layout za neautentifikovanog korisnika uvek vraca `null`, ukljucujuci `/admin/login`, pa login forma ne moze da se renderuje.
- Na mount-u se iz localStorage ucita samo token, ali `user` ostaje null; `isAuthenticated` zato ostaje false i validna sesija posle reload-a izgleda odjavljeno.
- Admin klijent koristi `NEXT_PUBLIC_API_URL` i fallback `http://localhost:5000`, dok ostatak aplikacije i dokumentacija koriste `NEXT_PUBLIC_API_BASE_URL` i `https://localhost:7002/api`.
- Next config nema API rewrite, a API nema eksplicitan CORS setup za browser admin pozive.
- Login forma je unapred popunjena demo kredencijalima i prikazuje password u UI-u.

Zadatak:
1. Odvoji public admin auth layout od protected admin shell-a ili eksplicitno renderuj login bez sidebar guard-a.
2. Pri inicijalizaciji validiraj sesiju preko `/api/admin/auth/me`, ucitaj user-a i tek onda donesi redirect odluku; ukloni flash/redirect petlju.
3. Uvedi jedan kanonski API base URL helper za storefront i admin, sa jasnim pravilom da li vrednost sadrzi `/api`.
4. Izaberi i implementiraj browser connectivity ugovor: same-origin Next rewrite/BFF ili restriktivan CORS po konfigurisanom origin-u. Ne koristiti `AllowAnyOrigin` sa credential-ima.
5. Ukloni demo email/password vrednosti iz forme i dokumentuj development bootstrap van bundle-a.
6. Dodaj testove za login render, successful login, reload validne sesije, invalid token, 401 i pogresnu API konfiguraciju.

Acceptance criteria:
- `/admin/login` prikazuje formu u cistoj sesiji.
- Uspesan login otvara admin, a reload ne odjavljuje validnog korisnika.
- Invalid/expired token zavrsava na login-u bez beskrajne petlje.
- Admin i storefront koriste jedan dokumentovan API origin ugovor.
- Produkcioni bundle ne sadrzi demo password.

Verifikacija:
- browser smoke test iz cisteg storage-a i posle reload-a
- network provera preflight/rewrite poziva
- `npm run type-check`, frontend testovi i `npm run build`
```

## READY TP-027 - Zastiti order lookup od enumeracije i PII curenja

```text
Zatvori javno curenje podataka porudzbine kroz predvidiv order number i uvedi bezbedan confirmation/tracking pristup.

Kontekst:
- `GET /api/orders/{orderNumber}` je anoniman i vraca ime, email, telefon, punu adresu, payment/delivery metod, stavke i iznose.
- Order number je predvidiv: `TP-{godina}-{sekvencijalni ID}`.
- Storefront `/porudzbina/[orderNumber]` poziva endpoint samo sa order brojem i prikazuje sve PII podatke.
- Ljudski citljiv order number treba da ostane referenca, ali ne sme biti credential.

Zadatak:
1. Ukloni pristup punom OrderDto-u samo na osnovu order number-a.
2. Posle checkout-a izdaj kriptografski jak, ogranicen confirmation/tracking token vezan za order, sa expiry i mogucnoscu opoziva/rotacije.
3. Za kasniji tracking koristi autentifikovan customer nalog ili order number plus zaseban dokaz koji nije moguce enumerisati; ne koristiti email kao jedinu tajnu.
4. Razdvoji minimalni public confirmation DTO od admin OrderDto-a i vracaj samo podatke potrebne tom prikazu.
5. Dodaj `noindex`, `no-store`, odgovarajuci Referrer-Policy i zabranu edge/output cache-a za confirmation stranicu i API.
6. Genericki odgovori moraju spreciti proveru da li order postoji; dodaj rate limit i audit neuspesnih pokusaja.
7. Dodaj testove za validan token, pogresan order, istekao/opozvan/replay token, enumeraciju i cache headere.

Acceptance criteria:
- Poznavanje ili pogadjanje order number-a ne otkriva nijedan podatak kupca.
- Confirmation link radi samo sa validnim ogranicenim dokazom.
- PII odgovor se nikada ne kesira niti indeksira.
- Admin order pristup ostaje pod Admin policy-jem i nije regresiran.
- Postojeci checkout UX ima bezbedan success redirect.

Verifikacija:
- integration security testovi za order endpoint
- browser confirmation smoke test
- proveri response/cache/robots/referrer headere
- `dotnet test`, `npm run type-check`, `npm run build`
```

## READY TP-028 - Zastiti supplier i shoe-type sales analytics endpoint-e

```text
Zatvori neautorizovan pristup Trendplus poslovnim analytics izvestajima.

Kontekst:
- Minimal API endpoint-i `/api/analytics/supplier-sales-stats` i `/api/analytics/shoe-type-sales-stats` nemaju `RequireAuthorization`.
- Odgovori sadrze revenue, order count, conversion rate, market totals i supplier/category performanse.
- Frontend ih koristi iz admin analytics stranica, pa javni pristup nije potreban.

Zadatak:
1. Primeni `ApiAuthorizationPolicies.Admin` ili precizniju read-analytics policy na oba endpoint-a.
2. Proveri sve analytics/demand endpoint-e i napravi eksplicitnu matricu: anonymous ingest, admin reports, operational diagnostics.
3. Azuriraj admin API klijent da salje auth kroz kanonski session mehanizam.
4. Osiguraj da ovi odgovori nemaju public output/edge cache i dodaj `Cache-Control: private, no-store` gde je potrebno.
5. Dodaj authorization integration testove za anonymous 401, non-admin 403 i admin 200 za oba izvestaja.

Acceptance criteria:
- Anonymous i customer role ne mogu citati poslovne izvestaje.
- Admin UI i dalje ucitava oba izvestaja.
- Response nije dostupan iz public cache-a.
- OpenAPI jasno oznacava security zahtev.

Verifikacija:
- fokusirani authorization integration testovi
- pregled endpoint metadata/OpenAPI security ugovora
- `dotnet build` i `dotnet test`
```

## READY TP-029 - Ukloni exception detail disclosure iz javnog API-ja

```text
Uvedi centralno, bezbedno mapiranje gresaka i prestani da vracas interne exception poruke javnim klijentima.

Kontekst:
- `Program.cs` ima oko 48 `Results.Problem(detail: ex.Message, 500)` grana, plus vise `message = ex.Message` odgovora.
- Exception tekst moze otkriti SQL/provider detalje, identifikatore, konfiguraciju ili unutrasnju strukturu sistema.
- Admin kontroleri imaju poseban exception filter za poznate domenske greske, ali javni Minimal API tok je rucno dupliran.

Zadatak:
1. Dodaj centralni `IExceptionHandler`/ProblemDetails ugovor za unexpected, validation, not-found, conflict i cancellation slucajeve.
2. U produkciji vracaj stabilan error code, genericku poruku i correlation/trace ID; pun exception ostaje samo u structured log-u.
3. Zadrzi samo eksplicitno bezbedne validation poruke koje ne otkrivaju postojanje tudjih resursa ili interne detalje.
4. Ukloni siroke try/catch blokove iz endpoint-a gde centralni handler moze pouzdano da ih zameni.
5. Ne pretvaraj cancellation u 500 i ne loguj ocekivani 4xx kao error.
6. Dodaj contract testove za produkcioni i development response, correlation ID i log behavior.

Acceptance criteria:
- Nijedan unexpected 500 response ne sadrzi `ex.Message`, stack trace, SQL ili connection detalje.
- Klijent dobija dosledan RFC ProblemDetails oblik.
- 400/404/409 i cancellation zadrzavaju ispravnu semantiku.
- Jedan correlation ID povezuje response i server log.

Verifikacija:
- automatizovan scan response-a za simulirane DB i provider greske
- `rg` vise ne nalazi public 500 `detail: ex.Message` obrasce
- `dotnet build` i `dotnet test`
```

## READY TP-030 - Uskladi checkout totals i validaciju sa delivery metodom

```text
Popravi end-to-end checkout ugovor za Courier i StorePickup tako da korisnik vidi i plati isti iznos koji server upise.

Kontekst:
- Checkout summary uvek racuna `DeliveryMethod.Courier`, pre nego sto korisnik izabere metod.
- Frontend uvek zahteva adresu, grad i postanski broj, cak i za StorePickup.
- Backend validira adresu samo za Courier, ali pri kreiranju order-a bezuslovno poziva `.Trim()` nad adresnim poljima.
- StorePickup nema izbor store-a niti jasnu pickup lokaciju.
- UI nudi `CardPlaceholder` kao stvarni payment izbor iako nema payment gateway toka.

Zadatak:
1. Prosiri summary ugovor da prima delivery metod i, za pickup, store ID; server je jedini izvor delivery i total iznosa.
2. Courier zahteva validnu adresu; StorePickup zahteva aktivnu pickup prodavnicu i ne dereferencira prazna adresna polja.
3. Frontend dinamicki prikazuje relevantna polja i osvezava server summary pri promeni metoda/prodavnice.
4. Ukloni ili jasno onemoguci `CardPlaceholder` dok ne postoji payment authorization tok; ne kreirati order koji izgleda placeno.
5. Sacuvaj delivery/pickup snapshot potreban za fulfillment i confirmation prikaz.
6. Dodaj testove za oba metoda, neaktivnu prodavnicu, promenu total-a, null adresu i tampered client amount.

Acceptance criteria:
- Prikazani i upisani subtotal/delivery/total su identicni za oba delivery metoda.
- StorePickup se moze zavrsiti bez kurirske adrese, ali ne bez validne prodavnice.
- Server ignorise svaki client-supplied iznos i sam racuna total.
- Nedostupan payment metod nije moguce poslati kao uspesan checkout.

Verifikacija:
- Courier i StorePickup browser smoke tokovi
- backend unit/integration testovi
- `dotnet test`, `npm run type-check`, `npm run build`
```

## READY TP-031 - Popravi category snapshot podatke na order item-u

```text
Popravi korupciju category naziva u novim porudzbinama i zastiti shoe-type analytics od laznih `Default` kategorija.

Kontekst:
- Checkout upisuje pravi `CategoryIdSnapshot`, ali `CategoryNameSnapshot` postavlja na literal `Default` za svaki pozitivan category ID.
- Shoe-type analytics veruje snapshot nazivu kada ID postoji, pa sve nove kategorije mogu biti prikazane kao `Default`.
- Checkout query trenutno ucitava product i brand, ali ne i primarnu category vrednost potrebnu za snapshot.

Zadatak:
1. Ucitaj primarnu kategoriju u istom checkout consistency toku i upisi stvarni category name snapshot uz ID.
2. Ako kategorija nedostaje, checkout mora imati eksplicitnu poslovnu odluku: odbij order ili upisi kontrolisani unknown marker sa telemetry upozorenjem; ne upisivati `Default`.
3. Dodaj invariant/test da snapshot ID i name pripadaju istoj kategoriji u trenutku order-a.
4. Analiziraj postojece `Default` zapise i napravi bezbedan backfill/report proceduru; ne prepisivati istoriju bez evidencije.
5. Dodaj analytics regression test sa najmanje dve kategorije razlicitog imena.

Acceptance criteria:
- Nove order stavke imaju tacan, nepromenljiv category ID/name par.
- Shoe-type izvestaj razdvaja stvarne category nazive.
- Nema literalnog `Default` snapshot-a u checkout kodu.
- Backfill je idempotentan i auditabilan ili je istorijski rizik jasno dokumentovan.

Verifikacija:
- checkout snapshot unit/integration test
- shoe-type analytics regression test
- SQL/report provera postojecih `Default` zapisa
- `dotnet build` i `dotnet test`
```

## READY TP-032 - Harden anonymous analytics event ingestion

```text
Zastiti `/api/analytics/track` od neogranicenog unosa, nevalidnih dogadjaja i kvarenja analytics podataka.

Kontekst:
- Endpoint je namerno anonymous, ali nema vidljiv rate limit, payload size limit ni idempotency/deduplication.
- Servis direktno cuva EventType, ProductId, SessionId, PageUrl, ReferrerUrl i proizvoljan `EventData` jsonb string.
- Nevalidan JSON ili prevelik payload moze izazvati DB greske/trosak; bot moze lazno naduvati conversion metrike.
- User ID citanje se oslanja na `sub`, dok JWT konfiguracija koristi NameIdentifier claim mapping.

Zadatak:
1. Definisi event schema po tipu i validiraj obavezna/dozvoljena polja, enum vrednost, product postojanje i URL origin.
2. Ograniciti request i `EventData` velicinu, zahtevati validan JSON i odbaciti nepoznata/high-cardinality polja.
3. Dodaj event ID/idempotency prozor za retry deduplikaciju i rate limit po kombinaciji session/IP uz proxy-aware IP konfiguraciju.
4. Nemoj verovati client-supplied `OrderCompleted`; conversion dogadjaj mora nastati iz server-side order toka.
5. Ispravno procitaj autentifikovani user claim bez `long.Parse` 500 greske.
6. Definisi IP retention/anonymization i ne loguj raw session/PII vrednosti.
7. Dodaj testove za malformed JSON, oversized payload, fake conversion, duplicate, rate limit i validan event.

Acceptance criteria:
- Anonymous klijent ne moze proizvoljno kreirati order conversion.
- Malformed/oversized/duplicate zahtevi imaju kontrolisan 4xx/429 odgovor bez DB greske.
- Validni storefront eventi i dalje prolaze sa stabilnim event ID-em.
- User attribution radi za stvarni JWT claim ugovor.

Verifikacija:
- ingestion contract i rate-limit integration testovi
- load test sa kontrolisanim limitom
- `dotnet build` i `dotnet test`
```

## READY TP-033 - Popravi postojeci admin UI contract drift i mrtve linkove

```text
Popravi vec prikazane Trendplus admin funkcije koje trenutno vode na nepostojece rute ili prikazuju netacne podatke.

Kontekst:
- Products ekran linkuje `/admin/products/new` i `/admin/products/{id}`, ali postoje samo `products/page.tsx`.
- Orders ekran linkuje `/admin/orders/{id}`, ali detail ruta ne postoji.
- Oba ekrana uvek traze page 1; pagination dugmad su placeholder.
- UI koristi `$` iako backend/cart valuta koristi RSD.
- Dashboard racuna Pending Orders i Active Products samo iz poslednjih 5 redova, ne iz ukupnog skupa.
- Product `isActive` mapping tretira svaki non-Archived proizvod, ukljucujuci Draft, kao Active.
- UI statusi `confirmed`/`delivered` ne odgovaraju domenskim `Paid`/`Completed` vrednostima.

Zadatak:
1. Dodaj stvarne product new/detail/edit i order detail rute ili ukloni linkove dok funkcija nije dostupna; nijedan CTA ne sme zavrsiti na 404.
2. Implementiraj server-backed paginaciju i URL state za products/orders, sa ispravnim page/pageSize granicama.
3. Koristi zajednicki RSD formatter i stvarni currency podatak; ukloni hardkodovani `$`.
4. Dashboard KPI-jeve dobavi iz agregatnog admin stats endpoint-a, ne iz sample liste.
5. Mapiraj product status direktno iz domenskog statusa i uskladi order status opcije sa server ugovorom iz TP-034.
6. Dodaj empty/error/loading i route/DTO contract testove.

Acceptance criteria:
- Svi admin linkovi vode na postojecu i funkcionalnu rutu.
- KPI vrednosti ostaju tacne kada ima vise od 5 zapisa.
- Draft/Published/Archived i Pending/Paid/Shipped/Completed/Cancelled su prikazani bez izmisljenih statusa.
- Svi iznosi koriste RSD format.
- Paginacija radi i cuva stanje posle reload/back navigacije.

Verifikacija:
- admin browser smoke mapa svih CTA linkova
- frontend route/contract testovi
- `npm run type-check`, testovi i `npm run build`
```

## READY TP-034 - Uvedi order status state machine i optimistic concurrency

```text
Zastiti Trendplus order lifecycle od proizvoljnih ili izgubljenih status promena.

Kontekst:
- `OrderAdminService.UpdateStatusAsync` parsira bilo koji enum i direktno ga upisuje bez provere dozvoljene tranzicije.
- Completed ili Cancelled order moze biti vracen u Pending/Paid ili poslat u drugi nevalidan status.
- `UpdateOrderStatusRequest.Notes` se prihvata, ali se nigde ne cuva niti koristi.
- Nema version/concurrency token-a u admin order update ugovoru.
- Trenutni frontend nudi statuse koji ne postoje u domenskom enum-u.

Zadatak:
1. Definisi order state machine u domenskom/application sloju sa eksplicitnim dozvoljenim tranzicijama i terminalnim statusima, uz odluku za COD tok.
2. Centralizuj tranziciju; kontroler/service ne smeju direktno dodeljivati Status.
3. Dodaj optimistic concurrency token i vrati 409 kada je order promenjen posle ucitavanja.
4. Sacuvaj transition timestamp, actor i optional sanitizovanu napomenu kroz order history/audit ugovor; ne ignorisati Notes.
5. Frontend treba da dobije dozvoljene sledece akcije iz server ugovora ili zajednicke mape, ne hardkodovan paralelni enum.
6. Dodaj testove za svaki dozvoljen i zabranjen prelaz, terminal state, concurrent update i invalid status.

Acceptance criteria:
- Nevalidan status skok ili povratak iz terminalnog stanja nije moguc ni direktnim API pozivom.
- Dve paralelne admin izmene ne mogu tiho pregaziti jedna drugu.
- Svaka promena ima vreme, actor-a i razlog/napomenu kada je poslata.
- UI nudi samo stvarno dozvoljene akcije.

Verifikacija:
- exhaustive state-transition unit test tabela
- authorization/concurrency integration testovi
- order admin browser smoke tok
- `dotnet test`, `npm run type-check`, `npm run build`
```

## Blocked / ceka preduslov

### BLOCKED TP-011 - Snimi performance baseline

Blokirano dok nisu dostupni reprezentativan PostgreSQL dataset/connection string, pokrenut API/storefront i staging ili production URL-ovi. Kada su preduslovi dostupni, koristiti `PERFORMANCE_MEASUREMENT_CHECKLIST.md`, `scripts/performance/postgres-explain-baseline.sql` i `scripts/performance/measure-ttfb.ps1` i popuniti before tabelu za home, PLP i PDP.

Acceptance signal je sacuvan `EXPLAIN (ANALYZE, BUFFERS)`, cold/warm origin i edge TTFB, Web Vitals/Lighthouse mobile rezultat i numericki top 3 bottleneck-a.

### BLOCKED TP-012 - Query/cache/media tuning po izmerenim bottleneck-ovima

Zavisi od TP-011. Ne dodavati partial indekse, precomputed read modele, cache key promene ili media prioritization naslepo. Posle baseline-a napraviti odvojene promptove samo za top bottleneck-ove sa before/after metrikama.

### BLOCKED TP-013 - Production Slack i retention konfiguracija

Code/config priprema je u TP-009, ali stvarno ukljucivanje zahteva produkcioni webhook/secret, retention politiku, backup destinaciju i vlasnika alert kanala.

### BLOCKED TP-024 - Storefront recommendation sekcije

Zavisi od TP-014. Kada recommendation servis i endpoint-i prodju runtime/integration proveru, dodati zaseban prompt za related products na PDP-u i homepage recommendations. Prompt mora pokriti typed API klijent, empty fallback, cache ugovor, server-first render, analytics impression/click evente i zastitu od prikaza nevidljivih ili nedostupnih proizvoda.

### BLOCKED TP-025 - Inventory operations admin workspace

Zavisi od TP-014. Kada inventory servis i write endpoint-i prodju provider i authorization testove, dodati admin ekran za stock pregled, count, adjustment, reserve/release istoriju i optimistic concurrency. Ne spajati ovo sa TP-023 demand planning ekranom dok inventory write model nije stabilan.

## Done

- SEO route contract je migriran na `/kategorije/[slug]`; preostali artefakti su izdvojeni u TP-004.
- PDP breadcrumb-i koriste kanonske category rute.
- BreadcrumbList JSON-LD vec postoji u zajednickoj Breadcrumbs komponenti; stari SEO future item je uklonjen.
- ProductCard je server-first bez starog state hydration troska; performance Phase 2 stavka je zatvorena.
- Merchandising je povezan sa listing servisima; stari pending guide je zastareo.
- Search queue, DLQ i `SearchIndexEventLog` persistence su implementirani.
- A/B Experiment/Assignment modeli i migracije vec postoje; ne generisati zastarelu `AddExperimentsAndAssignments` migraciju iz guide-a.
- Listing compatibility endpoint-i vec delegiraju kanonskom `ProductListingReadService` toku; ne uvoditi treci listing engine.

## Removed / promenjeno

- Stari prompt `Run the performance measurement baseline` je pomeren u `BLOCKED TP-011` jer bez podataka i URL-ova nije odmah izvodljiv.
- Stari prompt `Finish observability wiring` je bio prevelik i netacan; zamenjen je sa TP-008, TP-009 i TP-013.
- Stari prompt `Triage package warning drift` je promovisan u P0 TP-002 jer trenutni build prijavljuje poznate high/moderate ranjivosti.
- Stari SEO pagination prompt je prosiren u TP-003 sa search/filter/sort pravilima i testovima.
- TP-017 je promovisan u P0 nakon potvrde da postojeci `LockProductVariantsAsync` ne izdaje PostgreSQL row lock.
- TP-026 do TP-034 su dodati iz staticke bug analize aktuelnog koda; nisu prepisani iz starih future/next lista.
- TODO komentari u `page.template.tsx` nisu feature backlog; njihovo uklanjanje je deo TP-004.
- Next Steps iz `TrendplusProdavnica.Web/STATUS.md`, merchandising guide-a, A/B migration guide-a i sacuvanih audit logova nisu aktivni queue dok ih trenutni kod ne potvrdi.

## Poslednja provera

- `dotnet build TrendplusProdavnica.slnx --no-restore`: prolazi, 23 security upozorenja (TP-002).
- `dotnet test TrendplusProdavnica.Tests/TrendplusProdavnica.Tests.csproj --no-restore`: prethodno provereno, 30 passed, 2 skipped, 0 failed.
- `npm run type-check`: pada sa 2 greske u `src/lib/api/analytics.ts` (TP-001).
- Staticka bug revizija: 9 novih promptova dodato kao TP-026 do TP-034; checkout lock nalaz dodat u TP-017.
- Aktivni queue fajlovi u repozitorijumu: samo `PROMPT_QUEUE.md`.
