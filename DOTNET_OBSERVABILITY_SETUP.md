# .NET Observability Setup - TrendplusProdavnica

## NuGet Packages

Instalacija potrebnih paketa:

```bash
# Logging
dotnet add package Serilog
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File
dotnet add package Serilog.Sinks.Seq

# Metrics
dotnet add package OpenTelemetry
dotnet add package OpenTelemetry.Exporter.Prometheus

# Tracing
dotnet add package OpenTelemetry.Api
dotnet add package OpenTelemetry.Sdk
dotnet add package OpenTelemetry.Exporter.Jaeger
dotnet add package OpenTelemetry.Instrumentation.AspNetCore
dotnet add package OpenTelemetry.Instrumentation.Http
dotnet add package OpenTelemetry.Instrumentation.SqlClient

# Entity Framework tracing
dotnet add package OpenTelemetry.Instrumentation.EntityFrameworkCore
```

## Program.cs Setup

### 1. Serilog Logging Configuration

```csharp
using Serilog;
using Serilog.Context;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Serilog configuration
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code)
    .WriteTo.File(
        "logs/trendplus-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.Seq("http://localhost:5341")
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentUserName()
    .Enrich.When(_ => builder.Environment.IsProduction(), e => e.WithProperty("Environment", "Production"))
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to container
var services = builder.Services;

// ... postojeće servise ...
```

### 2. OpenTelemetry Metrics Setup

```csharp
using OpenTelemetry;
using OpenTelemetry.Metrics;

var meterProvider = Sdk.CreateMeterProviderBuilder()
    .AddAspNetCoreInstrumentation()
    .AddHttpClientInstrumentation()
    .AddRuntimeInstrumentation()
    .AddProcessInstrumentation()
    .AddPrometheusExporter()
    .Build();

services.AddSingleton(meterProvider);

// Custom metrics
var meter = new Meter("TrendplusProdavnica.Observability");

var requestLatencyHistogram = meter.CreateHistogram<double>(
    "http_request_duration_ms",
    unit: "ms",
    description: "HTTP request latency");

var dbQueryHistogram = meter.CreateHistogram<double>(
    "db_query_duration_ms",
    unit: "ms",
    description: "Database query duration");

var cacheHitsCounter = meter.CreateCounter<long>(
    "cache_hits_total",
    description: "Total cache hits");

var cacheMissesCounter = meter.CreateCounter<long>(
    "cache_misses_total",
    description: "Total cache misses");

var errorCounter = meter.CreateCounter<long>(
    "http_errors_total",
    description: "Total HTTP errors");

services.AddSingleton(meter);
services.AddSingleton(new MetricsAccessor 
{ 
    RequestLatencyHistogram = requestLatencyHistogram,
    DbQueryHistogram = dbQueryHistogram,
    CacheHitsCounter = cacheHitsCounter,
    CacheMissesCounter = cacheMissesCounter,
    ErrorCounter = errorCounter
});
```

### 3. OpenTelemetry Tracing Setup

```csharp
using OpenTelemetry.Trace;

var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddAspNetCoreInstrumentation()
    .AddHttpClientInstrumentation()
    .AddEntityFrameworkCoreInstrumentation()
    .AddJaegerExporter(options =>
    {
        options.AgentHost = "localhost";
        options.AgentPort = 6831;
        options.ExportProcessorType = ExportProcessorType.Batch;
    })
    .Build();

services.AddSingleton(tracerProvider);
```

### 4. Middleware for Metrics Collection

```csharp
using System.Diagnostics;

public class MetricsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly MetricsAccessor _metricsAccessor;
    private readonly ILogger<MetricsMiddleware> _logger;

    public MetricsMiddleware(RequestDelegate next, MetricsAccessor metricsAccessor, ILogger<MetricsMiddleware> logger)
    {
        _next = next;
        _metricsAccessor = metricsAccessor;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            using (LogContext.PushProperty("RequestId", context.TraceIdentifier))
            using (LogContext.PushProperty("UserAgent", context.Request.Headers["User-Agent"]))
            {
                await _next(context);
                
                stopwatch.Stop();
                _metricsAccessor.RequestLatencyHistogram.Record(stopwatch.ElapsedMilliseconds);
                
                _logger.LogInformation(
                    "HTTP {Method} {Path} {StatusCode} completed in {Duration}ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
                
                if (context.Response.StatusCode >= 500)
                {
                    _metricsAccessor.ErrorCounter.Add(1, new("status_code", context.Response.StatusCode.ToString()));
                }
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _metricsAccessor.ErrorCounter.Add(1, new("exception_type", ex.GetType().Name));
            
            _logger.LogError(ex, "Unhandled exception in HTTP {Method} {Path}", 
                context.Request.Method, context.Request.Path);
            
            throw;
        }
    }
}

// Register middleware
app.UseMiddleware<MetricsMiddleware>();
```

### 5. EF Core Instrumentation

```csharp
public class DbMetricsInterceptor : DbCommandInterceptor
{
    private readonly MetricsAccessor _metricsAccessor;
    private readonly ILogger<DbMetricsInterceptor> _logger;

    public DbMetricsInterceptor(MetricsAccessor metricsAccessor, ILogger<DbMetricsInterceptor> logger)
    {
        _metricsAccessor = metricsAccessor;
        _logger = logger;
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override async ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = (Stopwatch)eventData.ExecutionId;
        var duration = stopwatch?.ElapsedMilliseconds ?? 0;
        
        _metricsAccessor.DbQueryHistogram.Record(duration);
        
        _logger.LogDebug(
            "DB Query executed: {CommandText} in {Duration}ms",
            command.CommandText.Substring(0, Math.Min(100, command.CommandText.Length)),
            duration);
        
        return result;
    }
}

// Register in DbContext
options.AddInterceptors(new DbMetricsInterceptor(...));
```

## Health Check Integration

```csharp
services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddDbContextCheck<TrendplusDbContext>();

// Endpoint
app.MapHealthChecks("/health");
```

## Prometheus Endpoint

```csharp
// Add Prometheus endpoint
var prometheusOptions = new PrometheusAspNetCoreOptions
{
    SuppressRedirectsForGet = true
};
app.UseOpenTelemetryPrometheusScrapingEndpoint(prometheusOptions);

// Accessible at: http://localhost:5000/metrics
```

## Seeding Initial Metrics

```csharp
// Initialize gauge values
var cacheHitRatio = meter.CreateObservableGauge<double>(
    "cache_hit_ratio",
    () => CalculateCacheHitRatio(),
    unit: "%",
    description: "Cache hit ratio percentage");

private static double CalculateCacheHitRatio()
{
    // Implementacija bazirana na vašem cache sistemu
    return (cacheHits / (cacheHits + cacheMisses)) * 100;
}
```

## Structured Logging Examples

```csharp
// Simple message
_logger.LogInformation("Product loaded: {ProductId}", productId);

// With context
using (LogContext.PushProperty("UserId", userId))
using (LogContext.PushProperty("ProductId", productId))
{
    _logger.LogInformation("User added product to cart");
}

// Exceptions
_logger.LogError(ex, "Failed to process demand prediction for product {ProductId}", productId);

// Performance tracking
_logger.LogInformation("Database query completed in {Duration}ms", sw.ElapsedMilliseconds);
```

## Configuration per Environment

### Development (appsettings.Development.json)
```json
{
  "Serilog": {
    "MinimumLevel": "Debug",
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "File", "Args": { "path": "logs/dev-.txt" } }
    ]
  },
  "Observability": {
    "EnableTracing": true,
    "JaegerEndpoint": "http://localhost:6831",
    "PrometheusEnabled": true
  }
}
```

### Production (appsettings.Production.json)
```json
{
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      { "Name": "Seq", "Args": { "serverUrl": "https://seq.production.com" } },
      { "Name": "File", "Args": { "path": "/var/log/trendplus/app-.txt" } }
    ]
  },
  "Observability": {
    "EnableTracing": true,
    "JaegerEndpoint": "http://jaeger:6831",
    "PrometheusEnabled": true
  }
}
```

## Usage in Services

```csharp
public class DemandPredictionService : IDemandPredictionService
{
    private readonly ILogger<DemandPredictionService> _logger;
    private readonly MetricsAccessor _metricsAccessor;
    private readonly Meter _meter;

    public DemandPredictionService(
        ILogger<DemandPredictionService> logger,
        MetricsAccessor metricsAccessor)
    {
        _logger = logger;
        _metricsAccessor = metricsAccessor;
    }

    public async Task<DemandPredictionDto> PredictDemandAsync(DemandPredictionRequest request)
    {
        var sw = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Starting demand prediction for product {ProductId}", request.ProductId);
            
            // Your logic here
            var result = await _calculatePrediction(request);
            
            sw.Stop();
            _metricsAccessor.RequestLatencyHistogram.Record(sw.ElapsedMilliseconds);
            
            _logger.LogInformation(
                "Demand prediction completed for product {ProductId} in {Duration}ms",
                request.ProductId,
                sw.ElapsedMilliseconds);
            
            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _metricsAccessor.ErrorCounter.Add(1);
            
            _logger.LogError(ex, "Error in demand prediction for product {ProductId}", request.ProductId);
            throw;
        }
    }
}
```

## Testing Observability

```bash
# Check Prometheus metrics
curl http://localhost:9090/metrics

# Check Grafana
# http://localhost:3001 (admin/admin123)

# Check Seq logs
# http://localhost:5341

# Check Jaeger traces
# http://localhost:16686
```
