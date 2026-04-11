# Alerting Rules - Detaljno Objašnjenje

## 1. Request Latency Alerts

### HighRequestLatency (Warning)

```prometheus
histogram_quantile(0.95, rate(http_request_duration_ms_bucket[5m])) > 500
for: 5m
```

**Šta znači:**
- P95 (95. percentil) request latency je veći od 500ms
- Aktivira se ako je stanje aktivno 5 minuta

**Razlog:**
- Spora API response
- Mogući problem sa bazom podataka
- Preplavljeni server

**Akcija:**
1. Proverite Grafana dashboard
2. Pogledajte koja endpoint je spora najviše
3. Analzujte database performance
4. Pogledajte application logs u Seq-u

**SLA:** < 500ms (P95)

---

### CriticalRequestLatency (Critical)

```prometheus
histogram_quantile(0.99, rate(http_request_duration_ms_bucket[5m])) > 1000
for: 2m
```

**Šta znači:**
- P99 latency je veći od 1 sekunde
- Kritična, aktivira se već posle 2 minuta

**Razlog:**
- Ozbiljan performance problem
- Mogući outage

**Akcija:**
1. **HITNO** proverite status API-ja
2. Pogledajte CPU i memory usage
3. Proverite database connection pool
4. Restart servisa ako je potrebno

---

## 2. Database Query Alerts

### SlowDatabaseQueries (Warning)

```prometheus
histogram_quantile(0.95, rate(db_query_duration_ms_bucket[5m])) > 100
for: 5m
```

**Šta znači:**
- P95 database query je duža od 100ms
- Mogući problem sa SQL performance-om

**Razlog:**
- Nedostaje index
- Loše napisana query
- Preplavljeni database

**Akcija:**
1. Identifikujte spore query-je
2. Pogledajte Grafana za top queries
3. Analizirajte query execution plan
4. Dodate index ako je potrebno

**SLA:** < 100ms (P95)

---

### CriticallySlowDatabaseQueries (Critical)

```prometheus
histogram_quantile(0.99, rate(db_query_duration_ms_bucket[5m])) > 250
for: 2m
```

**Šta znači:**
- P99 database query je duža od 250ms

**Razlog:**
- Ozbiljan problem sa bazom podataka
- Mogući table lock

**Akcija:**
1. Proverite database lock status
2. Pogledajte long running queries
3. Možda Kill problematic query
4. Kontaktirajte DBA

---

## 3. Error Rate Alerts

### HighErrorRate (Warning)

```prometheus
(rate(http_errors_total{code=~"5.."}[5m]) / rate(http_requests_total[5m])) > 0.005
for: 5m
```

**Šta znači:**
- Više od 0.5% zahteva vraća 5xx grešku

**Razlog:**
- Primena greške
- Dependency problem (API, DB, cache)
- Resource exhaustion

**Akcija:**
1. Pogledajte exception logove u Seq-u
2. Identifikujte koji endpoint generiše greške
3. Analizirajte stack trace
4. Deploy fix ako je potrebno

**SLA:** < 0.1% error rate

---

### CriticalErrorRate (Critical)

```prometheus
(rate(http_errors_total{code=~"5.."}[5m]) / rate(http_requests_total[5m])) > 0.01
for: 2m
```

**Šta znači:**
- Više od 1% zahteva vraća 5xx grešku

**Razlog:**
- Ozbiljan bug u aplikaciji
- Mogući cascade failure

**Akcija:**
1. **HITNO** proverite aplikaciju
2. Pogledajte sve error logove
3. Možda rollback poslednjeg deploymnta
4. Iskažite incident

---

## 4. Cache Alerts

### LowCacheHitRatio (Warning)

```prometheus
cache_hit_ratio < 75
for: 10m
```

**Šta znači:**
- Manje od 75% zahteva je servovano iz cache-a

**Razlog:**
- Cache je invalidiran
- Cache nije konfiguriran pravilno
- Preplavljeni zahtevi (cache warming problem)

**Akcija:**
1. Proverite cache size
2. Analizirajte cache invalidation logiku
3. Možda povećajte cache retention time
4. Proverite cache key strategy

**Target:** > 80%

---

### CriticalLowCacheHitRatio (Critical)

```prometheus
cache_hit_ratio < 50
for: 5m
```

**Šta znači:**
- Manje od 50% hit rate-a

**Razlog:**
- Cache je kompletno neučinkovit
- Mogući cache outage

**Akcija:**
1. Proverite cache serviser (Redis/Memcached)
2. Pogledajte error logove
3. Možda restart cache servisa
4. Fallback na database (sa performance penalti)

---

## 5. Service Availability Alerts

### ServiceUnavailable (Critical)

```prometheus
up{job="trendplus-api"} == 0
for: 1m
```

**Šta znači:**
- API servis je down

**Razlog:**
- Crash aplikacije
- Network problem
- Hardware failure

**Akcija:**
1. **HITNO** restart servisa
2. Proverite prozesne logove
3. Proverite network connectivity
4. Iskažite incident

---

### ServiceDegraded (Warning)

```prometheus
(count(up{job="trendplus-api"} == 1) / count(up{job="trendplus-api"})) < 0.8
for: 5m
```

**Šta znači:**
- Manje od 80% instanci API-ja je dostupno

**Razlog:**
- Neki serverisu down ili problematični
- Load balancer issue

**Akcija:**
1. Identifikujte koje instance su down
2. Proverite zdravlje instanci
3. Pogledajte deployment status
4. Scale up ako je potrebno

---

## 6. Demand Prediction Service Alerts

### DemandPredictionErrors (Warning)

```prometheus
rate(demand_prediction_errors_total[5m]) > 0.01
for: 5m
```

**Šta znači:**
- Više od 1% zahteva za demand prediction vraća grešku

**Razlog:**
- Invalid input data
- Model problem
- Insufficient training data

**Akcija:**
1. Proverite demand prediction logove
2. Analizirajte input data
3. Možda retrairate model
4. Kontaktirajte data science tim

---

### DemandPredictionSlow (Warning)

```prometheus
histogram_quantile(0.95, rate(demand_prediction_duration_ms_bucket[5m])) > 5000
for: 5m
```

**Šta znači:**
- P95 demand prediction traje duže od 5 sekundi

**Razlog:**
- Velike količine podataka
- Kompleksna kalkulacija
- Resource contention

**Akcija:**
1. Proverite algorithm complexity
2. Možda dodajte caching
3. Razmotrite async processing
4. Mogući optimization potreban

---

## 7. Analytics Service Alerts

### AnalyticsEventBacklog (Warning)

```prometheus
analytics_unprocessed_events > 1000
for: 10m
```

**Šta znači:**
- Više od 1000 neprocesuiranih analytics događaja

**Razlog:**
- Event processing je sporა
- Preplavljeni event stream
- Processing worker je down

**Akcija:**
1. Proverite event processing queue
2. Pogledajte worker status
3. Možda scale up workers
4. Analizirajte event processing performance

---

## Runbook Template za svaku Alert

```
# Alert: [Alert Name]

## Overview
[Kratko objašnjenje šta alert znači]

## Impact
[Šta je uticaj na users]

## Diagnostika
1. [Prva dijagnostička akcija]
2. [Druga dijagnostička akcija]
3. [Treća dijagnostička akcija]

## Remediation
1. [Prva akcija]
2. [Druga akcija]
3. [Treća akcija]

## Monitoring
[Što da monitoring-ujete dok ste fixing]

## Prevention
[Kako sprečiti u budućnosti]

## Eskalacija
- Level 1: [Ko da je kontaktiran prvi]
- Level 2: [Ko da je kontaktiran sledeći]
- Level 3: [Critical escalation]

## Post-Incident
- [ ] RCA (Root Cause Analysis)
- [ ] Update runbook
- [ ] Deploy fix
- [ ] Monitor for recurrence
```

## Alert Severity Mapping

| Severity | Response Time | Escalation | Action |
|----------|---------------|-----------|--------|
| Critical | 5 min | PagerDuty | Page on-call engineer |
| Warning  | 30 min | Slack | Notify team |
| Info     | 4 hours | Email | Log and track |

## Alert Fatigue Prevention

1. **Tune thresholds** - Premalo false positives
2. **Group similar alerts** - Ne pošaljite 100 identičnih alertsa
3. **Context** - Dodajte runbook linke
4. **Escalation** - Automatski eskalira ako nije resolved
5. **Review regularly** -月ly audit alert effectiveness

## Kontakt Info

| Role | Kontakt | Availability |
|------|---------|---|
| On-call Engineer | Slack → #on-call | 24/7 |
| Platform Team | platform@company.com | Business hours |
| DBA | dba@company.com | Business hours |
| Infrastructure | infra@company.com | Business hours |

## Dodaci

- Grafana Dashboard: http://localhost:3001
- Prometheus UI: http://localhost:9090
- AlertManager: http://localhost:9093
- Jaeger: http://localhost:16686
