namespace TrendplusProdavnica.Api.Infrastructure.Middleware;

using System.Diagnostics;
using Microsoft.Extensions.Primitives;

public sealed class StorefrontPerformanceTelemetryMiddleware
{
    private const double SlowRequestThresholdMs = 200d;

    private readonly RequestDelegate _next;
    private readonly ILogger<StorefrontPerformanceTelemetryMiddleware> _logger;

    public StorefrontPerformanceTelemetryMiddleware(
        RequestDelegate next,
        ILogger<StorefrontPerformanceTelemetryMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!TryResolveHotPath(context.Request, out var routeGroup))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var activity = Activity.Current;

        activity?.SetTag("trendplus.hot_path", true);
        activity?.SetTag("trendplus.route_group", routeGroup);
        activity?.SetTag("trendplus.measurement.phase", "p0-baseline");

        context.Response.OnStarting(() =>
        {
            AppendServerTimingHeader(context.Response.Headers, routeGroup, stopwatch.Elapsed.TotalMilliseconds);
            return Task.CompletedTask;
        });

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            var elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
            var statusCode = context.Response.StatusCode;

            activity?.SetTag("trendplus.duration_ms", elapsedMilliseconds);
            activity?.SetTag("trendplus.status_code", statusCode);

            if (context.Response.Headers.TryGetValue("Cache-Control", out var cacheControl))
            {
                activity?.SetTag("trendplus.cache_control", cacheControl.ToString());
            }

            if (elapsedMilliseconds >= SlowRequestThresholdMs || statusCode >= StatusCodes.Status500InternalServerError)
            {
                _logger.LogInformation(
                    "Hot route {Method} {Path} [{RouteGroup}] completed in {ElapsedMilliseconds:0.00} ms with status {StatusCode}",
                    context.Request.Method,
                    context.Request.Path,
                    routeGroup,
                    elapsedMilliseconds,
                    statusCode);
            }
            else
            {
                _logger.LogDebug(
                    "Hot route {Method} {Path} [{RouteGroup}] completed in {ElapsedMilliseconds:0.00} ms with status {StatusCode}",
                    context.Request.Method,
                    context.Request.Path,
                    routeGroup,
                    elapsedMilliseconds,
                    statusCode);
            }
        }
    }

    private static bool TryResolveHotPath(HttpRequest request, out string routeGroup)
    {
        routeGroup = string.Empty;

        if (!HttpMethods.IsGet(request.Method) && !HttpMethods.IsHead(request.Method))
        {
            return false;
        }

        var path = request.Path;

        if (path.StartsWithSegments("/api/pages/home"))
        {
            routeGroup = "home";
            return true;
        }

        if (path.StartsWithSegments("/api/catalog/products") || path.StartsWithSegments("/api/listings"))
        {
            routeGroup = "plp";
            return true;
        }

        if (path.StartsWithSegments("/api/catalog/product"))
        {
            routeGroup = "pdp";
            return true;
        }

        if (path.StartsWithSegments("/api/brands"))
        {
            routeGroup = "brand";
            return true;
        }

        if (path.StartsWithSegments("/api/collections"))
        {
            routeGroup = "collection";
            return true;
        }

        if (path.StartsWithSegments("/api/stores"))
        {
            routeGroup = "store";
            return true;
        }

        if (path.StartsWithSegments("/api/editorial"))
        {
            routeGroup = "editorial";
            return true;
        }

        return false;
    }

    private static void AppendServerTimingHeader(IHeaderDictionary headers, string routeGroup, double durationMs)
    {
        var value = FormattableString.Invariant($"app;desc=\"{routeGroup}\";dur={durationMs:0.##}");

        if (headers.TryGetValue("Server-Timing", out var existing))
        {
            headers["Server-Timing"] = StringValues.Concat(existing, value);
            return;
        }

        headers.Append("Server-Timing", value);
    }
}

public static class StorefrontPerformanceTelemetryMiddlewareExtensions
{
    public static IApplicationBuilder UseStorefrontPerformanceTelemetry(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<StorefrontPerformanceTelemetryMiddleware>();
    }
}
