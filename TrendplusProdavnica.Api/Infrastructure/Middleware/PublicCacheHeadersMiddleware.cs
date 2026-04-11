namespace TrendplusProdavnica.Api.Infrastructure.Middleware;

using Microsoft.Extensions.Options;
using TrendplusProdavnica.Infrastructure.Caching;

public sealed class PublicCacheHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly OutputCacheSettings _outputCacheSettings;
    private readonly TimeSpan _listingPageDuration;

    public PublicCacheHeadersMiddleware(
        RequestDelegate next,
        IOptions<CacheSettings> cacheOptions,
        IConfiguration configuration)
    {
        _next = next;
        _outputCacheSettings = cacheOptions.Value.OutputCache;
        _listingPageDuration = configuration.GetValue<TimeSpan?>("Cache:OutputCache:ListingPageDuration")
            ?? TimeSpan.FromMinutes(2);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method))
        {
            await _next(context);
            return;
        }

        var policy = ResolvePolicy(context.Request.Path);
        if (policy is null)
        {
            await _next(context);
            return;
        }

        context.Response.OnStarting(() =>
        {
            if (context.Response.StatusCode != StatusCodes.Status200OK ||
                context.Response.Headers.ContainsKey("Set-Cookie"))
            {
                return Task.CompletedTask;
            }

            context.Response.Headers["Cache-Control"] =
                $"public, max-age={policy.BrowserMaxAgeSeconds}, s-maxage={policy.SharedMaxAgeSeconds}, stale-while-revalidate={policy.StaleWhileRevalidateSeconds}";

            return Task.CompletedTask;
        });

        await _next(context);
    }

    private CachePolicy? ResolvePolicy(PathString path)
    {
        if (path.StartsWithSegments("/api/pages/home"))
        {
            return new CachePolicy(30, ToWholeSeconds(_outputCacheSettings.HomePageDuration), 60);
        }

        if (path.StartsWithSegments("/api/listings") || path.StartsWithSegments("/api/catalog/products"))
        {
            return new CachePolicy(15, ToWholeSeconds(_listingPageDuration), 30);
        }

        if (path.StartsWithSegments("/api/catalog/product"))
        {
            return new CachePolicy(30, ToWholeSeconds(_outputCacheSettings.ProductDetailDuration), 60);
        }

        if (path.StartsWithSegments("/api/brands") ||
            path.StartsWithSegments("/api/collections") ||
            path.StartsWithSegments("/api/stores"))
        {
            return new CachePolicy(30, ToWholeSeconds(_outputCacheSettings.EntityPageDuration), 30);
        }

        return null;
    }

    private static int ToWholeSeconds(TimeSpan duration)
    {
        return Math.Max(1, (int)Math.Round(duration.TotalSeconds, MidpointRounding.AwayFromZero));
    }

    private sealed record CachePolicy(
        int BrowserMaxAgeSeconds,
        int SharedMaxAgeSeconds,
        int StaleWhileRevalidateSeconds);
}

public static class PublicCacheHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UsePublicCacheHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<PublicCacheHeadersMiddleware>();
    }
}
