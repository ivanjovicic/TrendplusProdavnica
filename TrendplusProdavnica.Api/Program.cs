#nullable enable
using Microsoft.EntityFrameworkCore;
using TrendplusProdavnica.Application.Catalog.Queries;
using TrendplusProdavnica.Application.Catalog.Services;
using TrendplusProdavnica.Application.Content.Queries;
using TrendplusProdavnica.Application.Content.Services;
using TrendplusProdavnica.Application.Stores.Queries;
using TrendplusProdavnica.Application.Stores.Services;
using TrendplusProdavnica.Application.Cart.Services;
using TrendplusProdavnica.Application.Cart.Dtos;
using TrendplusProdavnica.Infrastructure.DependencyInjection;
using TrendplusProdavnica.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("TrendplusDb");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'TrendplusDb' is not configured.");
}

// Add services to the container
builder.Services.AddOpenApi();
builder.Services.AddDbContext<TrendplusDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddInfrastructureQueries();
builder.Services.AddCartServices();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await app.Services.SeedDevelopmentDataAsync();
}

app.UseHttpsRedirection();

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

// ============================================================
// HOME PAGE ENDPOINTS
// ============================================================

app.MapGet("/api/pages/home", HomePageEndpoint)
    .WithName("GetHomePage")
    .WithSummary("Get home page content")
    .WithDescription("Returns home page with featured products, hero section, and dynamic modules")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status500InternalServerError);

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
        var query = new GetSaleListingQuery(page, pageSize, sort, sizes, colors, brands, priceFrom, priceTo, isNew, inStockOnly);
        var result = await listingService.GetSaleListingAsync(query);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
}

// ============================================================
// PRODUCT ENDPOINTS
// ============================================================

app.MapGet("/api/products/{slug}", ProductDetailEndpoint)
    .WithName("GetProductDetail")
    .WithSummary("Get product details")
    .WithDescription("Returns complete product information including variants, media, and related data")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status404NotFound);

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

app.MapGet("/api/brands/{slug}", BrandPageEndpoint)
    .WithName("GetBrandPage")
    .WithSummary("Get brand page")
    .WithDescription("Returns brand information with featured products and category links")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status404NotFound);

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

app.MapGet("/api/collections/{slug}", CollectionPageEndpoint)
    .WithName("GetCollectionPage")
    .WithSummary("Get collection page")
    .WithDescription("Returns collection information with featured products and content blocks")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status404NotFound);

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

app.MapGet("/api/stores/{slug}", StoreDetailEndpoint)
    .WithName("GetStoreDetail")
    .WithSummary("Get store details")
    .WithDescription("Returns complete store information with location, hours, and featured content")
    .Produces(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status404NotFound);

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

app.Run();
