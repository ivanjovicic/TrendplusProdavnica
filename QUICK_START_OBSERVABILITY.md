# Observability System - Quick Start Guide

## Pokretanje kompletan observability stack-a

### 1. Docker Compose (Preporučeno)

```bash
# Pokrenite observability stack (Prometheus, Grafana, Jaeger, Seq, AlertManager)
docker-compose -f docker-compose.observability.yml up -d

# Proverite status
docker-compose -f docker-compose.observability.yml ps

# Pogledajte logove
docker-compose -f docker-compose.observability.yml logs -f grafana
```

### 2. Pristup Servisima

Nakon pokretanja Docker Compose:

- **Grafana Dashboard**: http://localhost:3001 (admin/admin123)
- **Prometheus**: http://localhost:9090
- **Jaeger Traces**: http://localhost:16686
- **Seq Logs**: http://localhost:5341
- **AlertManager**: http://localhost:9093
- **API Metrics**: http://localhost:5000/metrics

### 3. TrendplusProdavnica API Setup

#### a) Instalirajte NuGet pakete

```bash
cd TrendplusProdavnica.Api
dotnet package add Serilog
dotnet package add Serilog.AspNetCore
dotnet package add Serilog.Sinks.Seq
dotnet package add OpenTelemetry
dotnet package add OpenTelemetry.Exporter.Prometheus
dotnet package add OpenTelemetry.Exporter.Jaeger
dotnet package add OpenTelemetry.Instrumentation.AspNetCore
dotnet package add OpenTelemetry.Instrumentation.Http
dotnet package add OpenTelemetry.Instrumentation.SqlClient
dotnet package add OpenTelemetry.Instrumentation.EntityFrameworkCore
```

#### b) Kreirajte `MetricsAccessor.cs` klasu

```csharp
using System.Diagnostics.Metrics;

namespace TrendplusProdavnica.Api.Observability;

public class MetricsAccessor
{
    public Histogram<double> RequestLatencyHistogram { get; set; }
    public Histogram<double> DbQueryHistogram { get; set; }
    public Counter<long> CacheHitsCounter { get; set; }
    public Counter<long> CacheMissesCounter { get; set; }
    public Counter<long> ErrorCounter { get; set; }
}
```

#### c) Ažurirajte `Program.cs`

Videti `DOTNET_OBSERVABILITY_SETUP.md` za kompletan setup kod.

#### d) Kreirajte `appsettings.Development.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "Serilog": {
    "MinimumLevel": "Debug",
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/app-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7
        }
      },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://localhost:5341"
        }
      }
    ],
    "Enrich": [
      "FromLogContext",
      "WithMachineName",
      "WithEnvironmentUserName"
    ]
  },
  "Observability": {
    "Jaeger": {
      "AgentHost": "localhost",
      "AgentPort": 6831
    }
  }
}
```

### 4. Pokrenite Aplikaciju

```bash
# U root direktorijumu
dotnet run --project TrendplusProdavnica.Api

# ILI koristite batch fajl koji ste kreirali
.\start_all_Services.bat
```

### 5. Testiranje Observability

#### a) Generiši trafik

```bash
# Terminal 1: Pokrenite API
dotnet run --project TrendplusProdavnica.Api

# Terminal 2: Generiši zahteve
for i in {1..100}; do
  curl http://localhost:5000/api/analytics/track \
    -X POST \
    -H "Content-Type: application/json" \
    -d '{"eventType":"product_view","productId":1}' &
done
wait
```

#### b) Pogledate metrike u Grafana

1. Otvorite http://localhost:3001
2. Login sa admin/admin123
3. Idite na Dashboards -> TrendplusProdavnica - Overview
4. Trebalo bi da vidite:
   - API Request Latency
   - HTTP Error Rate
   - Database Query Latency
   - Cache Hit Ratio
   - Request Rate by Method
   - Active Alerts

#### c) Pogledate logove u Seq

1. Otvorite http://localhost:5341
2. Videćete sve strukturirane logove sa kontekstom
3. Filtrirajte po Logger, Level, Property, itd.

#### d) Pogledate trace-ove u Jaeger

1. Otvorite http://localhost:16686
2. Izaberite servis "trendplus-api"
3. Videćete sve distribuirane trace-eve sa latency informacijama

### 6. Konfiguracija Alertinga (Opciono)

#### a) Konekcija sa Slack-om

1. Kreirajte Incoming Webhook: https://api.slack.com/messaging/webhooks
2. Ažurirajte `prometheus/alertmanager.yml`:
   ```yaml
   global:
     slack_api_url: 'https://hooks.slack.com/services/YOUR_WEBHOOK_URL'
   ```

#### b) Pokrenite AlertManager

```bash
docker-compose -f docker-compose.observability.yml restart alertmanager
```

### 7. Čitanje Alerting Rules

Pogledajte `prometheus/rules/alerts.yml` da razumete:
- High Request Latency (P95 > 500ms)
- Slow Database Queries (P95 > 100ms)
- High Error Rate (> 0.5%)
- Low Cache Hit Ratio (< 75%)
- Service Unavailability

## Struktura Direktorijuma

```
TrendplusProdavnica/
├── prometheus/
│   ├── prometheus.yml          # Prometheus konfiguracija
│   ├── rules/
│   │   └── alerts.yml          # Alerting rules
│   └── alertmanager.yml        # AlertManager konfiguracija
├── grafana/
│   ├── dashboards/
│   │   └── trendplus-overview.json
│   └── provisioning/
│       └── datasources/
│           └── prometheus.yml
├── docker-compose.observability.yml
├── OBSERVABILITY_ARCHITECTURE.md
├── METRICS_SETUP.md
├── DOTNET_OBSERVABILITY_SETUP.md
├── ALERTING_RULES.md
└── QUICK_START.md (ovaj fajl)
```

## Troubleshooting

### Prometheus ne prikuplja metrike

1. Proverite da li je `/metrics` endpoint dostupan:
   ```bash
   curl http://localhost:5000/metrics
   ```

2. Proverite Prometheus target status:
   - http://localhost:9090/targets

3. Proverite Prometheus logs:
   ```bash
   docker-compose logs prometheus
   ```

### Grafana ne prikazuje podatke

1. Proverite Prometheus data source:
   - Settings -> Data Sources -> Prometheus
   - Kliknite "Test" dugme

2. Proverite da li Prometheus ima podatke:
   - http://localhost:9090/graph
   - Unesite query kao: `http_requests_total`

### Jaeger ne prikazuje trace-eve

1. Proverite da li aplikacija šalje trace-eve:
   - Proverite u logovima da li postoje OpenTelemetry poruke

2. Proverite Jaeger status:
   ```bash
   curl http://localhost:16686/api/health
   ```

### Seq ne prikuplja logove

1. Proverite da li je Seq dostupan:
   ```bash
   curl http://localhost:5341/api/events
   ```

2. Proverite Serilog konfiguraciju u `appsettings.json`

## KPI Metrike za Monitoring

| Metrika | Target | Warning | Critical |
|---------|--------|---------|----------|
| P95 Latency | < 300ms | > 500ms | > 1000ms |
| P99 Latency | < 500ms | > 1000ms | > 2000ms |
| Error Rate | < 0.1% | > 0.5% | > 1% |
| Cache Hit | > 85% | < 75% | < 50% |
| DB P95 | < 50ms | > 100ms | > 250ms |
| Availability | > 99.5% | < 99% | < 95% |

## Sledeće Korake

1. [x] Postavi Prometheus sa alert rules
2. [x] Postavi Grafana dashboard
3. [x] Integruj Jaeger za distributed tracing
4. [x] Integruj Seq za centralized logging
5. [ ] **Integruj Serilog u Program.cs** ← SLEDEĆE
6. [ ] **Kreiraj custom metrics** ← SLEDEĆE
7. [ ] **Konfiguruj Slack notifikacije** ← SLEDEĆE
8. [ ] **Kreiraj runbooks za alerts** ← SLEDEĆE

## Dodatne Reference

- [Prometheus Documentation](https://prometheus.io/docs/)
- [Grafana Documentation](https://grafana.com/docs/)
- [OpenTelemetry .NET](https://github.com/open-telemetry/opentelemetry-dotnet)
- [Serilog Documentation](https://serilog.net/)
- [Jaeger Documentation](https://www.jaegertracing.io/)
