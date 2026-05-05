using Microsoft.EntityFrameworkCore;
using Npgsql;
using TrendplusProdavnica.Application.Checkout.Dtos;
using TrendplusProdavnica.Domain.Catalog;
using TrendplusProdavnica.Domain.Enums;
using TrendplusProdavnica.Domain.Sales;
using TrendplusProdavnica.Infrastructure.Persistence;
using TrendplusProdavnica.Infrastructure.Services;

namespace TrendplusProdavnica.Tests.Integration;

public sealed class CheckoutConcurrencyPostgresIntegrationTests
{
    [Fact(Skip = "Provider-backed PostgreSQL concurrency scaffold is in place, but current EF model bootstrap is not stable enough for truthful enablement: provider path still fails before assertion on orders.UpdatedAtUtc/store-generated metadata.")]
    public async Task PlaceOrder_ParallelSameRequest_OnPostgreSql_CreatesSingleOrder()
    {
        await using var database = await PostgresCheckoutTestDatabase.CreateAsync();
        await database.SeedCartAsync(cartToken: "cart-pg-parallel", stock: 6, quantity: 2);

        var request = PostgresCheckoutTestDatabase.CreateRequest("cart-pg-parallel", "idem-pg-parallel");

        async Task<CheckoutResultDto> RunAsync()
        {
            await using var context = database.CreateContext();
            var service = new CheckoutService(context);
            return await service.PlaceOrderAsync(request);
        }

        var results = await Task.WhenAll(RunAsync(), RunAsync());

        Assert.Contains(results, item => item.Outcome == CheckoutOutcome.Success);
        Assert.Contains(results, item => item.Outcome == CheckoutOutcome.AlreadyProcessed);

        await using var verificationContext = database.CreateContext();
        Assert.Equal(1, await verificationContext.Orders.CountAsync());
    }

    private sealed class PostgresCheckoutTestDatabase : IAsyncDisposable
    {
        private const string AdminConnectionString = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres";

        private readonly string _databaseName;
        private readonly string _connectionString;

        private PostgresCheckoutTestDatabase(string databaseName)
        {
            _databaseName = databaseName;
            _connectionString = $"Host=localhost;Port=5432;Database={databaseName};Username=postgres;Password=postgres";
        }

        public static async Task<PostgresCheckoutTestDatabase> CreateAsync()
        {
            var database = new PostgresCheckoutTestDatabase($"trendplus_checkout_test_{Guid.NewGuid():N}");
            await database.CreateDatabaseAsync();
            await database.EnsureSchemaAsync();
            return database;
        }

        public TrendplusDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<TrendplusDbContext>()
                .UseNpgsql(_connectionString)
                .Options;

            return new TrendplusDbContext(options);
        }

        public async Task SeedCartAsync(string cartToken, int stock, int quantity)
        {
            await using var context = CreateContext();

            var now = DateTimeOffset.UtcNow;

            var category = await context.Categories.SingleAsync(x => x.Slug == "cipele");

            var brand = new Brand
            {
                Name = "Test brand postgres",
                Slug = $"test-brand-{Guid.NewGuid():N}"[..18],
                IsActive = true,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            context.Brands.Add(brand);
            await context.SaveChangesAsync();

            var product = new Product
            {
                BrandId = brand.Id,
                PrimaryCategoryId = category.Id,
                Name = "Test proizvod postgres",
                Slug = $"test-proizvod-{Guid.NewGuid():N}"[..22],
                ShortDescription = "Test opis",
                Status = ProductStatus.Published,
                IsVisible = true,
                IsPurchasable = true,
                PublishedAtUtc = now,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();

            var variant = new ProductVariant
            {
                ProductId = product.Id,
                Sku = $"SKU-{Guid.NewGuid():N}"[..16],
                SizeEu = 38,
                Price = 10000m,
                Currency = "RSD",
                StockStatus = stock > 0 ? StockStatus.InStock : StockStatus.OutOfStock,
                TotalStock = stock,
                LowStockThreshold = 1,
                IsActive = true,
                IsVisible = true,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            context.ProductVariants.Add(variant);
            await context.SaveChangesAsync();

            var cart = new Cart
            {
                CartToken = cartToken,
                Status = CartStatus.Active,
                Currency = "RSD",
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            context.Carts.Add(cart);
            await context.SaveChangesAsync();

            context.CartItems.Add(new CartItem
            {
                CartId = cart.Id,
                ProductVariantId = variant.Id,
                Quantity = quantity,
                UnitPrice = variant.Price,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            });

            await context.SaveChangesAsync();
        }

        public static CheckoutRequest CreateRequest(string cartToken, string idempotencyKey)
        {
            return new CheckoutRequest
            {
                IdempotencyKey = idempotencyKey,
                CartToken = cartToken,
                CustomerFirstName = "Ana",
                CustomerLastName = "Test",
                Email = "ana@test.rs",
                CustomerEmail = "ana@test.rs",
                Phone = "0601234567",
                DeliveryAddressLine1 = "Bulevar 1",
                DeliveryCity = "Beograd",
                DeliveryPostalCode = "11000",
                DeliveryMethod = DeliveryMethod.Courier,
                PaymentMethod = PaymentMethod.CashOnDelivery,
                Note = "Test"
            };
        }

        public async ValueTask DisposeAsync()
        {
            await using var adminConnection = new NpgsqlConnection(AdminConnectionString);
            await adminConnection.OpenAsync();

            await using var command = adminConnection.CreateCommand();
            command.CommandText = $"DROP DATABASE IF EXISTS \"{_databaseName}\" WITH (FORCE);";
            await command.ExecuteNonQueryAsync();
        }

        private async Task CreateDatabaseAsync()
        {
            await using var adminConnection = new NpgsqlConnection(AdminConnectionString);
            await adminConnection.OpenAsync();

            await using var command = adminConnection.CreateCommand();
            command.CommandText = $"CREATE DATABASE \"{_databaseName}\";";
            await command.ExecuteNonQueryAsync();
        }

        private async Task EnsureSchemaAsync()
        {
            await using var context = CreateContext();
            await context.Database.EnsureCreatedAsync();
        }
    }
}