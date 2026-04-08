using Microsoft.EntityFrameworkCore;
using TrendplusProdavnica.Application.Catalog.Queries;
using TrendplusProdavnica.Application.Catalog.Services;
using TrendplusProdavnica.Infrastructure.DependencyInjection;
using TrendplusProdavnica.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("TrendplusDb");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'TrendplusDb' is not configured.");
}

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<TrendplusDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddInfrastructureQueries();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/catalog/category/{slug}", async (
    string slug,
    IProductListingQueryService listingService,
    int page,
    int pageSize,
    string? sort,
    long[]? sizes,
    string[]? colors,
    long[]? brands,
    decimal? priceFrom,
    decimal? priceTo,
    bool? isOnSale,
    bool? isNew,
    bool? inStockOnly) =>
{
    var query = new GetCategoryListingQuery(slug, page, pageSize, sort, sizes, colors, brands, priceFrom, priceTo, isOnSale, isNew, inStockOnly);
    var result = await listingService.GetCategoryListingAsync(query);
    return Results.Ok(result);
})
.WithName("GetCategoryListing");

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
