# Metrics Setup - TrendplusProdavnica

## Implementacija Metriks

### 1. Request Latency (Histogram)

```csharp
var requestLatency = new Histogram<double>(
    "http_request_duration_ms",
    unit: "ms",
    description: "HTTP request latency in milliseconds"
);

app.UseMiddleware<MetricsMiddleware>(requestLatency);
```

**SLA**: P95 < 500ms, P99 < 1000ms

### 2. Database Query Time (Histogram)

```csharp
var dbQueryDuration = new Histogram<double>(
    "db_query_duration_ms",
    unit: "ms",
    description: "Database query duration in milliseconds"
);
```

**SLA**: P95 < 100ms, P99 < 250ms

### 3. Cache Hit Ratio (Gauge)

```csharp
var cacheHitRatio = new ObservableGauge<double>(
    "cache_hit_ratio",
    () => CalculateCacheHitRatio(),
    unit: "%",
    description: "Cache hit ratio percentage"
);
```

**Target**: > 80%

### 4. Error Rate (Counter)

```csharp
var errorCounter = new Counter<long>(
    "http_errors_total",
    description: "Total HTTP errors"
);

var errorsByType = new Counter<long>(
    "http_errors_by_type",
    description: "HTTP errors by status code"
);
```

**SLA**: < 0.1% error rate

### 5. Request Count by Endpoint (Counter)

```csharp
var requestCounter = new Counter<long>(
    "http_requests_total",
    description: "Total HTTP requests"
);
```

## Metrike po Servisima

### Demand Prediction Service
- `demand_prediction_requests_total` - broj prognoza
- `demand_prediction_duration_ms` - vreme obrade
- `demand_prediction_success_rate` - stopa uspeha

### Analytics Service
- `analytics_events_tracked_total` - broj praćenih događaja
- `analytics_event_processing_duration_ms` - vreme obrade
- `analytics_batch_size` - veličina batch-a

### Catalog Service
- `catalog_search_duration_ms` - vreme pretrage
- `catalog_products_total` - broj proizvoda
- `catalog_category_depth` - dubina kategorija

## Dimensije (Labels)

- `method` - HTTP metod (GET, POST, itd)
- `endpoint` - API endpoint
- `status` - HTTP status kod
- `service` - naziv servisa
- `cache_type` - tip cache-a
- `query_type` - tip DB query-ja

## Scrape Config (Prometheus)

```yaml
scrape_configs:
  - job_name: 'trendplus-api'
    static_configs:
      - targets: ['localhost:9090']
    scrape_interval: 15s
    scrape_timeout: 10s
```

## Retention Policy

- Metrics: 30 dana (production), 7 dana (development)
- Logs: 90 dana (production), 7 dana (development)
- Traces: 72 sata (production), 24 sata (development)
