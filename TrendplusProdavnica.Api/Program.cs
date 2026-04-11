#nullable enable
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using TrendplusProdavnica.Application.Catalog.Listing;
using TrendplusProdavnica.Application.Catalog.Queries;
using TrendplusProdavnica.Application.Catalog.Services;
using TrendplusProdavnica.Application.Content.Queries;
using TrendplusProdavnica.Application.Content.Services;
using TrendplusProdavnica.Application.Content.CategorySeo;
using TrendplusProdavnica.Application.Stores.Queries;
using TrendplusProdavnica.Application.Stores.Services;
using TrendplusProdavnica.Application.Search.Queries;
using TrendplusProdavnica.Application.Search.Services;
using TrendplusProdavnica.Application.Checkout.Dtos;
using TrendplusProdavnica.Application.Checkout.Services;
using TrendplusProdavnica.Application.Wishlist.Dtos;
using TrendplusProdavnica.Application.Wishlist.Services;
using TrendplusProdavnica.Application.Cart.Services;
using TrendplusProdavnica.Application.Cart.Dtos;
using TrendplusProdavnica.Api.Infrastructure.Auth;
using TrendplusProdavnica.Api.Infrastructure;
using TrendplusProdavnica.Api.Infrastructure.Middleware;
using TrendplusProdavnica.Infrastructure.Caching;
using TrendplusProdavnica.Infrastructure.Services;
using TrendplusProdavnica.Infrastructure.DependencyInjection;
using TrendplusProdavnica.Infrastructure.Persistence;
using TrendplusProdavnica.Infrastructure.Search.Services;
using TrendplusProdavnica.Application.Admin.Services;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
var connectionString = builder.Configuration.GetConnectionString("TrendplusDb");
var outputCacheSettings = builder.Configuration.GetSection("Cache:OutputCache").Get<OutputCacheSettings>() ?? new OutputCacheSettings();
var listingOutputCacheDuration = builder.Configuration.GetValue<TimeSpan?>("Cache:OutputCache:ListingPageDuration")
    ?? TimeSpan.FromMinutes(2);

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'TrendplusDb' is not configured.");
}

// Add services to the container
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<AdminApiExceptionFilter>();
});
builder.Services.AddDbContext<TrendplusDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddInfrastructurePerformance(builder.Configuration);
builder.Services.AddInfrastructureQueries();
builder.Services.AddCartServices();
builder.Services.AddWishlistServices();
// Temporarily disabled due to missing StoreInventories DbSet
// builder.Services.AddInventoryServices(builder.Configuration);
builder.Services.AddAdminServices();
// Temporarily disabled due to IFusionCache API issues
// builder.Services.AddRecommendationServices();
builder.Services.AddMerchandisingServices();
builder.Services.AddCategorySeoServices();
builder.Services.AddAnalyticsServices();
builder.Services.AddExperimentServices();
builder.Services.AddScoped<ICheckoutService, CheckoutService>();

// Add JWT service for admin authentication
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

var jwtConfiguration = builder.Configuration.GetSection("Jwt");
var jwtSecret = jwtConfiguration["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured.");
var jwtIssuer = jwtConfiguration["Issuer"] ?? "TrendplusProdavnica";
var jwtAudience = jwtConfiguration["Audience"] ?? "TrendplusProdavnica.Admin";
var jwtSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.SaveToken = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = jwtSigningKey,
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(ApiAuthorizationPolicies.Admin, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context => ApiAuthorizationPolicies.HasAdminAccess(context.User));
    });

    options.AddPolicy(ApiAuthorizationPolicies.Operational, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context => ApiAuthorizationPolicies.HasAdminAccess(context.User));
    });
});

builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("public-home", policy => policy.Expire(outputCacheSettings.HomePageDuration));
    options.AddPolicy("public-listing", policy =>
    {
        policy.Expire(listingOutputCacheDuration);
        policy.SetVaryByRouteValue("slug");
        policy.SetVaryByRouteValue("categorySlug");
        policy.SetVaryByQuery("page");
        policy.SetVaryByQuery("pageSize");
        policy.SetVaryByQuery("sort");
        policy.SetVaryByQuery("sizes");
        policy.SetVaryByQuery("colors");
        policy.SetVaryByQuery("brands");
        policy.SetVaryByQuery("priceFrom");
        policy.SetVaryByQuery("priceTo");
        policy.SetVaryByQuery("minPrice");
        policy.SetVaryByQuery("maxPrice");
        policy.SetVaryByQuery("isOnSale");
        policy.SetVaryByQuery("isNew");
        policy.SetVaryByQuery("inStockOnly");
        policy.SetVaryByQuery("category");
        policy.SetVaryByQuery("brand");
        policy.SetVaryByQuery("collection");
    });
    options.AddPolicy("public-entity-page", policy => policy.Expire(outputCacheSettings.EntityPageDuration));
    options.AddPolicy("public-product", policy => policy.Expire(outputCacheSettings.ProductDetailDuration));
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await app.Services.SeedDevelopmentDataAsync();
}

app.UseHttpsRedirection();
app.UseStorefrontPerformanceTelemetry();
app.UseAuthentication();
app.UseAuthorization();
app.UsePublicCacheHeaders();
app.UseOutputCache();
app.MapControllers();
app.MapDefaultEndpoints();

// ============================================================
// VALIDATION HELPERS
// ============================================================

static (bool isValid, IResult? errorResult) ValidateListingParameters(
    int page,
    int pageSize,
    string? sort)
{
    const int MaxPageSize = 100;

    if (page < 1)
        return (false, Results.BadRequest(new { error = "Invalid page number", message = "Page number must be 1 or greater." }));

    if (pageSize <= 0 || pageSize > MaxPageSize)
        return (false, Results.BadRequest(new { error = "Invalid page size", message = $"Page size must be between 1 and {MaxPageSize}." }));

    if (!string.IsNullOrEmpty(sort) &&
        !new[] { "recommended", "popular", "newest", "price_asc", "price_desc", "price-asc", "price-desc", "bestsellers" }
            .Contains(sort, StringComparer.OrdinalIgnoreCase))
    {
        return (false, Results.BadRequest(new
        {
            error = "Invalid sort parameter",
            message = "Sort must be one of: recommended, popular, newest, price_asc, price_desc, price-asc, price-desc, bestsellers."
        }));
    }

    return (true, null);
}

static (bool isValid, IResult? errorResult) ValidateSearchParameters(
    int page,
    int pageSize,
    string? sort)
{
    const int MaxPageSize = 100;

    if (page < 1)
        return (false, Results.BadRequest(new { error = "Invalid page number", message = "Page number must be 1 or greater." }));

    if (pageSize <= 0 || pageSize > MaxPageSize)
        return (false, Results.BadRequest(new { error = "Invalid page size", message = $"Page size must be between 1 and {MaxPageSize}." }));

    if (!string.IsNullOrWhiteSpace(sort) &&
        !new[] { "relevance", "popular", "newest", "price_asc", "price_desc", "bestsellers" }.Contains(sort, StringComparer.OrdinalIgnoreCase))
    {
        return (false, Results.BadRequest(new { error = "Invalid sort parameter", message = "Sort must be one of: relevance, popular, newest, price_asc, price_desc, bestsellers." }));
    }

    return (true, null);
}

static (bool isValid, IResult? errorResult) ValidatePlpParameters(
    int page,
    int pageSize,
    string? sort)
{
    const int MaxPageSize = 120;

    if (page < 1)
        return (false, Results.BadRequest(new { error = "Invalid page", message = "Page must be 1 or greater." }));

    if (pageSize <= 0 || pageSize > MaxPageSize)
        return (false, Results.BadRequest(new { error = "Invalid pageSize", message = $"PageSize must be between 1 and {MaxPageSize}." }));

    if (!string.IsNullOrWhiteSpace(sort) &&
        !new[] { "popular", "recommended", "price_asc", "price_desc", "price-asc", "price-desc", "newest", "bestsellers" }
            .Contains(sort, StringComparer.OrdinalIgnoreCase))
    {
        return (false, Results.BadRequest(new
        {
            error = "Invalid sort",
            message = "Sort must be one of: popular, recommended, price_asc, price_desc, price-asc, price-desc, newest, bestsellers."
        }));
    }

    return (true, null);
}

static decimal[] ParseDecimalFilters(string[]? values)
{
    if (values is null || values.Length == 0)
    {
        return Array.Empty<decimal>();
    }

    var parsed = new List<decimal>();
    foreach (var token in values.SelectMany(value => value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)))
    {
        if (decimal.TryParse(token, NumberStyles.Number, CultureInfo.InvariantCulture, out var number))
        {
            parsed.Add(number);
        }
    }

    return parsed.Distinct().ToArray();
}

static long[] ParseLongFilters(string[]? values)
{
    if (values is null || values.Length == 0)
    {
        return Array.Empty<long>();
    }

    var parsed = new List<long>();
    foreach (var token in values.SelectMany(value => value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)))
    {
        if (long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
        {
            parsed.Add(number);
        }
    }

    return parsed.Distinct().ToArray();
}

static string[] ParseStringFilters(string[]? values)
{
    if (values is null || values.Length == 0)
    {
        return Array.Empty<string>();
    }

    return values
        .SelectMany(value => value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        .Where(value => !string.IsNullOrWhiteSpace(value))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();
}

static string[] CombineFilters(string[]? values, string? singleValue)
{
    if (string.IsNullOrWhiteSpace(singleValue))
    {
        return values ?? Array.Empty<string>();
    }

    if (values is null || values.Length == 0)
    {
        return new[] { singleValue };
    }

    return values.Concat(new[] { singleValue }).ToArray();
}

// ============================================================
// HOME PAGE ENDPOINTS
// ============================================================

var homePageEndpoint = app.MapGet("/api/pages/home", HomePageEndpoint)
    .WithName("GetHomePage")
    .WithSummary("Get home page content")
    .WithDescription("Returns home page with featured products, hero section, and dynamic modules")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status500InternalServerError);

if (outputCacheSettings.Enabled)
{
    homePageEndpoint.CacheOutput("public-home");
}

async Task<IResult> HomePageEndpoint(IHomePageQueryService homePageService)
{
    try
    {
        var result = await homePageService.GetHomePageAsync();
        return Results.Ok(result);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = "Home page not found", message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

// ============================================================
// LISTING ENDPOINTS
// ============================================================

var categoryListingEndpoint = app.MapGet("/api/listings/category/{slug}", CategoryListingEndpoint)
    .WithName("GetCategoryListing")
    .WithSummary("Get products by category")
    .WithDescription("Compatibility storefront category listing endpoint backed by the canonical ProductListingReadService flow.")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status404NotFound);

if (outputCacheSettings.Enabled)
{
    categoryListingEndpoint.CacheOutput("public-listing");
}

async Task<IResult> CategoryListingEndpoint(
    string slug,
    IProductListingQueryService listingService,
    int page = 1,
    int pageSize = 24,
    string? sort = null,
    long[]? sizes = null,
    string[]? colors = null,
    long[]? brands = null,
    decimal? priceFrom = null,
    decimal? priceTo = null,
    bool? isOnSale = null,
    bool? isNew = null,
    bool? inStockOnly = null)
{
    var (isValid, errorResult) = ValidateListingParameters(page, pageSize, sort);
    if (!isValid)
        return errorResult!;

    try
    {
        var query = new GetCategoryListingQuery(slug, page, pageSize, sort, sizes, colors, brands, priceFrom, priceTo, isOnSale, isNew, inStockOnly);
        var result = await listingService.GetCategoryListingAsync(query);
        return Results.Ok(result);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = "Category not found", message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

var brandListingEndpoint = app.MapGet("/api/listings/brand/{slug}", BrandListingEndpoint)
    .WithName("GetBrandListing")
    .WithSummary("Get products by brand")
    .WithDescription("Compatibility storefront brand listing endpoint backed by the canonical ProductListingReadService flow.")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status404NotFound);

if (outputCacheSettings.Enabled)
{
    brandListingEndpoint.CacheOutput("public-listing");
}

async Task<IResult> BrandListingEndpoint(
    string slug,
    IProductListingQueryService listingService,
    int page = 1,
    int pageSize = 24,
    string? sort = null,
    long[]? sizes = null,
    string[]? colors = null,
    long[]? brands = null,
    decimal? priceFrom = null,
    decimal? priceTo = null,
    bool? isOnSale = null,
    bool? isNew = null,
    bool? inStockOnly = null)
{
    var (isValid, errorResult) = ValidateListingParameters(page, pageSize, sort);
    if (!isValid)
        return errorResult!;

    try
    {
        var query = new GetBrandListingQuery(slug, page, pageSize, sort, sizes, colors, brands, priceFrom, priceTo, isOnSale, isNew, inStockOnly);
        var result = await listingService.GetBrandListingAsync(query);
        return Results.Ok(result);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = "Brand not found", message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

var collectionListingEndpoint = app.MapGet("/api/listings/collection/{slug}", CollectionListingEndpoint)
    .WithName("GetCollectionListing")
    .WithSummary("Get products by collection")
    .WithDescription("Compatibility storefront collection listing endpoint backed by the canonical ProductListingReadService flow.")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status404NotFound);

if (outputCacheSettings.Enabled)
{
    collectionListingEndpoint.CacheOutput("public-listing");
}

async Task<IResult> CollectionListingEndpoint(
    string slug,
    IProductListingQueryService listingService,
    int page = 1,
    int pageSize = 24,
    string? sort = null,
    long[]? sizes = null,
    string[]? colors = null,
    long[]? brands = null,
    decimal? priceFrom = null,
    decimal? priceTo = null,
    bool? isOnSale = null,
    bool? isNew = null,
    bool? inStockOnly = null)
{
    var (isValid, errorResult) = ValidateListingParameters(page, pageSize, sort);
    if (!isValid)
        return errorResult!;

    try
    {
        var query = new GetCollectionListingQuery(slug, page, pageSize, sort, sizes, colors, brands, priceFrom, priceTo, isOnSale, isNew, inStockOnly);
        var result = await listingService.GetCollectionListingAsync(query);
        return Results.Ok(result);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = "Collection not found", message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

var saleListingEndpoint = app.MapGet("/api/listings/sale", SaleListingEndpoint)
    .WithName("GetSaleListing")
    .WithSummary("Get products on sale")
    .WithDescription("Compatibility storefront sale listing endpoint backed by the canonical ProductListingReadService flow.")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status400BadRequest);

if (outputCacheSettings.Enabled)
{
    saleListingEndpoint.CacheOutput("public-listing");
}

async Task<IResult> SaleListingEndpoint(
    IProductListingQueryService listingService,
    int page = 1,
    int pageSize = 24,
    string? sort = null,
    long[]? sizes = null,
    string[]? colors = null,
    long[]? brands = null,
    decimal? priceFrom = null,
    decimal? priceTo = null,
    bool? isNew = null,
    bool? inStockOnly = null)
{
    var (isValid, errorResult) = ValidateListingParameters(page, pageSize, sort);
    if (!isValid)
        return errorResult!;

    try
    {
        var query = new GetSaleListingQuery(page, pageSize, sort, sizes, colors, brands, priceFrom, priceTo, null, isNew, inStockOnly);
        var result = await listingService.GetSaleListingAsync(query);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

var saleCategoryListingEndpoint = app.MapGet("/api/listings/sale/{categorySlug}", SaleCategoryListingEndpoint)
    .WithName("GetSaleCategoryListing")
    .WithSummary("Get products on sale in category")
    .WithDescription("Compatibility storefront sale-category listing endpoint backed by the canonical ProductListingReadService flow.")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status404NotFound);

if (outputCacheSettings.Enabled)
{
    saleCategoryListingEndpoint.CacheOutput("public-listing");
}

async Task<IResult> SaleCategoryListingEndpoint(
    string categorySlug,
    IProductListingQueryService listingService,
    int page = 1,
    int pageSize = 24,
    string? sort = null,
    long[]? sizes = null,
    string[]? colors = null,
    long[]? brands = null,
    decimal? priceFrom = null,
    decimal? priceTo = null,
    bool? isNew = null,
    bool? inStockOnly = null)
{
    var (isValid, errorResult) = ValidateListingParameters(page, pageSize, sort);
    if (!isValid)
        return errorResult!;

    try
    {
        var query = new GetSaleListingQuery(page, pageSize, sort, sizes, colors, brands, priceFrom, priceTo, null, isNew, inStockOnly, categorySlug);
        var result = await listingService.GetSaleListingAsync(query);
        return Results.Ok(result);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = "Category not found", message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

// ============================================================
// CATALOG PLP ENDPOINT
// ============================================================

var catalogProductsEndpoint = app.MapGet("/api/catalog/products", CatalogProductsEndpoint)
    .WithName("GetCatalogProducts")
    .WithSummary("Get product listing page data")
    .WithDescription("Returns paginated PLP products with facets and canonical URL.")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status400BadRequest);

if (outputCacheSettings.Enabled)
{
    catalogProductsEndpoint.CacheOutput("public-listing");
}

async Task<IResult> CatalogProductsEndpoint(
    IProductListingReadService listingReadService,
    string? category = null,
    string? brand = null,
    string? collection = null,
    decimal? minPrice = null,
    decimal? maxPrice = null,
    string[]? sizes = null,
    string[]? colors = null,
    string[]? brands = null,
    bool? isOnSale = null,
    bool? isNew = null,
    bool? inStockOnly = null,
    int page = 1,
    int pageSize = 24,
    string? sort = "popular")
{
    var (isValid, errorResult) = ValidatePlpParameters(page, pageSize, sort);
    if (!isValid)
    {
        return errorResult!;
    }

    if (minPrice.HasValue && minPrice.Value < 0)
    {
        return Results.BadRequest(new { error = "Invalid minPrice", message = "minPrice must be >= 0." });
    }

    if (maxPrice.HasValue && maxPrice.Value < 0)
    {
        return Results.BadRequest(new { error = "Invalid maxPrice", message = "maxPrice must be >= 0." });
    }

    if (minPrice.HasValue && maxPrice.HasValue && minPrice.Value > maxPrice.Value)
    {
        return Results.BadRequest(new { error = "Invalid range", message = "minPrice cannot be greater than maxPrice." });
    }

    try
    {
        var query = new ProductListingQuery(
            category,
            brand,
            collection,
            minPrice,
            maxPrice,
            ParseDecimalFilters(sizes),
            ParseStringFilters(colors),
            ParseLongFilters(brands),
            isOnSale,
            isNew,
            inStockOnly,
            page,
            pageSize,
            sort);

        var response = await listingReadService.GetProductsAsync(query);
        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

// ============================================================
// SEARCH ENDPOINTS
// ============================================================

app.MapGet("/api/search/autocomplete", SearchAutocompleteEndpoint)
    .WithName("SearchAutocomplete")
    .WithSummary("Get autocomplete suggestions")
    .WithDescription("Returns autocomplete suggestions for product search.")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status400BadRequest);

app.MapGet("/api/search/products", SearchProductsEndpoint)
    .WithName("SearchProducts")
    .WithSummary("Search products")
    .WithDescription("Searches products using OpenSearch with filters, sorting, and facets.")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status400BadRequest);

app.MapGet("/api/search", SearchProductsEndpoint)
    .WithSummary("Advanced faceted product search")
    .WithDescription("Searches products using OpenSearch with multi-select facets and aggregations.")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status400BadRequest);

// ============================================================
// SEARCH INDEX QUEUE ENDPOINTS
// ============================================================

var adminSearchGroup = app.MapGroup("/api/admin/search")
    .RequireAuthorization(ApiAuthorizationPolicies.Operational);

adminSearchGroup.MapPost("/queue/process", ProcessSearchIndexQueueEndpoint)
    .WithName("ProcessSearchIndexQueue")
    .WithSummary("Process pending search index events")
    .WithDescription("Processes all pending search index events from the queue and updates OpenSearch.")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status500InternalServerError);

adminSearchGroup.MapGet("/queue/status", GetSearchIndexQueueStatusEndpoint)
    .WithName("GetSearchIndexQueueStatus")
    .WithSummary("Get search index queue status")
    .WithDescription("Returns the current status of the search index queue including pending event count.")
    .Produces(StatusCodes.Status200OK);

adminSearchGroup.MapGet("/dlq/status", GetSearchIndexDLQStatusEndpoint)
    .WithName("GetSearchIndexDLQStatus")
    .WithSummary("Get dead letter queue status")
    .WithDescription("Returns the status of dead letter queue events that failed indexing.")
    .Produces(StatusCodes.Status200OK);

adminSearchGroup.MapPost("/dlq/retry", RetrySearchIndexDLQEndpoint)
    .WithName("RetrySearchIndexDLQ")
    .WithSummary("Retry dead letter queue events")
    .WithDescription("Attempts to reprocess failed events from the dead letter queue.")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status500InternalServerError);

adminSearchGroup.MapPost("/reindex", ReindexAllProductsEndpoint)
    .WithName("ReindexAllProducts")
    .WithSummary("Reindex all products")
    .WithDescription("Rebuilds the entire product search index from PostgreSQL.")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status500InternalServerError);

adminSearchGroup.MapPost("/reindex/{productId:long}", ReindexSingleProductEndpoint)
    .WithName("ReindexSingleProduct")
    .WithSummary("Reindex single product")
    .WithDescription("Reindexes one product in OpenSearch.")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status500InternalServerError);

async Task<IResult> SearchAutocompleteEndpoint(
    IProductSearchService searchService,
    string? q = null,
    int limit = 10)
{
    if (string.IsNullOrWhiteSpace(q))
    {
        return Results.Ok(new { items = Array.Empty<object>() });
    }

    try
    {
        var query = new ProductAutocompleteQuery(q, Math.Max(1, Math.Min(limit, 50)));
        var result = await searchService.AutocompleteAsync(query);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

async Task<IResult> ProcessSearchIndexQueueEndpoint(IProductSearchIndexer indexer)
{
    try
    {
        await indexer.ProcessQueueAsync();
        var queueSize = await indexer.GetQueueSizeAsync();
        return Results.Ok(new 
        { 
            status = "ok", 
            message = "Search index queue processed.",
            remainingQueueSize = queueSize
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

async Task<IResult> GetSearchIndexQueueStatusEndpoint(IProductSearchIndexer indexer)
{
    try
    {
        var queueSize = await indexer.GetQueueSizeAsync();
        var dlqSize = await indexer.GetDeadLetterQueueSizeAsync();
        
        return Results.Ok(new 
        { 
            queueSize,
            deadLetterQueueSize = dlqSize,
            timestamp = DateTimeOffset.UtcNow
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

async Task<IResult> GetSearchIndexDLQStatusEndpoint(IProductSearchIndexer indexer)
{
    try
    {
        var dlqSize = await indexer.GetDeadLetterQueueSizeAsync();
        
        return Results.Ok(new 
        { 
            deadLetterQueueSize = dlqSize,
            message = dlqSize > 0 
                ? "Found dead letter queue events. Use /api/admin/search/dlq/retry to recover."
                : "No dead letter queue events.",
            timestamp = DateTimeOffset.UtcNow
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

async Task<IResult> RetrySearchIndexDLQEndpoint(IProductSearchIndexer indexer)
{
    try
    {
        var initialDLQSize = await indexer.GetDeadLetterQueueSizeAsync();
        
        if (initialDLQSize == 0)
        {
            return Results.Ok(new 
            { 
                status = "ok",
                message = "No dead letter queue events to retry."
            });
        }

        await indexer.RetryDeadLetterAsync(maxAttempts: 10);
        
        var finalDLQSize = await indexer.GetDeadLetterQueueSizeAsync();
        var recovered = initialDLQSize - finalDLQSize;
        
        return Results.Ok(new 
        { 
            status = "ok",
            message = $"Attempted to recover {initialDLQSize} dead letter queue events.",
            recovered,
            remaining = finalDLQSize,
            timestamp = DateTimeOffset.UtcNow
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

async Task<IResult> SearchProductsEndpoint(
    IProductSearchService searchService,
    string? q = null,
    int page = 1,
    int pageSize = 24,
    string? brand = null,
    string[]? brands = null,
    string? color = null,
    string[]? colors = null,
    decimal? size = null,
    string[]? sizes = null,
    decimal? minPrice = null,
    decimal? maxPrice = null,
    string[]? availability = null,
    bool? isOnSale = null,
    bool? isNew = null,
    bool? inStockOnly = null,
    string? sort = "relevance")
{
    var (isValid, errorResult) = ValidateSearchParameters(page, pageSize, sort);
    if (!isValid)
    {
        return errorResult!;
    }

    if (minPrice.HasValue && minPrice.Value < 0)
    {
        return Results.BadRequest(new { error = "Invalid minPrice", message = "minPrice must be >= 0." });
    }

    if (maxPrice.HasValue && maxPrice.Value < 0)
    {
        return Results.BadRequest(new { error = "Invalid maxPrice", message = "maxPrice must be >= 0." });
    }

    if (minPrice.HasValue && maxPrice.HasValue && minPrice.Value > maxPrice.Value)
    {
        return Results.BadRequest(new { error = "Invalid price range", message = "minPrice must be less than or equal to maxPrice." });
    }

    var parsedBrands = ParseStringFilters(CombineFilters(brands, brand));
    var parsedColors = ParseStringFilters(CombineFilters(colors, color));
    var sizeInputs = size.HasValue
        ? CombineFilters(sizes, size.Value.ToString(CultureInfo.InvariantCulture))
        : sizes;
    var parsedSizes = ParseDecimalFilters(sizeInputs);
    var parsedAvailability = ParseStringFilters(availability);

    var invalidAvailability = parsedAvailability
        .Where(value => !new[] { "in_stock", "instock", "available", "in-stock", "out_of_stock", "outofstock", "unavailable", "out-of-stock" }
            .Contains(value, StringComparer.OrdinalIgnoreCase))
        .ToArray();

    if (invalidAvailability.Length > 0)
    {
        return Results.BadRequest(new
        {
            error = "Invalid availability filter",
            message = "availability must contain only: in_stock, out_of_stock."
        });
    }

    try
    {
        var query = new ProductSearchQuery(
            q,
            page,
            pageSize,
            parsedBrands.Length == 0 ? null : parsedBrands,
            parsedColors.Length == 0 ? null : parsedColors,
            parsedSizes.Length == 0 ? null : parsedSizes,
            minPrice,
            maxPrice,
            parsedAvailability.Length == 0 ? null : parsedAvailability,
            isOnSale,
            isNew,
            inStockOnly,
            sort);

        var result = await searchService.SearchProductsAsync(query);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

async Task<IResult> ReindexAllProductsEndpoint(IProductSearchIndexService searchIndexService)
{
    try
    {
        await searchIndexService.ReindexAllAsync();
        return Results.Ok(new { status = "ok", message = "Full product reindex completed." });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

async Task<IResult> ReindexSingleProductEndpoint(long productId, IProductSearchIndexService searchIndexService)
{
    if (productId <= 0)
    {
        return Results.BadRequest(new { error = "Invalid productId", message = "productId must be greater than 0." });
    }

    try
    {
        await searchIndexService.ReindexProductAsync(productId);
        return Results.Ok(new { status = "ok", message = $"Product {productId} reindexed." });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

// ============================================================
// PUBLIC CATEGORY SEO ENDPOINTS
// ============================================================

app.MapGet("/api/category-seo/{categoryId:long}", async Task<IResult> (
    long categoryId,
    ICategorySeoContentService categorySeoService) =>
{
    var content = await categorySeoService.GetByCategoryIdAsync(categoryId);
    return content is null
        ? Results.NotFound(new { error = "SEO sadržaj nije pronađen" })
        : Results.Ok(content);
})
    .WithName("GetCategorySeoByCategoryId")
    .WithSummary("Get category SEO content by category id")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status404NotFound);

app.MapGet("/api/category-seo", async Task<IResult> (
    ICategorySeoContentService categorySeoService) =>
{
    var contents = await categorySeoService.GetAllAsync();
    return Results.Ok(contents);
})
    .WithName("GetAllCategorySeo")
    .WithSummary("Get all category SEO content")
    .Produces(StatusCodes.Status200OK);

// ============================================================
// PRODUCT ENDPOINTS
// ============================================================

var productDetailEndpoint = app.MapGet("/api/catalog/product/{slug}", ProductDetailEndpoint)
    .WithName("GetProductDetail")
    .WithSummary("Get product details")
    .WithDescription("Returns complete product information including variants, media, and related data with structured data for SEO")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status404NotFound);

if (outputCacheSettings.Enabled)
{
    productDetailEndpoint.CacheOutput("public-product");
}

async Task<IResult> ProductDetailEndpoint(
    string slug,
    IProductDetailQueryService productService)
{
    try
    {
        var query = new GetProductDetailQuery(slug);
        var result = await productService.GetProductDetailAsync(query);
        return Results.Ok(result);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = "Product not found", message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

// ============================================================
// BRAND PAGE ENDPOINTS
// ============================================================

var brandPageEndpoint = app.MapGet("/api/brands/{slug}", BrandPageEndpoint)
    .WithName("GetBrandPage")
    .WithSummary("Get brand page")
    .WithDescription("Returns brand information with featured products and category links")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status404NotFound);

if (outputCacheSettings.Enabled)
{
    brandPageEndpoint.CacheOutput("public-entity-page");
}

async Task<IResult> BrandPageEndpoint(
    string slug,
    IBrandPageQueryService brandService)
{
    try
    {
        var query = new GetBrandPageQuery(slug);
        var result = await brandService.GetBrandPageAsync(query);
        return Results.Ok(result);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = "Brand not found", message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

// ============================================================
// COLLECTION PAGE ENDPOINTS
// ============================================================

var collectionPageEndpoint = app.MapGet("/api/collections/{slug}", CollectionPageEndpoint)
    .WithName("GetCollectionPage")
    .WithSummary("Get collection page")
    .WithDescription("Returns collection information with featured products and content blocks")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status404NotFound);

if (outputCacheSettings.Enabled)
{
    collectionPageEndpoint.CacheOutput("public-entity-page");
}

async Task<IResult> CollectionPageEndpoint(
    string slug,
    ICollectionPageQueryService collectionService)
{
    try
    {
        var query = new GetCollectionPageQuery(slug);
        var result = await collectionService.GetCollectionPageAsync(query);
        return Results.Ok(result);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = "Collection not found", message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

// ============================================================
// EDITORIAL ENDPOINTS
// ============================================================

app.MapGet("/api/editorial", EditorialListEndpoint)
    .WithName("GetEditorialList")
    .WithSummary("Get editorial articles")
    .WithDescription("Returns list of published editorial articles")
    .Produces(StatusCodes.Status200OK);

async Task<IResult> EditorialListEndpoint(IEditorialQueryService editorialService)
{
    try
    {
        var result = await editorialService.GetListAsync();
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

app.MapGet("/api/editorial/{slug}", EditorialDetailEndpoint)
    .WithName("GetEditorialArticle")
    .WithSummary("Get editorial article")
    .WithDescription("Returns complete editorial article with related content")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status404NotFound);

async Task<IResult> EditorialDetailEndpoint(
    string slug,
    IEditorialQueryService editorialService)
{
    try
    {
        var query = new GetEditorialArticleQuery(slug);
        var result = await editorialService.GetEditorialArticleAsync(query);
        return Results.Ok(result);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = "Article not found", message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

// ============================================================
// STORES ENDPOINTS
// ============================================================

app.MapGet("/api/stores", StoresListEndpoint)
    .WithName("GetStoresList")
    .WithSummary("Get stores")
    .WithDescription("Returns list of stores with pagination")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status400BadRequest);

async Task<IResult> StoresListEndpoint(
    IStoreQueryService storeService,
    string? city = null,
    int page = 1,
    int pageSize = 20)
{
    if (page < 1)
        return Results.BadRequest(new { error = "Invalid page number", message = "Page number must be 1 or greater." });

    if (pageSize <= 0 || pageSize > 100)
        return Results.BadRequest(new { error = "Invalid page size", message = "Page size must be between 1 and 100." });

    try
    {
        var query = new GetStoresQuery(city, page, pageSize);
        var result = await storeService.GetStoresAsync(query);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

var storeDetailEndpoint = app.MapGet("/api/stores/{slug}", StoreDetailEndpoint)
    .WithName("GetStoreDetail")
    .WithSummary("Get store details")
    .WithDescription("Returns complete store information with location, hours, and featured content")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status404NotFound);

if (outputCacheSettings.Enabled)
{
    storeDetailEndpoint.CacheOutput("public-entity-page");
}

async Task<IResult> StoreDetailEndpoint(
    string slug,
    IStoreQueryService storeService)
{
    try
    {
        var query = new GetStorePageQuery(slug);
        var result = await storeService.GetStorePageAsync(query);
        return Results.Ok(result);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = "Store not found", message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

// ============================================================
// CART ENDPOINTS
// ============================================================

app.MapGet("/api/cart", GetCartBySessionEndpoint)
    .WithName("GetCartBySession")
    .WithSummary("Get shopping cart by session")
    .WithDescription("Retrieves active cart by sessionId (and optional userId).")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status404NotFound);

async Task<IResult> GetCartBySessionEndpoint(
    string sessionId,
    string? userId,
    ICartService cartService)
{
    try
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return Results.BadRequest(new { error = "Invalid sessionId", message = "sessionId is required." });
        }

        var result = await cartService.GetCartBySessionAsync(sessionId, userId);
        if (result == null)
        {
            return Results.NotFound(new { error = "Cart not found", message = $"Active cart for session {sessionId} not found." });
        }

        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

app.MapPost("/api/cart/add", AddToCartBySessionEndpoint)
    .WithName("AddToCartBySession")
    .WithSummary("Add item to cart")
    .WithDescription("Adds an item to active cart by sessionId, creating cart if it does not exist.")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status404NotFound);

async Task<IResult> AddToCartBySessionEndpoint(
    AddToCartBySessionRequest request,
    ICartService cartService)
{
    try
    {
        if (string.IsNullOrWhiteSpace(request.SessionId))
            return Results.BadRequest(new { error = "Invalid sessionId", message = "SessionId is required." });

        if (request.Quantity <= 0)
            return Results.BadRequest(new { error = "Invalid quantity", message = "Quantity must be greater than 0." });

        var result = await cartService.AddItemBySessionAsync(request);
        return Results.Ok(result);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = "Not found", message = ex.Message });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = "Invalid request", message = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = "Invalid operation", message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

app.MapPost("/api/cart/update", UpdateCartBySessionEndpoint)
    .WithName("UpdateCartBySession")
    .WithSummary("Update cart item")
    .WithDescription("Updates quantity for a product variant in cart by sessionId.")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status404NotFound);

async Task<IResult> UpdateCartBySessionEndpoint(
    UpdateCartBySessionRequest request,
    ICartService cartService)
{
    try
    {
        if (string.IsNullOrWhiteSpace(request.SessionId))
            return Results.BadRequest(new { error = "Invalid sessionId", message = "SessionId is required." });

        if (request.Quantity <= 0)
            return Results.BadRequest(new { error = "Invalid quantity", message = "Quantity must be greater than 0." });

        var result = await cartService.UpdateItemBySessionAsync(request);
        return Results.Ok(result);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = "Not found", message = ex.Message });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = "Invalid request", message = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = "Invalid operation", message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

app.MapPost("/api/cart/remove", RemoveFromCartBySessionEndpoint)
    .WithName("RemoveFromCartBySession")
    .WithSummary("Remove item from cart")
    .WithDescription("Removes a product variant from active cart by sessionId.")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status404NotFound);

async Task<IResult> RemoveFromCartBySessionEndpoint(
    RemoveFromCartRequest request,
    ICartService cartService)
{
    try
    {
        if (string.IsNullOrWhiteSpace(request.SessionId))
            return Results.BadRequest(new { error = "Invalid sessionId", message = "SessionId is required." });

        var result = await cartService.RemoveItemBySessionAsync(request);
        return Results.Ok(result);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = "Not found", message = ex.Message });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = "Invalid request", message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

app.MapPost("/api/cart", CreateCartEndpoint)
    .WithName("CreateCart")
    .WithSummary("Create a new shopping cart")
    .WithDescription("Creates a new empty cart and returns it with a unique token")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status500InternalServerError);

async Task<IResult> CreateCartEndpoint(ICartService cartService)
{
    try
    {
        var result = await cartService.CreateCartAsync();
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

app.MapGet("/api/cart/{cartToken}", GetCartEndpoint)
    .WithName("GetCart")
    .WithSummary("Get shopping cart")
    .WithDescription("Retrieves cart contents with all items and product details")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status404NotFound);

async Task<IResult> GetCartEndpoint(
    string cartToken,
    ICartService cartService)
{
    try
    {
        var result = await cartService.GetCartAsync(cartToken);
        if (result == null)
            return Results.NotFound(new { error = "Cart not found", message = $"Cart {cartToken} not found" });
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

app.MapPost("/api/cart/{cartToken}/items", AddItemEndpoint)
    .WithName("AddCartItem")
    .WithSummary("Add item to cart")
    .WithDescription("Adds a product variant to cart (or increases quantity if already in cart)")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status404NotFound);

async Task<IResult> AddItemEndpoint(
    string cartToken,
    AddToCartRequest request,
    ICartService cartService)
{
    try
    {
        if (request.Quantity <= 0)
            return Results.BadRequest(new { error = "Invalid quantity", message = "Quantity must be greater than 0" });

        var result = await cartService.AddItemAsync(cartToken, request);
        return Results.Ok(result);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = "Not found", message = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = "Invalid operation", message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

app.MapPatch("/api/cart/{cartToken}/items/{itemId}", UpdateItemEndpoint)
    .WithName("UpdateCartItem")
    .WithSummary("Update cart item quantity")
    .WithDescription("Updates the quantity of an existing cart item")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status404NotFound);

async Task<IResult> UpdateItemEndpoint(
    string cartToken,
    long itemId,
    UpdateCartItemRequest request,
    ICartService cartService)
{
    try
    {
        if (request.Quantity <= 0)
            return Results.BadRequest(new { error = "Invalid quantity", message = "Quantity must be greater than 0" });

        var result = await cartService.UpdateItemQuantityAsync(cartToken, itemId, request);
        return Results.Ok(result);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = "Not found", message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

app.MapDelete("/api/cart/{cartToken}/items/{itemId}", RemoveItemEndpoint)
    .WithName("RemoveCartItem")
    .WithSummary("Remove item from cart")
    .WithDescription("Removes a single item from the cart")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status404NotFound);

async Task<IResult> RemoveItemEndpoint(
    string cartToken,
    long itemId,
    ICartService cartService)
{
    try
    {
        var result = await cartService.RemoveItemAsync(cartToken, itemId);
        return Results.Ok(result);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = "Not found", message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

app.MapDelete("/api/cart/{cartToken}/items", ClearCartEndpoint)
    .WithName("ClearCart")
    .WithSummary("Clear cart")
    .WithDescription("Removes all items from the cart")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status404NotFound);

async Task<IResult> ClearCartEndpoint(
    string cartToken,
    ICartService cartService)
{
    try
    {
        var result = await cartService.ClearCartAsync(cartToken);
        return Results.Ok(result);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = "Not found", message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

// ============================================================
// CHECKOUT ENDPOINTS
// ============================================================

app.MapGet("/api/checkout/{cartToken}", GetCheckoutSummaryEndpoint)
    .WithName("GetCheckoutSummary")
    .WithSummary("Get checkout summary for cart")
    .WithDescription("Returns checkout summary with items and calculated totals")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status404NotFound);

async Task<IResult> GetCheckoutSummaryEndpoint(
    string cartToken,
    ICheckoutService checkoutService)
{
    try
    {
        var summary = await checkoutService.GetCheckoutSummaryAsync(cartToken);
        if (summary == null)
            return Results.NotFound(new { error = "Cart not found or is empty", message = "Cannot proceed with checkout" });
        
        return Results.Ok(summary);
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

app.MapPost("/api/checkout", PlaceOrderEndpoint)
    .WithName("PlaceOrder")
    .WithSummary("Place an order from cart")
    .WithDescription("Creates an order from the cart and returns confirmation")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status409Conflict)
    .ProducesProblem(StatusCodes.Status500InternalServerError);

async Task<IResult> PlaceOrderEndpoint(
    CheckoutRequest request,
    HttpContext httpContext,
    ICheckoutService checkoutService)
{
    try
    {
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey) &&
            httpContext.Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKeyHeader))
        {
            request.IdempotencyKey = idempotencyKeyHeader.ToString();
        }

        if (string.IsNullOrWhiteSpace(request.CartToken))
            return Results.BadRequest(new { error = "Invalid order", message = "CartToken is required" });

        if (string.IsNullOrWhiteSpace(request.CustomerFirstName) || string.IsNullOrWhiteSpace(request.CustomerLastName))
            return Results.BadRequest(new { error = "Invalid order", message = "Customer name is required" });

        if (string.IsNullOrWhiteSpace(request.GetResolvedEmail()) || string.IsNullOrWhiteSpace(request.Phone))
            return Results.BadRequest(new { error = "Invalid order", message = "Email and phone are required" });

        if (request.DeliveryMethod == TrendplusProdavnica.Domain.Sales.DeliveryMethod.Courier &&
            (string.IsNullOrWhiteSpace(request.DeliveryAddressLine1) || string.IsNullOrWhiteSpace(request.DeliveryCity)))
            return Results.BadRequest(new { error = "Invalid order", message = "Delivery address is required" });

        var result = await checkoutService.PlaceOrderAsync(request);

        return result.Outcome switch
        {
            CheckoutOutcome.Success => Results.Ok(result),
            CheckoutOutcome.AlreadyProcessed => Results.Ok(result),
            CheckoutOutcome.InvalidCart => Results.BadRequest(result),
            CheckoutOutcome.InsufficientStock => Results.Conflict(result),
            CheckoutOutcome.ConflictLockTimeout => Results.Conflict(result),
            _ => Results.Problem(
                detail: "Unexpected checkout outcome.",
                statusCode: StatusCodes.Status500InternalServerError)
        };
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

app.MapGet("/api/orders/{orderNumber}", GetOrderEndpoint)
    .WithName("GetOrder")
    .WithSummary("Get order details")
    .WithDescription("Returns order information by order number")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status404NotFound);

async Task<IResult> GetOrderEndpoint(
    string orderNumber,
    ICheckoutService checkoutService)
{
    try
    {
        var order = await checkoutService.GetOrderByNumberAsync(orderNumber);
        if (order == null)
            return Results.NotFound(new { error = "Order not found", message = $"Order {orderNumber} does not exist" });

        return Results.Ok(order);
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

// ============================================================
// WISHLIST ENDPOINTS
// ============================================================

app.MapPost("/api/wishlist", CreateWishlistEndpoint)
    .WithName("CreateWishlist")
    .WithSummary("Create a new wishlist")
    .WithDescription("Creates a new anonymous wishlist and returns the wishlist token");

app.MapGet("/api/wishlist/{wishlistToken}", GetWishlistEndpoint)
    .WithName("GetWishlist")
    .WithSummary("Get wishlist details")
    .WithDescription("Returns wishlist items with product details");

app.MapPost("/api/wishlist/{wishlistToken}/items", AddWishlistItemEndpoint)
    .WithName("AddWishlistItem")
    .WithSummary("Add item to wishlist")
    .WithDescription("Adds a product to the wishlist");

app.MapDelete("/api/wishlist/{wishlistToken}/items/{productId}", RemoveWishlistItemEndpoint)
    .WithName("RemoveWishlistItem")
    .WithSummary("Remove item from wishlist")
    .WithDescription("Removes a product from the wishlist");

app.MapDelete("/api/wishlist/{wishlistToken}/items", ClearWishlistEndpoint)
    .WithName("ClearWishlist")
    .WithSummary("Clear all wishlist items")
    .WithDescription("Removes all products from the wishlist");

async Task<IResult> CreateWishlistEndpoint(IWishlistService wishlistService)
{
    try
    {
        var wishlist = await wishlistService.CreateWishlistAsync();
        return Results.Created($"/api/wishlist/{wishlist.WishlistToken}", wishlist);
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

async Task<IResult> GetWishlistEndpoint(
    string wishlistToken,
    IWishlistService wishlistService)
{
    try
    {
        var wishlist = await wishlistService.GetWishlistAsync(wishlistToken);
        if (wishlist == null)
            return Results.NotFound(new { error = "Wishlist not found", message = $"Wishlist {wishlistToken} does not exist" });

        return Results.Ok(wishlist);
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

async Task<IResult> AddWishlistItemEndpoint(
    string wishlistToken,
    AddToWishlistRequest request,
    IWishlistService wishlistService)
{
    try
    {
        if (request.ProductId <= 0)
            return Results.BadRequest(new { error = "Invalid product ID" });

        var result = await wishlistService.AddItemAsync(wishlistToken, request);
        return Results.Ok(result);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = "Not found", message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

async Task<IResult> RemoveWishlistItemEndpoint(
    string wishlistToken,
    long productId,
    IWishlistService wishlistService)
{
    try
    {
        if (productId <= 0)
            return Results.BadRequest(new { error = "Invalid product ID" });

        var result = await wishlistService.RemoveItemAsync(wishlistToken, productId);
        return Results.Ok(result);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = "Not found", message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

async Task<IResult> ClearWishlistEndpoint(
    string wishlistToken,
    IWishlistService wishlistService)
{
    try
    {
        var result = await wishlistService.ClearAsync(wishlistToken);
        return Results.Ok(result);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = "Not found", message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

// ============================================================
// SEO ENDPOINTS (Public - for sitemap generation)
// ============================================================

// GET /api/seo/categories
app.MapGet("/api/seo/categories", GetSeoCategories)
    .CacheOutput("seo-cache")
    .WithName("GetSeoCategories")
    .WithSummary("Get all categories for SEO (sitemap)")
    .WithDescription("Returns list of all active categories for sitemap generation. Public endpoints only - no admin dependency.")
    .Produces(StatusCodes.Status200OK);

async Task<IResult> GetSeoCategories(IProductListingQueryService queryService)
{
    try
    {
        var categories = await queryService.GetAllCategoriesForSeoAsync();
        var items = categories.Select(c => new
        {
            Slug = c.Slug,
            IsActive = true,
            IsIndexable = true,
            UpdatedAtUtc = c.UpdatedAtUtc.ToString("O")
        }).ToList();
        return Results.Ok(items);
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

// GET /api/seo/brands
app.MapGet("/api/seo/brands", GetSeoBrands)
    .CacheOutput("seo-cache")
    .WithName("GetSeoBrands")
    .WithSummary("Get all brands for SEO (sitemap)")
    .WithDescription("Returns list of all active brands for sitemap generation. Public endpoints only.")
    .Produces(StatusCodes.Status200OK);

async Task<IResult> GetSeoBrands(TrendplusDbContext dbContext)
{
    try
    {
        var brands = await dbContext.Brands
            .AsNoTracking()
            .Where(b => b.IsActive)
            .Select(b => new
            {
                Slug = b.Slug,
                IsActive = b.IsActive,
                IsIndexable = true,
                UpdatedAtUtc = b.UpdatedAtUtc.ToString("O")
            })
            .ToListAsync();
        return Results.Ok(brands);
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

// GET /api/seo/collections
app.MapGet("/api/seo/collections", GetSeoCollections)
    .CacheOutput("seo-cache")
    .WithName("GetSeoCollections")
    .WithSummary("Get all collections for SEO (sitemap)")
    .WithDescription("Returns list of all active collections for sitemap generation. Public endpoints only.")
    .Produces(StatusCodes.Status200OK);

async Task<IResult> GetSeoCollections(TrendplusDbContext dbContext)
{
    try
    {
        var collections = await dbContext.Collections
            .AsNoTracking()
            .Where(c => c.IsActive)
            .Select(c => new
            {
                Slug = c.Slug,
                IsActive = c.IsActive,
                IsIndexable = true,
                UpdatedAtUtc = c.UpdatedAtUtc.ToString("O")
            })
            .ToListAsync();
        return Results.Ok(collections);
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

// GET /api/seo/products
app.MapGet("/api/seo/products", GetSeoProducts)
    .CacheOutput("seo-cache")
    .WithName("GetSeoProducts")
    .WithSummary("Get all products for SEO (sitemap)")
    .WithDescription("Returns list of all visible and purchasable products for sitemap generation. Public endpoints only.")
    .Produces(StatusCodes.Status200OK);

async Task<IResult> GetSeoProducts(TrendplusDbContext dbContext)
{
    try
    {
        var products = await dbContext.Products
            .AsNoTracking()
            .Where(p => p.IsVisible && p.IsPurchasable)
            .Select(p => new
            {
                Slug = p.Slug,
                IsVisible = p.IsVisible,
                IsPurchasable = p.IsPurchasable,
                IsIndexable = true,
                UpdatedAtUtc = p.UpdatedAtUtc.ToString("O")
            })
            .ToListAsync();
        return Results.Ok(products);
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

// GET /api/seo/stores
app.MapGet("/api/seo/stores", GetSeoStores)
    .CacheOutput("seo-cache")
    .WithName("GetSeoStores")
    .WithSummary("Get all stores for SEO (sitemap)")
    .WithDescription("Returns list of all active stores for sitemap generation. Public endpoints only.")
    .Produces(StatusCodes.Status200OK);

async Task<IResult> GetSeoStores(TrendplusDbContext dbContext)
{
    try
    {
        var stores = await dbContext.Stores
            .AsNoTracking()
            .Where(s => s.IsActive)
            .Select(s => new
            {
                Slug = s.Slug,
                IsActive = s.IsActive,
                IsIndexable = true,
                UpdatedAtUtc = s.UpdatedAtUtc.ToString("O")
            })
            .ToListAsync();
        return Results.Ok(stores);
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

// GET /api/seo/editorial
app.MapGet("/api/seo/editorial", GetSeoEditorial)
    .CacheOutput("seo-cache")
    .WithName("GetSeoEditorial")
    .WithSummary("Get all published editorial for SEO (sitemap)")
    .WithDescription("Returns list of all published editorial content for sitemap generation. Public endpoints only.")
    .Produces(StatusCodes.Status200OK);

async Task<IResult> GetSeoEditorial(TrendplusDbContext dbContext)
{
    try
    {
        var editorial = await dbContext.EditorialArticles
            .AsNoTracking()
            .Where(e => e.Status == TrendplusProdavnica.Domain.Enums.ContentStatus.Published)
            .Select(e => new
            {
                Slug = e.Slug,
                IsActive = e.Status == TrendplusProdavnica.Domain.Enums.ContentStatus.Published,
                IsIndexable = true,
                UpdatedAtUtc = e.UpdatedAtUtc.ToString("O")
            })
            .ToListAsync();
        return Results.Ok(editorial);
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

app.Run();

