# TrendplusProdavnica - Observability System
## Kompletan Pregled i Dokumentacija

---

## 🎯 Brz Pregled

Implementiran je **kompletna observability arhitektura** za TrendplusProdavnica sa:
- ✅ **Logging**: Serilog + Seq
- ✅ **Metrics**: Prometheus
- ✅ **Tracing**: OpenTelemetry + Jaeger
- ✅ **Visualization**: Grafana dashboards
- ✅ **Alerting**: Prometheus AlertManager

**Status**: Sve komponente su konfigurisane i spremne za deployment.

---

## 📚 Dokumentacija

### Pregled Arhitekture
👉 **[OBSERVABILITY_ARCHITECTURE.md](OBSERVABILITY_ARCHITECTURE.md)**
- Visoki nivo arhitekture
- Komponente sistema
- KPI metrike

### Brzi Start (POČNITE OVDE!)
👉 **[QUICK_START_OBSERVABILITY.md](QUICK_START_OBSERVABILITY.md)**
- Step-by-step pokretanje Docker stacka
- Testiranje komponenti
- Troubleshooting

### Metrics Setup
👉 **[METRICS_SETUP.md](METRICS_SETUP.md)**
- Sve metrike koje se prikupljaju
- Dimenzije i labels
- Prometheus konfiguracija

### .NET Integration
👉 **[DOTNET_OBSERVABILITY_SETUP.md](DOTNET_OBSERVABILITY_SETUP.md)**
- NuGet paketi za instalaciju
- Program.cs setup kod
- Middleware za metrics collection
- Repository pattern za servise
- appsettings.json primeri

### Alerting i Runbooks
👉 **[ALERTING_RUNBOOKS.md](ALERTING_RUNBOOKS.md)**
- Detaljno objašnjenje svake alert rule
- Kako reagovati na svaki alert
- Runbook template
- Eskalaciona procedura

### Implementaciona Svesmka
👉 **[OBSERVABILITY_SUMMARY.md](OBSERVABILITY_SUMMARY.md)**
- Implementacioni plan po fazama
- Kreirani fajlovi
- SLA ciljevi
- Production checklist

---

## 🗂️ Konfiguracijske Datoteke

### Docker & Infrastructure

```
docker-compose.observability.yml          # Kompletan Docker stack
```

**Pokretanje:**
```bash
docker-compose -f docker-compose.observability.yml up -d
```

### Prometheus

```
prometheus/
├── prometheus.yml                         # Scrape config
└── rules/
    └── alerts.yml                         # Alerting rules (13 alerts)
```

### AlertManager

```
prometheus/
└── alertmanager.yml                       # Alert routing & channels
```

### Grafana

```
grafana/
├── provisioning/
│   ├── datasources/
│   │   └── prometheus.yml                 # Prometheus datasource
│   └── dashboards/
│       └── dashboards.yml                 # Dashboard provisioning
└── dashboards/
    └── trendplus-overview.json            # Main dashboard
```

---

## 🚀 Pokretanje u 5 Minuta

### 1. Pokrenite Docker Stack
```bash
docker-compose -f docker-compose.observability.yml up -d
```

### 2. Proverite Status
```bash
docker-compose -f docker-compose.observability.yml ps
```

### 3. Pristupite Servisima

| Servis | URL | Login |
|--------|-----|-------|
| **Grafana** | http://localhost:3001 | admin / admin123 |
| **Prometheus** | http://localhost:9090 | (no auth) |
| **Seq (Logs)** | http://localhost:5341 | (no auth) |
| **Jaeger (Traces)** | http://localhost:16686 | (no auth) |
| **AlertManager** | http://localhost:9093 | (no auth) |

### 4. Vidite Dashboard
- Grafana: http://localhost:3001 → Dashboards → TrendplusProdavnica - Overview

### 5. Krenirajте Test Trafik
```bash
# Terminal 1
dotnet run --project TrendplusProdavnica.Api

# Terminal 2
for i in {1..100}; do
  curl -X POST http://localhost:5000/api/analytics/track \
    -H "Content-Type: application/json" \
    -d '{"eventType":"product_view","productId":1}' &
done
```

---

## 📊 Konfigurisane Metrike (30+)

### API Performance
- `http_request_duration_ms` - Request latency (histogram)
- `http_requests_total` - Request count (counter)
- `http_errors_total` - Error count (counter)

### Database
- `db_query_duration_ms` - Query latency (histogram)
- `db_connections_active` - Active connections (gauge)

### Cache
- `cache_hit_ratio` - Cache effectiveness (gauge)
- `cache_hits_total` - Cache hits (counter)
- `cache_misses_total` - Cache misses (counter)

### Services
- `demand_prediction_requests_total` - Prediction count
- `demand_prediction_duration_ms` - Prediction time
- `analytics_events_tracked_total` - Analytics count
- I više...

---

## 🚨 Konfigurisani Alerts (13)

| Alert | Threshold | Severity |
|-------|-----------|----------|
| HighRequestLatency | P95 > 500ms | 🟡 Warning |
| CriticalRequestLatency | P99 > 1000ms | 🔴 Critical |
| SlowDatabaseQueries | P95 > 100ms | 🟡 Warning |
| CriticallySlowDatabaseQueries | P99 > 250ms | 🔴 Critical |
| HighErrorRate | > 0.5% | 🟡 Warning |
| CriticalErrorRate | > 1% | 🔴 Critical |
| LowCacheHitRatio | < 75% | 🟡 Warning |
| CriticalLowCacheHitRatio | < 50% | 🔴 Critical |
| ServiceUnavailable | API down | 🔴 Critical |
| ServiceDegraded | <80% instances | 🟡 Warning |
| DemandPredictionErrors | > 1% | 🟡 Warning |
| DemandPredictionSlow | P95 > 5s | 🟡 Warning |
| AnalyticsEventBacklog | > 1000 | 🟡 Warning |

Sve alert rules su definisane u `prometheus/rules/alerts.yml`

---

## 🎯 SLA Ciljevi

| Metrika | Target | Warning | Critical |
|---------|--------|---------|----------|
| **Latency (P95)** | < 300ms | > 500ms | > 1000ms |
| **Error Rate** | < 0.1% | > 0.5% | > 1% |
| **Cache Hit** | > 85% | < 75% | < 50% |
| **DB Query (P95)** | < 50ms | > 100ms | > 250ms |
| **Availability** | > 99.5% | < 99% | < 95% |

---

## 📋 Kako Integrovati sa .NET Aplikacijom

### Faza 1: Instalacija NuGet Paketa

```bash
cd TrendplusProdavnica.Api
dotnet add package Serilog
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Seq
dotnet add package OpenTelemetry
dotnet add package OpenTelemetry.Exporter.Prometheus
dotnet add package OpenTelemetry.Exporter.Jaeger
# ... i ostali iz DOTNET_OBSERVABILITY_SETUP.md
```

### Faza 2: Program.cs Setup

1. Kreirajte `MetricsAccessor.cs` klasu
2. Konfigurujte Serilog u `Program.cs`
3. Konfigurujte OpenTelemetry
4. Dodajte middleware za metrics
5. Ažurirajte `appsettings.json`

Detaljne instrukcije: [DOTNET_OBSERVABILITY_SETUP.md](DOTNET_OBSERVABILITY_SETUP.md)

### Faza 3: Test & Verify

1. Pokrenite aplikaciju
2. Kreirajte test trafik
3. Proverite metrike u Prometheus-u
4. Proverite logove u Seq-u
5. Proverite traces u Jaeger-u

---

## 🔔 Slack Notifikacije (Setup)

1. Kreirajte Slack Incoming Webhook
2. Postavite environment varijablu:
   ```bash
   export SLACK_WEBHOOK_URL="https://hooks.slack.com/services/..."
   ```
3. Ažurirajte `prometheus/alertmanager.yml`
4. Restart AlertManager:
   ```bash
   docker-compose -f docker-compose.observability.yml restart alertmanager
   ```

---

## 📈 Grafana Dashboards

### TrendplusProdavnica - Overview
- API Request Latency (P50, P95, P99)
- HTTP Error Rate (gauge sa color thresholds)
- Database Query Latency
- Cache Hit Ratio
- Request Rate by Method
- Active Alerts by Severity

Fajl: `grafana/dashboards/trendplus-overview.json`

### Kako Dodati Novi Dashboard

1. Otvorite Grafana: http://localhost:3001
2. Kreirajte novi dashboard
3. Dodajte panels sa queries iz Prometheus-a
4. Export JSON
5. Stavite u `grafana/dashboards/`
6. Restart Grafana ili edit `grafana/provisioning/dashboards/dashboards.yml`

---

## 🔧 Docker Stack - Komponente

```
Servis               Port    Image
─────────────────────────────────────────────
Prometheus           9090    prom/prometheus
Grafana              3001    grafana/grafana
Seq                  5341    datalust/seq
Jaeger               16686   jaegertracing/all-in-one
AlertManager         9093    prom/alertmanager
Node Exporter        9100    prom/node-exporter (optional)
```

**Kompletan setup**: `docker-compose.observability.yml`

---

## 📊 Monitoring Checklist

- [ ] Docker stack je pokrenut
- [ ] Grafana je dostupna na 3001
- [ ] Prometheus je dostupan na 9090
- [ ] Seq je dostupan na 5341
- [ ] Jaeger je dostupan na 16686
- [ ] .NET paketi instalirani
- [ ] Program.cs je updatiran
- [ ] Metrike se prikupljaju
- [ ] Logovi se centralizuju
- [ ] Traces se slanju
- [ ] Grafana dashboard je dostupan
- [ ] Alerts su aktivne
- [ ] Slack je integrisan

---

## 📞 Support i Help

**Q: Gde početi?**
A: Počnite sa [QUICK_START_OBSERVABILITY.md](QUICK_START_OBSERVABILITY.md)

**Q: Kako integrovati u .NET?**
A: Pogledajte [DOTNET_OBSERVABILITY_SETUP.md](DOTNET_OBSERVABILITY_SETUP.md)

**Q: Kako reagovati na alerts?**
A: Pogledajte [ALERTING_RUNBOOKS.md](ALERTING_RUNBOOKS.md)

**Q: Koja je arhitektura?**
A: Pogledajte [OBSERVABILITY_ARCHITECTURE.md](OBSERVABILITY_ARCHITECTURE.md)

**Q: Šta se može monitorovati?**
A: Pogledajte [METRICS_SETUP.md](METRICS_SETUP.md)

**Q: Gde sam u implementaciji?**
A: Pogledajte [OBSERVABILITY_SUMMARY.md](OBSERVABILITY_SUMMARY.md)

---

## 🎯 Sledeći Koraci

1. **Odmah** (5 min)
   - Pokrenite `docker-compose -f docker-compose.observability.yml up -d`
   - Pristupite http://localhost:3001

2. **Uskoro** (2-3 sata)
   - Integrirajte .NET kod
   - Pokrenite test trafik
   - Verifujte sve komponente

3. **Kasnije** (1 čas)
   - Testirajte alerting
   - Setup Slack integration
   - Obucite team

4. **Production** (4-6 sati)
   - Deploy Docker stack
   - Configure production settings
   - Monitor for issues

---

## ✅ Implementacioni Status

| Komponenta | Status | Fajl |
|-----------|--------|------|
| Prometheus | ✅ Ready | prometheus/ |
| Grafana | ✅ Ready | grafana/ |
| Jaeger | ✅ Ready | docker-compose |
| Seq | ✅ Ready | docker-compose |
| AlertManager | ✅ Ready | prometheus/ |
| .NET Code | 🔄 Next | Need to implement |

---

## 📄 Datuma Kreiranja

- **Arhitektura**: [OBSERVABILITY_ARCHITECTURE.md](OBSERVABILITY_ARCHITECTURE.md)
- **Metrics**: [METRICS_SETUP.md](METRICS_SETUP.md)
- **Docker**: [docker-compose.observability.yml](docker-compose.observability.yml)
- **Prometheus**: [prometheus/](prometheus/)
- **Grafana**: [grafana/](grafana/)
- **.NET Guide**: [DOTNET_OBSERVABILITY_SETUP.md](DOTNET_OBSERVABILITY_SETUP.md)
- **Quick Start**: [QUICK_START_OBSERVABILITY.md](QUICK_START_OBSERVABILITY.md)
- **Alerts**: [ALERTING_RUNBOOKS.md](ALERTING_RUNBOOKS.md)
- **Summary**: [OBSERVABILITY_SUMMARY.md](OBSERVABILITY_SUMMARY.md)
- **Index**: [OBSERVABILITY_INDEX.md](OBSERVABILITY_INDEX.md) (ovaj fajl)

---

**Hvala na korišćenju TrendplusProdavnica Observability Sistema! 🚀**

Za dodatne linke i resurse, kontaktirajte monitoring team.
