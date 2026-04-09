#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OutputCaching;
using System.Globalization;
using TrendplusProdavnica.Application.Catalog.Listing;
using TrendplusProdavnica.Application.Catalog.Queries;
using TrendplusProdavnica.Application.Catalog.Services;
using TrendplusProdavnica.Application.Content.Queries;
using TrendplusProdavnica.Application.Content.Services;
using TrendplusProdavnica.Application.Stores.Queries;
using TrendplusProdavnica.Application.Stores.Services;
using TrendplusProdavnica.Application.Search.Queries;
using TrendplusProdavnica.Application.Search.Services;
using TrendplusProdavnica.Application.Checkout.Dtos;
using TrendplusProdavnica.Application.Wishlist.Dtos;
using TrendplusProdavnica.Application.Wishlist.Services;
using TrendplusProdavnica.Application.Cart.Services;
using TrendplusProdavnica.Application.Cart.Dtos;
using TrendplusProdavnica.Api.Infrastructure;
using TrendplusProdavnica.Infrastructure.Caching;
using TrendplusProdavnica.Infrastructure.Services;
using TrendplusProdavnica.Infrastructure.DependencyInjection;
using TrendplusProdavnica.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("TrendplusDb");
var outputCacheSettings = builder.Configuration.GetSection("Cache:OutputCache").Get<OutputCacheSettings>() ?? new OutputCacheSettings();

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
builder.Services.AddAdminServices();
builder.Services.AddScoped<ICheckoutService, CheckoutService>();
builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("public-home", policy => policy.Expire(outputCacheSettings.HomePageDuration));
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
app.UseOutputCache();
app.MapControllers();

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

    if (!string.IsNullOrEmpty(sort) && !new[] { "recommended", "newest", "price_asc", "price_desc", "bestsellers" }.Contains(sort))
        return (false, Results.BadRequest(new { error = "Invalid sort parameter", message = "Sort must be one of: recommended, newest, price_asc, price_desc, bestsellers." }));

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
        !new[] { "relevance", "newest", "price_asc", "price_desc", "bestsellers" }.Contains(sort, StringComparer.OrdinalIgnoreCase))
    {
        return (false, Results.BadRequest(new { error = "Invalid sort parameter", message = "Sort must be one of: relevance, newest, price_asc, price_desc, bestsellers." }));
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
        !new[] { "popular", "price_asc", "price_desc", "newest" }.Contains(sort, StringComparer.OrdinalIgnoreCase))
    {
        return (false, Results.BadRequest(new { error = "Invalid sort", message = "Sort must be one of: popular, price_asc, price_desc, newest." }));
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

app.MapGet("/api/listings/category/{slug}", CategoryListingEndpoint)
    .WithName("GetCategoryListing")
    .WithSummary("Get products by category")
    .WithDescription("Returns paginated product listing for a specific category with filtering and sorting options")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status404NotFound);

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

app.MapGet("/api/listings/brand/{slug}", BrandListingEndpoint)
    .WithName("GetBrandListing")
    .WithSummary("Get products by brand")
    .WithDescription("Returns paginated product listing for a specific brand with filtering and sorting options")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status404NotFound);

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

app.MapGet("/api/listings/collection/{slug}", CollectionListingEndpoint)
    .WithName("GetCollectionListing")
    .WithSummary("Get products by collection")
    .WithDescription("Returns paginated product listing for a specific collection with filtering and sorting options")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status404NotFound);

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

app.MapGet("/api/listings/sale", SaleListingEndpoint)
    .WithName("GetSaleListing")
    .WithSummary("Get products on sale")
    .WithDescription("Returns paginated product listing of items on sale")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status400BadRequest);

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

app.MapGet("/api/listings/sale/{categorySlug}", SaleCategoryListingEndpoint)
    .WithName("GetSaleCategoryListing")
    .WithSummary("Get products on sale in category")
    .WithDescription("Returns paginated product listing of items on sale within a specific category")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status404NotFound);

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

app.MapGet("/api/catalog/products", CatalogProductsEndpoint)
    .WithName("GetCatalogProducts")
    .WithSummary("Get product listing page data")
    .WithDescription("Returns paginated PLP products with facets and canonical URL.")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status400BadRequest);

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

app.MapGet("/api/search/products", SearchProductsEndpoint)
    .WithName("SearchProducts")
    .WithSummary("Search products")
    .WithDescription("Searches products using OpenSearch with filters, sorting, and facets.")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status400BadRequest);

app.MapPost("/api/admin/search/reindex", ReindexAllProductsEndpoint)
    .WithName("ReindexAllProducts")
    .WithSummary("Reindex all products")
    .WithDescription("Rebuilds the entire product search index from PostgreSQL.")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status500InternalServerError);

app.MapPost("/api/admin/search/reindex/{productId:long}", ReindexSingleProductEndpoint)
    .WithName("ReindexSingleProduct")
    .WithSummary("Reindex single product")
    .WithDescription("Reindexes one product in OpenSearch.")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status500InternalServerError);

async Task<IResult> SearchProductsEndpoint(
    IProductSearchService searchService,
    string? q = null,
    int page = 1,
    int pageSize = 24,
    string? brand = null,
    string? color = null,
    decimal? size = null,
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

    try
    {
        var query = new ProductSearchQuery(
            q,
            page,
            pageSize,
            brand,
            color,
            size,
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
    .ProducesProblem(StatusCodes.Status500InternalServerError);

async Task<IResult> PlaceOrderEndpoint(
    CheckoutRequest request,
    ICheckoutService checkoutService)
{
    try
    {
        if (string.IsNullOrWhiteSpace(request.CartToken))
            return Results.BadRequest(new { error = "Invalid order", message = "CartToken is required" });

        if (string.IsNullOrWhiteSpace(request.CustomerFirstName) || string.IsNullOrWhiteSpace(request.CustomerLastName))
            return Results.BadRequest(new { error = "Invalid order", message = "Customer name is required" });

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Phone))
            return Results.BadRequest(new { error = "Invalid order", message = "Email and phone are required" });

        if (string.IsNullOrWhiteSpace(request.DeliveryAddressLine1) || string.IsNullOrWhiteSpace(request.DeliveryCity))
            return Results.BadRequest(new { error = "Invalid order", message = "Delivery address is required" });

        var result = await checkoutService.PlaceOrderAsync(request);
        return Results.Ok(result);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = "Invalid order", message = ex.Message });
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

app.Run();
