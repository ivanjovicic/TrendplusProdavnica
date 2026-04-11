# Observability System - TrendplusProdavnica

## Arhitektura

```
┌─────────────────────────────────────────────────────────┐
│                  Aplikacija                             │
│  ┌─────────────────────────────────────────────────┐   │
│  │ Application Code                                │   │
│  │ ├─ Serilog Logging                              │   │
│  │ ├─ OpenTelemetry Tracing                        │   │
│  │ ├─ Prometheus Metrics                           │   │
│  └─────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────┘
         │              │              │
         ▼              ▼              ▼
    ┌─────────┐   ┌──────────┐   ┌──────────┐
    │ Seq/    │   │  Tempo   │   │ Prometheus
    │ Grafana │   │  (Traces)│   │ (Metrics)
    │ (Logs)  │   └──────────┘   └──────────┘
    └────┬────┘         │              │
         │              │              │
         └──────────────┬──────────────┘
                        ▼
                   ┌────────────┐
                   │  Grafana   │
                   │ Dashboard  │
                   └────────────┘
                        │
                        ▼
                   ┌────────────┐
                   │  Alerting  │
                   │  Rules     │
                   └────────────┘
```

## Komponente

### 1. Logging (Serilog)
- Centralizovano u Seq ili Grafana Loki
- Structured logging sa kontekstom
- Log nivoi: Debug, Information, Warning, Error, Fatal

### 2. Tracing (OpenTelemetry + Jaeger/Tempo)
- Distribuirano tracing preko servisa
- Request tracking kroz sve komponente
- Dependency tracking (HTTP, DB, Cache)

### 3. Metrics (Prometheus)
- Request latency (histogram)
- DB query time (histogram)
- Cache hit ratio (gauge)
- Error rates (counter)
- Request count by endpoint (counter)

### 4. Alerting
- Baziran na Prometheus rules
- Notifikacije preko Slack/Email
- Grafana alert notifications

### 5. Dashboard (Grafana)
- Real-time monitoring
- Service health overview
- Performance metrics
- Error tracking
- Distributed traces

## Okruženja

- **Development**: Console + File logging, no external dependencies
- **Production**: Full observability stack, Prometheus, Tempo, Seq/Loki

## KPI Metrike

- P95 Request Latency (SLA: < 500ms)
- Error Rate (SLA: < 0.1%)
- Cache Hit Ratio (Target: > 80%)
- DB Query Time P95 (SLA: < 100ms)
- Service Availability (SLA: > 99.5%)
