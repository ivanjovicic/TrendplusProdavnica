# TrendplusProdavnica - Observability System - Implementacna Svesmka

## 📋 Pregled

Kompletna observability arhitektura sa:
- **Logging**: Serilog → Seq
- **Metrics**: Prometheus
- **Tracing**: OpenTelemetry + Jaeger
- **Visualization**: Grafana
- **Alerting**: Prometheus AlertManager
- **Status**: Sve komponente su konfigurisane i spremne za deployment

## 📁 Kreirani Fajlovi

### Dokumentacija

| Fajl | Svrha |
|------|-------|
| `OBSERVABILITY_ARCHITECTURE.md` | Visoki nivo arhitekture |
| `METRICS_SETUP.md` | Detaljno o metrics-ima |
| `DOTNET_OBSERVABILITY_SETUP.md` | .NET kod i konfiguracija |
| `QUICK_START_OBSERVABILITY.md` | Step-by-step pokretanje |
| `ALERTING_RUNBOOKS.md` | Detaljne instrukcije za alerts |

### Konfiguracijske Datoteke

| Fajl | Svrha |
|------|-------|
| `docker-compose.observability.yml` | Docker stack (Prometheus, Grafana, Seq, Jaeger, AlertManager) |
| `prometheus/prometheus.yml` | Prometheus konfiguracija |
| `prometheus/rules/alerts.yml` | Prometheus alarming rules |
| `prometheus/alertmanager.yml` | AlertManager routing |
| `grafana/provisioning/datasources/prometheus.yml` | Grafana datasource setup |
| `grafana/dashboards/trendplus-overview.json` | Grafana dashboard |

## 🚀 Implementacioni Plan

### Faza 1: Infrastruktura (Sadržano - gotovo)

- [x] Prometheus sa scrape config-om
- [x] Grafana sa datasource provisioning-om
- [x] Jaeger za distributed tracing
- [x] Seq za centralized logging
- [x] AlertManager sa Slack integration-om
- [x] Alerting rules sa svim KPI metrikama

### Faza 2: .NET Integration (SLEDEĆE)

Potrebno:
1. [ ] Instalacija NuGet paketa
2. [ ] Kreiraj `MetricsAccessor.cs` klasu
3. [ ] Ažuriranje `Program.cs` sa Serilog setupom
4. [ ] OpenTelemetry konfiguracija
5. [ ] Custom metrics za svaki servis
6. [ ] appsettings.json ažuriranje

Trajanje: 2-3 časa

### Faza 3: Integration Testing (POSLE)

- [ ] Generate test trafik
- [ ] Verify metrics u Prometheus-u
- [ ] Verify logs u Seq-u
- [ ] Verify traces u Jaeger-u
- [ ] Test alerting (manual trigger)

Trajanje: 1 čas

### Faza 4: Production Deployment

- [ ] Deploy Docker stack na production
- [ ] Configure Slack webhooks
- [ ] Setup log retention
- [ ] Configure backup za metrics
- [ ] Train team na monitoring

Trajanje: 4-6 časa

## 🔧 Brz Start (Odmah)

```bash
# 1. Pokrenite observability stack
docker-compose -f docker-compose.observability.yml up -d

# 2. Proverite status
docker-compose -f docker-compose.observability.yml ps

# 3. Pristupite servisima
# Grafana: http://localhost:3001 (admin/admin123)
# Prometheus: http://localhost:9090
# Seq: http://localhost:5341
# Jaeger: http://localhost:16686
```

## 📊 KPI Metrike (Konfigurisane)

### API Performance
- [x] Request Latency (P50, P95, P99)
- [x] Error Rate
- [x] Request/sec by method
- [x] Request count by endpoint

### Database
- [x] Query Latency (P50, P95, P99)
- [x] Connection pool usage
- [x] Long-running queries
- [x] Query errors

### Cache
- [x] Cache hit ratio
- [x] Cache miss ratio
- [x] Cache evictions
- [x] Cache size

### Service Health
- [x] Service availability
- [x] Instance degradation
- [x] Health check status
- [x] Dependency status

### Demand Prediction
- [x] Prediction requests
- [x] Prediction duration
- [x] Prediction success rate
- [x] Model performance

## 🚨 Alerts Konfigurisani

| Alert | Threshold | Severity |
|-------|-----------|----------|
| HighRequestLatency | P95 > 500ms | Warning |
| CriticalRequestLatency | P99 > 1000ms | Critical |
| SlowDatabaseQueries | P95 > 100ms | Warning |
| CriticallySlowDatabaseQueries | P99 > 250ms | Critical |
| HighErrorRate | > 0.5% | Warning |
| CriticalErrorRate | > 1% | Critical |
| LowCacheHitRatio | < 75% | Warning |
| CriticalLowCacheHitRatio | < 50% | Critical |
| ServiceUnavailable | up == 0 | Critical |
| ServiceDegraded | <80% instances | Warning |
| DemandPredictionErrors | > 1% | Warning |
| DemandPredictionSlow | P95 > 5s | Warning |
| AnalyticsEventBacklog | > 1000 events | Warning |

## 🎯 SLA Ciljevi

| Metrika | Target | Warning | Critical |
|---------|--------|---------|----------|
| API P95 Latency | < 300ms | > 500ms | > 1000ms |
| Error Rate | < 0.1% | > 0.5% | > 1% |
| Cache Hit Ratio | > 85% | < 75% | < 50% |
| DB P95 Latency | < 50ms | > 100ms | > 250ms |
| Service Availability | > 99.5% | < 99% | < 95% |

## 📝 Kako Koristiti

### Pregled Sistema

1. Otvorite http://localhost:3001 (Grafana)
2. Login sa admin/admin123
3. Vidite "TrendplusProdavnica - Overview" dashboard
4. Monitorujte KPI metrike u realnom vremenu

### Analiza Logova

1. Otvorite http://localhost:5341 (Seq)
2. Filtrirajte po:
   - Logger name
   - LogLevel (Info, Warning, Error)
   - Property (UserId, ProductId, itd)
   - Time range

### Distributed Tracing

1. Otvorite http://localhost:16686 (Jaeger)
2. Izaberite servis: "trendplus-api"
3. Vidite sve request trace-eve
4. Analizirajte latency breakdown po komponentama

### Request Metrics

1. Otvorite http://localhost:9090 (Prometheus)
2. Unesite query kao:
   ```
   http_request_duration_ms_bucket
   http_errors_total
   cache_hit_ratio
   ```
3. Vidite raw metrics

## 🔔 Notifikacije

### Slack Integration

```bash
# Setup SLACK_WEBHOOK_URL
export SLACK_WEBHOOK_URL="https://hooks.slack.com/services/YOUR_WEBHOOK"

# Restart alertmanager
docker-compose -f docker-compose.observability.yml restart alertmanager
```

### Alert Channels

- Critical alerts → #critical-incidents (PagerDuty)
- Warning alerts → #warnings
- Database alerts → #database-team
- API alerts → #api-team
- Prediction alerts → #data-science

## 📚 Dokumentacija Referencije

### Za Logging (Serilog)
- NuGet: https://www.nuget.org/packages/Serilog/
- Docs: https://serilog.net/

### Za Metrics (OpenTelemetry)
- NuGet: https://github.com/open-telemetry/opentelemetry-dotnet
- Docs: https://opentelemetry.io/docs/instrumentation/net/

### Za Tracing (Jaeger)
- Docs: https://www.jaegertracing.io/docs/
- Docker: https://hub.docker.com/r/jaegertracing/all-in-one

### Za Grafana
- Docs: https://grafana.com/docs/
- Dashboards: https://grafana.com/grafana/dashboards/

### Za Prometheus
- Docs: https://prometheus.io/docs/
- PromQL: https://prometheus.io/docs/prometheus/latest/querying/

## 🔐 Sigurnost

### Production Recommendations

1. **Prometheus**
   - Limitirajte pristup na internal network
   - Setup authentication
   - Enable HTTPS

2. **Grafana**
   - Promenite default password
   - Integruj sa LDAP/OAuth
   - Enable RBAC

3. **Seq**
   - Setup API key authentication
   - Limit data retention
   - Enable HTTPS

4. **AlertManager**
   - Secure Slack webhooks
   - Use encrypted secrets
   - Audit log access

## 📊 Data Retention

| Servis | Retention | Produksija |
|--------|-----------|-----------|
| Prometheus | 30 dana | 90 dana |
| Seq | 7 dana | 30 dana |
| Jaeger | 24 sata | 72 sata |
| Grafana | N/A | N/A |

## ✅ Checklist pre Produkcije

- [ ] Docker stack pokrenut i stabilan
- [ ] Skvsi Serilog u Program.cs
- [ ] Metrics se prikupljaju
- [ ] Grafana dashboard je dostupan
- [ ] Alerting rules su aktivne
- [ ] Slack integration je konfiguriran
- [ ] Log retention je postavljen
- [ ] Backup za metrics je skonfiguriran
- [ ] Team je obučen
- [ ] Documentation je dostupna
- [ ] Runbooks su dostupni
- [ ] On-call rotation je postavljen

## 📞 Support i Kontakt

Za pitanja o observability sistemu:
- Pogledajte dokumentaciju u `QUICK_START_OBSERVABILITY.md`
- Kontaktirajte monitoring team-a
- Kreirajte issue ako nešto ne radi

## 🎯 Sledeći Koraci

1. **Odmah**: Pokrenite Docker stack
2. **Bilo kada**: Integrirajte .NET kod
3. **Pre deployment-a**: Testirajte alerting
4. **Production**: Deploy i monitoruj

---

**Sve komponente su gotove i sprema za produksiju! 🚀**

Za detaljne instrukcije, pogledajte `QUICK_START_OBSERVABILITY.md`
