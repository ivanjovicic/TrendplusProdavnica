cat > prometheus/rules/alerts.yml << 'EOF'
groups:
  - name: TrendplusProdavnica
    interval: 30s
    rules:
      # ===== Request Latency Alerts =====
      
      - alert: HighRequestLatency
        expr: histogram_quantile(0.95, rate(http_request_duration_ms_bucket[5m])) > 500
        for: 5m
        labels:
          severity: warning
          component: api
        annotations:
          summary: "High API request latency detected"
          description: "P95 request latency is {{ $value }}ms (threshold: 500ms)"
          runbook_url: "https://wiki.company.com/runbooks/high-latency"
      
      - alert: CriticalRequestLatency
        expr: histogram_quantile(0.99, rate(http_request_duration_ms_bucket[5m])) > 1000
        for: 2m
        labels:
          severity: critical
          component: api
        annotations:
          summary: "Critical API request latency"
          description: "P99 request latency is {{ $value }}ms (threshold: 1000ms)"
      
      # ===== Database Query Alerts =====
      
      - alert: SlowDatabaseQueries
        expr: histogram_quantile(0.95, rate(db_query_duration_ms_bucket[5m])) > 100
        for: 5m
        labels:
          severity: warning
          component: database
        annotations:
          summary: "Slow database queries detected"
          description: "P95 DB query time is {{ $value }}ms (threshold: 100ms)"
          impact: "May be causing cascading latency issues"
      
      - alert: CriticallySlowDatabaseQueries
        expr: histogram_quantile(0.99, rate(db_query_duration_ms_bucket[5m])) > 250
        for: 2m
        labels:
          severity: critical
          component: database
        annotations:
          summary: "Critically slow database queries"
          description: "P99 DB query time is {{ $value }}ms (threshold: 250ms)"
      
      # ===== Error Rate Alerts =====
      
      - alert: HighErrorRate
        expr: (rate(http_errors_total{code=~"5.."}[5m]) / rate(http_requests_total[5m])) > 0.005
        for: 5m
        labels:
          severity: warning
          component: api
        annotations:
          summary: "High error rate detected"
          description: "Error rate is {{ $value | humanizePercentage }} (threshold: 0.5%)"
          runbook_url: "https://wiki.company.com/runbooks/high-error-rate"
      
      - alert: CriticalErrorRate
        expr: (rate(http_errors_total{code=~"5.."}[5m]) / rate(http_requests_total[5m])) > 0.01
        for: 2m
        labels:
          severity: critical
          component: api
        annotations:
          summary: "Critical error rate"
          description: "Error rate is {{ $value | humanizePercentage }} (threshold: 1%)"
      
      - alert: ServiceErrors
        expr: rate(http_errors_total[5m]) > 0
        for: 10m
        labels:
          severity: warning
          component: "{{ $labels.endpoint }}"
        annotations:
          summary: "Service error detected"
          description: "{{ $labels.endpoint }} is returning errors"
      
      # ===== Cache Alerts =====
      
      - alert: LowCacheHitRatio
        expr: cache_hit_ratio < 75
        for: 10m
        labels:
          severity: warning
          component: cache
        annotations:
          summary: "Low cache hit ratio"
          description: "Cache hit ratio is {{ $value }}% (target: >80%)"
          impact: "Database may be experiencing excessive load"
      
      - alert: CriticalLowCacheHitRatio
        expr: cache_hit_ratio < 50
        for: 5m
        labels:
          severity: critical
          component: cache
        annotations:
          summary: "Critical cache hit ratio"
          description: "Cache hit ratio is {{ $value }}% (target: >80%)"
      
      # ===== Service Availability Alerts =====
      
      - alert: ServiceUnavailable
        expr: up{job="trendplus-api"} == 0
        for: 1m
        labels:
          severity: critical
          component: "{{ $labels.instance }}"
        annotations:
          summary: "TrendplusProdavnica API is down"
          description: "{{ $labels.instance }} has been down for more than 1 minute"
      
      - alert: ServiceDegraded
        expr: (count(up{job="trendplus-api"} == 1) / count(up{job="trendplus-api"})) < 0.8
        for: 5m
        labels:
          severity: warning
          component: deployment
        annotations:
          summary: "Service degradation detected"
          description: "Only {{ $value | humanizePercentage }} of instances are healthy (threshold: 80%)"
      
      # ===== Demand Prediction Service Alerts =====
      
      - alert: DemandPredictionErrors
        expr: rate(demand_prediction_errors_total[5m]) > 0.01
        for: 5m
        labels:
          severity: warning
          component: demand_prediction
        annotations:
          summary: "High error rate in demand prediction"
          description: "Error rate is {{ $value | humanizePercentage }}"
      
      - alert: DemandPredictionSlow
        expr: histogram_quantile(0.95, rate(demand_prediction_duration_ms_bucket[5m])) > 5000
        for: 5m
        labels:
          severity: warning
          component: demand_prediction
        annotations:
          summary: "Slow demand prediction processing"
          description: "P95 processing time is {{ $value }}ms"
      
      # ===== Analytics Service Alerts =====
      
      - alert: AnalyticsEventBacklog
        expr: analytics_unprocessed_events > 1000
        for: 10m
        labels:
          severity: warning
          component: analytics
        annotations:
          summary: "Event processing backlog"
          description: "{{ $value }} unprocessed events in queue"
      
      # ===== Resource Alerts =====
      
      - alert: HighMemoryUsage
        expr: (container_memory_usage_bytes / container_spec_memory_limit_bytes) > 0.85
        for: 5m
        labels:
          severity: warning
          component: infrastructure
        annotations:
          summary: "High memory usage"
          description: "Memory usage is {{ $value | humanizePercentage }}"
      
      - alert: HighCPUUsage
        expr: (rate(container_cpu_usage_seconds_total[5m])) > 0.8
        for: 5m
        labels:
          severity: warning
          component: infrastructure
        annotations:
          summary: "High CPU usage"
          description: "CPU usage is {{ $value | humanizePercentage }}"
      
      # ===== Database Connection Alerts =====
      
      - alert: HighDatabaseConnections
        expr: pg_stat_activity_count > 80
        for: 5m
        labels:
          severity: warning
          component: database
        annotations:
          summary: "High database connection count"
          description: "{{ $value }} active connections (pool size: 100)"
      
      - alert: DatabaseConnectionPoolExhausted
        expr: pg_stat_activity_count >= 99
        for: 1m
        labels:
          severity: critical
          component: database
        annotations:
          summary: "Database connection pool exhausted"
          description: "{{ $value }} connections used"

EOF
